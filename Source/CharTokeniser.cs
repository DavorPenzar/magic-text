using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    /// <summary>Tokeniser which shatters text at each character.</summary>
    /// <remarks>
    ///     <para>
    ///         Empty tokens (<see cref="ShatteringOptions.IgnoreEmptyTokens" />) are considered those characters that yield <c>true</c> when converted to strings via <see cref="Char.ToString(Char)" /> method and checked via <see cref="String.IsNullOrEmpty(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same. These + the end of the input are considered line ends and are substituted by <see cref="ShatteringOptions.LineEndToken" /> if <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c>.
    ///     </para>
    ///
    ///     <para>
    ///         Empty lines are substituted by <see cref="ShatteringOptions.EmptyLineToken" /> if <see cref="ShatteringOptions.IgnoreEmptyLines" /> is <c>false</c>.
    ///     </para>
    /// </remarks>
    public class CharTokeniser : LineByLineTokeniser
    {
        /// <summary>Create a default tokeniser.</summary>
        public CharTokeniser() : base()
        {
        }

        /// <summary>Shatter a single line into tokens.</summary>
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
