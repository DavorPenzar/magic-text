using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text at each character.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Empty tokens are considered those characters that yield <c>true</c> when converted to strings via <see cref="Char.ToString()" /> method and checked via <see cref="String.IsNullOrEmpty(String)" /> method.
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
        ///         Instantiate a tokeniser.
        ///     </para>
        /// </summary>
        public CharTokeniser() : base()
        {
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        ///
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c> and <paramref name="tokens" /> is non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to <paramref name="tokens" /> before any token extracted from <paramref name="line" />. However, <see cref="ShatteringOptions.LineEndToken" /> is not added after <paramref name="line" />'s tokens.
        ///     </para>
        /// </summary>
        /// <param name="tokens">List of tokens</param>
        /// <param name="line"></param>
        /// <param name="options"></param>
        override protected void ShatterLine(ref List<String?> tokens, String line, ShatteringOptions options)
        {
            // Add `options.LineEndToken` if necessary.
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            // Shatter `line` and filter out empty tokens.
            IEnumerable<String?> lineTokens = line.Select(c => c.ToString());
            if (options.IgnoreEmptyTokens)
            {
                lineTokens = lineTokens.Where(t => !String.IsNullOrEmpty(t));
            }
            lineTokens = lineTokens.ToList();

            // Add tokens to `tokens` if necessary.
            if (lineTokens.Any())
            {
                tokens.AddRange(lineTokens);
            }
            else if (!options.IgnoreEmptyLines)
            {
                tokens.Add(options.EmptyLineToken);
            }
        }
    }
}
