using MagicText.Internal.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MagicText.Internal
{
    /// <summary>Provides methods for <see cref="System.String" /> comparison to a predefined reference <see cref="System.String" />.</summary>
    internal sealed class BoundStringComparer : Object, IComparable<String>, IEquatable<String>, IComparable
    {
        private const string ComparerNullErrorMessage = "String comparer cannot be null.";
        private const string ComparisonNotSupportedErrorMessage = "The string comparison type passed in is currently not supported.";

        /// <summary>Converts a <see cref="System.String" /> into a <see cref="BoundStringComparer" />.</summary>
        /// <param name="string">The predefined reference <see cref="System.String" /> of the resulting <see cref="BoundStringComparer" />.</param>
        /// <returns>A <see cref="BoundStringComparer" /> bound to the <c><paramref name="string" /></c>.</returns>
        /// <remarks>
        ///     <para>The internal (bound) <see cref="StringComparer" /> is set to <see cref="StringComparer.Ordinal" />.</para>
        ///     <para>Even if <c><paramref name="string" /></c> is <c>null</c>, the conversion <strong>does not</strong> result in a <c>null</c> reference. Rather, a <see cref="BoundStringComparer" /> bound to a <c>null</c> <see cref="System.String" /> is returned.</para>
        /// </remarks>
        [return: NotNull]
        public static explicit operator BoundStringComparer([AllowNull] System.String @string) =>
            new BoundStringComparer(@string);

        /// <summary>Retrieves the <see cref="System.String" /> to which a <see cref="BoundStringComparer" /> is bound.</summary>
        /// <param name="boundStringComparer">The <see cref="BoundStringComparer" /> bound to the returned <see cref="System.String" />.</param>
        /// <returns>The internal reference <see cref="System.String" /> of the <c><paramref name="boundStringComparer" /></c>.</returns>
        /// <remarks>
        ///     <para>The conversion is essentially the same as simply using the <see cref="String" /> property of the <c><paramref name="boundStringComparer" /></c>.</para>
        /// </remarks>
        [return: MaybeNull]
        public static explicit operator System.String([AllowNull] BoundStringComparer boundStringComparer) =>
            boundStringComparer?.String!;

        private readonly StringComparer _comparer;
        private readonly System.String? _string;

        /// <summary>Gets the bound <see cref="StringComparer" /> used by the comparer for comparing <see cref="System.String" />s.</summary>
        /// <returns>The internal <see cref="StringComparer" />.</returns>
        private StringComparer Comparer => _comparer;

        /// <summary>Gets the predefined reference <see cref="System.String" /> used by the comparer.</summary>
        /// <returns>The internal predefined reference <see cref="System.String" />.</returns>
        /// <remarks>
        ///     <para>When a <see cref="System.String" /> or an <see cref="Object" /> is passed as the parameter to the <see cref="Compare(System.String)" />, <see cref="Compare(Object)" />, <see cref="CompareTo(System.String)" />, <see cref="CompareTo(Object)" />, <see cref="Equals(System.String?)" />, <see cref="Equals(Object?)" /> methods, it is compared to the <see cref="String" /> (or vice versa) by the <see cref="Comparer" />.</para>
        /// </remarks>
        public System.String? String => _string;

        /// <summary>Creates a comparer.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used to bound.</param>
        /// <param name="string">The predefined reference <see cref="System.String" />.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="comparer" /></c> parameter is <c>null</c>.</exception>
        public BoundStringComparer(StringComparer comparer, System.String? @string) : base()
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            _string = @string;
        }

        /// <summary>Creates a comparer.</summary>
        /// <param name="comparisonType">One of the enumeration values that specifies which <see cref="StringComparer" /> should be bound, i. e. how <see cref="System.String" />s should be compared.</param>
        /// <param name="string">The predefined reference <see cref="System.String" />.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="comparisonType" /></c> is not a supported <see cref="StringComparison" /> value.</exception>
        public BoundStringComparer(StringComparison comparisonType, System.String? @string) : base()
        {
            try
            {
#if NETSTANDARD2_1_OR_GREATER
                _comparer = StringComparer.FromComparison(comparisonType);
#else
                _comparer = StringComparerExtensions.GetComparerFromComparison(comparisonType);
#endif // NETSTANDARD2_1_OR_GREATER
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(ComparisonNotSupportedErrorMessage, nameof(comparisonType), exception);
            }
            _string = @string;
        }

        /// <summary>Creates a comparer.</summary>
        /// <param name="string">The predefined reference <see cref="System.String" />.</param>
        /// <remarks>
        ///     <para>The internal (bound) <see cref="StringComparer" /> is set to <see cref="StringComparer.Ordinal" />.</para>
        /// </remarks>
        public BoundStringComparer(System.String? @string) : this(StringComparer.Ordinal, @string)
        {
        }

        /// <summary>Creates a default comparer.</summary>
        /// <remarks>
        ///     <para>The internal (bound) <see cref="StringComparer" /> is set to <see cref="StringComparer.Ordinal" />, and the internal reference <see cref="System.String" /> (<see cref="String" />) is set to <c>null</c>.</para>
        /// </remarks>
        public BoundStringComparer() : this(null)
        {
        }

        /// <summary>Deconstructs the <see cref="BoundStringComparer" />.</summary>
        /// <param name="comparer">The bound <see cref="StringComparer" /> (<see cref="Comparer" />).</param>
        /// <param name="string">The internal reference <see cref="System.String" /> (<see cref="String" />).</param>
        public void Deconstruct(out StringComparer comparer, out System.String? @string)
        {
            comparer = Comparer;
            @string = String;
        }

        /// <summary>Returns the hash code for the <see cref="String" />.</summary>
        /// <returns>The hash code for the <see cref="String" />.</returns>
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

        /// <summary>Compares <c><paramref name="str" /></c> to the <see cref="String" /> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="str">The <see cref="System.String" /> to compare.</param>
        /// <returns>If <c><paramref name="str" /></c> is less than the <see cref="String" />, a value less than 0; if <c><paramref name="str" /></c> is greater than the <see cref="String" />, a value greater than 0; if <c><paramref name="str" /></c> is equal to the <see cref="String" />, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(System.String, System.String)" /> method call are not caught.</para>
        /// </remarks>
        public Int32 Compare(System.String str) =>
            Comparer.Compare(str, String);

        /// <summary>Compares <c><paramref name="obj" /></c> to the <see cref="String" /> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare.</param>
        /// <returns>If <c><paramref name="obj" /></c> is less than the <see cref="String" />, a value less than 0; if <c><paramref name="obj" /></c> is greater than the <see cref="String" />, a value greater than 0; if <c><paramref name="obj" /></c> is equal to the <see cref="String" />, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(Object, Object)" /> method call (notably the <see cref="ArgumentNullException" /> and the <see cref="ArgumentException" />) are not caught.</para>
        /// </remarks>
        public Int32 Compare(Object obj) =>
            Comparer.Compare(obj, String);

        /// <summary>Compares the <see cref="String" /> to the <c><paramref name="other" /></c> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="other">The <see cref="System.String" /> to compare.</param>
        /// <returns>If the <see cref="String" /> is less than the <c><paramref name="other" /></c>, a value less than 0; if the <see cref="String" /> is greater than the <c><paramref name="other" /></c>, a value greater than 0; if the <see cref="String" /> is equal to the <c><paramref name="other" /></c>, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(System.String, System.String)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        public Int32 CompareTo(System.String other) =>
            -Compare(other);

        /// <summary>Compares the <see cref="String" /> to <c><paramref name="obj" /></c> and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="obj">The <see cref="System.String" /> to compare.</param>
        /// <returns>If the <see cref="String" /> is less than <c><paramref name="obj" /></c>, a value less than 0; if the <see cref="String" /> is greater than <c><paramref name="obj" /></c>, a value greater than 0; if the <see cref="String" /> is equal to <c><paramref name="obj" /></c>, 0.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="StringComparer.Compare(Object, Object)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        public Int32 CompareTo(Object obj) =>
            -Compare(obj);

        /// <summary>Determines whether <c><paramref name="other" /></c> and the <see cref="String" /> are equal.</summary>
        /// <param name="other">The <see cref="System.String" /> to compare.</param>
        /// <returns>If <c><paramref name="other" /></c> and the <see cref="String" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public Boolean Equals(System.String? other) =>
            Comparer.Equals(other, String);

        /// <summary>Determines whether <c><paramref name="obj" /></c> and the <see cref="String" /> are equal.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare.</param>
        /// <returns>If <c><paramref name="obj" /></c> and the <see cref="String" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public sealed override Boolean Equals(Object? obj) =>
            Comparer.Equals(obj, String);

        /// <summary>Returns the <see cref="String" />.</summary>
        /// <returns>If the <see cref="String" /> is not <c>null</c>, it is returned; otherwise the empty <see cref="System.String" /> (<see cref="System.String.Empty" />) is returned.</returns>
        public sealed override String ToString() =>
            String ?? System.String.Empty;
    }
}
