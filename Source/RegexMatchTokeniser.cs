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
    /// <summary>Implements a <see cref="LineByLineTokeniser" /> which shatters lines of text by capturing specific regular expression pattern matches.</summary>
    /// <remarks>
    ///     <para>This <see cref="LineByLineTokeniser" /> extension class simulates the <see cref="Regex.Matches(String)" /> method, unlike the <see cref="RegexBreakTokeniser" />, which simulates the <see cref="Regex.Split(String)" /> method, on each line of text.</para>
    ///     <para>Additional to shattering text into tokens, the <see cref="RegexMatchTokeniser" /> provides a possibility to customise the resulting tokens from raw <see cref="Match" />es (and not necessarily using the raw <see cref="Capture.Value" /> property) immediately after their capture and prior to checking for empty tokens. However, no built-in way of skipping tokens is provided through such customised token extraction. One possibility is to assign empty token values only to those tokens that ought to be skipped, and set the <see cref="ShatteringOptions.IgnoreEmptyTokens" /> option to <c>true</c>. Another possibility is to reserve a special <see cref="String" /> value for tokens to skip�e. g. a <c>null</c>�which would be assigned only to those tokens that should be skipped, and then manually filter out such tokens from the resulting token enumerable. The filtering part in the latter case may be implemented using the <see cref="Enumerable.Where{TSource}(IEnumerable{TSource}, Func{TSource, Boolean})" /> extension method.</para>
    ///     <para>If a default regular expression match pattern (<see cref="DefaultWordsOrNonWordsPattern" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />, <see cref="DefaultWordsOrPunctuationPattern" /> or <see cref="DefaultWordsPattern" />) should be used without special <see cref="RegexOptions" />, a better performance is achieved when using the default <see cref="RegexMatchTokeniser()" /> constructor or the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor, in which case a pre-built <see cref="Regex" /> object is used constructed (with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" />), instead of the <see cref="RegexMatchTokeniser(String, Boolean, RegexOptions, Func{Match, String})" /> constructor.</para>
    ///     <para>Empty tokens (which are ignored if <see cref="ShatteringOptions.IgnoreEmptyTokens" /> is <c>true</c>) are considered those tokens that yield <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> method. This behaviour cannot be overridden by a derived class.</para>
    ///     <para>No thread safety mechanism is implemented nor assumed by the class. If the token extraction function (<see cref="GetValue" />) should be thread-safe, lock the <see cref="RegexMatchTokeniser" /> instance during complete <see cref="ShatterLine(String)" />, <see cref="LineByLineTokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="LineByLineTokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method calls to ensure consistent behaviour of the function over a single shattering operation.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public class RegexMatchTokeniser : LineByLineTokeniser
    {
        protected const string RegexMatchPatternNullErrorMessage = "Regular expression match pattern cannot be null.";
        protected const string MatcherNullErrorMessage = "Regular expression matcher cannot be null.";
        protected const string RegexOptionsOutOfRangeErrorMessage = "The regex options argument passed in is out of the range of valid values.";
        protected const string RegexMatchPatternErrorMessage = "The regular expression match pattern is invalid.";
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

        /// <summary>The default regular expression match options.</summary>
        /// <remarks>
        ///     <para>The options are set to the following:</para>
        ///     <list type="number">
        ///         <item><see cref="RegexOptions.Singleline" />,</item>
        ///         <item><see cref="RegexOptions.CultureInvariant" />,</item>
        ///         <item><see cref="RegexOptions.Compiled" />.</item>
        ///     </list>
        /// </remarks>
        public const RegexOptions DefaultMatchOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static readonly Regex _defaultWordsOrNonWordsMatcher;
        private static readonly Regex _defaultWordsOrPunctuationOrWhiteSpaceMatcher;
        private static readonly Regex _defaultWordsOrPunctuationMatcher;
        private static readonly Regex _defaultWordsMatcher;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultWordsOrNonWordsPattern" />.</summary>
        /// <returns>The default words and <em>non-words</em> regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrNonWordsPattern" /> with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchTokeniser" /> using the default <see cref="RegexMatchTokeniser()" /> constructor or the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrNonWords" />, the <see cref="Matcher" /> shall be set to <see cref="DefaultWordsOrNonWordsMatcher" />.</para>
        /// </remarks>
        protected static Regex DefaultWordsOrNonWordsMatcher => _defaultWordsOrNonWordsMatcher;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" /> with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchTokeniser" /> using the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrPunctuationOrWhiteSpace" />, the <see cref="Matcher" /> shall be set to <see cref="DefaultWordsOrPunctuationOrWhiteSpaceMatcher" />.</para>
        /// </remarks>
        protected static Regex DefaultWordsOrPunctuationOrWhiteSpaceMatcher => _defaultWordsOrPunctuationOrWhiteSpaceMatcher;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultWordsOrPunctuationPattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsOrPunctuationPattern" /> with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchTokeniser" /> using the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.WordsOrPunctuation" />, the <see cref="Matcher" /> shall be set to <see cref="DefaultWordsOrPunctuationMatcher" />.</para>
        /// </remarks>
        protected static Regex DefaultWordsOrPunctuationMatcher => _defaultWordsOrPunctuationMatcher;

        /// <summary>Gets the regular expression breaker built upon the <see cref="DefaultWordsPattern" />.</summary>
        /// <returns>The default words, punctuation and white spaces regular expression matcher.</returns>
        /// <remarks>
        ///     <para>The regular expression matcher is constructed using the <see cref="DefaultWordsPattern" /> with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" />.</para>
        ///     <para>When constructing a <see cref="RegexMatchTokeniser" /> using the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> constructor with the <c>matching</c> parameter set to <see cref="DefaultTokenRegexMatchings.Words" />, the <see cref="Matcher" /> shall be set to <see cref="DefaultWordsMatcher" />.</para>
        /// </remarks>
        protected static Regex DefaultWordsMatcher => _defaultWordsMatcher;

        static RegexMatchTokeniser()
        {
            _defaultWordsOrNonWordsMatcher = new Regex(DefaultWordsOrNonWordsPattern, DefaultMatchOptions);
            _defaultWordsOrPunctuationOrWhiteSpaceMatcher = new Regex(DefaultWordsOrPunctuationOrWhiteSpacePattern, DefaultMatchOptions);
            _defaultWordsOrPunctuationMatcher = new Regex(DefaultWordsOrPunctuationPattern, DefaultMatchOptions);
            _defaultWordsMatcher = new Regex(DefaultWordsPattern, DefaultMatchOptions);
        }

        private readonly Regex _matcher;
        private readonly Func<Match, String?> _getValue;

        /// <summary>Gets the regular expression match pattern used by the tokeniser.</summary>
        /// <returns>The internal regular expression match pattern.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending with a line end (<a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF or CRLF</a>)) by the tokeniser, without transformation, filtering and replacement of empty lines, is equivalent (performance aside) to calling the <see cref="Regex.Matches(String, String)" /> method with <c>line</c> as the first argument and <see cref="MatchPattern" /> as the second.</para>
        /// </remarks>
        [RegexPattern]
        protected String MatchPattern => Matcher.ToString();

        /// <summary>Gets the regular expression matcher used by the tokeniser.</summary>
        /// <returns>The internal regular expression matcher.</returns>
        /// <remarks>
        ///     <para>Shattering a line of text <c>line</c> (not ending in a line end (<a href="http://en.wikipedia.org/wiki/Newline#Representation"> CR, LF or CRLF</a>)) by the tokeniser, without transformation, filtering and replacement of empty lines, is done by calling the <see cref="Matcher" />'s <see cref="Regex.Matches(String)" /> method with <c>line</c> as the argument.</para>
        /// </remarks>
        protected Regex Matcher => _matcher;

        /// <summary>Gets the token extraction function used by the tokeniser.</summary>
        /// <returns>The internal token extraction function.</returns>
        /// <remarks>
        ///     <para>Even if no explicit token extraction were provided in the construction of the tokeniser, or an explicit <c>null</c> was passed to the constructor, the <see cref="GetValue" /> property would not be <c>null</c>. Instead, it would be a default function which simply returns the <see cref="Match" />'s (its argument) <see cref="Capture.Value" /> property.</para>
        /// </remarks>
        protected Func<Match, String?> GetValue => _getValue;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="matcher">The regular expression matcher to use.</param>
        /// <param name="alterOptions">If not set (if <c>null</c>), the <c><paramref name="matcher" /></c>'s <see cref="Regex.Options" /> are used (actually, no new <see cref="Regex" /> is constructed but the original <c><paramref name="matcher" /></c> is used); otherwise the options passed to the <see cref="Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="matcher" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="alterOptions" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <remarks>
        ///     <para>Calling this constructor is essentially the same (performance aside) as calling the <see cref="RegexMatchTokeniser(String, Boolean, RegexOptions, Func{Match, String})" /> constructor as:</para>
        ///     <code>
        ///         <see cref="RegexBreakTokeniser" />(breakPattern: <paramref name="matcher" />.ToString(), options: <paramref name="alterOptions" /> ?? <paramref name="matcher" />.Options, getValue: <paramref name="getValue" />)
        ///     </code>
        /// </remarks>
        public RegexMatchTokeniser(Regex matcher, Nullable<RegexOptions> alterOptions = default, Func<Match, String?>? getValue = null) : base()
        {
            if (matcher is null)
            {
                throw new ArgumentNullException(nameof(matcher), MatcherNullErrorMessage);
            }

            if (alterOptions.HasValue)
            {
                try
                {
                    _matcher = new Regex(matcher.ToString(), alterOptions.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ArgumentOutOfRangeException(nameof(alterOptions), alterOptions.Value, RegexOptionsOutOfRangeErrorMessage);
                }
            }
            else
            {
                _matcher = matcher;
            }
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

        /// <summary>Creates a tokeniser with a default matching.</summary>
        /// <param name="matching">Specifies which default regular expression match pattern to use (<see cref="DefaultWordsOrNonWordsPattern" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpacePattern" />, <see cref="DefaultWordsOrPunctuationPattern" /> or <see cref="DefaultWordsPattern" />).</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <remarks>
        ///     <para>Actually, a pre-built <see cref="Regex" /> object (<see cref="DefaultWordsOrNonWordsMatcher" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpaceMatcher" />, <see cref="DefaultWordsOrPunctuationMatcher"/> or <see cref="DefaultWordsMatcher" />) with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" /> is used. Consider using this constructor or the default <see cref="RegexMatchTokeniser()" /> constructor if a default matching should be used to improve performance.</para>
        /// </remarks>
        public RegexMatchTokeniser(DefaultTokenRegexMatchings matching, Func<Match, String?>? getValue = null)
        {
            _matcher = matching switch
            {
                DefaultTokenRegexMatchings.WordsOrNonWords => DefaultWordsOrNonWordsMatcher,
                DefaultTokenRegexMatchings.WordsOrPunctuationOrWhiteSpace => DefaultWordsOrPunctuationOrWhiteSpaceMatcher,
                DefaultTokenRegexMatchings.WordsOrPunctuation => DefaultWordsOrPunctuationMatcher,
                DefaultTokenRegexMatchings.Words => DefaultWordsMatcher,
                _ => throw new ArgumentException(MatchingNotSupportedErrorMessage, nameof(matching)),
            };
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

        /// <summary>Creates a default tokeniser.</summary>
        /// <remarks>
        ///     <para>The <see cref="DefaultWordsOrNonWordsPattern" /> is used as the regular expression match pattern.</para>
        ///     <para>Actually, a pre-built <see cref="Regex" /> object (<see cref="DefaultWordsOrNonWordsMatcher" />, <see cref="DefaultWordsOrPunctuationOrWhiteSpaceMatcher" />, <see cref="DefaultWordsOrPunctuationMatcher"/> or <see cref="DefaultWordsMatcher" />) with <see cref="Regex.Options" /> set to <see cref="DefaultMatchOptions" /> is used. Consider using this constructor or the <see cref="RegexMatchTokeniser(DefaultTokenRegexMatchings, Func{Match, String})" /> if a default tokeniser should be used to improve performance.</para>
        /// </remarks>
        public RegexMatchTokeniser() : this(DefaultTokenRegexMatchings.WordsOrNonWords)
        {
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="matchPattern">The regular expression match pattern to use.</param>
        /// <param name="escape">If <c>true</c>, the <c><paramref name="matchPattern" /></c> is escaped using the <see cref="Regex.Escape(String)" /> method to construct the actual regular expression match pattern.</param>
        /// <param name="options">The options passed to the <see cref="Regex(String, RegexOptions)" /> constructor.</param>
        /// <param name="getValue">The optional token extraction function. If <c>null</c>, tokens are constructed from <see cref="Match" />es' raw <see cref="Capture.Value" /> properties.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="matchPattern" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="options" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="matchPattern" /></c> is not a valid regular expression.</exception>
        public RegexMatchTokeniser([RegexPattern] String matchPattern, Boolean escape = false, RegexOptions options = RegexOptions.None, Func<Match, String?>? getValue = null) : base()
        {
            if (matchPattern is null)
            {
                throw new ArgumentNullException(nameof(matchPattern));
            }

            try
            {
                _matcher = new Regex(escape ? Regex.Escape(matchPattern) : matchPattern, options);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options, RegexOptionsOutOfRangeErrorMessage);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(RegexMatchPatternErrorMessage, matchPattern, exception);
            }
            _getValue = getValue ?? CaptureExtensions.GetValueOrNull;
        }

#if NETSTANDARD2_0

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>After capturing <see cref="Match" />es from the <c><paramref name="line" /></c>, the token extraction from the <see cref="Match" />es is done using the <see cref="GetValue" /> function.</para>
        ///     <para>The returned enumerable is merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : Matcher.Matches(line).Cast<Match>().Select(GetValue);

#else

        /// <summary>Shatters a single <c><paramref name="line" /></c> into tokens.</summary>
        /// <param name="line">The line of text to shatter.</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="line" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="line" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>After capturing <see cref="Match" />es from the <c><paramref name="line" /></c>, the token extraction from the <see cref="Match" />es is done using the <see cref="GetValue" /> function.</para>
        ///     <para>The returned enumerable is merely be a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If multiple enumeration processes over the enumerable should be performed, it is advisable to convert it to a fully built container beforehand, such as a <see cref="List{T}" /> via the <see cref="List{T}.List(IEnumerable{T})" /> constructor or the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method.</para>
        ///
        ///     <h3>Notes to Implementers</h3>
        ///     <para>This method cannot be overridden.</para>
        /// </remarks>
        protected sealed override IEnumerable<String?> ShatterLine(String line) =>
            line is null ? throw new ArgumentNullException(nameof(line), LineNullErrorMessage) : Matcher.Matches(line).Select(GetValue);

#endif // NETSTANDARD2_0
    }
}
