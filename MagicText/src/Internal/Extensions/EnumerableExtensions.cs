using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary extension methods for <see cref="IEnumerable{T}" />s.</summary>
    internal static class EnumerableExtensions
    {
        private const string SourceNullErrorMessage = "Source cannot be null.";
        private const string TypeNullErrorMessage = "Type cannot be null.";

        /// <summary>Converts <c><paramref name="source" /></c> to an <see cref="IReadOnlyList{T}" /> of item type <c><typeparamref name="TSource" /></c>.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> to convert to an <see cref="IReadOnlyList{T}"/>.</param>
        /// <returns>An <see cref="IReadOnlyList{T}" /> equivalent to the <c><paramref name="source" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="source" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>This method attempts to convert <c><paramref name="source" /></c> with as little work as possible (if possible, enumeration is avoided). More precisely, if <c><paramref name="source" /></c> is already an <see cref="IReadOnlyList{T}" />, it is returned without conversion; if the <c><paramref name="source" /></c> is an <see cref="IList{T}" />, the wrapper <see cref="ReadOnlyCollection{T}" /> around it is returned.</para>
        /// </remarks>
        public static IReadOnlyList<TSource> AsReadOnlyList<TSource>(this IEnumerable<TSource> source) =>
            source switch
            {
                null => throw new ArgumentNullException(nameof(source), SourceNullErrorMessage),
                IReadOnlyList<TSource> sourceReadOnlyList => sourceReadOnlyList,
                IList<TSource> sourceList => new ReadOnlyCollection<TSource>(sourceList),
                _ => Buffering.AsBuffer(source)
            };

        /// <summary>Determines whether or not the <c><paramref name="source" /></c> contains an item of a <c><paramref name="type" /></c>.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> in which to search for a <c><paramref name="type" /></c> instance.</param>
        /// <param name="type">The type to find in the <c><paramref name="source" /></c>.</param>
        /// <param name="firstMatch">If an instance of the <c><paramref name="type" /></c> is found in the <c><paramref name="source" /></c>, <c><paramref name="firstMatch" /></c> is set to the first such occurance.</param>
        /// <returns>If an element in the <c><paramref name="source" /></c> is an instance of the <c><paramref name="type" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>The <c><paramref name="source" /></c> parameter is <c>null</c>, or</item>
        ///         <item>The <c><paramref name="type" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>In order to (try to) find an instance of the <c><paramref name="type" /></c> in the <c><paramref name="source" /></c>, the <c><paramref name="source" /></c> must be enumerated. If a match is found (<c><paramref name="firstMatch" /></c>), the enumeration stops immediately.</para>
        /// </remarks>
        public static Boolean ContainsType<TSource>(this IEnumerable<TSource> source, Type type, [NotNullWhen(true)] out TSource firstMatch)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), SourceNullErrorMessage);
            }
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type), TypeNullErrorMessage);
            }

            foreach (TSource item in source)
            {
                if (type.IsAssignableFrom(item?.GetType()))
                {
                    firstMatch = item!;

                    return true;
                }
            }

            firstMatch = default!;

            return false;
        }

        /// <summary>Determines whether or not the <c><paramref name="source" /></c> contains an item of type <c><typeparamref name="TMatch" /></c>.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <typeparam name="TMatch">The type to find in the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> in which to search for a <c><typeparamref name="TMatch" /></c> instance.</param>
        /// <param name="firstMatch">If an instance of type <c><typeparamref name="TMatch" /></c> is found in the <c><paramref name="source" /></c>, <c><paramref name="firstMatch" /></c> is set to the first such occurance.</param>
        /// <returns>If an element in the <c><paramref name="source" /></c> is of type <c><typeparamref name="TMatch" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="source" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>In order to (try to) find an instance of type <c><typeparamref name="TMatch" /></c> in the <c><paramref name="source" /></c>, the <c><paramref name="source" /></c> must be enumerated. If a match is found (<c><paramref name="firstMatch" /></c>), the enumeration stops immediately.</para>
        /// </remarks>
        public static Boolean ContainsType<TSource, TMatch>(this IEnumerable<TSource> source, [NotNullWhen(true)] out TMatch firstMatch)
        {
            Boolean matching = ContainsType(source, typeof(TMatch), out TSource firstMatchSource);
            firstMatch = firstMatchSource is TMatch firstMatchMatch ? firstMatchMatch : default!;

            return matching;
        }

        /// <summary>Extracts an ordered <see cref="Array" /> of distinct items from the <c><paramref name="source" /></c>.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> from which to extract distinct and sorted values.</param>
        /// <param name="comparer">The <see cref="IComparer{T}" /> of <c><typeparamref name="TSource" /></c> to use for comparing items (to order and check for duplicates). If <c>null</c>, <see cref="Comparer{T}.Default" /> is used.</param>
        /// <returns>The ordered <see cref="Array" /> of distinct items from the <c><paramref name="source" /></c>.</returns>
        /// <remarks>
        ///     <para>The resulting <see cref="Array" /> is ordered ascendingly according to the <c><paramref name="comparer" /></c> and no two items in the <see cref="Array" /> compare equal by the <c><paramref name="comparer" /></c>.</para>
        ///     <para>In order to extract distinct values from the <c><paramref name="source" /></c>, it must be enumerated until the end.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <c><paramref name="source" /></c> parameter is <c>null</c>.</exception>
        public static TSource[] OrderedDistinct<TSource>(this IEnumerable<TSource> source, IComparer<TSource>? comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), SourceNullErrorMessage);
            }

            comparer ??= Comparer<TSource>.Default;

            Int32 count = 0;
            TSource[] distinctItems = source switch
            {
                ICollection<TSource> sourceCollection => new TSource[sourceCollection.Count],
                IReadOnlyCollection<TSource> sourceReadOnlyCollection => new TSource[sourceReadOnlyCollection.Count],
                _ => Array.Empty<TSource>()
            };

            foreach (TSource item in source)
            {
                Int32 position = Array.BinarySearch(distinctItems, 0, count, item, comparer);
                if (position >= 0)
                {
                    continue;
                }

                position = ~position;

                if (count >= distinctItems.Length - 1)
                {
                    Buffering.Expand(ref distinctItems);
                }

                Array.Copy(distinctItems, position, distinctItems, position + 1, count - position);
                distinctItems[position] = item;
                ++count;
            }

            Buffering.TrimExcess(ref distinctItems, count);

            return distinctItems;
        }

        /// <summary>Extracts distinct items from the <c><paramref name="source" /></c> preserving their original order from the <c><paramref name="source" /></c>.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> from which to extract distinct and sorted values.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> of <c><typeparamref name="TSource" /></c> to use for comparing items. If <c>null</c>, <see cref="EqualityComparer{T}.Default" /> of <c><typeparamref name="TSource" /></c> is used.</param>
        /// <returns>Distinct items from the <c><paramref name="source" /></c> as compared by the <c><paramref name="comparer" /></c>.</returns>
        /// <remarks>
        ///     <para>Unlike the <see cref="Enumerable.Distinct{TSource}(IEnumerable{TSource}, IEqualityComparer{TSource})" /> method, it is guaranteed that the distinct items are returned in the order of their first appearances in the <c><paramref name="source" /></c>.</para>
        ///     <para>The returned enumerable is merely a query for enumerating items (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>).</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <c><paramref name="source" /></c> parameter is <c>null</c>.</exception>
        public static IEnumerable<TSource> DistinctPreserveOrder<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource>? comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), SourceNullErrorMessage);
            }

            comparer ??= EqualityComparer<TSource>.Default;

#if NETSTANDARD2_1_OR_GREATER
            HashSet<TSource> distinctItems = source switch
            {
                ICollection<TSource> sourceCollection => new HashSet<TSource>(sourceCollection.Count, comparer),
                IReadOnlyCollection<TSource> sourceReadOnlyCollection => new HashSet<TSource>(sourceReadOnlyCollection.Count, comparer),
                _ => new HashSet<TSource>(comparer)
            };
#else
            HashSet<TSource> distinctItems = new HashSet<TSource>(comparer);
#endif // NETSTANDARD2_1_OR_GREATER

            foreach (TSource item in source)
            {
                if (distinctItems.Contains(item))
                {
                    continue;
                }

                yield return item;
                distinctItems.Add(item);
            }
        }
    }
}
