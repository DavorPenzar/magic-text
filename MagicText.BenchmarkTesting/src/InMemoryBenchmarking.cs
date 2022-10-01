using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MagicText.BenchmarkTesting
{
    [MemoryDiagnoser]
    public class InMemoryBenchmarking : BenchmarkingBase
    {
        private String text;
        private ITokeniser tokeniser;
        private System.IO.TextReader inputReader;

        public String Text
        {
            get => text;
            private set
            {
                text = value;
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

        public System.IO.TextReader InputReader
        {
            get => inputReader;
            private set
            {
                inputReader = value;
            }
        }

        public InMemoryBenchmarking() : base()
        {
            text = null!;
            tokeniser = null!;
            inputReader = null!;
        }

        [GlobalSetup]
        public async Task GlobalSetupAsync()
        {
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, ResourceUri))
            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                Text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            Tokeniser = new ToCharsTokeniser();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            InputReader = new StringReader(Text);
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
        public void IterationCleanup()
        {
            InputReader.Dispose();

            InputReader = null!;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing && !Disposed)
            {
                InputReader?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
