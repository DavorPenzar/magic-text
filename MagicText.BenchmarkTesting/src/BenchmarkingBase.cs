using BenchmarkDotNet.Attributes;
using System;

namespace MagicText.BenchmarkTesting
{
    public abstract class BenchmarkingBase : Object, IDisposable
    {
        private Boolean disposed;
        private String resourceUri;

        private Boolean ignoreEmptyTokens;
        private Boolean ignoreLineEnds;
        private Boolean ignoreEmptyLines;
        private String? lineEndToken;
        private String? emptyLineToken;

        public Boolean Disposed
        {
            get => disposed;
            private set
            {
                disposed = value;
            }
        }

        [Params(@"http://gutenberg.org/ebooks/10.txt.utf-8")]
        public String ResourceUri
        {
            get => resourceUri;
            set
            {
                resourceUri = value;
            }
        }

        [Params(true)]
        public Boolean IgnoreEmptyTokens
        {
            get => ignoreEmptyTokens;
            set
            {
                ignoreEmptyTokens = value;
            }
        }

        [Params(false)]
        public Boolean IgnoreLineEnds
        {
            get => ignoreLineEnds;
            set
            {
                ignoreLineEnds = value;
            }
        }

        [Params(false)]
        public Boolean IgnoreEmptyLines
        {
            get => ignoreEmptyLines;
            set
            {
                ignoreEmptyLines = value;
            }
        }

        [Params("\n")]
        public String? LineEndToken
        {
            get => lineEndToken;
            set
            {
                lineEndToken = value;
            }
        }

        [Params("")]
        public String? EmptyLineToken
        {
            get => emptyLineToken;
            set
            {
                emptyLineToken = value;
            }
        }

        public BenchmarkingBase() : base()
        {
            disposed = false;
            resourceUri = null!;

            ignoreEmptyTokens = false;
            ignoreLineEnds = false;
            ignoreEmptyLines = false;
            lineEndToken = Environment.NewLine;
            emptyLineToken = String.Empty;
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing && !Disposed)
            {
                // ...
            }

            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        ~BenchmarkingBase()
        {
            Dispose(false);
        }
    }
}
