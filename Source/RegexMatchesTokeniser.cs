using JetBrains.Annotations;
using MagicText.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace MagicText
{
    /// <summary>Implements a <see cref="LineShatteringTokeniser" /> which shatters lines of text by capturing and extracting specific regular expression pattern matches.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineShatteringTokeniser" /> extension class simulates the <see cref="System.Text.RegularExpressions.Regex.Matches(String)" /> method, unlike the <see cref="RegexSplitTokeniser" />, which simulates the <see cref="System.Text.RegularExpressions.Regex.Split(String)" /> method, on each line of text.</para>
    ///     <para>Additional to shattering text into tokens, the <see cref="RegexMatchesTokeniser" /> provides a possibility to customise the resulting tokens from raw <see cref="Match" />es (and not necessarily using the raw <see cref="Capture.Value" /> property) immediately after their capture and prior to checking for empty tokens. However, no built-in way of skipping tokens is provided through such customised token extraction. One possibility is to assign empty token values only to those tokens that ought to be skipped, and set the <see cref="ShatteringOptions.IgnoreEmptyTokens" /> option to <c>true</c>. Another possibility is to reserve a special <see cref="String" /> value for tokens to skip—e. g. a <c>null</c>—which would be assigned only to those tokens that should be skipped, and then manually filter out such tokens from the resulting token enumerable. The filtering part in the latter case may be implemented using the <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, Boolean})" /> extension method.</para>
    ///     <para>If a default regular expression match pattern (<see cref="DefaultWordsOrNonWordsPattern" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />, <see cref="DefaultWordsOrPunctuationPattern" /> or <see cref="DefaultWordsPattern" />) should be used without special <see cref="RegexOptions" />, a better performance is achieved when using the default <see cref="RegexMatchesTokeniser()" /> constructor or the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor, in which case a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object is used (constructed with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />), instead of the <see cref="RegexMatchesTokeniser(String, Boolean, RegexOptions, Func{Match, String})" /> constructor.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the token extraction function (<see cref="GetValue" />) should be thread-safe, lock the <see cref="RegexMatchesTokeniser" /> instance during complete <see cref="ShatterLine(String)" />, <see cref="LineShatteringTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineShatteringTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls to ensure consistent behaviour of the function over a single shattering operation.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public class RegexMatchesTokeniser : RegexTokeniser
    {
        protected const string MatchingNotSupportedErrorMessage = "The default matching passed in is currently not supported.";

        /// <summary>The default regular expression match pattern that matches all words and <em>non-words</em>.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of either word characters (<c>"\\w"</c>) or non-word characters (<c>"\\W"</c>). That is, each match is non-empty (at least one character) and entirely consisted of word characters or entirely consisted of non-word characters.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultWordsOrNonWordsPattern = @"\w+|\W+";

        /// <summary>The default regular expression match pattern that matches all words, punctuation and white spaces.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of either word characters (<c>"\\w"</c>), punctuation and mathematics symbols (<c>"\\p{P}"</c> and <c>"\\p{Sm}"</c> respectively), or white spaces and separator characters (<c>"\\s"</c> and <c>"\\p{Z}"</c> respectively). That is, each match is non-empty (at least one character) and entirely consisted of word characters, entirely consisted of punctuation and mathematics symbols, or entirely consisted of white spaces and separator characters.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultWordsOrPunctuationOrWhiteSpacePattern = @"\w+|[\p{P}\p{Sm}]+|[\s\p{Z}]+";

        /// <summary>The default regular expression match pattern that matches all words and punctuation.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of either word characters (<c>"\\w"</c>), or punctuation and mathematics symbols (<c>"\\p{P}"</c> and <c>"\\p{Sm}"</c> respectively). That is, each match is non-empty (at least one character) and entirely consisted of word characters, or entirely consisted of punctuation and mathematics symbols.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultWordsOrPunctuationPattern = @"\w+|[\p{P}\p{Sm}]+";

        /// <summary>The default regular expression match pattern that matches all words.</summary>
        /// <remarks>
        ///     <para>The pattern matches all non-empty continuous groups of word characters (<c>"\\w"</c>).</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public const string DefaultWordsPattern = @"\w+";

        private static readonly System.Text.RegularExpressions.Regex _defaultWordsOrNonWordsRegex;
        private static readonly System.Text.RegularExpressions.Regex _defaultWordsOrPunctuationOrWhiteSpaceRegex;
        private static readonly System.Text.RegularExpressions.Regex _defaultWordsOrPunctuationRegex;
        private static readonly System.Text.RegularExpressions.Regex _defaultWordsRegex;

        /// <summary>Gets the regular expression matcher built upon the <see cref="DefaultWordsOrNonWordsPattern" />.</summary>
        /// <returns>The default words and <em>non-words</em> regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrNonWordsPattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchesTokeniser" /> using the default <see cref="RegexMatchesTokeniser()" /> constructor or the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrNonWords" />, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultWordsOrNonWordsRegex" />.</para>
        /// </remarks>
        protected static System.Text.RegularExpressions.Regex DefaultWordsOrNonWordsRegex => _defaultWordsOrNonWordsRegex;

        /// <summary>Gets the regular expression matcher built upon the <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchesTokeniser" /> using the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrPunctuationOrWhiteSpace" />, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultWordsOrPunctuationOrWhiteSpaceRegex" />.</para>
        /// </remarks>
        protected static System.Text.RegularExpressions.Regex DefaultWordsOrPunctuationOrWhiteSpaceRegex => _defaultWordsOrPunctuationOrWhiteSpaceRegex;

        /// <summary>Gets the regular expression matcher built upon the <see cref="DefaultWordsOrPunctuationPattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrPunctuationPattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchesTokeniser" /> using the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrPunctuation" />, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultWordsOrPunctuationRegex" />.</para>
        /// </remarks>
        protected static System.Text.RegularExpressions.Regex DefaultWordsOrPunctuationRegex => _defaultWordsOrPunctuationRegex;

        /// <summary>Gets the regular expression matcher built upon the <see cref="DefaultWordsPattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsPattern" /> with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchesTokeniser" /> using the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.Words" />, the <see cref="RegexTokeniser.Regex" /> shall be set to <see cref="DefaultWordsRegex" />.</para>
        /// </remarks>
        protected static System.Text.RegularExpressions.Regex DefaultWordsRegex => _defaultWordsRegex;

        /// <summary>Initialises static fields.</summary>
        static RegexMatchesTokeniser()
        {
            _defaultWordsOrNonWordsRegex = new System.Text.RegularExpressions.Regex(DefaultWordsOrNonWordsPattern, DefaultOptions);
            _defaultWordsOrPunctuationOrWhiteSpaceRegex = new System.Text.RegularExpressions.Regex(DefaultWordsOrPunctuationOrWhiteSpacePattern, DefaultOptions);
            _defaultWordsOrPunctuationRegex = new System.Text.RegularExpressions.Regex(DefaultWordsOrPunctuationPattern, DefaultOptions);
            _defaultWordsRegex = new System.Text.RegularExpressions.Regex(DefaultWordsPattern, DefaultOptions);
        }

        private readonly Func<Match, String?> _getValue;
        
        /// <summary>Gets the token extraction function used by the tokeniser.</summary>
        /// <returns>The internal token extraction function.</returns>
        /// <remarks>
        ///     <para>Even if no explicit token extraction function were provided in the construction of the tokeniser, or an explicit <c>null</c> was passed to the constructor, the <see cref="GetValue" /> property would not be <c>null</c>. Instead, it is going to be a default function which simply returns the <see cref="Match" />'s (its argument's) <see cref="Capture.Value" /> property.</para>
        /// </remarks>
        protected Func<Match, String?> GetValue => _getValue;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="regex">The regular expression matcher to use.</param>
        /// <param name="alterOptions">If not set (if <c>null</c>), the <c><paramref name="regex" /></c>'s <see cref="System.Text.RegularExpressions.Regex.Options" /> are used (actually, no new <see cref="System.Text.RegularExpressions.Regex" /> is constructed but the original <c><paramref name="regex" /></c> is used); otherwise the options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="regex" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="alterOptions" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <remarks>
        ///     <para>Calling this constructor is essentially the same (performance aside) as calling the <see cref="RegexMatchesTokeniser(String, Boolean, RegexOptions, Func{Match, String})" /> constructor as:</para>
        ///     <code>
        ///         <see cref="RegexMatchesTokeniser" />(pattern: <paramref name="regex" />.ToString(), options: <paramref name="alterOptions" /> ?? <paramref name="regex" />.Options, getValue: <paramref name="getValue" />)
        ///     </code>
        /// </remarks>
        public RegexMatchesTokeniser(System.Text.RegularExpressions.Regex regex, Nullable<RegexOptions> alterOptions = default, Func<Match, String?>? getValue = null) : base(regex, alterOptions)
        {
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

        /// <summary>Creates a tokeniser with a default matching.</summary>
        /// <param name="matching">Specifies which default regular expression match pattern to use (<see cref="DefaultWordsOrNonWordsPattern" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />, <see cref="DefaultWordsOrPunctuationPattern" /> or <see cref="DefaultWordsPattern" />).</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <remarks>
        ///     <para>Actually, a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object (<see cref="DefaultWordsOrNonWordsRegex" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpaceRegex" />, <see cref="DefaultWordsOrPunctuationRegex"/> or <see cref="DefaultWordsRegex" />) with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" /> is used. Consider using this constructor or the default <see cref="RegexMatchesTokeniser()" /> constructor if a default matching should be used to improve performance.</para>
        /// </remarks>
        public RegexMatchesTokeniser(DefaultTokenRegexMatchings matching, Func<Match, String?>? getValue = null) : base(matching switch { DefaultTokenRegexMatchings.WordsOrNonWords => DefaultWordsOrNonWordsRegex, DefaultTokenRegexMatchings.WordsOrPunctuationOrWhiteSpace => DefaultWordsOrPunctuationOrWhiteSpaceRegex, DefaultTokenRegexMatchings.WordsOrPunctuation => DefaultWordsOrPunctuationRegex, DefaultTokenRegexMatchings.Words => DefaultWordsRegex, _ => throw new ArgumentException(MatchingNotSupportedErrorMessage, nameof(matching)) })
        {
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The <see cref="DefaultWordsOrNonWordsPattern" /> is used as the regular expression match pattern.</para>
        ///     <para>Actually, a pre-built <see cref="System.Text.RegularExpressions.Regex" /> object (<see cref="DefaultWordsOrNonWordsRegex" />) with <see cref="System.Text.RegularExpressions.Regex.Options" /> set to <see cref="RegexTokeniser.DefaultOptions" /> is used. Consider using this constructor or the <see cref="RegexMatchesTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor if a default tokeniser should be used to improve performance.</para>
        /// </remarks>
        public RegexMatchesTokeniser() : this(DefaultTokenRegexMatchings.WordsOrNonWords)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="pattern">The regular expression match pattern to use.</param>
        /// <param name="escape">If <c>true</c>, the <c><paramref name="pattern" /></c> is escaped using the <see cref="System.Text.RegularExpressions.Regex.Escape(String)" /> method to construct the actual regular expression match pattern.</param>
        /// <param name="options">The options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="pattern" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="options" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="pattern" /></c> is not a valid regular expression pattern.</exception>
        public RegexMatchesTokeniser([RegexPattern] String pattern, Boolean escape = false, RegexOptions options = RegexOptions.None, Func<Match, String?>? getValue = null) : base(pattern, escape, options)
        {
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

#if NETSTANDARD2_0

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>Performance aside, the initial capturing is equivalent to calling the <see cref="System.Text.RegularExpressions.Regex.Matches(String, String, RegexOptions)" /> method with the <c><paramref name="line" /></c> as the first argument, the <see cref="RegexTokeniser.Pattern" /> as the second and <see cref="RegexTokeniser.Options" /> as the third.</para>
        ///     <para>After capturing <see cref="Match" />es from the <c><paramref name="line" /></c>, the token extraction from the <see cref="Match" />es is done using the <see cref="GetValue" /> function.</para>
        ///     <para>The returned enumerable is merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : Regex.Matches(line).Cast<Match>().Select(GetValue);

#else

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>Performance aside, the initial capturing is equivalent to calling the <see cref="System.Text.RegularExpressions.Regex.Matches(String, String, RegexOptions)" /> method with the <c><paramref name="line" /></c> as the first argument, the <see cref="RegexTokeniser.Pattern" /> as the second and <see cref="RegexTokeniser.Options" /> as the third.</para>
        ///     <para>After capturing <see cref="Match" />es from the <c><paramref name="line" /></c>, the token extraction from the <see cref="Match" />es is done using the <see cref="GetValue" /> function.</para>
        ///     <para>The returned enumerable is merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : Regex.Matches(line).Select(GetValue);

#endif // NETSTANDARD2_0
    }
}
