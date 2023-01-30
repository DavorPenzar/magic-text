#if !NETSTANDARD2_1_OR_GREATER

using System;

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    internal sealed class MemberNotNullAttribute : Attribute
    {
        private readonly String[] _members;

        public String[] Members => _members;

        public MemberNotNullAttribute(params String[] members) : base()
        {
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

        public MemberNotNullAttribute(String member) : this(new String[] { member })
        {
        }

        public MemberNotNullAttribute() : this(Array.Empty<String>())
        {
        }
    }
}

#endif // NETSTANDARD2_1_OR_GREATER
