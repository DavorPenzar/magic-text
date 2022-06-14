using System;
using System.Collections.Generic;
using System.Globalization;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary methods for buffering <see cref="IEnumerable{T}" />s to <see cref="Array" />s.</summary>
    internal static class Buffering
    {
        private const string BufferNullErrorMessage = "Buffer cannot be null.";
        private const string SizeOutOfRangeFormatErrorMessage = "Size is out of range. Must be greater than or equal to {0:D}, and less than or equal to buffer capacity ({1:D}).";
        private const string BufferToLargeFormatErrorMessage = "Cannot expand buffer beyond maximal size ({0:D).";
        private const string SourceNullErrorMessage = "Source cannot be null.";

        /// <summary>Expands <c><paramref name="buffer" /></c>'s capacity.</summary>
        /// <typeparam name="T">The type of the items in the <c><paramref name="buffer" /></c>.</typeparam>
        /// <param name="buffer">The buffer to expand.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="buffer" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">When <c><paramref name="buffer" /></c> is already at a maximal capacity (<see cref="Int32.MaxValue" />).</exception>
        public static void Expand<T>(ref T[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer), BufferNullErrorMessage);
            }

            T[] newBuffer = buffer.Length switch
            {
                0 => new T[1],
                _ when buffer.Length < 0x40000000 => new T[buffer.Length << 1],
                _ when buffer.Length < 0x7fffefff => new T[buffer.Length + 0x1000],
                _ when buffer.Length < Int32.MaxValue => new T[Int32.MaxValue],
                _ => throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, BufferToLargeFormatErrorMessage, Int32.MaxValue))
            };
            buffer.CopyTo(newBuffer, 0);
            buffer = newBuffer;
        }

        /// <summary>Trims <c><paramref name="buffer" /></c>'s capacity to <c><paramref name="size" /></c>.</summary>
        /// <typeparam name="T">The type of the items in the <c><paramref name="buffer" /></c>.</typeparam>
        /// <param name="buffer">The buffer to trim.</param>
        /// <param name="size">The size to which to trim the <c><paramref name="buffer" /></c>.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="buffer" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="size" /></c> is less than 0 or greater than the <c><paramref name="buffer" /></c>'s capacity (its <see cref="Array.Length" /> property).</exception>
        public static void TrimExcess<T>(ref T[] buffer, Int32 size)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer), BufferNullErrorMessage);
            }

            if (size < 0 || size > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, String.Format(CultureInfo.CurrentCulture, SizeOutOfRangeFormatErrorMessage, 0, buffer.Length));
            }

            if (size == buffer.Length)
            {
                return;
            }

            T[] newBuffer = new T[size];
            Array.Copy(buffer, 0, newBuffer, 0, size);
            buffer = newBuffer;
        }

        /// <summary>Returns a buffer (<see cref="Array" /> of item type <c><typeparamref name="T" /></c>) equivalent to the <c><paramref name="source" /></c>.</summary>
        /// <typeparam name="T">The type of the items in the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The original enumerable of items.</param>
        /// <returns>An <see cref="Array" /> of item type <c><typeparamref name="T" /></c> equivalent to the <c><paramref name="source" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="source" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the <c><paramref name="source" /></c> is already an <see cref="Array" /> of item type <c><typeparamref name="T" /></c>, it is simply returned. Otherwise a new <see cref="Array" /> is constructed by enumerating the <c><paramref name="source" /></c> once.</para>
        /// </remarks>
        public static T[] AsBuffer<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), SourceNullErrorMessage);
            }

            if (source is T[] sourceArray)
            {
                return sourceArray;
            }

            Int32 size = 0;
            T[] buffer = source switch
            {
                IReadOnlyCollection<T> readOnlyCollection => new T[readOnlyCollection.Count],
                ICollection<T> collection => new T[collection.Count],
                _ => Array.Empty<T>()
            };

            if (source is ICollection<T> sourceCollection)
            {
                sourceCollection.CopyTo(buffer, 0);
                size = buffer.Length;
            }
            else
            {
                foreach (T item in source)
                {
                    if (size >= buffer.Length)
                    {
                        Expand(ref buffer);
                    }

                    buffer[size++] = item;
                }
            }

            TrimExcess(ref buffer, size);

            return buffer;
        }
    }
}
