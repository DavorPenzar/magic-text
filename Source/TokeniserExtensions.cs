using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        private const string TokensTaskNullErrorMessage = "Shatterig operation task is `null`.";
        private const string TokensNullErrorMessage = "Returned tokens enumerable is `null`.";

        /// <summary>
        ///     <para>
        ///         Shatter <paramref name="text" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. Parameter <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?, CancellationToken)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Since <see cref="String" />s are immutable and encapsulated <see cref="StringReader" /> is not available outside of the method, <paramref name="cancellationToken" /> may be used to cancel the shattering process without extra caution.
        ///     </para>
        /// </remarks>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, CancellationToken cancellationToken = default)
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
                tokens = new List<String?>(tokeniser.Shatter(textReader, options, cancellationToken) ?? throw new InvalidOperationException(TokensNullErrorMessage));
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
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context. See <em>Remarks</em> for additional information.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is enumerable of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. If <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Since <see cref="String" />s are immutable and encapsulated <see cref="StringReader" /> is not available outside of the method, <paramref name="cancellationToken" /> may be used to cancel the shattering process without extra caution.
        ///     </para>
        ///
        ///     <para>
        ///         Parameter <paramref name="continueOnCapturedContext" /> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <paramref name="text" />. Providing <c>true</c> as <paramref name="continueOnCapturedContext" /> indeed passes the value to <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may in turn slow down the shattering process. The parameter is exposed merely to mantain consistency of method signatures and calls.
        ///     </para>
        /// </remarks>
        public static async Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueOnCapturedContext = false)
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
                var tokensTask = tokeniser.ShatterAsync(textReader, options, cancellationToken, continueOnCapturedContext) ?? throw new InvalidOperationException(TokensTaskNullErrorMessage);
                tokens = new List<String?>(await tokensTask ?? throw new InvalidOperationException(TokensNullErrorMessage));
            }

            tokens.TrimExcess();

            return tokens;
        }
    }
}
