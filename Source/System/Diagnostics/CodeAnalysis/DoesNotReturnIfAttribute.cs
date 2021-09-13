#if NETSTANDARD2_0

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

#endif
