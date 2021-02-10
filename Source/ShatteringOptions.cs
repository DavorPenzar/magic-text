using System;

namespace RandomText
{
    public class ShatteringOptions
    {
        public Boolean IgnoreEmptyTokens { get; set; } = false;
        public Boolean IgnoreLineEnds { get; set; } = false;
        public Boolean IgnoreEmptyLines { get; set; } = false;
        public String? LineEndToken { get; set; } = Environment.NewLine;
        public String? EmptyLineToken { get; set; } = String.Empty;
    }
}
