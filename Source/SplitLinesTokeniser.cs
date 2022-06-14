using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineShatteringTokeniser" /> which shatters text into lines of text.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineShatteringTokeniser" /> extension class is inspired by <a href="http://docs.python.org/"><em>Python</em>'s</a> built-in <a href="http://docs.python.org/library/stdtypes.html#str.splitlines"><c>str.splitlines</c></a> and <a href="http://docs.python.org/library/io.html#io.IOBase.readlines"><c>io.IOBase.readlines</c></a> methods (but does not mimick them entirely), unlike the <see cref="StringSplitTokeniser" />, which simulates the <see cref="String.Split(String[], StringSplitOptions)" /> method, on each line of text.</para>
    ///     <para>Empty lines and tokens (which are ignored if <see cref="IgnoreEmptyLines" /> and <see cref="ShatteringOptions.IgnoreEmptyLines" /> or <see cref="ShatteringOptions.IgnoreEmptyTokens" /> are <c>true</c>) are considered those lines that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public class SplitLinesTokeniser : LineShatteringTokeniser
    {
        private readonly Boolean _ignoreEmptyLines;

        /// <summary>Gets the policy of ignoring empty lines: <c>true</c> if ignoring, <c>false</c> otherwise.</summary>
        /// <returns>If empty lines should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>If empty lines should be ignored, shattering an empty line via the <see cref="ShatterLine(String)" /> returns an empty enumerable of tokens. Otherwise empty lines are shattered the same as non-empty lines.</para>
        ///     <para>A <see cref="String" /> <c>line</c> is considered empty if it yields <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method.</para>
        /// </remarks>
        public Boolean IgnoreEmptyLines => _ignoreEmptyLines;

        /// <summary>Gets the function (predicate) to check if a line is empty: it returns <c>true</c> if and only if the line to check is empty.</summary>
        /// <returns>The line emptiness checking function (predicate).</returns>
        /// <remarks>
        ///     <para>This property merely returns the <see cref="LineShatteringTokeniser.IsEmptyToken" /> property.</para>
        /// </remarks>
        protected Func<String?, Boolean> IsEmptyLine => IsEmptyToken;

        /// <summary>Gets the function (predicate) to check if a line is non-empty: it returns <c>true</c> if and only if the line to check is non-empty.</summary>
        /// <returns>The line non-emptiness checking function (predicate).</returns>
        /// <remarks>
        ///     <para>This property merely returns the <see cref="LineShatteringTokeniser.IsNonEmptyToken" /> property.</para>
        /// </remarks>
        protected Func<String?, Boolean> IsNonEmptyLine => IsNonEmptyToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored. See <see cref="IgnoreEmptyLines" /> for more information.</param>
        public SplitLinesTokeniser(Boolean ignoreEmptyLines) : base()
        {
            _ignoreEmptyLines = ignoreEmptyLines;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The property <see cref="IgnoreEmptyLines" /> is set to <c>true</c>, i. e. empty lines are ignored by the constructed <see cref="SplitLinesTokeniser" />.</para>
        /// </remarks>
        public SplitLinesTokeniser() : this(true)
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>If the <c><paramref name="line" /></c> is non-empty or the <see cref="IgnoreEmptyLines" /> property is <c>false</c>, an enumerable of a single element <c><paramref name="line" /></c> is returned; otherwise an empty enumerable is returned.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            if (!(IgnoreEmptyLines && IsEmptyLine(line)))
            {
                yield return line;
            }
        }
    }
}
