using MagicText.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineByLineTokeniser" /> which shatters lines of text by breaking it at specific <em>substring</em> separators.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineByLineTokeniser" /> extension class simulates the <see cref="String.Split(String[], StringSplitOptions)" /> method, unlike the <see cref="RegexBreakTokeniser" />, which simulates the <see cref="Regex.Split(String)" /> method, on each line of text.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    ///     <para>Changing any of the properties—public or private—breaks the consistency or even the functionality of the <see cref="StringSplitTokeniser" />. By doing so, the behaviour of the <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods, as well as the <see cref="GetSeparators" /> method, is unexpected and no longer guaranteed.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public class StringSplitTokeniser : LineByLineTokeniser
    {
        private const string SeparatorsNullErrorMessage = "Separators enumerable cannot be null.";
        private const string SeparatorEmptyOrNullErrorMessage = "Separators cannot be null or empty.";
        private const string InvalidOptionsErrorMessage = "An option value flag is invalid.";

        private static readonly StringSplitOptions _validOptions;

        /// <summary>Gets the union of all possible <see cref="StringSplitOptions" /> values.</summary>
        /// <returns>The bitwise union (or) of all values in the <see cref="StringSplitOptions" /> enumeration type.</returns>
        protected static StringSplitOptions ValidOptions => _validOptions;

        /// <summary>Initialises static fields.</summary>
        static StringSplitTokeniser()
        {
            StringSplitOptions validOptions = (StringSplitOptions)0;
            foreach (Object option in Enum.GetValues(typeof(StringSplitOptions)))
            {
                validOptions |= (StringSplitOptions)option;
            }

            _validOptions = validOptions;
        }

        private readonly StringSplitOptions _options;
        private readonly String[] _separators;

        /// <summary>Gets the <see cref="StringSplitOptions" /> used by the tokeniser.</summary>
        /// <returns>The internal <see cref="StringSplitOptions" /> used for shattering lines of text at the <see cref="Separators" /> into tokens.</returns>
        /// <remarks>
        ///     <para>Note that <see cref="StringSplitOptions.RemoveEmptyEntries" /> overrides the <see cref="ShatteringOptions.IgnoreEmptyTokens" />: if the former is <c>true</c>, empty tokens are ignored even if the latter is <c>false</c>. This is because the <see cref="Options" /> are applied in the <see cref="ShatterLine(String)" /> method, before evaluating the <see cref="ShatteringOptions" /> in the <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions)" /> metod or the <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method.</para>
        /// </remarks>
        public StringSplitOptions Options => _options;

        /// <summary>Gets the <em>substring</em> separators used by the tokeniser.</summary>
        /// <returns>The internal <em>substring</em> separators.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending in a line end (<a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF or CRLF</a>)) by the tokeniser, without filtering and replacement of empty lines, is done by calling the <c>line</c>'s <see cref="String.Split(String[], StringSplitOptions)" /> method with <see cref="Separators" /> as the first argument and <see cref="Options" /> as the second argument.</para>
        ///     <para>By default, it is guaranteed that the <see cref="Separators" /> are all mutually distinct and sorted ascendingly as compared by the <see cref="StringComparer.Ordinal" />. Changing the values of the <see cref="Separators" /> shall cause inconsistent behaviour across multiple calls to the <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(System.IO.TextReader, ShatteringOptions)" /> and <see cref="LineByLineTokeniser.ShatterAsync(System.IO.TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods.</para>
        /// </remarks>
        private String[] Separators => _separators;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separators">The <em>substring</em> separators to use. May be empty, but must not be <c>null</c>.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="separators" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>an item in the <c><paramref name="separators" /></c> is <c>null</c> or an empty <see cref="String" /> (<see cref="String.Empty" />), or </item>
        ///         <item>the <c><paramref name="options" /></c> contain an invalid flag.</item>
        ///     </list>
        /// </exception>
        public StringSplitTokeniser(IEnumerable<String> separators, StringSplitOptions options = StringSplitOptions.None) : base()
        {
            if (separators is null)
            {
                throw new ArgumentNullException(nameof(separators), SeparatorsNullErrorMessage);
            }

            String[] separatorsArray = separators.DistinctSorted(StringComparer.Ordinal);
            if (separatorsArray.Length != 0 && String.IsNullOrEmpty(separatorsArray[0]))
            {
                throw new ArgumentException(SeparatorEmptyOrNullErrorMessage, nameof(separators));
            }

            if ((options & ~ValidOptions) != 0)
            {
                throw new ArgumentException(InvalidOptionsErrorMessage, nameof(options));
            }

            _separators = separatorsArray;
            _options = options;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>An empty <see cref="Array" /> of <em>substring</em> separators is used by the tokeniser, as well as <see cref="StringSplitOptions.None" />.</para>
        /// </remarks>
        public StringSplitTokeniser() : this(Enumerable.Empty<String>())
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <em>substring</em> separator to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="separator" /></c> is <c>null</c> or an empty <see cref="String" /> (<see cref="String.Empty" />), or </item>
        ///         <item>the <c><paramref name="options" /></c> contain an invalid flag.</item>
        ///     </list>
        /// </exception>
        public StringSplitTokeniser(String separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator, 1), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> contain an invalid flag.</exception>
        /// <remarks>
        ///     <para>An empty <see cref="Array" /> of <em>substring</em> separators is used by the tokeniser.</para>
        /// </remarks>
        public StringSplitTokeniser(StringSplitOptions options) : this(Enumerable.Empty<String>(), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separators">The <em>substring</em> separators to use. May be empty, but must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="separators" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">An item in the <c><paramref name="separators" /></c> is <c>null</c> or an empty <see cref="String" /> (<see cref="String.Empty" />).</exception>
        public StringSplitTokeniser(params String[] separators) : this((IEnumerable<String>)separators)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separators">The <see cref="Char" /> separators to use. May be empty, but must not be <c>null</c>.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="separators" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> contain an invalid flag.</exception>
        public StringSplitTokeniser(IEnumerable<Char> separators, StringSplitOptions options = StringSplitOptions.None) : this(separators?.Select(Char.ToString)!, options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separator">The <see cref="Char" /> separator to use.</param>
        /// <param name="options">The <see cref="StringSplitOptions" /> to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="options" /></c> contain an invalid flag.</exception>
        public StringSplitTokeniser(Char separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator.ToString(), 1), options)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="separators">The <see cref="Char" /> separators to use. May be empty, but must not be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="separators" /></c> parameter is <c>null</c>.</exception>
        public StringSplitTokeniser(params Char[] separators) : this(separators?.Select(Char.ToString)!)
        {
        }

        /// <summary>Returns the <em>substring</em> separators used by the tokeniser.</summary>
        /// <returns>The internal <em>substring</em> separators.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending in a line end (<a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF or CRLF</a>)) by the tokeniser, without filtering and replacement of empty lines, is done by calling the <c>line</c>'s <see cref="String.Split(String[], StringSplitOptions)" /> method with <see cref="Separators" /> as the first argument and <see cref="Options" /> as the second argument.</para>
        ///     <para>It is guaranteed that the returned separators are all mutually distinct and sorted ascendingly as compared by the <see cref="StringComparer.Ordinal" />.</para>
        ///     <para>This method always creates a new <see cref="Array" /> of <see cref="String" />s and returns it. Changing the contents of the returned <see cref="Array" /> by any of the calls to this method shall affect neither the <see cref="StringSplitTokeniser" /> nor any subsequent calls to this method.</para>
        /// </remarks>
        public String[] GetSeparators()
        {
            String[] separators = new String[Separators.Length];
            Array.Copy(Separators, separators, Separators.Length);

            return separators;
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        protected sealed override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            return line.Split(Separators, Options);
        }
    }
}
