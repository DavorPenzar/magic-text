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
    ///     <para>It is strongly advised to implement both <see cref="Shatter(TextReader, ShatteringOptions)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods to return non-executed queries for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>)—moreover, the extension methods from <see cref="TokeniserExtensions" /> <em>expect</em> this. Such queries would allow simultaneous reading and shattering operations from the input, which might be useful when reading the input text from an <em>infinite</em> source (e. g. from <see cref="Console.In" /> or a network channel). Contrarily, the extension methods such as <see cref="TokeniserExtensions.ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> and <see cref="TokeniserExtensions.ShatterToArrayAsync(ITokeniser, TextReader, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> are provided to extract tokens into a fully built container, which is useful when the tokens are read from a finite immutable source (e. g. a <see cref="String" /> or a read-only text file).</para>
    ///     <para>When both <see cref="Shatter(TextReader, ShatteringOptions)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods are implemented (none of them throws <see cref="NotImplementedException" />), they should ultimately return the same enumerable of tokens if called with the same parameters. For example, to implement the <see cref="ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method from the <see cref="Shatter(TextReader, ShatteringOptions)" /> method's code, substitute all <see cref="TextReader.Read(Char[], Int32, Int32)" />, <see cref="TextReader.ReadBlock(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLine()" />, <see cref="TextReader.ReadToEnd()" /> etc. method calls with awaited <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadBlockAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" />, <see cref="TextReader.ReadToEndAsync()" /> etc. method calls.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public interface ITokeniser
    {
        /// <summary>Shatters text read from the <c><paramref name="inputReader" /></c> into tokens.</summary>
        /// <param name="inputReader">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="inputReader" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="inputReader" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable might merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="inputReader" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="inputReader" /></c>.</para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader inputReader, ShatteringOptions? options = null);

        /// <summary>Shatters text read from the <c><paramref name="inputReader" /></c> into tokens asynchronously.</summary>
        /// <param name="inputReader">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="inputReader" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="inputReader" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="inputReader" /></c> is a <see cref="StreamReader" />, the data having already been read from the underlying <see cref="Stream" /> may be irrecoverable after cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <see cref="ITokeniser" /> instance's logic might be <see cref="SynchronizationContext" />-dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="inputReader" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable might merely be an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="inputReader" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToArrayAsync(ITokeniser, TextReader, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="inputReader" /></c>.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>When implementing the method using asynchronous iterators, the <c><paramref name="cancellationToken" /></c> parameter should be set with the <see cref="EnumeratorCancellationAttribute" /> attribute to support cancellation via the <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method.</para>
        /// </remarks>
        public IAsyncEnumerable<String?> ShatterAsync(TextReader inputReader, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, CancellationToken cancellationToken = default);
    }
}
