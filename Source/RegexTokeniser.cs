using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Tokeniser which shatters text at specific regular expression pattern matches.</summary>
    /// <remarks>
    ///     <para>
    ///         Additional to shattering text into tokens, the tokeniser provides a possibility to transform tokens immediately after the extraction of regular expression matches and prior to checking for empty tokens. Initially, the idea was to use regular expressions for the transformation as regular expressions are often used for text replacement, but the tokeniser accepts any <see cref="Func{T, TResult}" /> delegate for the transformation function (<see cref="Transform" />). This way <see cref="RegexTokeniser" /> class provides a wider range of tokenising policies and, at the same time, its implementation and interface are more consistent with other libraries, most notably the standard <em>.NET</em> library (such as <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})" /> extension method). Still, to use a regular expression based replacement, lambda-function <c>t => <see cref="Regex" />.Replace(t, matchPattern, replacementPattern)</c>, where <c>matchPattern</c> and <c>replacementPattern</c> are regular expressions to match and to use for replacement respectively, may be provided (amongst other solutions).
    ///     </para>
    ///
    ///     <para>
    ///         If the default regular expression break pattern (<see cref="DefaultBreakPattern" />) should be used without special <see cref="RegexOptions" />, better performance is achieved when using <see cref="RegexTokeniser.RegexTokeniser()" /> constructor, in which case a pre-built <see cref="Regex" /> object is used constructed with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />, instead of <see cref="RegexTokeniser.RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" /> constructor.
    ///     </para>
    ///
    ///     <para>
    ///         Empty tokens (<see cref="ShatteringOptions.IgnoreEmptyTokens" />) are considered those tokens that yield <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> method (after the transformation).
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same. These + the end of the input are considered line ends and are substituted by <see cref="ShatteringOptions.LineEndToken" /> if <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c>.
    ///     </para>
    ///
    ///     <para>
    ///         Each line from the input is split into <em>raw</em> tokens via <see cref="Regex.Split(String)" /> method (using the internal regular expression breaker (<see cref="Break" />) defined on construction of the tokeniser). If a transformation function (<see cref="Transform" />) is set, it is then used to transform each raw token. The filtering of empty tokens is done <strong>after</strong> the transformation.
    ///     </para>
    ///
    ///     <para>
    ///         Empty lines are substituted by <see cref="ShatteringOptions.EmptyLineToken" /> if <see cref="ShatteringOptions.IgnoreEmptyLines" /> is <c>false</c>.
    ///     </para>
    ///
    ///     <para>
    ///         No thread safety mechanism is implemented nor assumed by the class. If the transformation function should be thread-safe, lock the tokeniser during complete <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method calls to ensure consistent behaviour of the function over a single shattering process.
    ///     </para>
    /// </remarks>
    public class RegexTokeniser : LineByLineTokeniser
    {
        protected const string RegexPatternNullErrorMessage = "Regular expression pattern may not be `null`.";
        protected const string BreakNullErrorMessage = "Regular expression breaker may not be `null`.";

        /// <summary>Default regular expression break pattern.</summary>
        /// <remarks>
        ///     <para>
        ///         The pattern matches all non-empty continuous groups of white spaces (<c>"\\s"</c>), punctuation symbols (<c>"\\p{P}"</c>), mathematics symbols (<c>"\\p{Sm}"</c>) and separator characters (<c>"\\p{Z}"</c>). The pattern is enclosed in (round) brackets to be captured by <see cref="Regex.Split(String)" /> method.
        ///     </para>
        ///
        ///     <para>
        ///         <strong>Nota bene.</strong> If a match occurs exactly at the beginning or the end of the (line of) text to split, the splitting shall yield empty strings at the corresponding end of the input. Empty strings (tokens) may be ignored by passing adequate <see cref="ShatteringOptions" /> to <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods, but this could unintentionally ignore some results of token transformation (via <see cref="Transform" /> function). Although the occurance of empty strings is the case with splitting the input using any regular expression breaking pattern in the described scenario(s), the remark is stated here since a grammatically correct text (organised in complete sentences) usually ends with a punctuation symbol, which is considered a breaking point by this breaking pattern.
        ///     </para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultBreakPattern = @"([\s\p{P}\p{Sm}\p{Z}]+)";

        private static readonly Regex _defaultBreak;

        /// <summary>Regular expression breaker used by tokenisers constructed with the default constructor (<see cref="RegexTokeniser.RegexTokeniser()" />).</summary>
        /// <returns>Default regular expression breaker.</returns>
        /// <remarks>
        ///     <para>
        ///         The regular expression breaker is constructed using the default regular expression break pattern (<see cref="DefaultBreakPattern" />) with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />.
        ///     </para>
        /// </remarks>
        protected static Regex DefaultBreak => _defaultBreak;

        /// <summary>Initialise static fields.</summary>
        static RegexTokeniser()
        {
            _defaultBreak = new Regex(DefaultBreakPattern, RegexOptions.Compiled);
        }

        /// <summary><em>Transform</em> <paramref name="token" /> using the identity function (return <paramref name="token" /> unchanged).</summary>
        /// <param name="token">Token to transform.</param>
        /// <returns><paramref name="token" /></returns>
        protected static String? IdentityTransform(String? token) =>
            token;

        private readonly Regex _break;
        private readonly Func<String?, String?>? _transform;

        /// <summary>Regular expression breaker used by the tokeniser.</summary>
        /// <returns>Internal regular expression breaker.</returns>
        /// <remarks>
        ///     <para>
        ///         Shattering a line of text <c>line</c> (not ending with a line end (CR, LF or CRLF)) by the tokeniser, without transformation, filtering and replacement of empty lines, is done by calling <c><see cref="Break" />.Split(line)</c>.
        ///     </para>
        /// </remarks>
        protected Regex Break => _break;

        /// <summary>Transformation function used by the tokeniser. The value of <c>null</c> means no transformation function is used.</summary>
        /// <returns>Internal transformation function.</returns>
        protected Func<String?, String?>? Transform => _transform;

        /// <summary>Regular expression break pattern used by the tokeniser.</summary>
        /// <returns>Internal regular expression break pattern.</returns>
        /// <remarks>
        ///     <para>
        ///         Shattering a line of text <c>line</c> (not ending with a line end (CR, LF or CRLF)) by the tokeniser, without transformation, filtering and replacement of empty lines, is equivalent (performance aside) to calling <see cref="Regex.Split(String, String)" /> with <c>line</c> as the first argument and <see cref="BreakPattern" /> as the second.
        ///     </para>
        /// </remarks>
        [RegexPattern]
        public String BreakPattern => Break.ToString();

        /// <summary>Create a default tokeniser.</summary>
        public RegexTokeniser() : this(DefaultBreak)
        {
        }

        /// <summary>Create a tokeniser with provided options.</summary>
        /// <param name="breakPattern">Regular expression break pattern to use.</param>
        /// <param name="options">Options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transform">Optional transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="breakPattern" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.
        ///     </para>
        /// </remarks>
        public RegexTokeniser([RegexPattern] String breakPattern, RegexOptions options = RegexOptions.None, Func<String?, String?>? transform = null) : this(@break: new Regex(breakPattern ?? throw new ArgumentNullException(nameof(breakPattern), RegexPatternNullErrorMessage), options), transform: transform)
        {
        }

        /// <summary>Create a tokeniser with provided options.</summary>
        /// <param name="break">Regular expression breaker to use.</param>
        /// <param name="alterOptions">If not set, <paramref name="break" />'s options (<see cref="Regex.Options" />) are used (actually, no new <see cref="Regex" /> is constructed); otherwise options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="transform">Optional transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="break" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.
        ///     </para>
        ///
        ///     <para>
        ///         Calling this constructor is essentially the same (performance aside) as calling <see cref="RegexTokeniser.RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" /> constructor as:
        ///     </para>
        ///
        ///     <code>
        ///         <see cref="RegexTokeniser" />.RegexTokeniser(breakPattern: @<paramref name="break" />.ToString(), options: <paramref name="alterOptions" /> ?? @<paramref name="break" />.Options, transform: <paramref name="transform" />)
        ///     </code>
        /// </remarks>
        /// <seealso cref="RegexTokeniser(String, RegexOptions, Func{String?, String?}?)" />
        public RegexTokeniser(Regex @break, Nullable<RegexOptions> alterOptions = default, Func<String?, String?>? transform = null) : base()
        {
            if (@break is null)
            {
                throw new ArgumentNullException(nameof(@break), BreakNullErrorMessage);
            }

            _break = alterOptions.HasValue ? new Regex(@break.ToString(), alterOptions.Value) : @break;
            _transform = transform;
        }

        /// <summary>Shatter a single line into tokens.</summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="line" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         After splitting <paramref name="line" /> using the internal regular expression breaker (<see cref="Break" />), the transformation of tokens is done using the transformation function (<see cref="Transform" />) if it is set.
        ///     </para>
        ///
        ///     <para>
        ///         The returned enumerable is merely a query. If multiple enumerations over it should be performed, it is advisable to convert it to a fully built container beforehand, such as <see cref="List{T}" /> via <see cref="List{T}.List(IEnumerable{T})" /> constructor or <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.
        ///     </para>
        /// </remarks>
        protected override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            IEnumerable<String?> tokens = Break.Split(line);
            if (!(Transform is null))
            {
                tokens = tokens.Select(Transform);
            }

            return tokens;
        }
    }
}
