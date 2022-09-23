#if !NET7_0_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class StringSyntaxAttribute : Attribute
    {
        public const string CompositeFormat = nameof(CompositeFormat);
        public const string DateOnlyFormat = nameof(DateOnlyFormat);
        public const string DateTimeFormat = nameof(DateTimeFormat);
        public const string EnumFormat = nameof(EnumFormat);
        public const string GuidFormat = nameof(GuidFormat);
        public const string Json = nameof(Json);
        public const string NumericFormat = nameof(NumericFormat);
        public const string Regex = nameof(Regex);
        public const string TimeOnlyFormat = nameof(TimeOnlyFormat);
        public const string TimeSpanFormat = nameof(TimeSpanFormat);
        public const string Uri = nameof(Uri);
        public const string Xml = nameof(Xml);

        private readonly String _syntax;
        private readonly Object?[] _arguments;

        public String Sytnax => _syntax;
        public Object?[] Arguments => _arguments;

        public StringSyntaxAttribute(String syntax, params Object?[] arguments) : base()
        {
            _syntax = syntax;

            if (arguments is null)
            {
                _arguments = Array.Empty<Object>();
            }
            else
            {
                _arguments = new Object?[arguments.Length];
                arguments.CopyTo(_arguments, 0);
            }
        }

        public StringSyntaxAttribute(String syntax) : this(syntax, Array.Empty<Object>())
        {
        }

        private StringSyntaxAttribute() : this(null!)
        {
        }
    }
}

#endif // NET7_0_OR_GREATER
