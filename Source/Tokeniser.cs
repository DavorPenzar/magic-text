using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RandomText
{
    public static class Tokeniser
    {
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)
        {
            using var stream = new MemoryStream(encoding.GetBytes(text));
            using var reader = new StreamReader(stream);
            return tokeniser.Shatter(reader, options);
        }

        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, ShatteringOptions? options = null) =>
            tokeniser.Shatter(text, Encoding.Default, options);

        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)
        {
            using var stream = new MemoryStream(encoding.GetBytes(text));
            using var reader = new StreamReader(stream);
            return await tokeniser.ShatterAsync(reader, options);
        }

        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, ShatteringOptions? options = null) =>
            await tokeniser.ShatterAsync(text, Encoding.Default, options);
    }
}
