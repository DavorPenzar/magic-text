using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text shattering its lines one by one.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same.
    ///     </para>
    /// </remarks>
    public abstract class LineByLineTokeniser : ITokeniser
    {
        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser.
        ///     </para>
        /// </summary>
        public LineByLineTokeniser()
        {
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        ///
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c> and <paramref name="tokens" /> is non-empty, <see cref="ShatteringOptions.LineEndToken" /> should be added to <paramref name="tokens" /> before any token extracted from <paramref name="line" />. However, <see cref="ShatteringOptions.LineEndToken" /> should not be added after <paramref name="line" />'s tokens.
        ///     </para>
        /// </summary>
        /// <param name="tokens">List of tokens</param>
        /// <param name="line"></param>
        /// <param name="options"></param>
        protected abstract void ShatterLine(ref List<String?> tokens, String line, ShatteringOptions options);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Stream for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <remarks>
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is false and the resulting enumerable of tokens is (otherwise) non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to the end of the resulting tokens.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null)
        {
            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Initialise tokens.
            var tokens = new List<String?>();

            // Shatter text from `input` line-by-line.
            while (true)
            {
                var line = input.ReadLine();
                if (line is null)
                {
                    break;
                }

                ShatterLine(ref tokens, line, options);
            }

            // Add `options.LineEndToken` to `tokens` if necessary.
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            // Finalise tokens and return.

            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Stream for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Task whose result is enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <remarks>
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is false and the resulting enumerable of tokens is (otherwise) non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to the end of the resulting tokens.
        ///     </para>
        /// </remarks>
        public async Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null)
        {
            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Initialise tokens.
            var tokens = new List<String?>();

            // Shatter text from `input` line-by-line.
            while (true)
            {
                var line = await input.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                ShatterLine(ref tokens, line, options);
            }

            // Add `options.LineEndToken` to `tokens` if necessary.
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            // Finalise tokens and return.

            tokens.TrimExcess();

            return tokens;
        }
    }
}
