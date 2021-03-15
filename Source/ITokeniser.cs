using System;
using System.Collections.Generic;
using System.IO;
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
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data already read from underlying <see cref="Stream" /> may be irrecoverable. Therefore implementations of the method should return the enumerable of tokens extracted up until the moment of cancellation, and not just throw an exception (<see cref="OperationCanceledException" /> or <see cref="TaskCanceledException" />), in case the task was cancelled.
        ///     </para>
        ///
        ///     <para>
        ///         The method should return the same enumerable of tokens as <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context. See <em>Remarks</em> for additional information.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, <paramref name="input" /> is <see cref="StreamReader" />, data already read from underlying <see cref="Stream" /> may be irrecoverable. Therefore implementations of the method should return the enumerable of tokens extracted up until the moment of cancellation, and not just throw an exception (<see cref="OperationCanceledException" /> or <see cref="TaskCanceledException" />), in case the task was cancelled.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueOnCapturedContext" /> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by <paramref name="input" />, and thus <paramref name="continueOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        ///
        ///     <para>
        ///         The method should ultimately return the same enumerable of tokens as <see cref="Shatter(TextReader, ShatteringOptions?, CancellationToken)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public Task<IEnumerable<String?>> ShatterAsync(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueOnCapturedContext = false);
    }
}
