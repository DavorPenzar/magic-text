using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Interface for tokenising (<em>shattering</em> into tokens) input texts.
    ///     </para>
    /// </summary>
    public interface ITokeniser
    {
        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Returned enumerable might only be a query for enumerating tokens to allow simultaneously reading and enumerating tokens from <paramref name="input" />. If a fully built container is needed, consider using <see cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" /> extension method instead to improve performance and avoid accidentally enumerating the query after disposing <paramref name="input" />.
        ///     </para>
        ///
        ///     <para>
        ///         The method should return the equivalent enumeration of tokens as <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method). See <em>Remarks</em> for additional information.</param>
        /// <returns>Asynchronous enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data having already been read from underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.
        ///     </para>
        ///
        ///     <para>
        ///         When implementing the method using iterators, <paramref name="cancellationToken" /> may be set with <see cref="EnumeratorCancellationAttribute" /> attribute to support cancellation via <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method. Similarly otherwise, of course, it may be passed to the underlying <see cref="IAsyncEnumerable{T}" /> using the same extension method.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueTasksOnCapturedContext" /> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by <paramref name="input" />, and thus <paramref name="continueTasksOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        ///
        ///     <para>
        ///         Returned enumerable might only be a query for enumerating tokens to allow simultaneously reading and enumerating tokens from <paramref name="input" />. If a fully built container is needed, consider using <see cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance and avoid accidentally enumerating the query after disposing <paramref name="input" />.
        ///     </para>
        ///
        ///     <para>
        ///         The method should ultimately return the equivalent enumeration of tokens as <see cref="Shatter(TextReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IAsyncEnumerable<String?> ShatterAsync(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false);
    }
}
