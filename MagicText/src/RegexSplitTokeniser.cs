using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="RegexTokeniser" /> which shatters lines of text by splitting at specific regular expression pattern matches.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineShatteringTokeniser" /> extension class simulates the <see cref="System.Text.RegularExpressions.Regex.Split(String)" /> method, unlike the <see cref="StringSplitTokeniser" />, which simulates the <see cref="String.Split(String[], StringSplitOptions)" /> method, or <see cref="RegexMatchesTokeniser" />, which simulates the <see cref="System.Text.RegularExpressions.Regex.Matches(String)" /> method, on each line of text.</para>
    ///     <para>Additional to shattering text into tokens, the <see cref="RegexSplitTokeniser" /> provides a possibility to transform tokens immediately after the splitting at regular expression matches and prior to checking for empty tokens. Initially, the idea was to use regular expressions for the transformation as regular expressions are often used for text replacement alongside other uses (such as text breaking/splitting), but the tokeniser accepts any <see cref="Func{T, TResult}" /> delegate for the transformation function <see cref="TransformToken" />. This way the <see cref="RegexSplitTokeniser" /> class provides a wider range of tokenising policies and, at the same time, its implementation and programming interface are more consistent with other libraries, most notably the standard <a href="http://docs.microsoft.com/en-gb/dotnet/"><em>.NET</em></a> library (such as the <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" /> extension method). Still, to use a regular expression based replacement, a lambda-function <c>t => <see cref="System.Text.RegularExpressions.Regex" />.Replace(t, matchPattern, replacementPattern)</c>, where <c>matchPattern</c> and <c>replacementPattern</c> are regular expressions to match and to use for replacement respectively, may be provided (amongst other solutions).</para>
    ///     <para>If a default regular expression break pattern (<see cref="DefaultInclusivePattern" /> or <see cref="DefaultExclusivePattern" />) should be used without special <see cref="RegexOptions" />, better performance is achieved when using the default <see cref="RegexSplitTokeniser()" /> constructor or the <see cref="RegexSplitTokeniser(Boolean, Func{String, String})" /> constructor, in which case a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object is used (constructed with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />), instead of the <see cref="RegexSplitTokeniser(String, RegexOptions, Func{String, String})" /> constructor.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method after the potential transformation via the <see cref="TransformToken" /> function if it is set.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the transformation function (<see cref="TransformToken" />) should be thread-safe, lock the <see cref="RegexSplitTokeniser" /> instance during complete <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls to ensure consistent behaviour of the function over a single shattering operation.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class RegexSplitTokeniser : RegexTokeniser
    {
        /// <summary>The default regular expression separator pattern which excludes the breaks as tokens.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of white spaces (<c>"\\s"</c>), punctuation symbols (<c>"\\p{P}"</c>), mathematics symbols (<c>"\\p{Sm}"</c>) and separator characters (<c>"\\p{Z}"</c>). The pattern is not enclosed in (round) brackets to be skipped by the <see cref="System.Text.RegularExpressions.Regex.Split(String)" /> method call and therefore not yielded as a token by the <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls.</para>
        ///     <para>If a match occurs exactly at the beginning or the end of the (line of) text to split, the splitting shall yield an empty <see cref="String" /> at the corresponding end of the input. The empty <see cref="String" />s (tokens) may be ignored by passing the adequate <see cref="ShatteringOptions" /> to the <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls, but this could unintentionally ignore some other tokens, possibly after transformation via the <see cref="TransformToken" /> delegate. Although the occurrence of empty strings is the case with splitting the input using any regular expression separator pattern in the described scenario(s), the remark is stated here because a grammatically correct text (i. e. organised in complete sentences) usually ends with a punctuation symbol, which is considered a separator by this pattern.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string DefaultExclusivePattern = @"[\s\p{P}\p{Sm}\p{Z}]+";

        /// <summary>The default regular expression separator pattern which includes the separators as tokens.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of white spaces (<c>"\\s"</c>), punctuation (<c>"\\p{P}"</c>), mathematics symbols (<c>"\\p{Sm}"</c>) and separator characters (<c>"\\p{Z}"</c>). The pattern is enclosed in (round) brackets to be captured by the <see cref="System.Text.RegularExpressions.Regex.Split(String)" /> method call and therefore yielded as a token by the <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls.</para>
        ///     <para>If a match occurs exactly at the beginning or the end of the (line of) text to split, the splitting shall yield an empty <see cref="String" /> at the corresponding end of the input. The empty <see cref="String" />s (tokens) may be ignored by passing the adequate <see cref="ShatteringOptions" /> to the <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls, but this could unintentionally ignore some other tokens, possibly after transformation via the <see cref="TransformToken" /> function. Although the occurrence of empty strings is the case with splitting the input using any regular expression separator pattern in the described scenario(s), the remark is stated here because a grammatically correct text (i. e. organised in complete sentences) usually ends with a punctuation symbol, which is considered a separator by this pattern.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [StringSyntax(StringSyntaxAttribute.Regex)]
        public const string DefaultInclusivePattern = (@"(" + DefaultExclusivePattern + @")");

        private static readonly System.Text.RegularExpressions.Regex _defaultExclusiveRegex;
        private static readonly System.Text.RegularExpressions.Regex _defaultInclusiveRegex;


        /// <summary>Gets the regular expression splitter built upon the <see cref="DefaultExclusivePattern" />.</summary>
        /// <returns>The default exclusive regular expression breaker.</returns>
        /// <remarks>
        ///     <para>The regular expression splitter is constructed using the <see cref="DefaultExclusivePattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexSplitTokeniser" /> using the <see cref="RegexSplitTokeniser(Boolean, Func{String, String})" /> constructor with the <c>inclusiveSplitter</c> parameter set to <c>false</c>, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultExclusiveRegex" />.</para>
        /// </remarks>
        private static System.Text.RegularExpressions.Regex DefaultExclusiveRegex => _defaultExclusiveRegex;
        /// <summary>Gets the regular expression splitter built upon the <see cref="DefaultInclusivePattern" />.</summary>
        /// <returns>The default inclusive regular expression splitter.</returns>
        /// <remarks>
        ///     <para>The regular expression splitter is constructed using the <see cref="DefaultInclusivePattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexSplitTokeniser" /> using the default <see cref="RegexSplitTokeniser()" /> constructor or the <see cref="RegexSplitTokeniser(Boolean, Func{String, String})" /> constructor with the <c>inclusiveSplitter</c> parameter set to <c>true</c>, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultInclusiveRegex" />.</para>
        /// </remarks>
        private static System.Text.RegularExpressions.Regex DefaultInclusiveRegex => _defaultInclusiveRegex;

        /// <summary>Initialises static fields.</summary>
        static RegexSplitTokeniser()
        {
            _defaultExclusiveRegex = new System.Text.RegularExpressions.Regex(DefaultExclusivePattern, DefaultOptions);
            _defaultInclusiveRegex = new System.Text.RegularExpressions.Regex(DefaultInclusivePattern, DefaultOptions);
        }

        private readonly Func<String, String?>? _transformToken;

        /// <summary>Gets the token transformation function used by the tokeniser or <c>null</c> if no transformation function is used.</summary>
        /// <returns>If a transformation function is used, the internal transformation function; <c>null</c> otherwise.</returns>
        private Func<String, String?>? TransformToken => _transformToken;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="regex">The regular expression splitter to use.</param>
        /// <param name="alterOptions">If not set (if <c>null</c>), the <c><paramref name="regex" /></c>'s <see cref="System.Text.RegularExpressions.Regex.Options" /> are used (actually, no new <see cref="System.Text.RegularExpressions.Regex" /> is constructed but the original <c><paramref name="regex" /></c> is used); otherwise the options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transformToken">The optional token transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="regex" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="alterOptions" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <remarks>
        ///     <para>Calling this constructor is essentially the same (performance aside) as calling the <see cref="RegexSplitTokeniser(String, RegexOptions, Func{String, String})" /> constructor as:</para>
        ///     <code>
        ///         <see cref="RegexSplitTokeniser" />(pattern: <paramref name="regex" />.ToString(), options: <paramref name="alterOptions" /> ?? <paramref name="regex" />.Options, transformToken: <paramref name="transformToken" />)
        ///     </code>
        /// </remarks>
        public RegexSplitTokeniser(System.Text.RegularExpressions.Regex regex, Nullable<RegexOptions> alterOptions = default, Func<String, String?>? transformToken = null) : base(regex, alterOptions)
        {
            _transformToken = transformToken;
        }

        /// <summary>Creates a tokeniser with a default inclusive or exclusive regular expression separator pattern.</summary>
        /// <param name="inclusiveSplitter">If <c>true</c>, the <see cref="DefaultInclusivePattern" /> is used as the regular expression separator pattern; otherwise the <see cref="DefaultExclusivePattern" /> is used.</param>
        /// <param name="transformToken">The optional token transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <remarks>
        ///     <para>Actually, a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object (<see cref="DefaultInclusiveRegex" /> or <see cref="DefaultExclusiveRegex" />) with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" /> is used. Consider using this constructor or the default <see cref="RegexSplitTokeniser()" /> constructor if a default separator pattern should be used to improve performance.</para>
        /// </remarks>
        public RegexSplitTokeniser(Boolean inclusiveSplitter, Func<String, String?>? transformToken = null) : this(inclusiveSplitter ? DefaultInclusiveRegex : DefaultExclusiveRegex, transformToken: transformToken)
        {
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The <see cref="DefaultInclusivePattern" /> is used as the regular expression break pattern.</para>
        ///     <para>Actually, a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object (<see cref="DefaultInclusiveRegex" />) with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" /> is used. Consider using this constructor or the <see cref="RegexSplitTokeniser(Boolean, Func{String, String})" /> constructor if a default tokeniser should be used to improve performance.</para>
        /// </remarks>
        public RegexSplitTokeniser() : this(true)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="pattern">The regular expression separator pattern to use.</param>
        /// <param name="options">The options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transformToken">The optional token transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="pattern" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="options" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="pattern" /></c> is not a valid regular expression pattern.</exception>
        public RegexSplitTokeniser([StringSyntax(StringSyntaxAttribute.Regex)] String pattern, RegexOptions options = RegexOptions.None, Func<String, String?>? transformToken = null) : base(pattern, options)
        {
            _transformToken = transformToken;
        }

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>Performance aside, the initial splitting is equivalent to calling the <see cref="System.Text.RegularExpressions.Regex.Split(String, String, RegexOptions)" /> method with the <c><paramref name="line" /></c> as the first argument, the <see cref="RegexTokeniser.Pattern" /> as the second and <see cref="RegexTokeniser.Options" /> as the third.</para>
        ///     <para>After splitting the <c><paramref name="line" /></c> at regular expression separator pattern matches, the transformation of tokens is done using the <see cref="TransformToken" /> function if it is set.</para>
        ///     <para>The returned enumerable might merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        /// </remarks>
        protected override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            IEnumerable<String?> tokens = Regex.Split(line);
            if (!(TransformToken is null))
            {
                tokens = tokens.Select(TransformToken!);
            }

            return tokens;
        }
    }
}
