using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagicText.BenchmarkTesting
{
    [MemoryDiagnoser]
    public class StreamBenchmarking : BenchmarkingBase, IAsyncDisposable
    {
        private HttpClient client;
        private HttpRequestMessage request;
        private HttpResponseMessage response;
        private Stream input;

        private ITokeniser tokeniser;
        private TextReader inputReader;

        public HttpClient Client
        {
            get => client;
            private set
            {
                client = value;
            }
        }

        public HttpRequestMessage Request
        {
            get => request;
            private set
            {
                request = value;
            }
        }

        public HttpResponseMessage Response
        {
            get => response;
            private set
            {
                response = value;
            }
        }

        public Stream Input
        {
            get => input;
            private set
            {
                input = value;
            }
        }

        public ITokeniser Tokeniser
        {
            get => tokeniser;
            private set
            {
                tokeniser = value;
            }
        }

        public TextReader InputReader
        {
            get => inputReader;
            private set
            {
                inputReader = value;
            }
        }

        public StreamBenchmarking() : base()
        {
            client = null!;
            request = null!;
            response = null!;
            input = null!;

            tokeniser = null!;
            inputReader = null!;
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            Client = new HttpClient();

            Tokeniser = new ToCharsTokeniser();
        }

        [IterationSetup]
        public async Task IterationSetupAsync()
        {
            Request = new HttpRequestMessage(HttpMethod.Get, ResourceUri);
            Response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            Response.EnsureSuccessStatusCode();

            Input = await Response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            InputReader = new StreamReader(
                stream: Input,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 0x400,
                leaveOpen: true
            );
        }

        [Benchmark(Baseline = true)]
        public void Linq()
        {
            tokeniser.Shatter(InputReader).ToArray();
        }

        [Benchmark]
        public void Builtin()
        {
            tokeniser.ShatterToArray(InputReader);
        }

        [Benchmark]
        public async Task LinqAsync()
        {
            await tokeniser.ShatterAsync(InputReader).ToArrayAsync().ConfigureAwait(false);
        }

        [Benchmark]
        public async Task BuiltinAsync()
        {
            await tokeniser.ShatterToArrayAsync(InputReader).ConfigureAwait(false);
        }

        [IterationCleanup]
        public async void IterationCleanup()
        {
            InputReader.Dispose();

#if NETSTANDARD2_1_OR_GREATER
            await Input.DisposeAsync().ConfigureAwait(false);
#else
            Input.Dispose();
            await ValueTask.CompletedTask.ConfigureAwait(false);
#endif // NETSTANDARD2_1_OR_GREATE

            Response.Dispose();
            Request.Dispose();

            InputReader = null!;
            Input = null!;
            Response = null!;
            Request = null!;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            Client.Dispose();

            Client = null!;
        }

        protected async ValueTask DisposeAsync(Boolean disposing)
        {
            if (disposing && !Disposed)
            {
                Client?.Dispose();
                Request?.Dispose();
                Response?.Dispose();

#if NETSTANDARD2_1_OR_GREATER
                await Input.DisposeAsync().ConfigureAwait(false);
#else
                Input.Dispose();
                await ValueTask.CompletedTask.ConfigureAwait(false);
#endif // NETSTANDARD2_1_OR_GREATE

                InputReader?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing && !Disposed)
            {
                Client?.Dispose();
                Request?.Dispose();
                Response?.Dispose();
                Input?.Dispose();

                InputReader?.Dispose();
            }

            base.Dispose(disposing);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true).ConfigureAwait(false);
        }
    }
}
