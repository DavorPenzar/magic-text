using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace MagicText.Extensions
{
    /// <summary>Provides auxiliary extension methods for <see cref="IEnumerable{T}" />s.</summary>
    [CLSCompliant(true)]
    public static class EnumerableExtensions
    {
        /// <summary>Converts the <c><paramref name="source" /></c> to an <see cref="IReadOnlyList{T}" />.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> to convert to an <see cref="IReadOnlyList{T}"/>.</param>
        /// <returns>If <c><paramref name="source" /></c> is non-<c>null</c>, an <see cref="IReadOnlyList{T}" /> equivalent to it is returned; otherwise a <c>null</c> is returned.</returns>
        /// <remarks>
        ///     <para>This method attempts to convert the <c><paramref name="source" /></c> with as little work as possible (if possible, enumeration is avoided). Namely, if the <c><paramref name="source" /></c> is already an <see cref="IReadOnlyList{T}" />, it is returned without conversion; if the <c><paramref name="source" /></c> is an <see cref="IList{T}" />, the wrapper <see cref="ReadOnlyCollection{T}" /> around it is returned.</para>
        ///     <para>The method does not throw an exception (such as the <see cref="ArgumentNullException" />) if the <c><paramref name="source" /></c> is <c>null</c>. Instead, <c>null</c> is simply returned. Such cases should be handled in the code surrounding the method call.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("source")]
        public static IReadOnlyList<TSource> ConvertToReadOnlyList<TSource>([AllowNull] this IEnumerable<TSource> source) =>
            source switch
            {
                null => null!,
                IReadOnlyList<TSource> sourceReadOnlyList => sourceReadOnlyList,
                IList<TSource> sourceList => new ReadOnlyCollection<TSource>(sourceList),
                _ => new List<TSource>(source)
            };

        /// <summary>Converts the <c><paramref name="source" /></c> to an <see cref="IList{T}" />.</summary>
        /// <typeparam name="TSource">The type of the elements of the <c><paramref name="source" /></c>.</typeparam>
        /// <param name="source">The <c><paramref name="source" /></c> to convert to an <see cref="IList{T}"/>.</param>
        /// <returns>If <c><paramref name="source" /></c> is non-<c>null</c>, an <see cref="IList{T}" /> equivalent to it is returned; otherwise a <c>null</c> is returned.</returns>
        /// <remarks>
        ///     <para>This method attempts to convert the <c><paramref name="source" /></c> with as little work as possible (if possible, enumeration is avoided). Namely, if the <c><paramref name="source" /></c> is already an <see cref="IList{T}" />, it is returned without conversion.</para>
        ///     <para>The method does not throw an exception (such as the <see cref="ArgumentNullException" />) if the <c><paramref name="source" /></c> is <c>null</c>. Instead, <c>null</c> is simply returned. Such cases should be handled in the code surrounding the method call.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("source")]
        public static IList<TSource> ConvertToList<TSource>([AllowNull] this IEnumerable<TSource> source) =>
            source switch
            {
                null => null!,
                IList<TSource> sourceList => sourceList,
                _ => new List<TSource>(source)
            };
    }
}
