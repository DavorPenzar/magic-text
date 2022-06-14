using MagicText.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineShatteringTokeniser" /> which shatters lines of text at random positions.</summary>
    /// <remarks>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the token breaking function (<see cref="BreakToken" />) should be thread-safe, lock the <see cref="RandomTokeniser" /> instance during complete <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls to ensure consistent behaviour of the function over a single shattering operation.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public class RandomTokeniser : LineShatteringTokeniser
    {
        protected const string BreakTokenNullErrorMessage = "Token breaking predicate cannot be null.";
        protected const string BiasOutOfRangeFormatErrorMessage = "Bias is out of range. Must be greater than {0:N0} and less than or equal to {1:N0}.";
        protected const string RandomNullErrorMessage = "(Pseudo-)Random number generator cannot be null.";

        private readonly Func<Int32, Int32, Int32, Boolean> _breakToken;

        /// <summary>Gets the token breaking function used by the tokeniser.</summary>
        /// <returns>The internal token breaking function.</returns>
        /// <remarks>
        ///     <para>The token breaking function takes three <see cref="Int32" />s as parameters and returns a <see cref="Boolean" />. Suppose the function is called as:</para>
        ///         <code><see cref="BreakToken" />(n, i, j)</code>
        ///     <para>The parameters depend on the <c>line</c> parameter (<see cref="String" />) of the <see cref="ShatterLine(String)" /> method. Their meaning is the following:</para>
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Parameter</term>
        ///             <description>Description</description>
        ///         </listheader>
        ///         <item>
        ///             <term><c>n</c>: <see cref="Int32" /></term>
        ///             <description>The length of the <c>line</c> (its <see cref="String.Length" /> property).</description>
        ///         </item>
        ///         <item>
        ///             <term><c>i</c>: <see cref="Int32" /></term>
        ///             <description>The current position in the <c>line</c> (at which the break might occur). It can be any value from 0 to <c>n</c> <strong>inclusively</strong>.</description>
        ///         </item>
        ///         <item>
        ///             <term><c>j</c>: <see cref="Int32" /></term>
        ///             <description>The counter of breaking prompts (both accepted and rejected) for the <c>line</c>, starting with 0.</description>
        ///         </item>
        ///     </list>
        ///     <para>If the function returns <c>true</c>, a token breaking point shall be put at position <c>i</c>.</para>
        ///     <para>The first call for any <c>line</c> in the <see cref="ShatterLine(String)" /> method shall always be <c><see cref="BreakToken" />(n, 0, 0)</c>, and the last call shall be <c><see cref="BreakToken" />(n, n, j)</c> (the value of <c>j</c> depends on the number of breaks: if no break has occurred yet, the call shall be <c><see cref="BreakToken" />(n, n, n)</c>; otherwise <c>j</c> shall be greater than <c>n</c>). If the line is empty (<c>n == 0</c>), only a single call <c><see cref="BreakToken" />(0, 0, j)</c> is guaranteed—for the value of <c>j == 0</c> (if it returns <c>false</c>, the shattering process of the line terminates).</para>
        ///     <para>If the function call <c><see cref="BreakToken" />(n, i, j)</c> returns <c>true</c> (a token breaking point should be put at position <c>i</c>), the next call shall be <c><see cref="BreakToken" />(n, i, j + 1)</c>. If the call returns <c>true</c> as well, another breaking point shall be put at position <c>i</c>, resulting in an empty token between the two breaking points, and the next call shall be <c><see cref="BreakToken" />(n, i, j + 2)</c>. The process is repeated (with constantly increasing the value of the third parameter) until <c>false</c> is returned. Note that the difference <c>j - i</c> is the total number of breaking points in the <c>line</c> up until the current breaking point prompt.</para>
        ///     <para>Returning <c>true</c> from <c><see cref="BreakToken" />(n, 0, j)</c> results in an empty token at the very beginning of the <c>line</c>.</para>
        ///     <para>If the <c>line</c> is empty (<c>n == 0</c>), no token breaking point is created by default. The only way possible to retrieve tokens from an empty <c>line</c> is to return <c>true</c> from <c><see cref="BreakToken" />(0, 0, j)</c> (note, however, that returning <c>false</c> from the first call <c><see cref="BreakToken" />(0, 0, 0)</c> shall result in no tokens because the shattering is terminated). Otherwise (the <c>line</c> is non-empty (<c>n != 0</c>)) a final breaking point is put at the position <c>i == n</c>, without calling the <see cref="BreakToken" /> function. Subsequently, returning <c>true</c> from <c><see cref="BreakToken" />(n, n, j)</c> shall result in empty tokens at the end of the (non-empty) <c>line</c>.</para>
        ///     <para>Having defined the token breaking points, the <c>line</c> is shattered into tokens. The first token starts at the beginning of the <c>line</c> and ends at the first token breaking point. The second token starts here and ends at the second token starting point, and so on. The breaking points are resolved in the ascending order of the value <c>j</c>, with the default final breaking point indeed being the last one, therefore any non-empty token ending at position <c>i</c> (there can only one such token at the most) occurs before all empty tokens starting and ending at position <c>i</c>.</para>
        ///     <para><strong>Nota bene.</strong> The preceding description assumes <see cref="BreakToken" /> is called from the <see cref="ShatterLine(String)" /> method, even where it is not explicitly mentioned. In that case all three parameters should be greater than or equal to 0, with an unlikely exception of the third parameter possibly being negative due to an overflow. If the tokeniser is used on a single thread, each call <c><see cref="BreakToken" />(n, 0, 0)</c> from the <see cref="ShatterLine(String)" /> method indicates the beginning of a <c>line</c> shattering process. However, when calling from a derived class, its programmer may call the function however they wish, with whatever the meaning and values of the parameters.</para>
        /// </remarks>
        protected Func<Int32, Int32, Int32, Boolean> BreakToken => _breakToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="breakToken">The token breaking function. See the <see cref="BreakToken" /> property and the <see cref="ShatterLine(String)" /> method for more details.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="breakToken" /></c> parameter is <c>null</c>.</exception>
        public RandomTokeniser(Func<Int32, Int32, Int32, Boolean> breakToken) : base()
        {
            _breakToken = breakToken ?? throw new ArgumentNullException(nameof(breakToken), BreakTokenNullErrorMessage);
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="randomTokenBreaker">The <see cref="RandomTokenBreaker" /> whose <see cref="RandomTokenBreaker.BreakToken(Int32, Int32, Int32)" /> method is used as the token breaking function. See the <see cref="BreakToken" /> property and the <see cref="ShatterLine(String)" /> method for more details.</param>
        private RandomTokeniser(RandomTokenBreaker randomTokenBreaker) : this(randomTokenBreaker.BreakToken)
        {
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>Each position in the input text lines has a 50 % chance of being a token break point or not. More precisely, each <see cref="BreakToken" /> function call has, on average, a 50 % chance of returning <c>true</c> or <c>false</c> (the <em>average</em> assumes using the function only through the <see cref="ShatterLine(String)" /> method with completely enumerating tokens of non-empty lines).</para>
        ///     <para>Generally speaking, the average token length—in an infinite line of text—shall be 1 or 2, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, and consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        public RandomTokeniser() : this(new RandomTokenBreaker())
        {
        }

        /// <summary>Creates a biased tokeniser.</summary>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="bias" /></c> parameter is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        /// <remarks>
        ///     <para>Each position in the input text lines becomes a breaking point with probability <c><paramref name="bias" /></c> and does not become one with probability <c>1 - <paramref name="bias" /></c>. More precisely, each <see cref="BreakToken" /> function call returns <c>true</c> with an average probability <c><paramref name="bias" /></c> or <c>false</c> with an average probability <c>1 - <paramref name="bias" /></c>.</para>
        ///     <para>Generally speaking, the average token length—in an infinite line of text—shall be <c>1 / <paramref name="bias" /> - { 1 or 0 }</c>, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). To achieve an average token length <c>n</c> (greater than or equal to 0, may be non-integral), set the <c><paramref name="bias" /></c> to <c>1 / (n + 1)</c> if empty tokens are included or to <c>1 / n</c> if empty tokens are not included. For instance, to set the average (expected) length of a token to 4 with empty tokens included, set <c><paramref name="bias" /></c> to 0.2 (1 / 5). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, and consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        public RandomTokeniser(Double bias) : this((Double.IsNaN(bias) || Double.IsInfinity(bias) || bias <= 0.0D || bias > 1.0D) ? throw new ArgumentOutOfRangeException(nameof(bias), bias, String.Format(CultureInfo.CurrentCulture, BiasOutOfRangeFormatErrorMessage, 0.0D, 1.0D)) : new RandomTokenBreaker(bias))
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="random">The (pseudo-)random number generator.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="random" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If no specific <see cref="Random" /> instance or seed should be used, the <see cref="RandomTokeniser()" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken" /> function.</para>
        ///     <para>Each position in the input text lines has a 50 % chance of being a token break point or not. More precisely, each <see cref="BreakToken" /> function call has, on average, a 50 % chance of returning <c>true</c> or <c>false</c>.</para>
        ///     <para>Generally speaking, the average token length—in an infinite line of text—shall be 1 or 2, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, and consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        public RandomTokeniser(Random random) : this(random is null ? throw new ArgumentNullException(nameof(random), RandomNullErrorMessage) : new RandomTokenBreaker(random))
        {
        }

        /// <summary>Creates a biased tokeniser.</summary>
        /// <param name="random">The (pseudo-)random number generator.</param>
        /// <param name="bias">The breaking point bias. The <c><paramref name="bias" /></c> must be greater than 0 and less than or equal to 1.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="random" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="bias" /></c> parameter is <c>NaN</c>, infinite, less than or equal to 0, or greater than 1.</exception>
        /// <remarks>
        ///     <para>If no specific <see cref="Random" /> instance or seed should be used, the <see cref="RandomTokeniser(Double)" /> constructor could be used instead. The <c><paramref name="random" /></c> is used for deciding about token breaking points via the <see cref="BreakToken" /> function.</para>
        ///     <para>Each position in the input text lines becomes a breaking point with probability <c><paramref name="bias" /></c> and does not become one with probability <c>1 - <paramref name="bias" /></c>. More precisely, each <see cref="BreakToken" /> function call returns <c>true</c> with an average probability <c><paramref name="bias" /></c> or <c>false</c> with an average probability <c>1 - <paramref name="bias" /></c>.</para>
        ///     <para>Generally speaking, the average token length—in an infinite line of text—shall be <c>1 / <paramref name="bias" /> - { 1 or 0 }</c>, depending on whether or not the empty tokens are included (v. <a href="http://en.wikipedia.org/wiki/Geometric_distribution#Expected_Value_Examples">expected values of the geometric distribution</a>; note that the <em>zeroth</em> position is also tried, therefore a <em>hit</em> on the first try would result in a token length 0, and a hit on the <c>n</c>-th try would result in a token length of <c>n - 1</c>). To achieve an average token length <c>n</c> (greater than or equal to 0, may be non-integral), set the <c><paramref name="bias" /></c> to <c>1 / (n + 1)</c> if empty tokens are included or to <c>1 / n</c> if empty tokens are not included. For instance, to set the average (expected) length of a token to 4 with empty tokens included, set <c><paramref name="bias" /></c> to 0.2 (1 / 5). However, finiteness of actual lines of text that are being shattered limits the length of the last token in a line, and consequently the actual <em>a posteriori</em> average may differ from the theoretical value.</para>
        /// </remarks>
        public RandomTokeniser(Random random, Double bias) : this(random is null ? throw new ArgumentNullException(nameof(random), RandomNullErrorMessage) : (Double.IsNaN(bias) || Double.IsInfinity(bias) || bias <= 0.0D || bias > 1.0D) ? throw new ArgumentOutOfRangeException(nameof(bias), bias, String.Format(CultureInfo.CurrentCulture, BiasOutOfRangeFormatErrorMessage, 0.0D, 1.0D)) : new RandomTokenBreaker(random, bias))
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The <c><paramref name="line" /></c> is shattered into tokens using the <see cref="BreakToken" /> function. The <see cref="BreakToken" /> function may be nondeterministic (and it generally is indeed nondeterministic, unless the <see cref="RandomTokeniser(Func{Int32, Int32, Int32, Boolean})" /> constructor was used with passing a deterministic function as the argument), therefore two different shattering operations of the same <c><paramref name="line" /></c> may yield different results.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If the <see cref="BreakToken" /> is not a deterministic function, two distinct enumerators over the query may return different results. Furthermore, if multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            // Declare:
            StringBuilder tokenBuilder = new StringBuilder(line.Length); // the token builder
            Int32 i; // the current position
            Int32 j; // the counter of token break point prompts

            // Iterate over characters in the `line`
            for (i = 0, j = 0; i < line.Length; ++j)
            {
                // Return the token, clear the `tokenBuilder` and move back a position if necessary.
                if (BreakToken(line.Length, i, j))
                {
                    yield return tokenBuilder.ToString();
                    tokenBuilder.Clear();

                    continue;
                }

                // Add the current character to the token.
                tokenBuilder.Append(line[i++]);
            }

            // Create and return tokens at the end of the `line` while necessary.
            while (i <= line.Length && i >= 0)
            {
                if (BreakToken(line.Length, i, j++))
                {
                    yield return tokenBuilder.ToString();
                    tokenBuilder.Clear();

                    continue;
                }

                ++i;
            }

            // Return the final token if necessary.
            if (line.Length != 0)
            {
                yield return tokenBuilder.ToString();
            }
        }
    }
}
