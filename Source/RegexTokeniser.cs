using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineByLineTokeniser" /> which shatters lines of text at specific regular expression pattern matches.</summary>
    /// <remarks>
    ///     <para>Additional to shattering text into tokens, <see cref="RegexTokeniser" /> provides a possibility to transform tokens immediately after the extraction of regular expression matches and prior to checking for empty tokens. Initially, the idea was to use regular expressions for the transformation as regular expressions are often used for text replacement alongside other uses (such as text breaking/splitting), but the tokeniser accepts any <see cref="Func{T, TResult}" /> delegate for the transformation function <see cref="Transform" />. This way the <see cref="RegexTokeniser" /> class provides a wider range of tokenising policies and, at the same time, its implementation and programming interface are more consistent with other libraries, most notably the standard <em>.NET</em> library (such as the <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" /> extension method). Still, to use a regular expression based replacement, a lambda-function <c>t => <see cref="Regex" />.Replace(t, matchPattern, replacementPattern)</c>, where <c>matchPattern</c> and <c>replacementPattern</c> are regular expressions to match and to use for replacement respectively, may be provided (amongst other solutions).</para>
    ///     <para>If a default regular expression break pattern <see cref="DefaultInclusiveBreakPattern" /> or <see cref="DefaultExclusiveBreaker" /> should be used without special <see cref="RegexOptions" />, a better performance is achieved when using the default <see cref="RegexTokeniser()" /> constructor or the <see cref="RegexTokeniser(Boolean)" /> constructor, in which case a pre-built <see cref="Regex" /> object is used constructed with <see cref="Regex.Options" />, instead of the <see cref="RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" /> constructor.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens which yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method after possible transformation via the <see cref="Transform" /> delegate if it is set. This behaviour cannot be overridden by a derived class.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the transformation function (<see cref="Transform" />) should be thread-safe, lock the tokeniser during complete <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls to ensure consistent behaviour of the function over a single shattering process.</para>
    /// </remarks>
    public class RegexTokeniser : LineByLineTokeniser
    {
        protected const string RegexPatternNullErrorMessage = "Regular expression pattern cannot be null.";
        protected const string BreakNullErrorMessage = "Regular expression breaker cannot be null.";

        /// <summary>The default regular expression break pattern which includes the breaks as tokens.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of white spaces (<c>"\\s"</c>), punctuation symbols (<c>"\\p{P}"</c>), mathematics symbols (<c>"\\p{Sm}"</c>) and separator characters (<c>"\\p{Z}"</c>). The pattern is enclosed in (round) brackets to be captured by the <see cref="Regex.Split(String)" /> method call and therefore yielded as a token by the <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" />, <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> and <see cref="ShatterLine(String)" /> method calls.</para>
        ///     <para>If a match occurs exactly at the beginning or the end of the (line of) text to split, the splitting shall yield an empty <see cref="String" /> at the corresponding end of the input. The empty <see cref="String" />s (tokens) may be ignored by passing the adequate <see cref="ShatteringOptions" /> to the <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls, but this could unintentionally ignore some other tokens, possibly after transformation via the <see cref="Transform" /> delegate. Although the occurrence of empty strings is the case with splitting the input using any regular expression breaking pattern in the described scenario(s), the remark is stated here because a grammatically correct text (i. e. organised in complete sentences) usually ends with a punctuation symbol, which is considered a breaking point by this pattern.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultInclusiveBreakPattern = @"([\s\p{P}\p{Sm}\p{Z}]+)";

        /// <summary>The default regular expression break pattern which excludes the breaks as tokens.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of white spaces (<c>"\\s"</c>), punctuation symbols (<c>"\\p{P}"</c>), mathematics symbols (<c>"\\p{Sm}"</c>) and separator characters (<c>"\\p{Z}"</c>). The pattern is not enclosed in (round) brackets to be skipped by the <see cref="Regex.Split(String)" /> method call and therefore not yielded as a token by the <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" />, <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> and <see cref="ShatterLine(String)" /> method calls.</para>
        ///     <para>If a match occurs exactly at the beginning or the end of the (line of) text to split, the splitting shall yield an empty <see cref="String" /> at the corresponding end of the input. The empty <see cref="String" />s (tokens) may be ignored by passing the adequate <see cref="ShatteringOptions" /> to the <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls, but this could unintentionally ignore some other tokens, possibly after transformation via the <see cref="Transform" /> delegate. Although the occurrence of empty strings is the case with splitting the input using any regular expression breaking pattern in the described scenario(s), the remark is stated here because a grammatically correct text (i. e. organised in complete sentences) usually ends with a punctuation symbol, which is considered a breaking point by this pattern.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultExclusiveBreakPattern = @"[\s\p{P}\p{Sm}\p{Z}]+";

        private static readonly Regex _defaultInclusiveBreaker;
        private static readonly Regex _defaultExclusiveBreaker;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultInclusiveBreakPattern" />.</summary>
        /// <returns>The default inclusive regular expression breaker.</returns>
        /// <remarks>
        ///     <para>The regular expression breaker is constructed using the <see cref="DefaultInclusiveBreakPattern" /> with the <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />.</para>
        ///     <para>When constructing a <see cref="RegexTokeniser" /> using the default <see cref="RegexTokeniser()" /> constructor or the constructor <see cref="RegexTokeniser(Boolean)" /> with the parameter <c>inclusiveBreak</c> set to <c>true</c>, the <see cref="Breaker" /> shall be set to <see cref="DefaultInclusiveBreakPattern" />.</para>
        /// </remarks>
        protected static Regex DefaultInclusiveBreaker => _defaultInclusiveBreaker;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultExclusiveBreakPattern" />.</summary>
        /// <returns>The default exclusive regular expression breaker.</returns>
        /// <remarks>
        ///     <para>The regular expression breaker is constructed using the <see cref="DefaultExclusiveBreakPattern" /> with the <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />.</para>
        ///     <para>When constructing a <see cref="RegexTokeniser" /> using the constructor <see cref="RegexTokeniser(Boolean)" /> with the parameter <c>inclusiveBreak</c> set to <c>false</c>, the <see cref="Breaker" /> shall be set to <see cref="DefaultExclusiveBreakPattern" />.</para>
        /// </remarks>
        protected static Regex DefaultExclusiveBreaker => _defaultExclusiveBreaker;

        /// <summary>Initialises static fields.</summary>
        static RegexTokeniser()
        {
            _defaultInclusiveBreaker = new Regex(DefaultInclusiveBreakPattern, RegexOptions.Compiled);
            _defaultExclusiveBreaker = new Regex(DefaultExclusiveBreakPattern, RegexOptions.Compiled);
        }

        /// <summary>Does not transform the <c><paramref name="token" /></c>, but returns it unchanged.</summary>
        /// <param name="token">Token to transform.</param>
        /// <returns>The <c><paramref name="token" /></c> unchanged.</returns>
        /// <remarks>
        ///     <para>Mathematically speaking, the method acts as an identity function over the set of (nullable) <see cref="String" />s.</para>
        /// </remarks>
        protected static String? IdentityTransform(String? token) =>
            token;

        private readonly Regex _breaker;
        private readonly Func<String?, String?>? _transform;

        /// <summary>Gets the regular expression breaker used by the tokeniser.</summary>
        /// <returns>The internal regular expression breaker.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending in a line end (CR, LF or CRLF)) by the tokeniser, without transformation, filtering and replacement of empty lines, is done by the call <c><see cref="Breaker" />.Split(line)</c>.</para>
        /// </remarks>
        protected Regex Breaker => _breaker;

        /// <summary>Gets the token transformation function used by the tokeniser. The value of <c>null</c> indicates that no transformation function is used.</summary>
        /// <returns>The internal transformation function.</returns>
        protected Func<String?, String?>? Transform => _transform;

        /// <summary>Gets the regular expression break pattern used by the tokeniser.</summary>
        /// <returns>The internal regular expression break pattern.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending with a line end (CR, LF or CRLF)) by the tokeniser, without transformation, filtering and replacement of empty lines, is equivalent (performance aside) to calling <see cref="Regex.Split(String, String)" /> with the <c>line</c> as the first argument and the <see cref="BreakPattern" /> as the second.</para>
        /// </remarks>
        [RegexPattern]
        public String BreakPattern => Breaker.ToString();

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="breaker">The regular expression breaker to use.</param>
        /// <param name="alterOptions">If not set (if <c>null</c>), the <c><paramref name="breaker" /></c>'s <see cref="Regex.Options" /> are used (actually, no new <see cref="Regex" /> is constructed but the original <c><paramref name="breaker" /></c> is used); otherwise the options passed to the <see cref="Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transform">The optional token transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="breaker" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.</para>
        ///     <para>Calling this constructor is essentially the same (performance aside) as calling the <see cref="RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" /> constructor as:</para>
        ///     <code>
        ///         <see cref="RegexTokeniser" />.RegexTokeniser(breakPattern: <paramref name="breaker" />.ToString(), options: <paramref name="alterOptions" /> ?? <paramref name="breaker" />.Options, transform: <paramref name="transform" />)
        ///     </code>
        /// </remarks>
        /// <seealso cref="RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" />
        public RegexTokeniser(Regex breaker, Nullable<RegexOptions> alterOptions = default, Func<String?, String?>? transform = null) : base()
        {
            if (breaker is null)
            {
                throw new ArgumentNullException(nameof(breaker), BreakNullErrorMessage);
            }

            _breaker = alterOptions.HasValue ? new Regex(breaker.ToString(), alterOptions.Value) : breaker;
            _transform = transform;
        }

        /// <summary>Creates a default tokeniser, with the inclusive break or not.</summary>
        /// <param name="inclusiveBreaker">If <c>true</c>, the <see cref="DefaultInclusiveBreakPattern" /> is used as the regular expression break pattern; otherwise the <see cref="DefaultExclusiveBreakPattern" /> is used.</param>
        /// <remarks>
        ///     <para>Actually, a pre-built <see cref="Regex" /> object (<see cref="DefaultInclusiveBreaker" /> or <see cref="DefaultExclusiveBreaker" />) with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" /> is used. Consider using this constructor or the default <see cref="RegexTokeniser()" /> constructor if a default tokeniser should be used to improve performance.</para>
        /// </remarks>
        public RegexTokeniser(Boolean inclusiveBreaker) : this(inclusiveBreaker ? DefaultInclusiveBreaker : DefaultExclusiveBreaker)
        {
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The <see cref="DefaultInclusiveBreakPattern" /> is used as the regular expression break pattern.</para>
        ///     <para>Actually, a pre-built <see cref="Regex" /> object (<see cref="DefaultInclusiveBreaker" />) with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" /> is used. Consider using this constructor or the <see cref="RegexTokeniser(Boolean)" /> constructor if a default tokeniser should be used to improve performance.</para>
        /// </remarks>
        public RegexTokeniser() : this(true)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="breakPattern">The regular expression break pattern to use.</param>
        /// <param name="options">The options passed to the <see cref="Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transform">The optional token transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="breakPattern" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="Regex" /> class's constructor and methods are not caught.</para>
        /// </remarks>
        public RegexTokeniser([RegexPattern] String breakPattern, RegexOptions options = RegexOptions.None, Func<String?, String?>? transform = null) : this(breaker: breakPattern is null ? throw new ArgumentNullException(nameof(breakPattern), RegexPatternNullErrorMessage) : new Regex(breakPattern, options), transform: transform)
        {
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="line" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>After splitting the <c><paramref name="line" /></c> using the <see cref="Breaker" />, the transformation of tokens is done using the <see cref="Transform" /> delegate if it is set.</para>
        ///     <para>The returned enumerable might merely be a query for enumerating tokens (also known as <em>deferred execution</em>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        /// <seealso cref="Transform" />
        protected sealed override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            IEnumerable<String?> tokens = Breaker.Split(line);
            if (!(Transform is null))
            {
                tokens = tokens.Select(Transform);
            }

            return tokens;
        }
    }
}
