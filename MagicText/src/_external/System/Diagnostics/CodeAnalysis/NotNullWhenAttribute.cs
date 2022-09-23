#if !NETSTANDARD2_1_OR_GREATER

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

        private NotNullWhenAttribute() : this(false)
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
