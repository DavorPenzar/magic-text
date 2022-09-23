#if !NETSTANDARD2_1_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
        private readonly Boolean _returnValue;
        private readonly String[] _members;

        public Boolean ReturnValue => _returnValue;
        public String[] Members => _members;

        public MemberNotNullWhenAttribute(Boolean returnValue, params String[] members) : base()
        {
            _returnValue = returnValue;
            if (members is null)
            {
                _members = Array.Empty<String>();
            }
            else
            {
                _members = new String[members.Length];
                members.CopyTo(_members, 0);
            }
        }

        public MemberNotNullWhenAttribute(Boolean returnValue, String member) : this(returnValue, new String[] { member })
        {
        }

        private MemberNotNullWhenAttribute() : this(false, Array.Empty<String>())
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
