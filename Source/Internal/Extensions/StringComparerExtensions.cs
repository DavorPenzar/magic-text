using System;

namespace MagicText.Internal.Extensions
{
    internal static class StringComparerExtensions
    {
        private const string StringComparisonNotSupportedErrorMessage = "The string comparison type passed in is currently not supported.";

#if NETSTANDARD2_0
        /// <summary>Converts the specified <c><paramref name="comparisonType" /></c> to a <see cref="StringComparer" />.</summary>
        /// <param name="comparisonType">The <see cref="StringComparison" /> type to convert.</param>
        /// <returns>A <see cref="StringComparer" /> representing the equivalent <see cref="String" /> comparison of the specified <c><paramref name="comparisonType" /></c>.</returns>
        /// <exception cref="ArgumentException">The <c><paramref name="comparisonType" /></c> is not a supported <see cref="StringComparison" /> value.</exception>
        public static StringComparer GetComparerFromComparison(StringComparison comparisonType) =>
            comparisonType switch
            {
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                _ => throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(comparisonType))
            };
#else
        /// <summary>Converts the specified <c><paramref name="comparisonType" /></c> to a <see cref="StringComparer" />.</summary>
        /// <param name="comparisonType">The <see cref="StringComparison" /> type to convert.</param>
        /// <returns>A <see cref="StringComparer" /> representing the equivalent <see cref="String" /> comparison of the specified <c><paramref name="comparisonType" /></c>.</returns>
        /// <exception cref="ArgumentException">The <c><paramref name="comparisonType" /></c> is not a supported <see cref="StringComparison" /> value.</exception>
        public static StringComparer GetComparerFromComparison(StringComparison comparisonType)
        {
            try
            {
                return StringComparer.FromComparison(comparisonType);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(comparisonType), exception);
            }
        }
#endif // NETSTANDARD2_0
    }
}
