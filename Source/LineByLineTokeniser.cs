using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        ///         Always say the token is not empty.
        ///     </para>
        /// </summary>
        /// <param name="token">Token to check.</param>
        /// <returns><c>false</c></returns>
        private static bool IsEmptyTokenAlwaysFalse(String? token) =>
            false;

        private Func<String?, Boolean> isEmptyToken;

        /// <summary>
        ///     <para>
        ///         Returns <c>true</c> if the token to check is empty.
        ///     </para>
        ///
        ///     <para>
        ///         This function is used in <see cref="Shatter(StreamReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(StreamReader, ShatteringOptions?)" /> methods to filter out empty tokens if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>.
        ///     </para>
        /// </summary>
        /// <value>Function to check if a token is empty. Default is <see cref="String.IsNullOrEmpty(String)" />.</value>
        [AllowNull]
        protected Func<String?, Boolean> IsEmptyToken
        {
            get => isEmptyToken;
            set
            {
                isEmptyToken = value ?? IsEmptyTokenAlwaysFalse;
            }
        }

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser.
        ///     </para>
        /// </summary>
        public LineByLineTokeniser()
        {
            isEmptyToken = String.IsNullOrEmpty;
        }

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="isEmptyToken">Function to check if a token is empty.</param>
        protected LineByLineTokeniser(Func<String?, Boolean> isEmptyToken)
        {
            this.isEmptyToken = isEmptyToken;
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        /// </summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <remarks>
        ///     <para>
        ///         It is guaranteed that, when called from <see cref="LineByLineTokeniser" />, <paramref name="line" /> will be a string not containing any line end (CR, LF or CRLF). Nonetheless, when calling from a subclass, its programmer may call the method however they wish, but this is beyond the original programmer's responsibility.
        ///     </para>
        ///
        ///     <para>
        ///         The method <strong>should not</strong> produce <see cref="ShatteringOptions.EmptyLineToken" />s nor <see cref="ShatteringOptions.LineEndToken" />s to represent empty lines and line ends. Also, the method <strong>should not</strong> manually filter out empty tokens. Hence no <see cref="ShatteringOptions" /> are available to the method. The result of an empty line (even without possible filtering out of empty tokens) should be an empty enumerable, while empty tokens, empty lines and line ends are treated within the scope of <see cref="LineByLineTokeniser" /> parent class and its methods.
        ///     </para>
        /// </remarks>
        protected abstract IEnumerable<String?> ShatterLine(String line);

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

            // Declare:
            var addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    tokens.Add(options.LineEndToken);
                }

                // Read and shatter next line.

                var line = input.ReadLine();
                if (line is null)
                {
                    break;
                }

                var lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(t => !IsEmptyToken(t));
                }
                lineTokens = lineTokens.ToList();

                // Add line's tokens to `tokens` if necessary. Update `addLineEnd`.
                if (lineTokens.Any())
                {
                    tokens.AddRange(lineTokens);
                    addLineEnd = true;
                }
                else if (options.IgnoreEmptyLines)
                {
                    addLineEnd = false;
                }
                else // `!options.IgnoreEmptyLines`
                {
                    tokens.Add(options.EmptyLineToken);
                    addLineEnd = true;
                }
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

            // Declare:
            var addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    tokens.Add(options.LineEndToken);
                }

                // Read and shatter next line.

                var line = await input.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                var lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(t => !IsEmptyToken(t));
                }
                lineTokens = lineTokens.ToList();

                // Add line's tokens to `tokens` if necessary. Update `addLineEnd`.
                if (lineTokens.Any())
                {
                    tokens.AddRange(lineTokens);
                    addLineEnd = true;
                }
                else if (options.IgnoreEmptyLines)
                {
                    addLineEnd = false;
                }
                else // `!options.IgnoreEmptyLines`
                {
                    tokens.Add(options.EmptyLineToken);
                    addLineEnd = true;
                }
            }

            // Finalise tokens and return.

            tokens.TrimExcess();

            return tokens;
        }
    }
}
