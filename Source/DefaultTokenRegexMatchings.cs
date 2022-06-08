using System;

namespace MagicText
{
    /// <summary>Specifies default matchings provided by the <see cref="RegexMatchTokeniser" /> class.</summary>
    public enum DefaultTokenRegexMatchings
    {
        /// <summary>Match all words and <em>non-words</em>.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchTokeniser.DefaultWordsOrNonWordsPattern" />.</para>
        /// </remarks>
        WordsOrNonWords = 0,

        /// <summary>Match all words, punctuation and white spaces.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchTokeniser.DefaultWordsOrPunctuationOrWhiteSpacePattern" />.</para>
        /// </remarks>
        WordsOrPunctuationOrWhiteSpace = 1,

        /// <summary>Match all words and punctuation.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchTokeniser.DefaultWordsOrPunctuationPattern" />.</para>
        /// </remarks>
        WordsOrPunctuation = 2,

        /// <summary>Match all words.</summary>
        /// <remarks>
        ///     <para>This value corresponds to the <see cref="RegexMatchTokeniser.DefaultWordsPattern" />.</para>
        /// </remarks>
        Words = 3
    }
}
