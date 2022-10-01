using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineShatteringTokeniser" /> which shatters lines of text at each character.</summary>
    /// <remarks>
    ///     <para>Empty characters (tokens) (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those characters that yield <c>true</c> when converted to <see cref="String" />s via the <see cref="Char.ToString(Char)" /> method and checked via the <see cref="String.IsNullOrEmpty(String)" /> method.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class ToCharsTokeniser : LineShatteringTokeniser
    {
        /// <summary>Creates a default tokeniser.</summary>
        public ToCharsTokeniser() : base()
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating characters (tokens) (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : line.Select(Char.ToString);
    }
}
