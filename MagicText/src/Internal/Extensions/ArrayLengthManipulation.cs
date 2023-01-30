using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary methods for resizing <see cref="Array" />s.</summary>
    internal static class ArrayLengthManipulation
    {
        private const string ArrayNullErrorMessage = "Array cannot be null.";
        private const string SourceNullErrorMessage = "Source cannot be null.";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        private const string SizeOutOfRangeFormatErrorMessage = "Size is out of range. Must be greater than or equal to {0:D}, and less than or equal to buffer capacity ({1:D}).";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        private const string ArrayToLargeFormatErrorMessage = "Cannot expand array beyond maximal size ({0:D).";

        /// <summary>Expands <c><paramref name="array" /></c>'s capacity.</summary>
        /// <typeparam name="T">The type of the items in the <c><paramref name="array" /></c>.</typeparam>
        /// <param name="array">The buffer to expand.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="array" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">When <c><paramref name="array" /></c> is already at a maximal capacity (<see cref="Int32.MaxValue" />).</exception>
        public static void Expand<T>(ref T[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array), ArrayNullErrorMessage);
            }

            T[] newBuffer = array.Length switch
            {
                0 => new T[1],
                _ when array.Length < 0x40000000 => new T[array.Length << 1],
                _ when array.Length < 0x7fffefff => new T[array.Length + 0x1000],
                _ when array.Length < Int32.MaxValue => new T[Int32.MaxValue],
                _ => throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ArrayToLargeFormatErrorMessage, Int32.MaxValue))
            };
            array.CopyTo(newBuffer, 0);
            array = newBuffer;
        }

        /// <summary>Trims <c><paramref name="array" /></c>'s capacity to <c><paramref name="size" /></c>.</summary>
        /// <typeparam name="T">The type of the items in the <c><paramref name="array" /></c>.</typeparam>
        /// <param name="array">The buffer to trim.</param>
        /// <param name="size">The size to which to trim the <c><paramref name="array" /></c>.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="array" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="size" /></c> is less than 0 or greater than the <c><paramref name="array" /></c>'s capacity (its <see cref="Array.Length" /> property).</exception>
        public static void TrimExcess<T>(ref T[] array, Int32 size)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array), ArrayNullErrorMessage);
            }

            if (size < 0 || size > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, String.Format(CultureInfo.CurrentCulture, SizeOutOfRangeFormatErrorMessage, 0, array.Length));
            }

            if (size == array.Length)
            {
                return;
            }

            T[] newBuffer = new T[size];
            Array.Copy(array, 0, newBuffer, 0, size);
            array = newBuffer;
        }
    }
}
