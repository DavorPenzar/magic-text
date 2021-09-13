#if NETSTANDARD2_0

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        private readonly Boolean _returnValue;

        public Boolean ReturnValue => _returnValue;

        public MaybeNullWhenAttribute(Boolean returnValue) : base()
        {
            _returnValue = returnValue;
        }

        private MaybeNullWhenAttribute() : this(false)
        {
        }
    }
}

#endif
