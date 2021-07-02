using System;

namespace MagicText.Internal
{
    /// <summary>Provides a method for the <see cref="RandomTokeniser" /> to choose token breaking points based using a <see cref="System.Random" />.</summary>
    internal class RandomTokenBreaker
    {
        protected const string POutOfRangeErrorMessage = "Probability threshold is out of range. Must be greater than 0 and less than or equal to 1.";
        protected const string RandomNullErrorMessage = "(Pseudo-)Random number generator cannot be null.";

        /// <summary>The default bias.</summary>
        /// <remarks>
        ///     <para>The default bias is 1 / 2 (<c>0.5</c>). This means that, on average (see <see cref="Bias" />), the <see cref="BreakToken(Int32, Int32, Int32)" /> method of a <see cref="RandomTokenBreaker" /> with the <see cref="DefaultBias" /> shall return both <c>true</c> and <c>false</c> with approximately 50 % chance.</para>
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
        ///     <para>The <em>average</em> scenario described in the previous paragraph assumes that the <see cref="BreakToken(Int32, Int32, Int32)" /> method is used <strong>only</strong> as used by the <see cref="RandomTokeniser" /> class through the <see cref="RandomTokeniser.BreakToken" /> property, and that each line is fully shattered in the <see cref="RandomTokeniser.ShatterLine(String)" /> method (without premature stopping).</para>
        /// </remarks>
        /// <seealso cref="BreakToken(Int32, Int32, Int32)" />
        public Double Bias => _bias;

        /// <summary>Get's the breaker's (pseudo-)random number generator.</summary>
        /// <returns>The internal (pseudo-)random number generator.</returns>
        /// <remarks>
        ///     <para>This (pseudo-)random number generator is used in the <see cref="BreakToken(Int32, Int32, Int32)" /> method for undeterministic and biased deciding.</para>
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
        public RandomTokenBreaker(System.Random random, Double bias)
        {
            _bias = (Double.IsNaN(bias) || Double.IsInfinity(bias) || bias <= 0.0 || bias > 1.0) ? throw new ArgumentOutOfRangeException(nameof(bias), POutOfRangeErrorMessage) : bias;
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
        /// <param name="n">The length of the line.</param>
        /// <param name="i">The current position in the line.</param>
        /// <param name="j">The counter of breaking prompts for the current line.</param>
        /// <returns>If a token breaking point should be created, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>The method assumes the values of all parameters are <em>meaningful</em> and <em>legal</em> (as described for the <see cref="RandomTokeniser.BreakToken" /> property). If the assumption is true, the method returns <c>true</c> with probability:</para>
        ///         <code>(<see cref="Bias" /> * <paramref name="n" /> + <paramref name="i" /> - <paramref name="j" />) / (<paramref name="n" /> - <paramref name="i" />)</code>
        ///     <para>However, the complete (in)equality is expanded to an expression without a division to avoid <see cref="DivideByZeroException" />s. In fact, even if all parameters were valid, a division by 0 would occur when <c><paramref name="n" /> == 0</c> and <c><paramref name="i" /> == 0</c> (which is possible when the line is empty).</para>
        /// </remarks>
        /// <seealso cref="Bias" />
        public Boolean BreakToken(Int32 n, Int32 i, Int32 j) =>
            Random.NextDouble() * (n - i) < Bias * n + i - j;
    }
}
