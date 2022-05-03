using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MagicText.Internal.Extensions
{
    /// <summary>Provides auxiliary extension methods for <see cref="Capture" />s.</summary>
    internal static class CaptureExtensions
    {
        /// <summary>Retrieves the <c><paramref name="capture" /></c>'s <see cref="Capture.Value" /> or <c>null</c>.</summary>
        /// <param name="capture">The <see cref="Capture" /> whose <see cref="Capture.Value" /> should be returned.</param>
        /// <returns>If <c><paramref name="capture" /></c> is non-<c>null</c>, its <see cref="Capture.Value" /> is returned; otherwise a <c>null</c> is returned.</returns>
        /// <remarks>
        ///     <para>This method does not throw an <see cref="ArgumentNullException" /> if <c><paramref name="capture" /></c> is <c>null</c>; instead, <c>null</c> is simply returned.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("capture")]
        public static String? GetValueOrNull([AllowNull] this Capture capture) =>
            capture?.Value;
    }
}
