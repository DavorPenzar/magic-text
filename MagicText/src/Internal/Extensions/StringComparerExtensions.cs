using System;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary extension methods for <see cref="StringComparer" />s.</summary>
    internal static class StringComparerExtensions
    {
        private const string StringComparisonNotSupportedErrorMessage = "The string comparison type passed in is currently not supported.";

        /// <summary>Converts the specified <see cref="StringComparer" /> to a <see cref="StringComparison" /> type.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> type to convert.</param>
        /// <param name="comparisonType">If the <c><paramref name="comparer" /></c>'s type is recognised as a valid <see cref="StringComparison" /> type, the <c><paramref name="comparisonType" /></c> is set to the appropriate value.</param>
        /// <returns>If the <c><paramref name="comparer" /></c>'s type is successfully recognised, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="comparer" /></c> parameter is <c>null</c>.</exception>
        public static Boolean TryGetComparison(this StringComparer comparer, out StringComparison comparisonType)
        {
            if (comparer is null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            Boolean recognised;

            if (StringComparer.CurrentCulture.Equals(comparer))
            {
                comparisonType = StringComparison.CurrentCulture;
                recognised = true;
            }
            else if (StringComparer.CurrentCultureIgnoreCase.Equals(comparer))
            {
                comparisonType = StringComparison.CurrentCultureIgnoreCase;
                recognised = true;
            }
            else if (StringComparer.InvariantCulture.Equals(comparer))
            {
                comparisonType = StringComparison.InvariantCulture;
                recognised = true;
            }
            else if (StringComparer.InvariantCultureIgnoreCase.Equals(comparer))
            {
                comparisonType = StringComparison.InvariantCultureIgnoreCase;
                recognised = true;
            }
            else if (StringComparer.Ordinal.Equals(comparer))
            {
                comparisonType = StringComparison.Ordinal;
                recognised = true;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(comparer))
            {
                comparisonType = StringComparison.OrdinalIgnoreCase;
                recognised = true;
            }
            else
            {
                comparisonType = (StringComparison)(-1);
                recognised = false;
            }

            return recognised;
        }

#if NETSTANDARD2_1_OR_GREATER

        /// <summary>Converts the specified <see cref="StringComparison" /> type to a <see cref="StringComparer" />.</summary>
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

#else

        /// <summary>Converts the specified <see cref="StringComparison" /> type to a <see cref="StringComparer" />.</summary>
        /// <param name="comparisonType">The <see cref="StringComparison" /> type to convert.</param>
        /// <returns>A <see cref="StringComparer" /> representing the equivalent <see cref="String" /> comparison of the specified <c><paramref name="comparisonType" /></c>.</returns>
        /// <exception cref="ArgumentException">The <c><paramref name="comparisonType" /></c> is not a supported <see cref="StringComparison" /> value.</exception>
        public static StringComparer GetComparerFromComparison(StringComparison comparisonType) =>
            comparisonType switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentException(StringComparisonNotSupportedErrorMessage, nameof(comparisonType))
            };

#endif // NETSTANDARD2_1_OR_GREATER
    }
}
