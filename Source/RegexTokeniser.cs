using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Tokeniser which shatters text at specific regular expression pattern matches.
    ///     </para>
    ///
    ///     <para>
    ///         Additionally, the tokeniser provides a possibility to transform tokens immediately after extraction of regular expression matches and prior to checking for empty tokens. Initially, the idea was to also use regular expressions for the transformation, but a custom transformation function may be provided instead. A convenient static method <see cref="CreateReplacementTransformationFunction(String, String, Boolean, Boolean, RegexOptions)" /> is implemented to easily create regular expression based replacement functions.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Empty tokens are considered those tokens that yield <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Shattering methods read and process text <em>line-by-line</em> with all CR, LF and CRLF line breaks treated the same. Consequently, as no inner buffer is used, regular expression breaking pattern cannot stretch over a line break, regardless of <see cref="RegexOptions" /> passed to the constructor.
    ///     </para>
    ///
    ///     <para>
    ///         Each line from the input is split into raw tokens via <see cref="Regex.Split(String)" /> method (using the internal regular expression breaker (<see cref="Break" />) defined on construction of the tokeniser). If a transformation function (<see cref="Transform" />) is set, it is then used to transform each raw token. The filtering of empty tokens is done <strong>after</strong> the transformation.
    ///     </para>
    /// </remarks>
    public class RegexTokeniser : LineByLineTokeniser
    {
        /// <summary>
        ///     <para>
        ///         Default regular expression break pattern.
        ///     </para>
        ///
        ///     <para>
        ///         The pattern matches all non-empty continuous white space groups, punctuation symbol groups (periods, exclamation marks, question marks, colons, semicolons, brackets and dashes are included, but quotation marks are not) and the horizontal ellipsis. The pattern is enclosed in (round) brackets to be captured by <see cref="Regex.Split(String)" /> method.
        ///     </para>
        /// </summary>
        public const string DefaultBreakPattern = @"(\s+|[\.!\?‽¡¿⸘,:;\(\)\[\]\{\}\-—–]+|…)";

        /// <summary>
        ///     <para>
        ///         Default regular expression breaker.
        ///     </para>
        /// </summary>
        protected static readonly Regex DefaultBreak = new Regex(DefaultBreakPattern, RegexOptions.Compiled);

        /// <summary>
        ///     <para>
        ///         Create a transformation function of regular expression based replacement.
        ///     </para>
        /// </summary>
        /// <param name="matchPattern">Regular expression pattern to match.</param>
        /// <param name="replacementPattern">Regular expression pattern for replacement of matches captured by <paramref name="matchPattern" />.</param>
        /// <param name="escapeMatch">If <c>true</c>, <paramref name="matchPattern" /> is escaped via <see cref="Regex.Escape(String)" /> method before usage.</param>
        /// <param name="escapeReplacement">If <c>true</c>, <paramref name="replacementPattern" /> is escaped via <see cref="Regex.Escape(String)" /> method before usage.</param>
        /// <param name="options">Options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <returns>Function that returns <c>null</c> when passed a <c>null</c>, otherwise performs the regular expression based replacement defined.</returns>
        public static Func<String?, String?> CreateReplacementTransformationFunction(string matchPattern, string replacementPattern, bool escapeMatch = false, bool escapeReplacement = false, RegexOptions options = RegexOptions.None)
        {
            var replace = new Regex(escapeMatch ? Regex.Escape(matchPattern) : matchPattern, options);
            if (escapeReplacement)
            {
                replacementPattern = Regex.Escape(replacementPattern);
            }

            return t => t is null ? null : replace.Replace(t, replacementPattern);
        }

        private readonly Regex _break;
        private readonly Func<String?, String?>? _transform;

        /// <summary>
        ///     <para>
        ///         Regular expression breaker used by the tokeniser.
        ///     </para>
        /// </summary>
        protected Regex Break => _break;

        /// <summary>
        ///     <para>
        ///         Regular expression break pattern used by the tokeniser.
        ///     </para>
        ///
        ///     <para>
        ///         Shattering a line of text <c>line</c> by the tokeniser, without transformation, filtering and replacement of empty lines and line ends, is equivalent (performance aside) to calling <see cref="Regex.Split(String, String)" /> with <c>line</c> as the first argument and <see cref="BreakPattern" /> as the second.
        ///     </para>
        /// </summary>
        public String BreakPattern => _break.ToString();

        /// <summary>
        ///     <para>
        ///         Transformation function used by the tokeniser. A <c>null</c> reference means no transformation function is used.
        ///     </para>
        ///
        ///     <para>
        ///         Transformation is done on raw regular expression pattern matches, before (potential) filtering of empty tokens.
        ///     </para>
        /// </summary>
        public Func<String?, String?>? Transform => _transform;

        /// <summary>
        ///     <para>
        ///         Create a default tokeniser.
        ///     </para>
        /// </summary>
        public RegexTokeniser() : base()
        {
            _break = DefaultBreak;
            _transform = null;
        }

        /// <summary>
        ///     <para>
        ///         Create a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="breakPattern">Regular expression break pattern to use.</param>
        /// <param name="transform">Optional transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <param name="escape">If <c>true</c>, <paramref name="breakPattern" /> is escaped via <see cref="Regex.Escape(String)" /> method before usage.</param>
        /// <param name="options">Options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        public RegexTokeniser(String breakPattern, Func<String?, String?>? transform = null, bool escape = false, RegexOptions options = RegexOptions.None) : base()
        {
            _break = new Regex(escape ? Regex.Escape(breakPattern) : breakPattern, options);
            _transform = transform;
        }

        /// <summary>
        ///     <para>
        ///         Create a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="break">Regular expression breaker to use.</param>
        /// <param name="transform">Optional transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <param name="alterOptions">If <c>null</c>, <paramref name="break" />'s options (<see cref="Regex.Options" />) are used (actually, no new <see cref="Regex" /> is constructed); otherwise options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <remarks>
        ///     <para>
        ///         Calling this constructor is essentially the same (performance aside) as calling <see cref="RegexTokeniser.RegexTokeniser(String, Func{String?, String?}?, bool, RegexOptions)" /> constructor as:
        ///     </para>
        ///
        ///     <code>
        ///         RegexTokeniser.RegexTokeniser(breakPattern: @break.ToString(), transform: transform, options: alterOptions ?? @break.Options)
        ///     </code>
        /// </remarks>
        public RegexTokeniser(Regex @break, Func<String?, String?>? transform = null, RegexOptions? alterOptions = null) : base()
        {
            _break = alterOptions is null ? @break : new Regex(@break.ToString(), (RegexOptions)alterOptions!);
            _transform = transform;
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        ///
        ///     <para>
        ///         If <see cref="ShatteringOptions.IgnoreLineEnds" /> is <c>false</c> and <paramref name="tokens" /> is non-empty, <see cref="ShatteringOptions.LineEndToken" /> is added to <paramref name="tokens" /> before any token extracted from <paramref name="line" />. However, <see cref="ShatteringOptions.LineEndToken" /> is not added after <paramref name="line" />'s tokens.
        ///     </para>
        /// </summary>
        /// <param name="tokens">List of tokens</param>
        /// <param name="line"></param>
        /// <param name="options"></param>
        override protected void ShatterLine(ref List<String?> tokens, String line, ShatteringOptions options)
        {
            // Add `options.LineEndToken` if necessary.
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            // Shatter `line`, perform transformation if necessary and filter out empty tokens.
            IEnumerable<String?> lineTokens = Break.Split(line);
            if (!(Transform is null))
            {
                lineTokens = lineTokens.Select(t => Transform(t));
            }
            if (options.IgnoreEmptyTokens)
            {
                lineTokens = lineTokens.Where(t => !String.IsNullOrEmpty(t));
            }
            lineTokens = lineTokens.ToList();

            // Add tokens to `tokens` if necessary.
            if (lineTokens.Any())
            {
                tokens.AddRange(lineTokens);
            }
            else if (!options.IgnoreEmptyLines)
            {
                tokens.Add(options.EmptyLineToken);
            }
        }
    }
}
