#if !NETSTANDARD2_1_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        private readonly String _parameterName;

        public String ParameterName => _parameterName;

        public NotNullIfNotNullAttribute(String parameterName) : base()
        {
            _parameterName = parameterName;
        }

        private NotNullIfNotNullAttribute() : this(null!)
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
