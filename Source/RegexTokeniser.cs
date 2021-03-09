using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MagicText
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
    ///         If the default regular expression break pattern (<see cref="DefaultBreakPattern" />) should be used without special <see cref="RegexOptions" />, better performance is achieved when using <see cref="RegexTokeniser.RegexTokeniser" /> constructor, in which case a pre-built <see cref="Regex" /> object is used constructed with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />.
    ///     </para>
    ///
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
    ///
    ///     <para>
    ///         No thread safety mechanism is implemented nor assumed by the class. If the function for token transformation (<see cref="Transform" />) should be thread-safe, lock the tokeniser during complete <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(StreamReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(StreamReader, ShatteringOptions?)" /> method calls to ensure consistent behaviour of the function over a single shattering process.
    ///     </para>
    /// </remarks>
    public class RegexTokeniser : LineByLineTokeniser
    {
        private const string RegexPatternNullErrorMessage = "Regular expression pattern may not be `null`.";
        private const string BreakNullErrorMessage = "Regular expression breaker may not be `null`.";

        /// <summary>
        ///     <para>
        ///         Default regular expression break pattern.
        ///     </para>
        ///
        ///     <para>
        ///         The pattern matches all non-empty continuous white space groups, punctuation symbol groups (periods, exclamation marks, question marks, colons, semicolons, brackets and dashes are included, but quotation marks are not) and the horizontal ellipsis. The pattern is enclosed in (round) brackets to be captured by <see cref="Regex.Split(String)" /> method.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         As the pattern captures both punctuation symbol groups and white space groups, and the latter usually follow the former, shattering inputs using the pattern may cause empty tokens. For instance, shattering the string <c>"Lorem. Ipsum."</c> shall result in enumerable of tokens <c>{ "Lorem", ".", "", " ", "Ipsum", "." }</c> (note the empty string between the first period (<c>'.'</c>) and the white space (<c>' '</c>)). The empty tokens may be ignored by providing corresponding <see cref="ShatteringOptions" /> to <see cref="LineByLineTokeniser.Shatter(StreamReader, ShatteringOptions?)" /> and <see cref="LineByLineTokeniser.ShatterAsync(StreamReader, ShatteringOptions?)" /> methods.
        ///     </para>
        /// </remarks>
        public const string DefaultBreakPattern = @"(\s+|[\.!\?‽¡¿⸘,:;\(\)\[\]\{\}\-—–]+|…)";

        private static readonly Regex _DefaultBreak;

        /// <summary>
        ///     <para>
        ///         The regular expression breaker is constructed using the default regular expression break pattern (<see cref="DefaultBreakPattern" />) and with <see cref="Regex.Options" /> set to <see cref="RegexOptions.Compiled" />.
        ///     </para>
        /// </summary>
        /// <returns>Default regular expression breaker.</returns>
        protected static Regex DefaultBreak => _DefaultBreak;

        /// <summary>
        ///     <para>
        ///         Initialise static fields.
        ///     </para>
        /// </summary>
        static RegexTokeniser()
        {
            _DefaultBreak = new Regex(DefaultBreakPattern, RegexOptions.Compiled);
        }

        /// <summary>
        ///     <para>
        ///         <em>Transform</em> <paramref name="token" /> using the identity function (return <paramref name="token" /> unchanged).
        ///     </para>
        /// </summary>
        /// <param name="token">Token to transform.</param>
        /// <returns><paramref name="token" /></returns>
        protected static String? IdentityTransform(String? token) =>
            token;

        /// <summary>
        ///     <para>
        ///         Create a transformation function of regular expression based replacement.
        ///     </para>
        ///
        ///     <para>
        ///         This method is useful for creating <see cref="RegexTokeniser" />s' regular expression based transformation functions (<see cref="RegexTokeniser.Transform" />).
        ///     </para>
        /// </summary>
        /// <param name="matchPattern">Regular expression pattern to match.</param>
        /// <param name="replacementPattern">Regular expression pattern for replacement of matches captured by <paramref name="matchPattern" />.</param>
        /// <param name="escapeMatch">If <c>true</c>, <paramref name="matchPattern" /> is escaped via <see cref="Regex.Escape(String)" /> method before usage.</param>
        /// <param name="escapeReplacement">If <c>true</c>, <paramref name="replacementPattern" /> is escaped via <see cref="Regex.Escape(String)" /> method before usage.</param>
        /// <param name="options">Options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <returns>Function that returns <c>null</c> when passed a <c>null</c>, otherwise performs the regular expression based replacement defined.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="matchPattern" /> is <c>null</c>. If <paramref name="replacementPattern" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.
        ///     </para>
        /// </remarks>
        /// <seealso cref="RegexTokeniser.Transform" />
        public static Func<String?, String?> CreateReplacementTransformationFunction(string matchPattern, string replacementPattern, bool escapeMatch = false, bool escapeReplacement = false, RegexOptions options = RegexOptions.None)
        {
            if (matchPattern is null || replacementPattern is null)
            {
                throw new ArgumentNullException(RegexPatternNullErrorMessage, (Exception?)null);
            }

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
        ///         In <see cref="ShatterLine(String)" /> method, <see cref="Regex.Split(String)" /> method is called on <see cref="Break" /> to extract raw tokens from <c>line</c>.
        ///     </para>
        /// </summary>
        /// <returns>Regular expression breaker used by the tokeniser.</returns>
        protected Regex Break => _break;

        /// <summary>
        ///     <para>
        ///         Transformation is done on raw regular expression pattern matches, before (potential) filtering of empty tokens.
        ///     </para>
        /// </summary>
        /// <returns>Transformation function used by the tokeniser. If <c>null</c>, no transformation function is used.</returns>

        protected Func<String?, String?>? Transform => _transform;

        /// <summary>
        ///     <para>
        ///         Shattering a line of text <c>line</c> by the tokeniser, without transformation, filtering and replacement of empty lines and line ends, is equivalent (performance aside) to calling <see cref="Regex.Split(String, String)" /> with <c>line</c> as the first argument and <see cref="BreakPattern" /> as the second.
        ///     </para>
        /// </summary>
        /// <returns>Regular expression break pattern used by the tokeniser.</returns>
        public String BreakPattern => Break.ToString();

        /// <summary>
        ///     <para>
        ///         Create a default tokeniser.
        ///     </para>
        /// </summary>
        public RegexTokeniser() : this(DefaultBreak)
        {
            _break = DefaultBreak;
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
        /// <exception cref="ArgumentNullException">If <paramref name="breakPattern" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.
        ///     </para>
        /// </remarks>
        public RegexTokeniser(String breakPattern, Func<String?, String?>? transform = null, bool escape = false, RegexOptions options = RegexOptions.None) : this(new Regex(escape ? Regex.Escape(breakPattern ?? throw new ArgumentNullException(nameof(breakPattern), RegexPatternNullErrorMessage)) : breakPattern, options), transform)
        {
        }

        /// <summary>
        ///     <para>
        ///         Create a tokeniser with provided options.
        ///     </para>
        /// </summary>
        /// <param name="isEmptyToken">Function to check if a token is empty.</param>
        /// <param name="break">Regular expression breaker to use.</param>
        /// <param name="transform">Optional transformation function. If <c>null</c>, no transformation function is used.</param>
        /// <param name="alterOptions">If <c>null</c>, <paramref name="break" />'s options (<see cref="Regex.Options" />) are used (actually, no new <see cref="Regex" /> is constructed); otherwise options passed to <see cref="Regex.Regex(String, RegexOptions)" /> constructor.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="break" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <see cref="Regex" /> class's constructor and methods are not caught.
        ///     </para>
        ///
        ///     <para>
        ///         Calling this constructor is essentially the same (performance aside) as calling <see cref="RegexTokeniser.RegexTokeniser(String, Func{String?, String?}?, Boolean, RegexOptions)" /> constructor as:
        ///     </para>
        ///
        ///     <code>
        ///         <see cref="RegexTokeniser" />.RegexTokeniser(breakPattern: @<paramref name="break" />.ToString(), transform: <paramref name="transform" />, options: <paramref name="alterOptions" /> ?? @<paramref name="break" />.Options)
        ///     </code>
        /// </remarks>
        /// <seealso cref="RegexTokeniser.RegexTokeniser(String, Func{String?, String?}?, Boolean, RegexOptions)"/>
        public RegexTokeniser(Regex @break, Func<String?, String?>? transform = null, RegexOptions? alterOptions = null) : base()
        {
            if (@break is null)
            {
                throw new ArgumentNullException(nameof(@break), BreakNullErrorMessage);
            }

            _break = alterOptions is null ? @break : new Regex(@break.ToString(), (RegexOptions)alterOptions!);
            _transform = transform ?? IdentityTransform;
        }

        /// <summary>
        ///     <para>
        ///         Shatter a single line into tokens.
        ///     </para>
        ///
        ///     <para>
        ///         After splitting <paramref name="line" /> using the internal regular expression breaker (<see cref="Break" />), the transformation of tokens is done using the transformation function (<see cref="Transform" />) if it is set.
        ///     </para>
        /// </summary>
        /// <param name="line">Line of text to shatter.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="line" />.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="line" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The returned enumerable is merely a query. If multiple enumerations over it should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.
        ///     </para>
        /// </remarks>
        override protected IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            IEnumerable<String?> tokens = Break.Split(line);
            if (!(Transform is null))
            {
                tokens = tokens.Select(t => Transform(t));
            }

            return tokens;
        }
    }
}
