using MagicText.Internal;
using MagicText.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineShatteringTokeniser" /> which shatters lines of text by splitting at specific <em>substring</em> separators.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineShatteringTokeniser" /> extension class simulates the <see cref="String.Split(String[], StringSplitOptions)" /> method, unlike the <see cref="RegexSplitTokeniser" />, which simulates the <see cref="Regex.Split(String)" /> method, on each line of text.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class StringSplitTokeniser : LineShatteringTokeniser
    {
        private const string InvalidOptionsErrorMessage = "An option value flag is invalid.";

        private static readonly StringSplitOptions _validOptions;
        private static readonly StringSplitOptions _validOptionsBitwiseComplement;

        /// <summary>Gets the combination of all possible <see cref="StringSplitOptions" /> values.</summary>
        /// <returns>The bitwise combination of all values in the <see cref="StringSplitOptions" /> enumeration type.</returns>
        /// <remarks>
        ///     <para>This property represents the bitwise complement of the <see cref="ValidOptionsBitwiseComplement" /> property.</para>
        /// </remarks>
        private static StringSplitOptions ValidOptions => _validOptions;

        /// <summary>Gets the complement of the combination of all possible <see cref="StringSplitOptions" /> values.</summary>
        /// <returns>The bitwise complement of the bitwise combination of all values in the <see cref="StringSplitOptions" /> enumeration type.</returns>
        /// <remarks>
        ///     <para>This property represents the bitwise complement of the <see cref="ValidOptions" /> property.</para>
        /// </remarks>
        private static StringSplitOptions ValidOptionsBitwiseComplement => _validOptionsBitwiseComplement;

        /// <summary>Initialises static fields.</summary>
        static StringSplitTokeniser()
        {
            StringSplitOptions validOptions = (StringSplitOptions)0;
            foreach (Object option in Enum.GetValues(typeof(StringSplitOptions)))
            {
                validOptions |= (StringSplitOptions)option;
            }

            _validOptions = validOptions;
            _validOptionsBitwiseComplement = ~validOptions;
        }

        private readonly StringSplitOptions _options;
        private readonly String[] _separator;

        /// <summary>Gets the <see cref="StringSplitOptions" /> used by the tokeniser.</summary>
        /// <returns>The internal <see cref="StringSplitOptions" /> used for shattering lines of text at the <see cref="Separator" /> into tokens.</returns>
        /// <remarks>
        ///     <para>Note that <see cref="StringSplitOptions.RemoveEmptyEntries" /> overrides the <see cref="ShatteringOptions.IgnoreEmptyTokens" />: if the former is <c>true</c>, empty tokens are ignored even if the latter is <c>false</c>. This is because the <see cref="Options" /> are applied in the <see cref="ShatterLine(String)" /> method, before evaluating the <see cref="ShatteringOptions" /> in the <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> or the <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method.</para>
        /// </remarks>
        public StringSplitOptions Options => _options;

        /// <summary>Gets the <em>substring</em> separators used by the tokeniser.</summary>
        /// <returns>The internal <em>substring</em> separators.</returns>
        /// <remarks>
        ///     <para>Even if no explicit enumerable of <em>substring</em> separators were provided in the construction of the tokeniser, or an explicit <c>null</c> was passed to the constructor, the <see cref="Separator" /> property would not be <c>null</c>. Instead, it is going to be an empty <see cref="Array" /> of <see cref="String" />s. Furthermore, <c>null</c>s, empty <see cref="String" />s (<see cref="String.Empty" />) and duplicates are eliminated from the <see cref="Array" />—this improves performance of the splitting, but does not affect its behaviour (see documenrtation for <see cref="String.Split(String[], StringSplitOptions)" />).</para>
        ///     <para>By default, it is guaranteed that the <see cref="Separator" /> are all mutually distinct and and ordered by their first appearance in the <see cref="IEnumerable{TSource}" /> passed to the constructor. Furthermore, no element is <c>null</c> or empty. Changing the values of the <see cref="Separator" /> shall cause inconsistent behaviour across multiple calls to the <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods.</para>
        /// </remarks>
        private String[] Separator => _separator;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <em>substring</em> separators to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> are an invalid <see cref="StringSplitOptions" /> value.</exception>
        public StringSplitTokeniser(IEnumerable<String?>? separator, StringSplitOptions options = StringSplitOptions.None) : base()
        {
            if ((Int32)(options & ValidOptionsBitwiseComplement) != 0)
            {
                throw new ArgumentException(InvalidOptionsErrorMessage, nameof(options));
            }

            _separator = (separator?.Where((new NegativePredicateWrapper<String?>(String.IsNullOrEmpty)).NegativePredicate).DistinctPreserveOrder(StringComparer.Ordinal).ToArray() ?? Array.Empty<String>())!; 
            _options = options;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>A <c>null</c> <see cref="Array" /> of <em>substring</em> separators is used by the tokeniser, as well as <see cref="StringSplitOptions.None" />.</para>
        /// </remarks>
        public StringSplitTokeniser() : this((IEnumerable<String>)null!)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <em>substring</em> separator to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> are an invalid <see cref="StringSplitOptions" /> value.</exception>
        public StringSplitTokeniser(String? separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator!, 1), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> are an invalid <see cref="StringSplitOptions" /> value.</exception>
        public StringSplitTokeniser(StringSplitOptions options) : this((IEnumerable<String>)null!, options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <em>substring</em> separators to use.</param>
        public StringSplitTokeniser(params String?[]? separator) : this((IEnumerable<String?>)separator!)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <see cref="Char" /> separators to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> are an invalid <see cref="StringSplitOptions" /> value.</exception>
        public StringSplitTokeniser(IEnumerable<Char>? separator, StringSplitOptions options = StringSplitOptions.None) : this(separator?.Select(Char.ToString), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <see cref="Char" /> separator to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> are an invalid <see cref="StringSplitOptions" /> value.</exception>
        public StringSplitTokeniser(Char separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator.ToString(), 1), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <see cref="Char" /> separators to use.</param>
        public StringSplitTokeniser(params Char[]? separator) : this(separator?.Select(Char.ToString))
        {
        }

        /// <summary>Returns the <em>substring</em> separators used by the tokeniser.</summary>
        /// <returns>The internal <em>substring</em> separators.</returns>
        /// <remarks>
        ///     <para>Even if no explicit enumerable of <em>substring</em> separators were provided in the construction of the tokeniser, or an explicit <c>null</c> was passed to the constructor, the <see cref="Separator" /> property would not be <c>null</c>. Instead, it is going to be an empty <see cref="Array" /> of <see cref="String" />s. Furthermore, <c>null</c>s, empty <see cref="String" />s (<see cref="String.Empty" />) and duplicates are eliminated from the <see cref="Array" />—this improves performance of the splitting, but does not affect its behaviour (see documenrtation for <see cref="String.Split(String[], StringSplitOptions)" />).</para>
        ///     <para>It is guaranteed that the returned separators are all mutually distinct and ordered by their first appearance in the <see cref="IEnumerable{TSource}" /> passed to the constructor.</para>
        ///     <para>This method always creates a new <see cref="Array" /> of <see cref="String" />s and returns it. Changing the contents of the returned <see cref="Array" /> by any of the calls to this method shall affect neither the <see cref="StringSplitTokeniser" /> nor any other <see cref="Array" />s returned by the calls to this method.</para>
        /// </remarks>
        public String[] GetSeparator()
        {
            String[] separator = new String[Separator.Length];
            Separator.CopyTo(separator, 0);

            return separator;
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The splitting is equivalent to calling the <c><paramref name="line" /></c>'s <see cref="String.Split(String[], StringSplitOptions)" /> method with the <see cref="Separator" /> (<see cref="GetSeparator()" />) as the first argument and the <see cref="Options" /> as the second.</para>
        /// </remarks>
        protected override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : line.Split(Separator, Options);
    }
}
