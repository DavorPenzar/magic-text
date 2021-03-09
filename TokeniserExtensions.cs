using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Static class with convenient extension methods for instances of <see cref="ITokeniser" /> interface.
    ///     </para>
    /// </summary>
    public static class TokeniserExtensions
    {
        private const string TokeniserNullErrorMessage = "Tokeniser instance may not be `null`.";
        private const string TextNullErrorMessage = "Input text string may not be `null`.";

        /// <summary>
        ///     <para>
        ///         Shatter <paramref name="text" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tokeniser" /> is <c>null</c>. If <paramref name="text" /> is <c>null</c>.</exception>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, ShatteringOptions? options = null)
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

            using (var textStream = new MemoryStream(Encoding.Default.GetBytes(text)))
            using (var textReader = new StreamReader(textStream, Encoding.Default))
            {
                tokens = tokeniser.Shatter(textReader, options).ToList();
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
        /// <returns>Task whose result is enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tokeniser" /> is <c>null</c>. If <paramref name="text" /> is <c>null</c>.</exception>
        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, ShatteringOptions? options = null)
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

            using (var textStream = new MemoryStream(Encoding.Default.GetBytes(text)))
            using (var textReader = new StreamReader(textStream, Encoding.Default))
            {
                tokens = (await tokeniser.ShatterAsync(textReader, options)).ToList();
            }

            tokens.TrimExcess();

            return tokens;
        }
    }
}
