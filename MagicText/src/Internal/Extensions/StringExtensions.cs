using System;
using System.Diagnostics.CodeAnalysis;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary extension methods for <see cref="String" />s.</summary>
    internal static class StringExtensions
    {
        /// <summary>Retrieves the system's reference to the specified nullable <see cref="String" />.</summary>
        /// <param name="str">The nullable <see cref="String" /> to search for in the intern pool.</param>
        /// <returns>If <c><paramref name="str" /></c> is <c>null</c>, a <c>null</c> is returned as well. Otherwise the system's reference to the <c><paramref name="str" /></c> is returned if it is interned; if not, a new reference to a <see cref="String" /> with the value of the <c><paramref name="str" /></c>, interned in the system's intern pool, is returned.</returns>
        /// <remarks>
        ///     <para>Unlike the <see cref="String.Intern(String)" /> method, this method does not throw an <see cref="ArgumentNullException" /> if the argument is <c>null</c>; instead, <c>null</c> is simply returned. However, the <see cref="String.Intern(String)" /> method is indeed called for non-<c>null</c> arguments and therefore <see cref="String" />s interned via this method are interned in the same system's intern pool of <see cref="String" />s as <see cref="String" />s interned via the <see cref="String.Intern(String)" /> method (and vice versa).</para>
        ///     <para>The method is implemented as an extension method, meaning a <em>syntactic sugar</em> such as <c>str.InternNullable()</c> may be used (on the <see cref="String" /> nullable instance <c>str</c>). This may be useful in chaining method calls on a single <see cref="String" /> instance, since the original <see cref="String.Intern(String)" /> method does not provide the possibility.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("str")]
        public static String? InternNullable([AllowNull] this String? str) =>
            str is null ? null : String.Intern(str);
    }
}
