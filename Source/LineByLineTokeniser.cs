using MagicText.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Implements an <see cref="ITokeniser" /> which shatters lines of text one by one.</summary>
    /// <remarks>
    ///     <para>By default, empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. Derived classes may override this behaviour.</para>
    ///     <para>Assuming the default behaviour of the <see cref="TextReader" /> used concerning its <see cref="TextReader.ReadLine()" /> and <see cref="TextReader.ReadLineAsync()" /> methods (such as <see cref="StringReader" />'s and <see cref="StreamReader" />'s behaviour), shattering methods read and process text <em>line-by-line</em> with all <a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF and CRLF line breaks</a> treated the same. These line breaks and the end of the input are considered line ends when shattering text, and are therefore substituted by a <see cref="ShatteringOptions.LineEndToken" /> if <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c>. This behaviour may not be overridden by a derived class.</para>
    ///     <para>The empty lines are substituted by a <see cref="ShatteringOptions.EmptyLineToken" /> if <see cref="ShatteringOptions.IgnoreEmptyLines" /> is <c>false</c>. This behaviour may also not be overridden by a derived class.</para>
    ///
    ///     <h3>Notes to Implementers</h3>
    ///     <para>A derived class must minimally implement the <see cref="ShatterLine(String)" /> method to make a useful instance of the <see cref="LineByLineTokeniser" />.</para>
    ///     <para>Emptiness and non-emptiness of tokens is checked with a <see cref="Func{T, TResult}" /> <c>delegate</c> that returns a <see cref="Boolean" /> rather than a <see cref="Predicate{T}" /> <c>delegate</c> to enable passing the <c>delegate</c> as a parameter to various <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> methods when <em>shattering</em> input text and dealing with output tokens. To convert a <see cref="Predicate{T}" /> to a <see cref="Func{T, TResult}" />, use the explicit conversion.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the function for checking emptiness of tokens (<see cref="IsEmptyToken" />) should be thread-safe, lock the <see cref="LineByLineTokeniser" /> instance during the complete <see cref="Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls to ensure consistent behaviour of the function over a single shattering operation.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public abstract class LineByLineTokeniser : Object, ITokeniser
    {
        protected const string IsEmptyTokenNullErrorMessage = "Token emptiness checking function cannot be null.";
        private const string InputNullErrorMessage = "Input reader cannot be null.";
        protected const string LineNullErrorMessage = "Line string cannot be null.";

        private readonly Func<String?, Boolean> _isEmptyToken;
        private readonly Func<String?, Boolean> _isNonEmptyToken;

        /// <summary>Gets the function (predicate) to check if a token is empty: it returns <c>true</c> if and only if the token to check is empty.</summary>
        /// <returns>The token emptiness checking function (predicate).</returns>
        protected Func<String?, Boolean> IsEmptyToken => _isEmptyToken;

        /// <summary>Gets the function (predicate) to check if a token is non-empty: it returns <c>true</c> if and only if the token to check is non-empty.</summary>
        /// <returns>The token non-emptiness checking function (predicate).</returns>
        protected Func<String?, Boolean> IsNonEmptyToken => _isNonEmptyToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="isEmptyToken">The function (predicate) for checking if a token is empty.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="isEmptyToken" /></c> parameter is <c>null</c>.</exception>
        protected LineByLineTokeniser(Func<String?, Boolean> isEmptyToken) : base()
        {
            _isEmptyToken = isEmptyToken ?? throw new ArgumentNullException(nameof(isEmptyToken), IsEmptyTokenNullErrorMessage);
            _isNonEmptyToken = (new NegativePredicateWrapper<String?>(IsEmptyToken)).NegativePredicate;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The method <see cref="String.IsNullOrEmpty(String)" /> is used for checking if a token is empty.</para>
        /// </remarks>
        public LineByLineTokeniser() : this(String.IsNullOrEmpty)
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <h3>Notes to Implementers</h3>
        ///     <para>The method <strong>should not</strong> produce an <see cref="ShatteringOptions.EmptyLineToken" /> to represent an empty <c><paramref name="line" /></c> or a <see cref="ShatteringOptions.LineEndToken" /> at the <c><paramref name="line" /></c>'s end. Also, the method <strong>should not</strong> manually filter out empty tokens. Hence no <see cref="ShatteringOptions" /> are available to the method. The result of an empty line should be an empty <see cref="IEnumerable{T}" /> of <see cref="String" />s (e. g. <see cref="Enumerable.Empty{TResult}()" />). Empty tokens, empty lines and line ends are treated within the scope of the <see cref="LineByLineTokeniser" /> parent class and its methods.</para>
        ///     <para>It is guaranteed that, when called from the <see cref="LineByLineTokeniser" />'s non-overridable methods, the <c><paramref name="line" /></c> shall be a non-<c>null</c> <see cref="String" /> that does not contain a line end (<a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF or CRLF</a>). Nonetheless, when calling from a derived class, its programmer may call the method however they wish.</para>
        /// </remarks>
        protected abstract IEnumerable<String?> ShatterLine(String line);

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="input" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="TextReader.ReadLine()" /> method call are not caught.</para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            options ??= ShatteringOptions.Default;

            // Declare:
            Boolean addLineEnd = false; // the indicator that a line end should be added

            // Shatter text from the `input` line-by-line.
            while (true)
            {
                // Return the `options.LineEndToken` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    yield return options.LineEndToken;
                }

                // Read and shatter the next line.

                String? line = input.ReadLine();
                if (line is null)
                {
                    yield break;
                }

                IEnumerable<String?> lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(IsNonEmptyToken);
                }

                // Return the appropriate tokens and update `addLineEnd`.
                {
                    Int32 i;

                    using (IEnumerator<String?> en = lineTokens.GetEnumerator())
                    {
                        for (i = 0; en.MoveNext(); ++i)
                        {
                            yield return en.Current;
                        }
                    }

                    do
                    {
                        if (i == 0) // <-- no new tokens were returned (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                yield return options.EmptyLineToken;
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="input" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="input" /></c> is a <see cref="StreamReader" />, the data having already been read from the underlying <see cref="Stream" /> may be irrecoverable after cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <see cref="LineByLineTokeniser" /> instance's logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="TokeniserExtensions.ShatterToArrayAsync(ITokeniser, TextReader, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="TextReader.ReadLineAsync()" /> method call are not caught.</para>
        /// </remarks>
        public async IAsyncEnumerable<String?> ShatterAsync(TextReader input, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            options ??= ShatteringOptions.Default;

            // Declare:
            Boolean addLineEnd = false; // the indicator that a line end should be added

            // Shatter text from the `input` line-by-line.
            while (true)
            {
                // Return the `options.LineEndToken` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return options.LineEndToken;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Read and shatter the next line.

                String? line = await input.ReadLineAsync().ConfigureAwait(continueTasksOnCapturedContext);
                if (line is null)
                {
                    yield break;
                }

                IEnumerable<String?> lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(IsNonEmptyToken);
                }

                // Return the appropriate tokens and update `addLineEnd`.
                {
                    Int32 i;

                    using (IEnumerator<String?> en = lineTokens.GetEnumerator())
                    {
                        for (i = 0; en.MoveNext(); ++i)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            yield return en.Current;
                        }
                    }

                    do
                    {
                        if (i == 0) // <-- no new tokens were returned (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                yield return options.EmptyLineToken;
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }
        }
    }
}
