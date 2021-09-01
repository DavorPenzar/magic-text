using System;
using System.Collections;
using System.Collections.Generic;

namespace MagicText.Internal
{
    /// <summary>Provides methods for <see cref="Int32" /> comparison as comparing indices of the <see cref="Pen.Context" /> tokens.</summary>
    /// <remarks>
    ///     <para>Only the reference to the list <c>tokens</c> passed to the constructor is stored by the comparer in the <see cref="Tokens" /> property. Changing the content of the enumerable externally, or even its order, results in inconsistent behaviour of comparison via the <see cref="IComparer{T}.Compare(T, T)" /> method.</para>
    /// </remarks>
    internal class IndexComparer : Object, IComparer<Int32>, IEqualityComparer<Int32>, IComparer, IEqualityComparer
    {
        private const string ComparerNullErrorMessage = "String comparer cannot be null.";
        private const string TokensNullErrorMessage = "Token list cannot be null.";
        private const string ObjectNotInt32ErrorMessage = "Object must be a 32-bit integer.";

        private readonly StringComparer _comparer;
        private readonly IReadOnlyList<String?> _tokens;

        /// <summary>Gets the <see cref="StringComparer" /> used by the comparer for comparing tokens.</summary>
        /// <returns>The internal <see cref="StringComparer" />.</returns>
        protected StringComparer Comparer => _comparer;

        /// <summary>Gets the reference tokens used by the comparer for comparing indices.</summary>
        /// <returns>The reference tokens.</returns>
        /// <remarks>
        ///     <para>If integers <c>x</c>, <c>y</c> are legal indices of the <see cref="Tokens" />, they are compared by comparing <c><see cref="Tokens" />[x]</c> and <c><see cref="Tokens" />[y]</c> using the <see cref="Comparer" /> in the <see cref="Compare(Int32, Int32)" /> method. Ties are resolved by comparing <c><see cref="Tokens" />[x + 1]</c> and <c><see cref="Tokens" />[y + 1]</c>, and so on; the first index to reach the end of <see cref="Tokens" /> is considered less if all prior tokens compared equal.</para>
        /// </remarks>
        /// <seealso cref="Comparer" />
        public IReadOnlyList<String?> Tokens => _tokens;

