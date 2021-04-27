using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Static class with auxiliary extension methods for instances of <see cref="ITokeniser" /> interface.</summary>
    public static class TokeniserExtensions
    {
        private const string TokeniserNullErrorMessage = "Tokeniser instance may not be `null`.";
        private const string TextNullErrorMessage = "Input text string may not be `null`.";
        private const string TokensNullErrorMessage = "Token enumerable returned by shattering may not be `null`.";

        /// <summary>Shatter <paramref name="text" /> into tokens synchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>Query to enumerate tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. Parameter <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Returned enumerable is merely a query for enumerating tokens (<em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from <paramref name="text" />. If a fully built container is needed, consider using <see cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" /> extension method instead to improve performance.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method method call and <see cref="IEnumerable{T}.GetEnumerator" /> are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
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

            using TextReader textReader = new StringReader(text);

            foreach (String? token in tokeniser.Shatter(textReader, options) ?? throw new InvalidOperationException(TokensNullErrorMessage))
            {
                yield return token;
            }
        }

        /// <summary>Shatter text read from <paramref name="input" /> into token list synchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>List of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The returned enumerable is a fully-built container (not merely a query (<em>deferred execution</em>)) and is therefore safe to enumerate even after disposing <paramref name="input" />. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method (notably <see cref="ArgumentNullException" />) are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IReadOnlyList<String?> ShatterToList(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(input, options) ?? throw new InvalidOperationException(TokensNullErrorMessage));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatter <paramref name="text" /> into token list synchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>List of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The returned enumerable is a fully-built container (not merely a query (<em>deferred execution</em>)). However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="Shatter(ITokeniser, String, ShatteringOptions?)" /> method (notably <see cref="ArgumentNullException" />) are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        public static IReadOnlyList<String?> ShatterToList(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(text, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatter <paramref name="text" /> into tokens asynchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> and <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension methods). See <em>Remarks</em> for additional information.</param>
        /// <returns>Query to asynchronously enumerate tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>. Parameter <paramref name="text" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Since <see cref="String" />s are immutable and encapsulated <see cref="StringReader" /> is not available outside of the method, <paramref name="cancellationToken" /> may be used to cancel the shattering process without extra caution.
        ///     </para>
        ///
        ///     <para>
        ///         Parameter <paramref name="continueTasksOnCapturedContext" /> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <paramref name="text" />. Providing <c>true</c> as <paramref name="continueTasksOnCapturedContext" /> indeed passes the value to <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may in turn slow down the shattering process. The parameter is exposed only to mantain consistency of method signatures and calls.
        ///     </para>
        ///
        ///     <para>
        ///         Returned enumerable is merely a query for enumerating tokens (<em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from <paramref name="text" />. If a fully built container is needed, consider using <see cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            using TextReader textReader = new StringReader(text);

            await foreach (String? token in tokeniser.ShatterAsync(textReader, options, cancellationToken, continueTasksOnCapturedContext)?.ConfigureAwait(continueTasksOnCapturedContext) ?? throw new InvalidOperationException(TokensNullErrorMessage))
            {
                yield return token;
            }
        }

        /// <summary>Shatter text read from <paramref name="input" /> into token list asynchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> and <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension methods). See <em>Remarks</em> for additional information.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is list of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data having already been read from underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueTasksOnCapturedContext" /> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by <paramref name="input" />, and thus <paramref name="continueTasksOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        ///
        ///     <para>
        ///         The ultimately returned enumerable is a fully-built container (not merely a query (<em>deferred execution</em>)). However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method (notably <see cref="ArgumentNullException" />) are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IReadOnlyList<String?>> ShatterToListAsync(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            await foreach (String? token in tokeniser.ShatterAsync(input, options, cancellationToken, continueTasksOnCapturedContext)?.ConfigureAwait(continueTasksOnCapturedContext) ?? throw new InvalidOperationException(TokensNullErrorMessage))
            {
                tokens.Add(token);
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatter <paramref name="text" /> into token list asynchronously.</summary>
        /// <param name="tokeniser">Tokeniser used for shattering.</param>
        /// <param name="text">Input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> and <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension methods). See <em>Remarks</em> for additional information.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is list of tokens (in the order they were read) read from <paramref name="text" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="tokeniser" /> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Method <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> call returns <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Since <see cref="String" />s are immutable and encapsulated <see cref="StringReader" /> is not available outside of the method, <paramref name="cancellationToken" /> may be used to cancel the shattering process without extra caution.
        ///     </para>
        ///
        ///     <para>
        ///         Parameter <paramref name="continueTasksOnCapturedContext" /> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <paramref name="text" />. Providing <c>true</c> as <paramref name="continueTasksOnCapturedContext" /> indeed passes the value to <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may in turn slow down the shattering process. The parameter is exposed only to mantain consistency of method signatures and calls.
        ///     </para>
        ///
        ///     <para>
        ///         The ultimately returned enumerable is a fully-built container (not merely a query (<em>deferred execution</em>)). However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <see cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" /> method (notably <see cref="ArgumentNullException" />) are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IReadOnlyList<String?>> ShatterToListAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            await foreach (String? token in tokeniser.ShatterAsync(text, options, cancellationToken, continueTasksOnCapturedContext).ConfigureAwait(continueTasksOnCapturedContext))
            {
                tokens.Add(token);
            }
            tokens.TrimExcess();

            return tokens;
        }
    }
}
