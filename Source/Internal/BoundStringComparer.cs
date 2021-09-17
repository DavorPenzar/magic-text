using System;

namespace MagicText.Internal
{
    /// <summary>Provides methods for <see cref="System.String" /> comparison to a predefined reference <see cref="System.String" />.</summary>
    internal class BoundStringComparer : Object, IComparable<String>, IEquatable<String>, IComparable
    {
        private const string ComparerNullErrorMessage = "String comparer cannot be null.";

        private readonly StringComparer _comparer;
        private readonly System.String? _string;

        /// <summary>Gets the <see cref="StringComparer" /> used by the comparer for comparing <see cref="System.String" />s.</summary>
        /// <returns>The internal <see cref="StringComparer" />.</returns>
        protected StringComparer Comparer => _comparer;

        /// <summary>Gets the predefined reference <see cref="System.String" /> used by the comparer.</summary>
        /// <returns>The internal predefined reference <see cref="System.String" />.</returns>
        /// <remarks>
        ///     <para>When a <see cref="System.String" /> or an <see cref="Object" /> is passed as the parameter to the <see cref="Compare(System.String)" />, <see cref="Compare(Object)" />, <see cref="CompareTo(System.String)" />, <see cref="CompareTo(Object)" />, <see cref="Equals(System.String?)" />, <see cref="Equals(Object?)" /> methods, it is compared to this <see cref="String" /> (or vice versa) by the <see cref="Comparer" />.</para>
        /// </remarks>
        public System.String? String => _string;

        /// <summary>Creates a comparer.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used to compare <see cref="System.String" />s.</param>
        /// <param name="string">The predefined reference <see cref="System.String" />. Other <see cref="System.String" />s are compared to this <c><paramref name="string" /></c> by the <c><paramref name="comparer" /></c>.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="comparer" /></c> is <c>null</c>.</exception>
        public BoundStringComparer(StringComparer comparer, System.String? @string) : base()
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            _string = @string;
        }

        /// <summary>Returns the hash code for the <see cref="String" />.</summary>
        /// <returns>The hash code for the <see cref="String" />.</returns>
        /// <seealso cref="String" />
        public sealed override Int32 GetHashCode()
        {
            try
            {
                return Comparer.GetHashCode(String);
            }
            catch (ArgumentNullException) when (String is null)
            {
            }

            return 0;
        }

        /// <summary>Compares the <c><paramref name="str" /></c> to the <see cref="String" /> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="str">The <see cref="System.String" /> to compare.</param>
        /// <returns>If the <c><paramref name="str" /></c> is less than the <see cref="String" />, a value less than 0; if the <c><paramref name="str" /></c> is greater than the <see cref="String" />, a value greater than 0; if the <c><paramref name="str" /></c> is equal to the <see cref="String" />, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(System.String, System.String)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="String" />
        /// <seealso cref="Compare(Object)" />
        /// <seealso cref="CompareTo(String)" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="Equals(System.String?)" />
        /// <seealso cref="Equals(Object?)" />
        public Int32 Compare(System.String str) =>
            Comparer.Compare(str, String);

        /// <summary>Compares the <c><paramref name="obj" /></c> to the <see cref="String" /> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is less than the <see cref="String" />, a value less than 0; if the <c><paramref name="obj" /></c> is greater than the <see cref="String" />, a value greater than 0; if the <c><paramref name="obj" /></c> is equal to the <see cref="String" />, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(Object, Object)" /> method call (notably the <see cref="ArgumentNullException" /> and the <see cref="ArgumentException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="String" />
        /// <seealso cref="Compare(System.String)" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="CompareTo(String)" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(System.String?)" />
        public Int32 Compare(Object obj) =>
            Comparer.Compare(obj, String);

        /// <summary>Compares the <see cref="String" /> to the <c><paramref name="other" /></c> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="other">The <see cref="System.String" /> to compare.</param>
        /// <returns>If the <see cref="String" /> is less than the <c><paramref name="other" /></c>, a value less than 0; if the <see cref="String" /> is greater than the <c><paramref name="other" /></c>, a value greater than 0; if the <see cref="String" /> is equal to the <c><paramref name="other" /></c>, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(System.String, System.String)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="String" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="Compare(System.String)" />
        /// <seealso cref="Compare(Object)" />
        /// <seealso cref="Equals(System.String?)" />
        /// <seealso cref="Equals(Object?)" />
        public Int32 CompareTo(System.String other) =>
            -Compare(other);

        /// <summary>Compares the <see cref="String" /> to the <c><paramref name="other" /></c> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="other">The <see cref="System.String" /> to compare.</param>
        /// <returns>If the <see cref="String" /> is less than the <c><paramref name="other" /></c>, a value less than 0; if the <see cref="String" /> is greater than the <c><paramref name="other" /></c>, a value greater than 0; if the <see cref="String" /> is equal to the <c><paramref name="other" /></c>, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(System.String, System.String)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="String" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="Compare(Object)" />
        /// <seealso cref="Compare(System.String)" />
        /// <seealso cref="Equals(System.String?)" />
        /// <seealso cref="Equals(Object?)" />
        public Int32 CompareTo(Object other) =>
            -Compare(other);

        /// <summary>Determines whether <c><paramref name="x" /></c> and the <see cref="String" /> are equal.</summary>
        /// <param name="x">The <see cref="System.String" /> to compare.</param>
        /// <returns>If <c><paramref name="x" /></c> and the <see cref="String" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="String" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="CompareTo(String)" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="Compare(System.String)" />
        /// <seealso cref="Compare(Object)" />
        public Boolean Equals(System.String? x) =>
            Comparer.Equals(x, String);

        /// <summary>Determines whether <c><paramref name="other" /></c> and the <see cref="String" /> are equal.</summary>
        /// <param name="other">The <see cref="Object" /> to compare.</param>
        /// <returns>If <c><paramref name="other" /></c> and the <see cref="String" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="String" />
        /// <seealso cref="Equals(System.String?)" />
        /// <seealso cref="CompareTo(Object)" />
        /// <seealso cref="CompareTo(String)" />
        /// <seealso cref="Compare(Object)" />
        /// <seealso cref="Compare(System.String)" />
        public sealed override Boolean Equals(Object? other) =>
            Comparer.Equals(other, String);
    }
}
