using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText.Example
{
    internal class TextDownloader : IDisposable
    {
        private readonly Boolean _disposeMembers;
        private readonly ILogger<TextDownloader> _logger;
        private readonly HttpClient _client;
        private readonly ITokeniser _tokeniser;
        private readonly ShatteringOptions _shatteringOptions;

        private Boolean disposed;

        protected Boolean DisposeMembers => _disposeMembers;
        protected ILogger<TextDownloader> Logger => _logger;
        protected HttpClient Client => _client;
        protected ITokeniser Tokeniser => _tokeniser;
        protected ShatteringOptions ShatteringOptions => _shatteringOptions;

        protected Boolean Disposed
        {
            get => disposed;
            private set
            {
                disposed = value;
            }
        }

        public TextDownloader(
            ILogger<TextDownloader> logger,
            HttpClient client,
            ITokeniser tokeniser,
            ShatteringOptions? shatteringOptions = null,
            Boolean disposeMembers = true
        )
        {
            _disposeMembers = disposeMembers;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _tokeniser = tokeniser ?? throw new ArgumentNullException(nameof(tokeniser));
            _shatteringOptions = shatteringOptions ?? ShatteringOptions.Default;

            disposed = false;
        }

        public async Task<String?[]> DownloadTextAsync(
            String? url = null,
            Encoding? encoding = null,
            CancellationToken cancellationToken = default
        )
        {
            String?[] tokens;

            Logger.LogDebug(
                "Fetching tokens from the web source at {url}.",
                    url
            );

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url ?? String.Empty))
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
                            "The web source at {url} returned no content ({statusCode:D} {status}).",
                                url,
                                Convert.ToInt32(HttpStatusCode.NoContent),
                                HttpStatusCode.NoContent.ToString()
                        );

                        tokens = Array.Empty<String>();
                    }
                    else
                    {
                        tokens = await Tokeniser.ShatterToArrayAsync(
                            input: await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                            encoding: encoding ?? Encoding.UTF8,
                            options: ShatteringOptions,
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new HttpRequestException(
                        await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false),
                        null,
                        statusCode
                    );
                }

                Logger.LogInformation(
                    "Tokens successfully fetched from the web source at {url} ({statusCode:D} {status}). Token count: {count:D}",
                        url,
                        Convert.ToInt32(statusCode),
                        statusCode.ToString(),
                        tokens.Length
                );

                return tokens;
            }
        }

        protected virtual void Dispose(Boolean disposing)
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
