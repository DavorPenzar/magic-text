using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: CLSCompliant(true), ComVisible(false)]

namespace MagicText
{
    /// <summary>Provides additional information about the library's <see cref="Assembly" />.</summary>
    internal static class AssemblyInfo
    {
        private static readonly DateTime _initialisationTimePoint;

        /// <summary>Gets the time point of the application's startup (initialisation).</summary>
        /// <returns>The <see cref="DateTime" /> representing the time point at which the application was initialised.</returns>
        /// <remarks>
        ///     <para>The <see cref="InitialisationTimePoint" /> time point is expressed as a <a href="http://en.wikipedia.org/wiki/Coordinated_Universal_Time">UTC</a> <see cref="DateTime" />.</para>
        /// </remarks>
        public static DateTime InitialisationTimePoint => _initialisationTimePoint;

        /// <summary>Initialises static fields.</summary>
        static AssemblyInfo()
        {
            _initialisationTimePoint = DateTime.UtcNow;
        }
    }
}
