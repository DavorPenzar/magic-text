using System;
using System.Globalization;

namespace MagicText.Internal
{
    /// <summary>Provides a method for the <see cref="RandomTokeniser" /> to choose token breaking points using a <see cref="System.Random" /> instance.</summary>
    internal class RandomTokenBreaker : Object
    {
        protected const string BiasOutOfRangeFormatErrorMessage = "Bias is out of range. Must be greater than {0:N0} and less than or equal to {1:N0}.";
        protected const string RandomNullErrorMessage = "(Pseudo-)Random number generator cannot be null.";

        private readonly Double _bias;
        private readonly System.Random _random;

        /// <summary>Gets the breaker's bias.</summary>
        /// <returns>The internal bias.</returns>
        /// <remarks>
        ///     <para>The <see cref="RandomTokenBreaker" /> is biased towards deciding on creating a token breaking point with bias <see cref="Bias" />. This means that the <see cref="BreakToken(Int32, Int32, Int32)" /> method, on average, returns <c>true</c> with probability <c><see cref="Bias" /></c> and <c>false</c> with probability <c>1 - <see cref="Bias" /></c>.</para>
        ///     <para>Generally speaking, the average length of tokens <em>cut out</em> by the <see cref="RandomTokenBreaker" />—in an infinite line of text—shall be <c>1 / <see cref="Bias" /> - { 1 or 0 }</c>, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried in the <see cref="RandomTokeniser.ShatterLine(String)" /> method, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). To achieve an average token length <c>n</c> (greater than or equal to 0, may be non-integral), the <see cref="Bias" /> should be <c>1 / (n + 1)</c> if empty tokens are included or <c>1 / n</c> if empty tokens are not included. For instance, for the average (expected) length of a token of 4 with empty tokens included, the <see cref="Bias" /> must be 0.2 (1 / 5). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, and consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        public Double Bias => _bias;

        /// <summary>Gets the breaker's (pseudo-)random number generator.</summary>
        /// <returns>The internal (pseudo-)random number generator.</returns>
        /// <remarks>
        ///     <para>The <see cref="Random" /> is used in the <see cref="BreakToken(Int32, Int32, Int32)" /> method for nondeterministic and biased deciding.</para>
        /// </remarks>
        protected System.Random Random => _random;

        /// <summary>Creates a biased token breaker.</summary>
        /// <param name="random">The (pseudo-)random number generator to use.</param>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="random" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="bias" /></c> is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        /// <remarks>
        ///     <para>If no specific <see cref="System.Random" /> instance or seed should be used, the <see cref="RandomTokenBreaker(Double)" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken(Int32, Int32, Int32)" /> method.</para>
        /// </remarks>
        public RandomTokenBreaker(System.Random random, Double bias) : base()
        {
            if (random is null)
            {
                throw new ArgumentNullException(nameof(random), RandomNullErrorMessage);
            }

            _bias = (Double.IsNaN(bias) || Double.IsInfinity(bias) || bias <= 0.0D || bias > 1.0D) ? throw new ArgumentOutOfRangeException(nameof(bias), bias, String.Format(CultureInfo.CurrentCulture, BiasOutOfRangeFormatErrorMessage, 0.0D, 1.0D)) : bias;
            _random = random;
        }

        /// <summary>Creates a default token breaker.</summary>
        /// <remarks>
        ///     <para>The breaker's <see cref="Bias" /> is set to 0.5.</para>
        /// </remarks>
        public RandomTokenBreaker() : this(new System.Random(), 0.5D)
        {
        }

        /// <summary>Creates a biased token breaker.</summary>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="bias" /></c> is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        public RandomTokenBreaker(Double bias) : this(new System.Random(), bias)
        {
        }

        /// <summary>Creates a token breaker.</summary>
        /// <param name="random">The (pseudo-)random number generator to use.</param>
        /// <remarks>
        ///     <para>If no specific <see cref="System.Random" /> instance or seed should be used, the <see cref="RandomTokenBreaker()" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken(Int32, Int32, Int32)" /> method.</para>
        ///     <para>The breaker's bias <see cref="Bias" /> is set to 0.5.</para>
        /// </remarks>
        public RandomTokenBreaker(System.Random random) : this(random, 0.5D)
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
        public Boolean BreakToken(Int32 _, Int32 _1, Int32 _2) =>
            (Random.NextDouble() < Bias);
    }
}
