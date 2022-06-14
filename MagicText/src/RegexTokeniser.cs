using JetBrains.Annotations;
using System;
using System.Text.RegularExpressions;

namespace MagicText
{
    /// <summary>Provides additional mechanisms to <see cref="LineShatteringTokeniser" /> for extending classes which shatter lines of text based on <see cref="System.Text.RegularExpressions.Regex" /> objects.</summary>
    /// <remarks>
    ///     <h3>Notes to Implementers</h3>
    ///     <para>Use the <see cref="Regex" /> property instead of the <see cref="Pattern" /> and <see cref="Options" /> properties when using the <see cref="System.Text.RegularExpressions.Regex" /> class' methods.</para>
    ///     <para>If a default/single/logical/usual regular expression pattern exists for the concrete extending class, consider constructing a pre-built static <see cref="System.Text.RegularExpressions.Regex" />(es) in the static constructor that would be imputed in all instances using the pattern to avoid multiple constructions of essentially the same <see cref="System.Text.RegularExpressions.Regex" /> instances. Use the <see cref="DefaultOptions" /> in their construction.</para>
    /// </remarks>
    public abstract class RegexTokeniser : LineShatteringTokeniser
    {
        protected const string RegexNullErrorMessage = "Regular expression instance cannot be null.";
        protected const string PatternNullErrorMessage = "Regular expression pattern cannot be null.";
        protected const string OptionsOutOfRangeErrorMessage = "The regex options argument passed in is out of the range of valid values.";
        protected const string InvalidPatternErrorMessage = "The regular expression break pattern is invalid.";

        /// <summary>The default regular expression options for pre-built (static) <see cref="System.Text.RegularExpressions.Regex" />es.</summary>
        /// <remarks>
        ///     <para>The options are set to the following:</para>
        ///     <list type="number">
        ///         <item><see cref="RegexOptions.Singleline" />,</item>
        ///         <item><see cref="RegexOptions.CultureInvariant" />,</item>
        ///         <item><see cref="RegexOptions.Compiled" />.</item>
        ///     </list>
        /// </remarks>
        public const RegexOptions DefaultOptions = RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private readonly System.Text.RegularExpressions.Regex _regex;

        /// <summary>Gets the regular expression used by the tokeniser.</summary>
        /// <returns>The internal regular expression.</returns>
        /// <remarks>
        ///     <para>For more information on how the <see cref="Regex" /> is utilised in the <see cref="LineShatteringTokeniser.ShatterLine(String)" /> method, please refer to the concrete tokeniser's class' documentation.</para>
        /// </remarks>
        protected System.Text.RegularExpressions.Regex Regex => _regex;

        /// <summary>Gets the regular expression pattern used by the tokeniser.</summary>
        /// <returns>The internal regular expression pattern.</returns>
        /// <remarks>
        ///     <para>For more information on how the <see cref="Pattern" /> is utilised in the <see cref="LineShatteringTokeniser.ShatterLine(String)" /> method, please refer to the concrete tokeniser's class' documentation.</para>
        /// </remarks>
        /* language = regexp | jsregexp */
        [RegexPattern]
        public String Pattern => Regex.ToString();

        /// <summary>Gets the regular expression options used by the tokeniser.</summary>
        /// <returns>The internal regular expression options.</returns>
        /// <remarks>
        ///     <para>For more information on how the <see cref="Options" /> are utilised in the <see cref="LineShatteringTokeniser.ShatterLine(String)" /> method, please refer to the concrete tokeniser's class' documentation.</para>
        /// </remarks>
        public RegexOptions Options => Regex.Options;

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="regex">The regular expression to use.</param>
        /// <param name="alterOptions">If not set (if <c>null</c>), the <c><paramref name="regex" /></c>'s <see cref="System.Text.RegularExpressions.Regex.Options" /> are used (actually, no new <see cref="System.Text.RegularExpressions.Regex" /> is constructed but the original <c><paramref name="regex" /></c> is used); otherwise the options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="regex" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="alterOptions" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <remarks>
        ///     <para>Calling this constructor is essentially the same (performance aside) as calling the <see cref="RegexTokeniser(String, Boolean, RegexOptions)" /> constructor as:</para>
        ///     <code>
        ///         <see cref="RegexTokeniser" />(pattern: <paramref name="regex" />.ToString(), options: <paramref name="alterOptions" /> ?? <paramref name="regex" />.Options)
        ///     </code>
        /// </remarks>
        public RegexTokeniser(System.Text.RegularExpressions.Regex regex, Nullable<RegexOptions> alterOptions = default) : base()
        {
            if (regex is null)
            {
                throw new ArgumentNullException(nameof(regex), RegexNullErrorMessage);
            }

            if (alterOptions.HasValue)
            {
                try
                {
                    _regex = new System.Text.RegularExpressions.Regex(regex.ToString(), alterOptions.Value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ArgumentOutOfRangeException(nameof(alterOptions), alterOptions.Value, OptionsOutOfRangeErrorMessage);
                }
            }
            else
            {
                _regex = regex;
            }
        }

        /// <summary>Creates a tokeniser.</summary>
        /// <param name="pattern">The regular expression pattern to use.</param>
        /// <param name="escape">If <c>true</c>, the <c><paramref name="pattern" /></c> is escaped using the <see cref="System.Text.RegularExpressions.Regex.Escape(String)" /> method to construct the actual regular expression pattern.</param>
        /// <param name="options">The options passed to the <see cref="System.Text.RegularExpressions.Regex(String, RegexOptions)" /> constructor.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="pattern" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The <c><paramref name="options" /></c> are an invalid <see cref="RegexOptions" /> value.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="pattern" /></c> is not a valid regular expression pattern.</exception>
        public RegexTokeniser([RegexPattern] String pattern, Boolean escape = false, RegexOptions options = RegexOptions.None) : base()
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern), PatternNullErrorMessage);
            }

            try
            {
                _regex = new System.Text.RegularExpressions.Regex(escape ? System.Text.RegularExpressions.Regex.Escape(pattern) : pattern, options);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(options), options, OptionsOutOfRangeErrorMessage);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(InvalidPatternErrorMessage, nameof(pattern), exception);
            }
        }
    }
}
