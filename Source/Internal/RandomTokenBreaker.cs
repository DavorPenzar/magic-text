using System;
using System.Globalization;

namespace MagicText.Internal
{
    /// <summary>Provides a method for the <see cref="RandomTokeniser" /> to choose token breaking points based using a <see cref="System.Random" />.</summary>
    internal class RandomTokenBreaker : Object
    {
        protected const string BiasOutOfRangeFormatErrorMessage = "Bias is out of range. Must be greater than {0:N0} and less than or equal to {1:N0}.";
        protected const string RandomNullErrorMessage = "(Pseudo-)Random number generator cannot be null.";

        /// <summary>The default bias.</summary>
        /// <remarks>
        ///     <para>The default bias is 0.5. This means that, on average (see <see cref="Bias" />), the <see cref="BreakToken(Int32, Int32, Int32)" /> method of a <see cref="RandomTokenBreaker" /> with the <see cref="DefaultBias" /> shall return both <c>true</c> and <c>false</c> with approximately 50 % chance.</para>
        ///     <para>Generally speaking, the average length of tokens <em>cut out</em> by a <see cref="RandomTokenBreaker" /> with the default bias—in an infinite line of text—shall be 1 or 2, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried in the <see cref="RandomTokeniser.ShatterLine(String)" /> method, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        ///     <para>When constructing a <see cref="RandomTokenBreaker" /> using the default <see cref="RandomTokenBreaker()" /> constructor or the constructor <see cref="RandomTokenBreaker(System.Random)" />, the <see cref="Bias" /> shall be set to <see cref="DefaultBias" />.</para>
        /// </remarks>
        /// <seealso cref="Bias" />
        public const double DefaultBias = 0.5D;

        private readonly Double _bias;
        private readonly System.Random _random;

        /// <summary>Gets the breaker's bias.</summary>
        /// <returns>The internal bias.</returns>
        /// <remarks>
        ///     <para>The <see cref="RandomTokenBreaker" /> is biased towards deciding on creating a token breaking point with bias <see cref="Bias" />. This means that the <see cref="BreakToken(Int32, Int32, Int32)" /> method, on average, returns <c>true</c> with probability <c><see cref="Bias" /></c> and <c>false</c> with probability <c>1 - <see cref="Bias" /></c>.</para>
        ///     <para>Generally speaking, the average length of tokens <em>cut out</em> by the <see cref="RandomTokenBreaker" />—in an infinite line of text—shall be <c>1 / <see cref="Bias" /> - { 1 or 0}</c>, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried in the <see cref="RandomTokeniser.ShatterLine(String)" /> method, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). To achieve an average token length <c>n</c> (greater than or equal to 0, may be non-integral) with empty tokens included, the <see cref="Bias" /> should be <c>1 / (n + 1)</c>. For instance, for the average (expected) length of a token to be 4, the <see cref="Bias" /> must be 0.2 (1 / 5). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        /// <seealso cref="BreakToken(Int32, Int32, Int32)" />
        public Double Bias => _bias;

        /// <summary>Get's the breaker's (pseudo-)random number generator.</summary>
        /// <returns>The internal (pseudo-)random number generator.</returns>
        /// <remarks>
        ///     <para>This (pseudo-)random number generator is used in the <see cref="BreakToken(Int32, Int32, Int32)" /> method for nondeterministic and biased deciding.</para>
        /// </remarks>
        /// <seealso cref="Bias" />
        /// <seealso cref="BreakToken(Int32, Int32, Int32)" />
        protected System.Random Random => _random;

        /// <summary>Creates a biased token breaker.</summary>
        /// <param name="random">The (pseudo-)random number generator.</param>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="random" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The parameter <c><paramref name="bias" /></c> is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        /// <remarks>
        ///     <para>If no specific <see cref="Random" /> object or seed should be used, the <see cref="RandomTokenBreaker(Double)" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken(Int32, Int32, Int32)" /> method.</para>
        /// </remarks>
        public RandomTokenBreaker(System.Random random, Double bias) : base()
        {
            _bias = (Double.IsNaN(bias) || Double.IsInfinity(bias) || bias <= 0.0D || bias > 1.0D) ? throw new ArgumentOutOfRangeException(nameof(bias), bias, String.Format(CultureInfo.CurrentCulture, BiasOutOfRangeFormatErrorMessage, 0.0D, 1.0D)) : bias;
            _random = random ?? throw new ArgumentNullException(nameof(random), RandomNullErrorMessage);
        }

        /// <summary>Creates a default token breaker.</summary>
        /// <remarks>
        ///     <para>The breaker's bias (<see cref="Bias" />) is set to <see cref="DefaultBias" />.</para>
        /// </remarks>
        public RandomTokenBreaker() : this(new System.Random(), DefaultBias)
        {
        }

        /// <summary>Creates a biased token breaker.</summary>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">The parameter <c><paramref name="bias" /></c> is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        public RandomTokenBreaker(Double bias) : this(new System.Random(), bias)
        {
        }

        /// <summary>Creates a token breaker.</summary>
        /// <remarks>
        ///     <para>If no specific <see cref="Random" /> object or seed should be used, the <see cref="RandomTokenBreaker()" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken(Int32, Int32, Int32)" /> method.</para>
        ///     <para>The breaker's bias (<see cref="Bias" />) is set to <see cref="DefaultBias" />.</para>
        /// </remarks>
        public RandomTokenBreaker(System.Random random) : this(random, DefaultBias)
        {
        }

        /// <summary>Decides whether or not to create a token breaking point.</summary>
        /// <param name="_">The length of the line. This parameter is unused.</param>
        /// <param name="_1">The current position in the line. This parameter is unused.</param>
        /// <param name="_2">The counter of breaking prompts for the current line. This parameter is unused.</param>
        /// <returns>If a token breaking point should be created, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>The method does not depend on the values of the parameters. In fact, the parameters' values are not even checked. However, parameters are needed to enable using the method as a <see cref="Func{T1, T2, T3, TResult}" /> delegate, namely for the <see cref="RandomTokeniser.BreakToken" /> property.</para>
        /// </remarks>
        /// <seealso cref="Bias" />
        public Boolean BreakToken(Int32 _, Int32 _1, Int32 _2) =>
            (Random.NextDouble() < Bias);
    }
}
