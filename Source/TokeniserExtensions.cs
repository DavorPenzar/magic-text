using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Static class with auxiliary extension methods for instances of <see cref="ITokeniser" /> interface.
    ///     </para>
    /// </summary>
    public static class TokeniserExtensions
    {
        private const string TokeniserNullErrorMessage = "Tokeniser instance may not be `null`.";
        private const string TextNullErrorMessage = "Input text string may not be `null`.";
        private const string TokensNullErrorMessage = "Returned tokens enumerable is `null`.";

        /// <summary>
        ///     <para>
        ///         Shatter <paramref name="text" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. Parameter <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Parameter <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> returns <c>null</c>.</exception>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            List<String?> tokens;

            using (TextReader textReader = new StringReader(text))
            {
                tokens = new List<String?>(tokeniser.Shatter(textReader, options) ?? throw new InvalidOperationException(TokensNullErrorMessage));
            }

            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>
        ///     <para>
        ///         Shatter <paramref name="text" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. If <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Parameter <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?)" /> ultimately returns <c>null</c>.</exception>
        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            List<String?> tokens;

            using (TextReader textReader = new StringReader(text))
            {
                tokens = new List<String?>(await tokeniser.ShatterAsync(textReader, options) ?? throw new InvalidOperationException(TokensNullErrorMessage));
            }

            tokens.TrimExcess();

            return tokens;
        }
    }
}
