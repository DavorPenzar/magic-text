#if !NETSTANDARD2_1_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        private readonly Boolean _parameterValue;

        public Boolean ParameterValue => _parameterValue;

        public DoesNotReturnIfAttribute(Boolean parameterValue) : base()
        {
            _parameterValue = parameterValue;
        }

        private DoesNotReturnIfAttribute() : this(false)
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
