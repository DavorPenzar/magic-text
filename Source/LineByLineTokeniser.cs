using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text shattering its lines one by one.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A derived class must minimally implement <see cref="ShatterLine(String)" /> method to make a useful (non-abstract) instance of <see cref="LineByLineTokeniser" />.
    ///     </para>
    ///
    ///     <para>
    ///         By default, empty tokens are considered those tokens that yield <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> method. Derived classes may override this behaviour.
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same.
    ///     </para>
    ///
    ///     <para>
    ///         No thread safety mechanism is implemented nor assumed by the class. If the function for checking emptiness of tokens (<see cref="IsEmptyToken" />) should be thread-safe, lock the tokeniser during complete <see cref="Shatter(TextReader, ShatteringOptions?, CancellationToken)" /> and <see cref="ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls to ensure consistent behaviour of the function over a single shattering process.
    ///     </para>
    /// </remarks>
    public abstract class LineByLineTokeniser : ITokeniser
    {
        private const string IsEmptyTokenNullErrorMessage = "Function for checking emptiness of tokens may not be `null`.";
        private const string InputNullErrorMessage = "Input stream reader may not be `null`.";
        protected const string LineNullErrorMessage = "Line string may not be `null`.";

        /// <summary>
        ///     <para>
        ///         Always indicate the token as non-empty.
        ///     </para>
        /// </summary>
        /// <param name="token">Token to check.</param>
        /// <returns><c>false</c></returns>
        protected static Boolean IsEmptyTokenAlwaysFalse(String? token) =>
            false;

        private readonly Func<String?, Boolean> _isEmptyToken;

        /// <returns>Function to check if a token is empty: it returns <c>true</c> if the token to check is empty.</returns>
        protected Func<String?, Boolean> IsEmptyToken => _isEmptyToken;

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser.
        ///     </para>
        /// </summary>
        public LineByLineTokeniser() : this(String.IsNullOrEmpty)
        {
        }

        /// <summary>
        ///     <para>
        ///         Initialise a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="isEmptyToken">Function to check if a token is empty.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="isEmptyToken" /> is <c>null</c>.</exception>
        protected LineByLineTokeniser(Func<String?, Boolean> isEmptyToken)
        {
            _isEmptyToken = isEmptyToken ?? throw new ArgumentNullException(nameof(isEmptyToken), IsEmptyTokenNullErrorMessage);
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        /// </summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="line" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The method <strong>should not</strong> produce <see cref="ShatteringOptions.EmptyLineToken" />s to represent empty lines and <see cref="ShatteringOptions.LineEndToken" />s for line ends. Also, the method <strong>should not</strong> manually filter out empty tokens. Hence no <see cref="ShatteringOptions" /> are available to the method. The result of an empty line should be an empty enumerable, while empty tokens, empty lines and line ends are treated within the scope of <see cref="LineByLineTokeniser" /> parent class and its methods.
        ///     </para>
        ///
        ///     <para>
        ///         It is guaranteed that, when called from <see cref="LineByLineTokeniser" />'s non-overridable methods, <paramref name="line" /> shall be a non-<c>null</c> string not containing a line end (CR, LF or CRLF). Nonetheless, when calling from a derived class, its programmer may call the method however they wish, but this is beyond the original programmer's scope.
        ///     </para>
        /// </remarks>
        protected abstract IEnumerable<String?> ShatterLine(String line);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data already read from underlying <see cref="Stream" /> may be irrecoverable. Therefore the method returns the enumerable of tokens extracted up until the moment of cancellation, without throwing an exception (<see cref="OperationCanceledException" /> or <see cref="TaskCanceledException" />), in case the task was cancelled.
        ///     </para>
        ///
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is false and the final line was non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to the end of the resulting tokens.
        ///     </para>
        ///
        ///     <para>
        ///         It is guaranteed that the returned enumerable is a fully built container, such as <see cref="List{T}" />, and not merely an enumeration query.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Initialise tokens.
            var tokens = new List<String?>();

            // Declare:
            var addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    tokens.Add(options.LineEndToken);
                }

                // Read and shatter next line if not cancelled.

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var line = input.ReadLine();
                if (line is null)
                {
                    break;
                }

                var lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(t => !IsEmptyToken(t));
                }

                // Add appropriate tokens to `tokens` if any. Update `addLineEnd`.
                {
                    int oldCount = tokens.Count;

                    tokens.AddRange(lineTokens);
                    do
                    {
                        if (tokens.Count == oldCount) // <-- no new tokens were added (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                tokens.Add(options.EmptyLineToken);
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }

            // Finalise tokens and return.

            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults are used.</param>
        /// <param name="cancellationToken">Cancellation token. See <em>Remarks</em> for additional information.</param>
        /// <param name="continueOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (<see cref="TextReader.ReadLineAsync" /> method calls) should be marshalled back to the original context. See <em>Remarks</em> for additional information.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Although the method accepts <paramref name="cancellationToken" /> to support cancelling the operation, this should be used with caution. For instance, if <paramref name="input" /> is <see cref="StreamReader" />, data already read from underlying <see cref="Stream" /> may be irrecoverable. Therefore the method returns the enumerable of tokens extracted up until the moment of cancellation, without throwing an exception (<see cref="OperationCanceledException" /> or <see cref="TaskCanceledException" />), in case the task was cancelled.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueOnCapturedContext" /> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by <paramref name="input" />, and thus <paramref name="continueOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        ///
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is false and the final line was non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to the end of the resulting tokens.
        ///     </para>
        ///
        ///     <para>
        ///         It is guaranteed that the returned enumerable is a fully built container, such as <see cref="List{T}" />, and not merely an enumeration query.
        ///     </para>
        /// </remarks>
        public async Task<IEnumerable<String?>> ShatterAsync(TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean continueOnCapturedContext = false)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            if (options is null)
            {
                options = new ShatteringOptions();
            }

            // Initialise tokens.
            var tokens = new List<String?>();

            // Declare:
            var addLineEnd = false; // indicator that a line end should be added

            // Shatter text from `input` line-by-line.
            while (true)
            {
                // Add `options.LineEndToken` to `tokens` if necessary.
                if (!options.IgnoreLineEnds && addLineEnd)
                {
                    tokens.Add(options.LineEndToken);
                }

                // Read and shatter next line if not cancelled.

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var line = await input.ReadLineAsync().ConfigureAwait(continueOnCapturedContext);
                if (line is null)
                {
                    break;
                }

                var lineTokens = ShatterLine(line);
                if (options.IgnoreEmptyTokens)
                {
                    lineTokens = lineTokens.Where(t => !IsEmptyToken(t));
                }

                // Add appropriate tokens to `tokens` if any. Update `addLineEnd`.
                {
                    int oldCount = tokens.Count;

                    tokens.AddRange(lineTokens);
                    do
                    {
                        if (tokens.Count == oldCount) // <-- no new tokens were added (the line is empty)
                        {
                            if (options.IgnoreEmptyLines)
                            {
                                addLineEnd = false;

                                break;
                            }
                            else
                            {
                                tokens.Add(options.EmptyLineToken);
                            }
                        }

                        addLineEnd = true;
                    }
                    while (false);
                }
            }

            // Finalise tokens and return.

            tokens.TrimExcess();

            return tokens;
        }
    }
}
