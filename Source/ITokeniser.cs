using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Provides methods for tokenising (<em>shattering</em> into tokens) input texts.</summary>
    /// <remarks>
    ///     <h3>Notes to Implementers</h3>
    ///     <para>It is strongly advised to implement both <see cref="Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods to return non-executed queries for enumerating tokens (also known as <em>deferred execution</em>)—moreover, the extension methods from <see cref="TokeniserExtensions" /> <em>expect</em> this. Such queries would allow simultaneous reading and shattering operations from the input, which might be useful when reading the input text from an <em>infinite</em> source (e. g. from <see cref="Console.In" /> or a network channel). Contrarily, the extension methods such as <see cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" /> and <see cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> are provided to extract tokens into a fully built container, which is useful when the tokens are read from a finite immutable source (e. g. a <see cref="String" /> or a read-only text file).</para>
    ///     <para>When both <see cref="Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods are implemented (none of them throws <see cref="NotImplementedException" />), they should ultimately return the same enumerable of tokens if called with the same parameters. For example, to implement the <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method from the <see cref="Shatter(TextReader, ShatteringOptions?)" /> method's code, substitute all <see cref="TextReader.Read(Span{Char})" />, <see cref="TextReader.ReadBlock(Span{Char})" />, <see cref="TextReader.ReadLine()" /> etc. method calls with <c>await</c> <see cref="TextReader.ReadAsync(Memory{Char}, CancellationToken)" />, <see cref="TextReader.ReadBlockAsync(Memory{Char}, CancellationToken)" />, <see cref="TextReader.ReadLineAsync()" /> etc. method calls.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public interface ITokeniser
    {
        /// <summary>Shatters the text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="input">The reader from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable might merely be a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        /// </remarks>
        /// <seealso cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null);

        /// <summary>Shatters the text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="input">The reader from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) should be used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" /> or <see cref="TextReader.ReadLineAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="input" /></c> is a <see cref="StreamReader" />, data having already been read from the underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable might merely be an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>If the method is implemented using iterators, the parameter <c><paramref name="cancellationToken" /></c> should be set with the <see cref="EnumeratorCancellationAttribute" /> attribute to support cancellation via the <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method. Similarly otherwise, of course, the parameter may be passed to the underlying <see cref="IAsyncEnumerable{T}" /> using the same extension method.</para>
        /// </remarks>
        /// <seealso cref="Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="TokeniserExtensions.ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="TokeniserExtensions.ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public IAsyncEnumerable<String?> ShatterAsync(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false);
    }
}
