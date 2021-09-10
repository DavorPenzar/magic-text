#if NETSTANDARD2_0

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        private readonly Boolean _returnValue;

        public Boolean ReturnValue => _returnValue;

        public NotNullWhenAttribute(Boolean returnValue) : base()
        {
            _returnValue = returnValue;
        }
    }
}

#endif
