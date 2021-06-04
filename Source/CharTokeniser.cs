using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineByLineTokeniser" /> which shatters lines of text at each character.</summary>
    /// <remarks>
    ///     <para>Empty characters (tokens) (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those characters that yield <c>true</c> when converted to <see cref="String" />s via the <see cref="Char.ToString(Char)" /> method and checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overriden by a derived class.</para>
    /// </remarks>
    public class CharTokeniser : LineByLineTokeniser
    {
        /// <summary>Creates a tokeniser.</summary>
        public CharTokeniser() : base()
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into characters (tokens).</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>The enumerable of characters (tokens) (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="line" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating characters (tokens) (also known as <em>deferred execution</em>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overriden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : line.Select(Char.ToString);
    }
}
