using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText.Example.BLL
{
    /// <summary>Provides methods for downloading and simultaneously tokenising (<em>shattering</em>) text from online resources.</summary>
    internal sealed class TextDownloader : Object, IDisposable
    {
        private const string LoggerNullErrorMessage = "Logger cannot be null.";
        private const string ClientNullErrorMessage = "HTTP client cannot be null.";
        private const string TokeniserNullErrorMessage = "Tokeniser cannot be null.";
        private const string DisposedErrorMessage = "Cannot use a disposed TextDownloader.";

        private readonly Boolean _disposeMembers;
        private readonly ILogger<TextDownloader> _logger;
        private readonly HttpClient _client;
        private readonly ITokeniser _tokeniser;
        private readonly ShatteringOptions _shatteringOptions;

        private Boolean disposed;

        /// <summary>Gets the indicator of whether or not disposable members should be disposed when disposing this <see cref="TextDownloader" />.</summary>
        /// <returns>The indicator of whether or not disposable members should be disposed.</returns>
        public Boolean DisposeMembers => _disposeMembers;

        /// <summary>Gets the <see cref="ILogger{TCategoryName}" /> of <see cref="TextDownloader" /> for logging status messages.</summary>
        /// <returns>The internal <see cref="ILogger{TCategoryName}" /> of <see cref="TextDownloader" />.</returns>
        private ILogger<TextDownloader> Logger => _logger;

        /// <summary>Gets the <see cref="HttpClient" /> for communicating with the online resource.</summary>
        /// <returns>The internal <see cref="HttpClient" />.</returns>
        private HttpClient Client => _client;

        /// <summary>Gets the <see cref="ITokeniser" /> for tokenising (<em>shattering</em>) downloaded text.</summary>
        /// <returns>The internal <see cref="ITokeniser" />.</returns>
        private ITokeniser Tokeniser => _tokeniser;

        /// <summary>Gets the <see cref="MagicText.ShatteringOptions" /> defining how the downloaded text should be tokenised (<em>shattered</em>).</summary>
        /// <returns>The internal <see cref="MagicText.ShatteringOptions" />.</returns>
        /// <remarks>
        ///    <para>Even if no explicit <see cref="MagicText.ShatteringOptions" /> were provided during the <see cref="TextDownloader" /> construction, or an explicit <c>null</c> was passed to the constructor, the <see cref="ShatteringOptions" /> property would not be <c>null</c>. Instead, it is going to be the default <see cref="MagicText.ShatteringOptions" /> (<see cref="MagicText.ShatteringOptions.Default" />).</para>
        /// </remarks>
        private MagicText.ShatteringOptions ShatteringOptions => _shatteringOptions;

        /// <summary>Gets or sets the flag indicating whether or not this <see cref="TextDownloader" /> is disposed.</summary>
        /// <value>The new flag indicating if this <see cref="TextDownloader" /> is disposed.</value>
        /// <returns>The flag indicating if this <see cref="TextDownloader" /> is disposed.</returns>
        /// <remarks>
        ///    <para>This property should be changed <strong>only once</strong>: in the <see cref="Dispose(Boolean)" /> method (or the original <see cref="Dispose()" /> method) to <c>true</c> when disposing the <see cref="TextDownloader" />.</para>
        /// </remarks>
        private Boolean Disposed
        {
            get => disposed;
            set
            {
                disposed = value;
            }
        }

        /// <summary>Creates a <see cref="TextDownloader" />.</summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /> of <see cref="TextDownloader" /> for logging status messages.</param>
        /// <param name="client">The <see cref="HttpClient" /> for communicating with the online resource.</param>
        /// <param name="tokeniser">The <see cref="ITokeniser" /> for tokenising (<em>shattering</em>) downloaded text.</param>
        /// <param name="shatteringOptions">The <see cref="MagicText.ShatteringOptions" /> defining how the downloaded text should be tokenised (<em>shattered</em>). If <c>null</c>, the default <see cref="MagicText.ShatteringOptions" /> (<see cref="MagicText.ShatteringOptions.Default" />) are used.</param>
        /// <param name="disposeMembers">Whether or not disposable members should be disposed when disposing.</param>
        /// <exception cref="ArgumentNullException">
        ///    <para>Either:</para>
        ///    <list type="number">
        ///         <item>the <c><paramref name="logger" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="client" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>

        public TextDownloader(
            ILogger<TextDownloader> logger,
            HttpClient client,
            ITokeniser tokeniser,
            MagicText.ShatteringOptions? shatteringOptions = null,
            Boolean disposeMembers = true
        ) : base()
        {
            _disposeMembers = disposeMembers;
            _logger = logger ??
                throw new ArgumentNullException(
                    nameof(logger),
                    LoggerNullErrorMessage
                );
            _client = client ??
                throw new ArgumentNullException(
                    nameof(client),
                    ClientNullErrorMessage
                );
            _tokeniser = tokeniser ??
                throw new ArgumentNullException(
                    nameof(tokeniser),
                    TokeniserNullErrorMessage
                );
            _shatteringOptions = shatteringOptions ?? ShatteringOptions.Default;

            disposed = false;
        }

        /// <summary>Downloads and simultaneously tokenises (<em>shatters</em>) text from the online resource.</summary>
        /// <param name="uri">Request <a href="http://en.wikipedia.org/wiki/Uniform_Resource_Identifier"><em>URI</em></a> from which to download text. If <c>null</c>, only the internal <see cref="HttpClient" />'s <see cref="HttpClient.BaseAddress" /> is used.</param>
        /// <param name="encoding">Encoding to use for decode the response as text. If <c>null</c>, <see cref="Encoding.UTF8" /> is used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the downloading and shattering operation.</param>
        /// <returns>A task that represents downloading and shattering text from the online resource. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the resource.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="TextDownloader" /> is already disposed.</exception>
        /// <exception cref="HttpRequestException">The <a href="http://httpwg.org/specs/"><em>HTTP</em></a> response is unsuccessful.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Exceptions thrown by the <see cref="HttpRequestMessage(HttpMethod, String)" /> constructor and the <see cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)" /> method are not caught.</para>
        /// </remarks>
        public async Task<String?[]> DownloadTextAsync(
            String? uri = null,
            Encoding? encoding = null,
            CancellationToken cancellationToken = default
        )
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(
                    nameof(TextDownloader),
                    DisposedErrorMessage
                );
            }

            String?[] tokens;

            Stopwatch stopwatch = new Stopwatch();

            if (!Uri.TryCreate(Client.BaseAddress, uri, out Uri? fullUri))
            {
                try
                {
                    fullUri = uri is null ?
                        Client.BaseAddress :
                        new Uri(uri, UriKind.RelativeOrAbsolute);
                }
                catch (UriFormatException exception)
                {
                    Logger.LogWarning(
                        exception,
                        "Failed to deduce resource URI. Base address: {baseAddress}, provided URI: {uri}",
                            Client.BaseAddress,
                            uri
                    );
                }
            }

            Logger.LogDebug(
                "Fetching tokens from the web source at {uri}.",
                    fullUri
            );

            stopwatch.Start();

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (
                HttpResponseMessage response = await Client.SendAsync(
                    request,
                    completionOption: HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false)
            )
            {
                HttpStatusCode statusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    if (statusCode == HttpStatusCode.NoContent)
                    {
                        Logger.LogWarning(
                            "The web source at {uri} returned no content ({statusCode:D} {status}).",
                                fullUri,
                                Convert.ToInt32(HttpStatusCode.NoContent),
                                HttpStatusCode.NoContent.ToString()
                        );

                        tokens = Array.Empty<String>();
                    }
                    else
                    {
#if NET5_0_OR_GREATER
                        Stream content = await response.Content.ReadAsStreamAsync(
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
#else
                        Stream content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif // NET5_0_OR_GREATER

                        try
                        {
                            tokens = await Tokeniser.ShatterToArrayAsync(
                                input: content,
                                encoding: encoding ?? Encoding.UTF8,
                                options: ShatteringOptions,
                                cancellationToken: cancellationToken
                            ).ConfigureAwait(false);
                        }
                        finally
                        {
#if NETSTANDARD2_1_OR_GREATER
                            await content.DisposeAsync().ConfigureAwait(false);
#else
                            content.Dispose();
#endif // NETSTANDARD2_1_OR_GREATER
                        }
                    }

                    stopwatch.Stop();
                }
                else
                {
                    stopwatch.Stop();

                    Logger.LogError(
                        "Failed to receive content from the web source at {uri} ({statusCode:D} {status}). Time elapsed: {duration:D} ms",
                            fullUri,
                            Convert.ToInt32(statusCode),
                            statusCode.ToString(),
                            stopwatch.ElapsedMilliseconds

                    );

#if NET5_0_OR_GREATER
                    String content = await response.Content.ReadAsStringAsync(
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
#else
                    String content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif // NET5_0_OR_GREATER

#if NET5_0_OR_GREATER
                    throw new HttpRequestException(
                        message: content,
                        inner: null,
                        statusCode: statusCode
                    );
#else
                    throw new HttpRequestException(
                        message: content
                    );
#endif // NET5_0_OR_GREATER
                }

                Logger.LogInformation(
                    "Tokens successfully fetched from the web source at {uri} ({statusCode:D} {status}). Time elapsed: {duration:D} ms, token count: {count:D}",
                        fullUri,
                        Convert.ToInt32(statusCode),
                        statusCode.ToString(),
                        stopwatch.ElapsedMilliseconds,
                        tokens.Length
                );
            }

            return tokens;
        }

        /// <summary>Releases the unmanaged resources used by this <see cref="TextDownloader" />, and optionally releases the managed resources.</summary>
        /// <param name="disposing">If <c>true</c>, both managed and unmanaged resources are released; otherwise only unmanaged resources are released.</param>
        /// <remarks>
        ///    <para>Once disposed, the <see cref="TextDownloader" /> cannot be used for downloading texts (using the <see cref="DownloadTextAsync(String, Encoding, CancellationToken)" /> method) anymore. This is true even if the <paramref name="disposing" /> parameter is <c>false</c>.</para>
        /// </remarks>
        private void Dispose(Boolean disposing)
        {
            if (disposing && !Disposed)
            {
                if (DisposeMembers)
                {
                    Client.Dispose();
                    if (Tokeniser is IDisposable disposableTokeniser)
                    {
                        disposableTokeniser.Dispose();
                    }
                }
            }

            Disposed = true;
        }

        /// <summary>Releases the managed and unmanaged resources used by this <see cref="TextDownloader" />.</summary>
        /// <remarks>
        ///    <para>Once disposed, the <see cref="TextDownloader" /> cannot be used for downloading texts (using the <see cref="DownloadTextAsync(String, Encoding, CancellationToken)" /> method) anymore.</para>
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~TextDownloader()
        {
            Dispose(false);
        }
    }
}
