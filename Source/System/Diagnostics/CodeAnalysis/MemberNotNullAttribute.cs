#if NETSTANDARD2_0

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
            _members = new String[members.Length];

            Array.Copy(members, _members, members.Length);
        }

        public MemberNotNullAttribute(String member) : this(new String[] { member })
        {
        }

        private MemberNotNullAttribute() : this(Array.Empty<String>())
        {
        }
    }
}

#endif // NETSTANDARD2_0
