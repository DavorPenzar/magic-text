using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Random text generator.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the pen should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable <c>tokens</c> passed to the constructor. To prevent overflowing from one source to another (e. g. if the last token from the first source is not a contextual predecessor of the first token from the second source), an ending token (<see cref="EndToken" />) should be put between the sources' tokens in the final enumerable <c>tokens</c>. Choosing the ending token in <see cref="Render(Int32, Func{Int32, Int32})" /> or <see cref="Render(Int32, Random)" /> methods will cause the rendering to stop—the same as when a successor of the last entry in tokens is chosen.
    ///     </para>
    ///
    ///     <para>
    ///         A complete deep copy of enumerable <c>tokens</c> (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large.
    ///     </para>
    ///
    ///     <para>
    ///         Changing any of the properties—public or protected—will break the functionality of the pen. This includes, but is not limited to, manually changing the contents or their order in collections <see cref="Tokens" /> and <see cref="Positions" />. By doing so, behaviour of <see cref="Render(Int32, Func{Int32, Int32})" /> and <see cref="Render(Int32, Random)" /> methods is unexpected and no longer guaranteed.
    ///     </para>
    /// </remarks>
    public class Pen
    {
        private const string RelevantTokensOutOfRangeErrorMessage = "Number of relevant tokens must be non-negative (greater than or equal to 0).";
        private const string PickOutOfRangeErrorMessage = "Picker function must return an integer from [0, n) union {0}.";

        /// <summary>
        ///     <para>
        ///         Find first position of <paramref name="t" /> in <paramref name="tokens" /> sorted by <paramref name="positions" />.
        ///     </para>
        /// </summary>
        /// <param name="comparer">String comparer used for lexicographic ordering of <paramref name="tokens" />.</param>
        /// <param name="tokens">List of tokens amongst which <paramref name="t" /> should be found.</param>
        /// <param name="positions">Positional ordering of <paramref name="tokens" /> in respect of <paramref name="comparer" />.</param>
        /// <param name="t">Token to find in <paramref name="tokens" />.</param>
        /// <returns>Index <c>i</c> such that <c>comparer.Equal(tokens[positions[i]], t)</c>, but <c>!comparer.Equal(tokens[positions[j]], t)</c> for each <c>j &lt; i</c> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).</returns>
        private static int FindPosition(StringComparer comparer, IReadOnlyCollection<String?> tokens, IReadOnlyCollection<Int32> positions, String? t)
        {
            // Binary search...

            // Initialise lower, upper and middle positions.
            int l = 0;
            int h = tokens.Count;
            int m;

            // Loop until found.
            while (true)
            {
                // Extract the middle token.
                m = (l + h) >> 1;
                var t2 = tokens.ElementAt(positions.ElementAt(m));

                // Compare tokens.
                int c = comparer.Compare(t2, t);

                // Break the loop or update bounds.
                if (c == 0)
                {
                    break;
                }
                else if (c < 0)
                {
                    l = m;
                }
                else if (c > 0)
                {
                    h = m;
                }
            }

            // Find the minimal index `m` of the position of a token equal to `t`.
            while (m > 0 && comparer.Equals(tokens.ElementAt(positions.ElementAt(m - 1)), t))
            {
                --m;
            }

            return m;
        }

        private readonly StringComparer _comparer;
        private readonly ReadOnlyCollection<String?> _tokens;
        private readonly ReadOnlyCollection<Int32> _positions;
        private readonly String? _endToken;
        private readonly Lazy<Boolean> _lazyAllEnds;

        /// <summary>
        ///     <para>
        ///         Sorting positions of entries in <see cref="Tokens" />.
        ///     </para>
        ///
        ///     <para>
        ///         If <c>i &lt; j</c>, then <c>Comparer.Compare(Tokens[Positions[i]], Tokens[Positions[j]]) &lt;= 0</c> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).
        ///     </para>
        /// </summary>
        /// <seealso cref="Tokens" />
        /// <seealso cref="Comparer" />
        protected IReadOnlyCollection<Int32> Positions => _positions;

        /// <summary>
        ///     <para>
        ///         String comparer used by the pen for comparing tokens.
        ///     </para>
        /// </summary>
        public StringComparer Comparer => _comparer;

        /// <summary>
        ///     <para>
        ///         Unsorted tokens of the pen. The order of tokens is kept as provided in the constructor.
        ///     </para>
        /// </summary>
        public IReadOnlyCollection<String?> Tokens => _tokens;

        /// <summary>
        ///     <para>
        ///         Ending token of the pen. This token (or any other comparing equal to it by <see cref="Comparer" />) shall never be rendered.
        ///     </para>
        /// </summary>
        public String? EndToken => _endToken;

        /// <summary>
        ///     <para>
        ///         Indicator of all tokens in <see cref="Tokens" /> being equal to <see cref="EndToken" /> as compared by <see cref="Comparer" />.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If <see cref="Tokens" /> is empty, <see cref="AllEnds" /> will be <c>true</c>. This coincides with mathematical logic of empty sets.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Tokens" />
        /// <seealso cref="EndToken" />
        /// <seealso cref="Comparer" />
        public Boolean AllEnds => _lazyAllEnds.Value;

        /// <summary>
        ///     <para>
        ///         Create a pen with provided values.
        ///     </para>
        /// </summary>
        /// <param name="tokens">Input tokens. Random text will be generated based on <paramref name="tokens" />: both by picking only from <paramref name="tokens" /> and by using the order of <paramref name="tokens" />.</param>
        /// <param name="comparer">String comparer. Tokens shall be compared by <paramref name="comparer" />.</param>
        /// <param name="endToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        public Pen(IEnumerable<String?> tokens, StringComparer comparer, String? endToken = null)
        {
            // Copy comparer and ending token.
            _comparer = comparer;
            _endToken = endToken;

            // Copy tokens.
            {
                var tokensList = new List<String?>(tokens);
                tokensList.TrimExcess();
                _tokens = tokensList.AsReadOnly();
            }


            // Find sorting positions of tokens.
            {
                var positionsList = Enumerable.Range(0, Tokens.Count).ToList();
                positionsList.Sort(
                    (i, j) =>
                    {
                        // Simplify check if indices are the same.
                        if (i == j)
                        {
                            return 0;
                        }

                        // Lexicographically check.
                        while (i < Tokens.Count && j < Tokens.Count)
                        {
                            // Extract current tokens.
                            var t1 = Tokens.ElementAt(i);
                            var t2 = Tokens.ElementAt(j);

                            // Compare tokens. If not equal, return the result.
                            int c = _comparer.Compare(t1, t2);
                            if (c != 0)
                            {
                                return c;
                            }

                            // Proceed to next tokens.
                            ++i;
                            ++j;
                        }

                        // The token that first reached the end is less by lexicographic order.
                        {
                            var counts = new Boolean[] { i == Tokens.Count, j == Tokens.Count };
                            if (counts.All(f => f))
                            {
                                return 0;
                            }
                            else if (counts[0])
                            {
                                return -1;
                            }
                            else if (counts[1])
                            {
                                return 1;
                            }
                        }

                        return 0;
                    }
                );
                positionsList.TrimExcess();
                _positions = positionsList.AsReadOnly();
            }

            // Lazily check if all tokens are ending tokens.
            _lazyAllEnds = new Lazy<Boolean>(() => !Tokens.Any() || Tokens.All(t => Comparer.Equals(t, EndToken)));
        }

        /// <summary>
        ///     <para>
        ///         Create a pen with provided values.
        ///     </para>
        ///
        ///     <para>
        ///         Tokens shall be compared by <see cref="StringComparer.InvariantCulture" />.
        ///     </para>
        /// </summary>
        /// <param name="tokens">Input tokens. Random text will be generated based on <paramref name="tokens" />: both by picking only from <paramref name="tokens" /> and by using the order of <paramref name="tokens" />.</param>
        /// <param name="endToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        public Pen(IEnumerable<String?> tokens, String? endToken = null) : this(tokens, StringComparer.InvariantCulture, endToken)
        {
        }

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text.
        ///     </para>
        ///
        ///     <para>
        ///         The first token is chosen by calling <see cref="picker" /> function. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <paramref name="picker" /> function. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="picker">Random number generator. When passed an integer <c>n</c> (<c>&gt;= 0</c>) as the argument, it should return an integer from range [0, max(<c>n</c>, 1)), i. e. greater than or equal to 0 but (strictly) less than max(<c>n</c>, 1).</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="relevantTokens" /> is (strictly) negative. If <paramref name="picker" /> returns a value outside of the legal range.</exception>
        /// <remarks>
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it (via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc.). If <paramref name="picker" /> is not a deterministic function, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This can be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query will immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <item>
        ///             the pen was constructed with an empty enumerable of tokens,
        ///         </item>
        ///         <item>
        ///             the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>subcase</em> of the first case),
        ///         </item>
        ///         <item>
        ///             a <em>successor</em> of the last token or an ending token is picked first.
        ///         </item>
        ///     </list>
        /// </remarks>
        public IEnumerable<String?> Render(int relevantTokens, Func<Int32, Int32> picker)
        {
            if (relevantTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relevantTokens), RelevantTokensOutOfRangeErrorMessage);
            }

            // Initialise the list of `relevantTokens` most recent tokens and its first position (the list will be cyclical after rendering `relevantTokens` tokens).
            var text = new List<String?>(Math.Max(relevantTokens, 1));
            int c = 0;

            // Render the first token.
            {
                while (!text.Any())
                {
                    int pick = picker(Tokens.Count + 1);
                    if (pick < 0 || pick > Tokens.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                    }

                    int first = pick == Tokens.Count ? Tokens.Count : Positions.ElementAt(pick);
                    var firstToken = first < Tokens.Count ? Tokens.ElementAt(first) : EndToken;
                    if (Comparer.Equals(firstToken, EndToken))
                    {
                        yield break;
                    }

                    text.Add(firstToken);
                    yield return text[0];
                }
            }

            // Loop rendering until over.
            while (true)
            {
                // Declare:
                int p; // the first position (index of `Positions`) of the most recent `relevantTokens` tokens rendered;                 0 if `relevantTokens == 0`
                int n; // the number of the most recent `relevantTokens` tokens rendered occurances in `Tokens`;                         `Tokens.Count` + 1 if `relevantTokens == 0`
                int d; // the distance (in number of tokens) between the first relevant token and the next to render (`relevantTokens`); 0 if `relevantTokens == 0`
                    // when `relevantTokens` tokens have not yet been rendered, `text.Count` is used as the number of relevant tokens, i. e. all rendered tokens are relevant

                // Find the values according to `relevantTokens`.
                if (relevantTokens == 0)
                {
                    p = 0;
                    n = Tokens.Count + 1;
                    d = 0;
                }
                else
                {
                    // Extract the first relevant token, find its first position `p` and initialise `n` to 0.
                    var t = text[c];
                    p = FindPosition(Comparer, Tokens, Positions, t);
                    n = 0;

                    // Find the actual value of index `p` and number `n`.
                    while (p + n < Tokens.Count)
                    {
                        // Extract the current position.
                        int i = Positions.ElementAt(p + n);

                        // Count the number of tokens equal to tokens in `text`, starting from `i` in `Tokens` until the end.
                        int k;
                        for (k = 0; i + k < Tokens.Count && k < text.Count; ++k)
                        {
                            if (!Comparer.Equals(Tokens.ElementAt(i + k), text[(c + k) % text.Count])) // <-- cyclicity of list `text`
                            {
                                break;
                            }
                        }

                        // Update values accordingly:
                        if (k == text.Count) // all `text.Count` (`relevantTokens`) are equal, meaning another occurance is found (increment `n`)
                        {
                            ++n;
                        }
                        else if (n == 0)     // no occurance is found, including the current index `i`, therefore proceed to the next candidate (increment `p`)
                        {
                            ++p;
                        }
                        else                 // after some occurances have been found, a mismatch is reached; as `Tokens` are sorted by `Positions`, no other occurance will be found
                        {
                            break;
                        }
                    }

                    // Set the value of `d` (note that `text` will never hold more than `relevantTokens` tokens).
                    d = text.Count;
                }

                // Render the next token.

                int pick = picker(n);
                if (pick < 0 || pick >= n)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                }

                int next = Positions.ElementAt(p + pick) + d;
                var nextToken = next < Tokens.Count ? Tokens.ElementAt(next) : EndToken;
                if (Comparer.Equals(nextToken, EndToken))
                {
                    yield break;
                }

                if (text.Count < text.Capacity)
                {
                    text.Add(nextToken);
                    yield return text[^1];
                }
                else
                {
                    text[c] = nextToken;
                    yield return text[c];
                    c = (c + 1) % text.Count; // <-- cyclicity of list `text`
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text.
        ///     </para>
        ///
        ///     <para>
        ///         The first token is chosen by calling <see cref="Random.Next(Int32)" /> method of <paramref name="random" />. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="Random.Next(Int32)" /> method of <paramref name="random" />. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="random">(Pseudo-)Random number generator.</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="relevantTokens" /> is (strictly) negative.</exception>
        /// <remarks>
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it (via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc.). Since the point of <see cref="Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This can be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query will immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <item>
        ///             the pen was constructed with an empty enumerable of tokens,
        ///         </item>
        ///         <item>
        ///             the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>subcase</em> of the first case),
        ///         </item>
        ///         <item>
        ///             a <em>successor</em> of the last token or an ending token is picked first.
        ///         </item>
        ///     </list>
        /// </remarks>
        public IEnumerable<String?> Render(int relevantTokens, Random random) =>
            Render(relevantTokens, n => random.Next(n));
    }
}
