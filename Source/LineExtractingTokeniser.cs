using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineByLineTokeniser" /> which shatters text into lines of text.</summary>
    /// <remarks>
    ///     <para>Empty lines and tokens (which are ignored if <see cref="IgnoreEmptyLines" /> and <see cref="ShatteringOptions.IgnoreEmptyLines" /> or <see cref="ShatteringOptions.IgnoreEmptyTokens" /> are <c>true</c>) are considered those lines that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    /// </remarks>
    /// <seealso cref="LineByLineTokeniser" />
    /// <seealso cref="ITokeniser" />
    [CLSCompliant(true)]
    public class LineExtractingTokeniser : LineByLineTokeniser
    {
        private readonly Boolean _ignoreEmptyLines;

        /// <summary>Gets the policy of ignoring empty lines: <c>true</c> if ignoring, <c>false</c> otherwise.</summary>
        /// <returns>If empty lines should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>If empty lines should be ignored, shattering an empty line via the <see cref="ShatterLine(String)" /> returns an empty enumerable of tokens. Otherwise empty lines are shattered the same as non-empty lines.</para>
        ///     <para>A <see cref="String" /> <c>line</c> is considered empty if it yields <c>true</c> when evaluated via the <see cref="IsEmptyLine" /> predicate.</para>
        /// </remarks>
        public Boolean IgnoreEmptyLines => _ignoreEmptyLines;

        /// <summary>Gets the function (predicate) to check if a line is empty: it returns <c>true</c> if and only if the line to check is empty.</summary>
        /// <returns>The line emptiness checking function (predicate).</returns>
        /// <remarks>
        ///     <para>This property merely returns the <see cref="LineByLineTokeniser.IsEmptyToken" /> property.</para>
        /// </remarks>
        /// <seealso cref="IsNonEmptyLine" />
        /// <seealso cref="LineByLineTokeniser.IsEmptyToken" />
        /// <seealso cref="LineByLineTokeniser.IsNonEmptyToken" />
        protected Func<String, Boolean> IsEmptyLine => IsEmptyToken;

        /// <summary>Gets the function (predicate) to check if a line is non-empty: it returns <c>true</c> if and only if the line to check is non-empty.</summary>
        /// <returns>The line non-emptiness checking function (predicate).</returns>
        /// <remarks>
        ///     <para>This property merely returns the <see cref="LineByLineTokeniser.IsNonEmptyToken" /> property.</para>
        /// </remarks>
        /// <seealso cref="IsEmptyLine" />
        /// <seealso cref="LineByLineTokeniser.IsNonEmptyToken" />
        /// <seealso cref="LineByLineTokeniser.IsEmptyToken" />
        protected Func<String, Boolean> IsNonEmptyLine => IsNonEmptyToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored. See <see cref="IgnoreEmptyLines" /> for more information.</param>
        /// <seealso cref="IgnoreEmptyLines" />
        public LineExtractingTokeniser(Boolean ignoreEmptyLines) : base()
        {
            _ignoreEmptyLines = ignoreEmptyLines;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The property <see cref="IgnoreEmptyLines" /> is set to <c>true</c>, i. e. empty lines are ignored by the constructed <see cref="LineExtractingTokeniser" />.</para>
        /// </remarks>
        /// <seealso cref="IgnoreEmptyLines" />
        public LineExtractingTokeniser() : this(true)
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into a token enumerable.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>If the <c><paramref name="line" /></c> is non-empty or the <see cref="IgnoreEmptyLines" /> property is <c>false</c>, the enumerable of a single element <c><paramref name="line" /></c> is returned; otherwise an empty enumerable is returned.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="line" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating characters (tokens) (also known as <em>deferred execution</em>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
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
