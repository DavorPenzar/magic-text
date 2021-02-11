using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Static class with convenient extension methods for instances of <see cref="ITokeniser" /> interface.
    ///     </para>
    /// </summary>
    public static class Tokeniser
    {
        /// <summary>
        ///     <para>
        ///         Shatter <paramref name="text" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, ShatteringOptions? options = null)
        {
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
        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, ShatteringOptions? options = null)
        {
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
