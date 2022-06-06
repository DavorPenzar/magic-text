using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

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
        ///         <item>The <c><paramref name="source" /></c> parameter is <c>null</c> or</item>
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
        public static Boolean ContainsType<TSource, TMatch>(this IEnumerable<TSource> source, [NotNullWhen(true)] out TMatch firstMatch) where TMatch : TSource
        {
            Boolean matching = ContainsType(source, typeof(TMatch), out TSource firstMatchObject);
            firstMatch = (TMatch)firstMatchObject!;

            return matching;
        }
    }
}
