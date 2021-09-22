using System;

namespace MagicText.Internal
{
    /// <summary>Defines default values to be used in various parts (classes) of the library.</summary>
    internal static class GlobalDefaults
    {
        /// <summary>The default <see cref="System.StringComparison" /> to use.</summary>
        /// <remarks>
        ///     <para>The default <see cref="System.StringComparison" /> is <see cref="System.StringComparison.Ordinal" />.</para>
        /// </remarks>
        public const System.StringComparison StringComparison = System.StringComparison.Ordinal;

        private static readonly System.StringComparer _stringComparer;

        /// <summary>Gets the default <see cref="System.StringComparer" /> to use.</summary>
        /// <returns>The default <see cref="System.StringComparer" /> to use in this library.</returns>
        /// <remarks>
        ///     <para>The default <see cref="System.StringComparer" /> is <see cref="System.StringComparer.Ordinal" />.</para>
        ///     <para>Since the property is read-only and it represents an immutable <see cref="Object" />, it always returns the same reference (to the same <see cref="System.StringComparer" />).</para>
        /// </remarks>
        public static System.StringComparer StringComparer => _stringComparer;

        /// <summary>Initialises static fields.</summary>
        static GlobalDefaults()
        {
            _stringComparer = StringComparer.Ordinal;
        }
    }
}
