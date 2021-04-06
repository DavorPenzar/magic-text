using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text at each character.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Empty tokens are considered those characters that yield <c>true</c> when converted to strings via <see cref="Char.ToString(Char)" /> method and checked via <see cref="String.IsNullOrEmpty(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same.
    ///     </para>
    /// </remarks>
    public class CharTokeniser : LineByLineTokeniser
    {
        /// <summary>
        ///     <para>
        ///         Create a default tokeniser.
        ///     </para>
        /// </summary>
        public CharTokeniser() : base()
        {
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        /// </summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="line" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The returned enumerable is merely a query. If multiple enumerations over it should be performed, it is advisable to convert it to a fully built container beforehand, such as <see cref="List{T}" /> via <see cref="List{T}.List(IEnumerable{T})" /> constructor or <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.
        ///     </para>
        /// </remarks>
        protected override IEnumerable<String?> ShatterLine(String line) =>
            line?.Select(Char.ToString) ?? throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
    }
}
