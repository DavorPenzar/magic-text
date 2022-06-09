using System;

namespace MagicText
{
    /// <summary>Specifies default matchings provided by the <see cref="RegexMatchesTokeniser" /> class.</summary>
    public enum DefaultTokenRegexMatchings
    {
        /// <summary>Match all words and <em>non-words</em>.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchesTokeniser.DefaultWordsOrNonWordsPattern" />.</para>
        /// </remarks>
        WordsOrNonWords = 0,

        /// <summary>Match all words, punctuation and white spaces.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchesTokeniser.DefaultWordsOrPunctuationOrWhiteSpacePattern" />.</para>
        /// </remarks>
        WordsOrPunctuationOrWhiteSpace = 1,

        /// <summary>Match all words and punctuation.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchesTokeniser.DefaultWordsOrPunctuationPattern" />.</para>
        /// </remarks>
        WordsOrPunctuation = 2,

        /// <summary>Match all words.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchesTokeniser.DefaultWordsPattern" />.</para>
        /// </remarks>
        Words = 3
    }
}