        /// <summary>Creates a comparer.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used to compare tokens.</param>
        /// <param name="tokens">The reference tokens. The indices shall be compared by comparing elements of the <c><paramref name="tokens" /></c>.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="comparer" /></c> is <c>null</c>. The parameter <c><paramref name="tokens" /></c> is <c>null</c>.</exception>
        public IndexComparer(StringComparer comparer, IReadOnlyList<String?> tokens) : base()
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens), TokensNullErrorMessage);
        }

        /// <summary>Returns the hash code for the <c><paramref name="obj" /></c>.</summary>
        /// <param name="obj">The index for which the hash code is to be returned.</param>
        /// <returns>The hash code for the <c><paramref name="obj" /></c>.</returns>
        /// <seealso cref="Tokens" />
        /// <seealso cref="GetHashCode(Object?)" />
        public Int32 GetHashCode(Int32 obj) =>
            obj.GetHashCode();

        /// <summary>Returns the hash code for the <c><paramref name="obj" /></c>.</summary>
        /// <param name="obj">The index for which the hash code is to be returned.</param>
        /// <returns>The hash code for the <c><paramref name="obj" /></c>.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="obj" /></c> is not an <see cref="Int32" />.</exception>
        /// <seealso cref="Tokens" />
        /// <seealso cref="GetHashCode(Int32)" />
        public Int32 GetHashCode(Object? obj) =>
            obj is Int32 objInt ? GetHashCode(objInt) : throw new ArgumentException(ObjectNotInt32ErrorMessage, nameof(obj));

        /// <summary>Determines whether <c><paramref name="x" /></c> and <c><paramref name="y" /></c> are equal.</summary>
        /// <param name="x">The first index to compare.</param>
        /// <param name="y">The second index to compare.</param>
        /// <returns>If <c><paramref name="x" /></c> and <c><paramref name="y" /></c> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="Tokens" />
        /// <seealso cref="Equals(Object?, Object?)" />
        /// <seealso cref="Compare(Int32, Int32)" />
        /// <seealso cref="Compare(Object?, Object?)" />
        public Boolean Equals(Int32 x, Int32 y) =>
            (x == y);

        /// <summary>Determines whether <c><paramref name="x" /></c> and <c><paramref name="y" /></c> are equal.</summary>
        /// <param name="x">The first <see cref="Object" /> to compare.</param>
        /// <param name="y">The second <see cref="Object" /> to compare.</param>
        /// <returns>If <c><paramref name="x" /></c> and <c><paramref name="y" /></c> are equal as indices, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="x" /></c> is not an <see cref="Int32" />. The parameter <c><paramref name="x" /></c> is not an <see cref="Int32" />.</exception>
        /// <seealso cref="Tokens" />
        /// <seealso cref="Equals(Int32, Int32)" />
        /// <seealso cref="Compare(Object?, Object?)" />
        /// <seealso cref="Compare(Int32, Int32)" />
        public new Boolean Equals(Object? x, Object? y)
        {
            if (!(x is Int32 xInt))
            {
                throw new ArgumentException(ObjectNotInt32ErrorMessage, nameof(x));
            }
            if (!(y is Int32 yInt))
            {
                throw new ArgumentException(ObjectNotInt32ErrorMessage, nameof(y));
            }

            return Equals(xInt, yInt);
        }

        /// <summary>Compares <c><paramref name="x" /></c> and <c><paramref name="y" /></c>, and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="x">The first index to compare.</param>
        /// <param name="y">The second index to compare.</param>
        /// <returns>If <c><paramref name="x" /></c> is less than <c><paramref name="y" /></c>, a value less than 0; if <c><paramref name="x" /></c> is greater than <c><paramref name="y" /></c>, a value greater than 0; if <c><paramref name="x" /></c> is equal to <c><paramref name="y" /></c>, 0.</returns>
        /// <remarks>
        ///     <para>See the <see cref="Tokens" /> property's description to understand how (legal) indices are compared. If any of <c><paramref name="x" /></c> and <c><paramref name="y" /></c> is out of range as an index of the <see cref="Tokens" />, simply the <see cref="Int32.CompareTo(Int32)" /> method is used for comparison, although in the opposite order (i. e. the greater value shall be considered the less).</para>
        /// </remarks>
        /// <seealso cref="Tokens" />
        /// <seealso cref="Compare(Object?, Object?)" />
        /// <seealso cref="Equals(Int32, Int32)" />
        /// <seealso cref="Equals(Object?, Object?)" />
        public Int32 Compare(Int32 x, Int32 y)
        {
            // Compare the indices. If not equal, compare the tokens (if possible).
            Int32 c = y.CompareTo(x);
            if (c != 0 && x >= 0 && y >= 0)
            {
                while (x < Tokens.Count && y < Tokens.Count)
                {
                    // Extract the current tokens.
                    String? t1 = Tokens[x];
                    String? t2 = Tokens[y];

                    // Compare the tokens. If not equal, return the result.
                    {
                        Int32 ct = Comparer.Compare(t1, t2);
                        if (ct != 0)
                        {
                            c = ct;

                            break;
                        }
                    }

                    // Proceed to the next tokens.
                    ++x;
                    ++y;
                }
            }

            // Return the comparison results.  If all tokens compared equal, the greater index has reached the end of `Context` first, implying the shorter (sub)sequence.
            return c;
        }

        /// <summary>Compares <c><paramref name="x" /></c> and <c><paramref name="y" /></c>, and returns a value indicating whether one is less than, equal to or greater than the other.</summary>
        /// <param name="x">The first <see cref="Object" /> to compare.</param>
        /// <param name="y">The second <see cref="Object" /> to compare.</param>
        /// <returns>If <c><paramref name="x" /></c> as an index is less than <c><paramref name="y" /></c> as an index, a value less than 0; if <c><paramref name="x" /></c> as an index is greater than <c><paramref name="y" /></c> as an index, a value greater than 0; if <c><paramref name="x" /></c> as an index is equal to <c><paramref name="y" /></c> as an index, 0.</returns>
        /// <exception cref="ArgumentException">The parameter <c><paramref name="x" /></c> is not an <see cref="Int32" />. The parameter <c><paramref name="x" /></c> is not an <see cref="Int32" />.</exception>
        /// <remarks>
        ///     <para>See the <see cref="Tokens" /> property's description to understand how (legal) indices are compared. If any of <c><paramref name="x" /></c> and <c><paramref name="y" /></c> is out of range as an index of the <see cref="Tokens" />, simply the <see cref="Int32.CompareTo(Int32)" /> method is used for comparison, although in the opposite order (i. e. the greater value shall be considered the less).</para>
        /// </remarks>
        /// <seealso cref="Tokens" />
        /// <seealso cref="Compare(Int32, Int32)" />
        /// <seealso cref="Equals(Object?, Object?)" />
        /// <seealso cref="Equals(Int32, Int32)" />
        public Int32 Compare(Object? x, Object? y)
        {
            if (!(x is Int32 xInt))
            {
                throw new ArgumentException(ObjectNotInt32ErrorMessage, nameof(x));
            }
            if (!(y is Int32 yInt))
            {
                throw new ArgumentException(ObjectNotInt32ErrorMessage, nameof(y));
            }

            return Compare(xInt, yInt);
        }
    }
}
