#if !NETSTANDARD2_1_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
        public DoesNotReturnAttribute() : base()
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
