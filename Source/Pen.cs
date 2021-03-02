using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Random text generator.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the pen should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable <c>context</c> passed to the constructor. To prevent overflowing from one source to another (e. g. if the last token from the first source is not a contextual predecessor of the first token from the second source), an ending token (<see cref="EndToken" />) should be put between the sources' tokens in the final enumerable <c>tokens</c>. Choosing the ending token in <see cref="Render(Int32, Func{Int32, Int32})" /> or <see cref="Render(Int32, Random)" /> methods will cause the rendering to stop—the same as when a successor of the last entry in tokens is chosen.
    ///     </para>
    ///
    ///     <para>
    ///         A complete deep copy of enumerable <c>context</c> (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large; although memory usage and even time consumption (if there are many identical duplicates amongst tokens in <c>context</c>) may be reduced by passing <c>true</c> as parameter <c>intern</c> to constructor(s), be aware of other side effects of string interning via <see cref="String.Intern(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Changing any of the properties—public or protected—breaks the functionality of the pen. This includes, but is not limited to, manually changing the behaviour of string comparer <see cref="Comparer" />. By doing so, behaviour of <see cref="Render(Int32, Func{Int32, Int32})" /> and <see cref="Render(Int32, Random)" /> methods is unexpected and no longer guaranteed.
    ///     </para>
    /// </remarks>
    public class Pen
    {
        private const string RelevantTokensOutOfRangeErrorMessage = "Number of relevant tokens must be non-negative (greater than or equal to 0).";
        private const string PickOutOfRangeErrorMessage = "Picker function must return an integer from [0, n) union {0}.";

        private static readonly Object _Locker;
        private static Int32 _RandomSeed;

        [ThreadStatic]
        private static System.Random? _Random;

        /// <summary>
        ///     <para>
        ///         Internal locking object of <see cref="Pen" /> class.
        ///     </para>
        /// </summary>
        private static Object Locker => _Locker;

        /// <summary>
        ///     <para>
        ///         Seed for the internal (pseudo-)random number generator (<see cref="Random" />).
        ///     </para>
        /// </summary>
        /// <value>New seed. If <c>value</c> is less than or equal to <c>0</c>, new seed is set to <c>1</c>.</value>
        /// <remarks>
        ///     <para>
        ///         The current value does not necessarily indicate the seeding value or the random state of the internal number generator if the number generator has already been instantiated and used and/or the value has changed.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Random" />
        private static Int32 RandomSeed
        {
            get
            {
                lock (Locker)
                {
                    return _RandomSeed;
                }
            }

            set
            {
                lock (Locker)
                {
                    _RandomSeed = Math.Max(value, 1);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Internal (pseudo-)random number generator.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The number generator is thread safe (actually, each thread has its own instance) and instances across multiple threads are seeded differently. However, instancess across multiple processes initiated at approximately the same time could be seeded with the same value. Therefore the main purpose of the number generator is to provide a virtually unpredictable (to an unconcerned human user) implementation of <see cref="Render(Int32, Boolean)" /> for a single process without having to provide a custom number generator (a <see cref="Func{T, TResult}" /> function or a <see cref="System.Random" /> object); no other properties are guaranteed.
        ///     </para>
        /// </remarks>
        protected static System.Random Random
        {
            get
            {
                lock (Locker)
                {
                    if (_Random is null)
                    {
                        unchecked
                        {
                            _Random = new Random(RandomSeed++);
                        }
                    }
                }

                return _Random;
            }
        }

        /// <summary>
        ///     <para>
        ///         (Pseudo-)Randomly choose a number in the specified range.
        ///     </para>
        /// </summary>
        /// <param name="n">Upper limit of the value to choose.</param>
        /// <returns>A number greater than or equal to <c>0</c> but (strictly) less than <paramref name="n" />, i. e. a number from range [<c>0</c>, <paramref name="n" />). However, if <paramref name="n" /> is less than or equal to <c>0</c>, <c>0</c> is returned.</returns>
        /// <remarks>
        ///     <para>
        ///         This method is intended to be used in <see cref="Render(Int32, Func{Int32, Int32}, Boolean)" /> method as parameter <c>picker</c> to randomly choose tokens. In fact, <see cref="Render(Int32, Boolean)" /> method overload is implemented this way.
        ///     </para>
        ///
        ///     <para>
        ///         Unlike <see cref="System.Random.Next(Int32)" />, this method does not throw an <see cref="ArgumentOutOfRangeException" /> if the argument is negative—it simply returns <c>0</c> instead. Moreover, if <paramref name="n" /> is strictly negative, no method of the internal (pseudo-)random number generator (<see cref="Random" />) is invoked hence its random state remains unchanged.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Random" />
        /// <seealso cref="Render(Int32, Func{Int32, Int32}, Boolean)" />
        /// <seealso cref="Render(Int32, System.Random, Boolean)" />
        /// <seealso cref="Render(Int32, Boolean)" />
        protected int RandomPicker(int n) =>
            n < 0 ? 0 : Random.Next(n);

        static Pen()
        {
            _Locker = new Object();

            lock (Locker)
            {
                unchecked
                {
                    _RandomSeed = Math.Max((Int32)((1073741827L * DateTime.UtcNow.Ticks + 1073741789L) & (Int64)Int32.MaxValue), 1);
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Retrieve the index of a value.
        ///     </para>
        /// </summary>
        /// <param name="values">List of values amongst which <paramref name="x" /> should be found.</param>
        /// <param name="x">Value to find.</param>
        /// <returns>Minimal index <c>i</c> such that <c>values[i] == x</c>, or <c>-1</c> if <paramref name="x" /> is not found amongst <paramref name="values" /> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).</returns>
        protected static int IndexOf(IEnumerable<Int32> values, int x)
        {
            for (var (en, i) = ValueTuple.Create(values.GetEnumerator(), 0); en.MoveNext(); ++i)
            {
                if (en.Current == x)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     <para>
        ///         Compare a subrange of <paramref name="tokens" /> with a sample of tokens <paramref name="sample" /> in respect of <paramref name="comparer" />.
        ///     </para>
        /// </summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens whose subrange is compared to <paramref name="sample" />.</param>
        /// <param name="sample">Cyclical sample list of tokens. The list represents range <c>{ sample[cycleStart], sample[cycleStart + 1], ..., sample[^1], sample[0], ..., sample[cycleStart - 1] }</c>.</param>
        /// <param name="i">Starting index of the subrange from <paramref name="tokens" /> to compare. The subrange <c>{ tokens[i], tokens[i + 1], ..., tokens[Math.Min(i + sample.Count - 1, tokens.Count - 1)] }</c> is used (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sample" />.</param>
        /// <returns>A signed integer that indicates the relative values of subrange from <paramref name="tokens" /> starting from <paramref name="i" /> and cyclical sample <paramref name="sample" />.</returns>
        /// <remarks>
        ///     <para>
        ///         Values from the subrange of <paramref name="tokens" /> and <paramref name="sample" /> are compared in order by calling <see cref="StringComparer.Compare(String, String)" /> method on <paramref name="comparer" />. If a comparison yields a non-zero value, it is returned. If the subrange from <paramref name="tokens" /> is shorter (in the number of tokens) than <paramref name="sample" /> but all of its tokens compare equal to tokens from the beginning of <paramref name="sample" />, a negative number is returned. If all tokens compare equal and the subrange from <paramref name="tokens" /> is the same length (in the number of tokens) as <paramref name="sample" />, <c>0</c> is returned.
        ///     </para>
        /// </remarks>
        private static int CompareRange(StringComparer comparer, IReadOnlyCollection<String?> tokens, IList<String?> sample, int i, int cycleStart)
        {
            int c = 0;

            int j;

            for (/* [`i` is set in function call,] */ j = 0; i < tokens.Count && j < sample.Count; ++i, ++j)
            {
                c = comparer.Compare(tokens.ElementAt(i), sample[(cycleStart + j) % sample.Count]);

                if (c != 0)
                {
                    break;
                }
            }

            if (c == 0 && i == tokens.Count && j < sample.Count)
            {
                c = -1;
            }

            return c;
        }

        /// <summary>
        ///     <para>
        ///         Find the first position index and the number of occurances of <paramref name="sample" /> in <paramref name="tokens" /> sorted by <paramref name="positions" />.
        ///     </para>
        /// </summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens amongst which <paramref name="sample" /> should be found.</param>
        /// <param name="positions">Sorted positions, or positional ordering of <paramref name="tokens" /> in respect of <paramref name="comparer" />.</param>
        /// <param name="sample">Cyclical sample list of tokens to find. The list represents range <c>{ sample[cycleStart], sample[cycleStart + 1], ..., sample[^1], sample[0], ..., sample[cycleStart - 1] }</c>.</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sample" />.</param>
        /// <returns>The minimal index <c>i</c> such that an occurance of <paramref name="sample" /> begins at <c>tokens[positions[i]]</c> and the total number of occurances amongst <paramref name="tokens" /> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).</returns>
        /// <remarks>
        ///     <para>
        ///         The implementation of the method assumes <paramref name="sample" /> actually exists (as compared by <paramref name="comparer" />) amongst <paramref name="tokens" /> and that <paramref name="positions" /> indeed sort <paramref name="tokens" /> ascendingly in respect of <paramref name="comparer" />. If the former is not true, the returned index shall point to the position at which <paramref name="t" />'s position should be inserted to retain the sorted order and the number of occurances shall be 0; if the latter is not true, the behaviour of the method is undefined.
        ///     </para>
        /// </remarks>
        private static ValueTuple<Int32, Int32> FindPositionIndexAndCount(StringComparer comparer, IReadOnlyCollection<String?> tokens, IReadOnlyCollection<Int32> positions, IList<String?> sample, int cycleStart)
        {
            // Binary search...

            // Initialise lower, upper and middle positions.
            int l = 0;
            int h = tokens.Count;
            int m = h >> 1;

            // Loop until found.
            {
                while (l < h)
                {
                    // Compare ranges.
                    int c = CompareRange(comparer, tokens, sample, positions.ElementAt(m), cycleStart);

                    // Break the loop or update positions.
                    if (c == 0)
                    {
                        l = m;
                        h = m + 1;

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
                    m = (l + h) >> 1;
                }
            }

            // Find the minimal position index `l` and the maximal index `h` of occurances of `sample` amongst `tokens`.
            while (l > 0 && CompareRange(comparer, tokens, sample, positions.ElementAt(l - 1), cycleStart) == 0)
            {
                --l;
            }
            while (h < tokens.Count && CompareRange(comparer, tokens, sample, positions.ElementAt(h), cycleStart) == 0)
            {
                ++h;
            }

            // Return the computed values.
            return ValueTuple.Create(l, h - l);
        }

        private readonly StringComparer _comparer;
        private readonly ReadOnlyCollection<String?> _context;
        private readonly ReadOnlyCollection<Int32> _positions;
        private readonly String? _endToken;
        private readonly Int32 _firstPosition;
        private readonly Boolean _allEnds;

        /// <summary>
        ///     <para>
        ///         String comparer used by the pen for comparing tokens.
        ///     </para>
        /// </summary>
        protected StringComparer Comparer => _comparer;

        /// <summary>
        ///     <para>
        ///         Sorting positions of entries in <see cref="Context" />. If <c>i &lt; j</c>, then <c>Comparer.Compare(Context[Positions[i]], Context[Positions[j]]) &lt;= 0</c> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).
        ///     </para>
        /// </summary>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        protected IReadOnlyCollection<Int32> Positions => _positions;

        /// <summary>
        ///     <para>
        ///         Position (index of <see cref="Positions" />) of the first non-ending token (<see cref="EndToken" />) in <see cref="Context" />. If such a token does not exist, the value is <see cref="IReadOnlyCollection{T}.Count" /> of <see cref="Context" />.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This position index points to the position of the <strong>actual</strong> first non-ending token (<see cref="EndToken" />) in <see cref="Context" />, even though there may exist other tokens comparing equal to it in respect of <see cref="Comparer" />. Hence <c>{ Positions[FirstPosition], Positions[FirstPosition] + 1, Positions[FirstPosition] + 2, ... }</c> enumerates <see cref="Context" /> from the beginning by ignoring potential initial ending tokens (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" />).
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        /// <seealso cref="EndToken" />
        /// <seealso cref="Positions" />
        protected Int32 FirstPosition => _firstPosition;

        /// <summary>
        ///     <para>
        ///         Unsorted tokens of the pen. The order of tokens is kept as provided in the constructor.
        ///     </para>
        /// </summary>
        public IReadOnlyCollection<String?> Context => _context;

        /// <summary>
        ///     <para>
        ///         Ending token of the pen.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This token (or any other comparing equal to it by <see cref="Comparer" />) shall never be rendered.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Comparer" />
        public String? EndToken => _endToken;

        /// <summary>
        ///     <para>
        ///         Indicator of all tokens in <see cref="Context" /> being equal to <see cref="EndToken" /> (as compared by <see cref="Comparer" />).
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If <see cref="Context" /> is empty, <see cref="AllEnds" /> is <c>true</c>. This coincides with mathematical logic of empty sets.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="EndToken" />
        /// <seealso cref="Comparer" />
        public Boolean AllEnds => _allEnds;

        /// <summary>
        ///     <para>
        ///         Create a pen with provided values.
        ///     </para>
        ///
        ///     <para>
        ///         Tokens shall be compared by <see cref="StringComparer.Ordinal" />.
        ///     </para>
        /// </summary>
        /// <param name="context">Input tokens. Random text shall be generated based on <paramref name="context" />: both by picking only from <paramref name="context" /> and by using the order of <paramref name="context" />.</param>
        /// <param name="endToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        /// <param name="intern">If <c>true</c>, tokens from <paramref name="context" /> shall be interned (via <see cref="String.Intern(String)" /> method) when being copied into the internal pen's container (<see cref="Context" />).</param>
        public Pen(IEnumerable<String?> context, String? endToken = null, bool intern = false) : this(context, endToken, StringComparer.Ordinal, intern)
        {
        }

        /// <summary>
        ///     <para>
        ///         Create a pen with provided values.
        ///     </para>
        /// </summary>
        /// <param name="context">Input tokens. Random text shall be generated based on <paramref name="context" />: both by picking only from <paramref name="context" /> and by using the order of <paramref name="context" />.</param>
        /// <param name="endToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        /// <param name="comparer">String comparer. Tokens shall be compared by <paramref name="comparer" />.</param>
        /// <param name="intern">If <c>true</c>, tokens from <paramref name="context" /> shall be interned (via <see cref="String.Intern(String)" /> method) when being copied into the internal pen's container (<see cref="Context" />).</param>
        public Pen(IEnumerable<String?> context, String? endToken, StringComparer comparer, bool intern = false)
        {
            // Copy comparer and ending token.
            _comparer = comparer;
            _endToken = endToken;

            // Copy context.
            {
                if (intern)
                {
                    context = context.Select(t => t is null ? null : String.Intern(t));
                }
                var contextList = context.ToList();
                contextList.TrimExcess();
                _context = contextList.AsReadOnly();
            }


            // Find sorting positions of tokens in context.
            {
                var positionsList = Enumerable.Range(0, Context.Count).ToList();
                positionsList.Sort(
                    (i, j) =>
                    {
                        // Simplify check if indices are the same.
                        if (i == j)
                        {
                            return 0;
                        }

                        // Lexicographically check.
                        while (i < Context.Count && j < Context.Count)
                        {
                            // Extract current tokens.
                            var t1 = Context.ElementAt(i);
                            var t2 = Context.ElementAt(j);

                            // Compare tokens. If not equal, return the result.
                            int c = Comparer.Compare(t1, t2);
                            if (c != 0)
                            {
                                return c;
                            }

                            // Proceed to next tokens.
                            ++i;
                            ++j;
                        }

                        // Compare indices in the end (the larger index is in fact `Context.Count` hence reached the end earlier).
                        return j.CompareTo(i);
                    }
                );
                positionsList.TrimExcess();
                _positions = positionsList.AsReadOnly();
            }

            // Find the position of the first (non-ending) token.
            try
            {
                _firstPosition = Context.Select((t, p) => Comparer.Equals(t, EndToken) ? -1 : IndexOf(Positions, p)).Where(i => i != -1).First();
            }
            catch (InvalidOperationException)
            {
                _firstPosition = Context.Count;
            }

            // Check if all tokens are ending tokens.
            _allEnds = (FirstPosition == Context.Count);
        }

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text from <see cref="Context" />.
        ///     </para>
        ///
        ///     <para>
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen internally; otherwise they are chosen by calling <see cref="picker" /> function. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <paramref name="picker" /> function. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="picker">Random number generator. When passed an integer <c>n</c> (<c>&gt;= 0</c>) as the argument, it should return an integer from range [0, max(<c>n</c>, 1)), i. e. greater than or equal to 0 but (strictly) less than max(<c>n</c>, 1).</param>
        /// <param name="fromBeginning">If <c>true</c>, <paramref name="picker" /> function is not called to choose the first max(<paramref name="relevantTokens" />, 1) tokens, but the beginning of the pen's context is chosen instead; otherwise the first token is chosen by immediately calling <paramref name="picker" /> function.</param>
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
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>subcase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Random, Boolean)" />
        /// <seealso cref="Render(Int32, Boolean)" />
        public IEnumerable<String?> Render(int relevantTokens, Func<Int32, Int32> picker, bool fromBeginning = false)
        {
            if (relevantTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relevantTokens), RelevantTokensOutOfRangeErrorMessage);
            }

            // Initialise the list of `relevantTokens` most recent tokens and its first position (the list will be cyclical after rendering `relevantTokens` tokens).
            var text = new List<String?>(Math.Max(relevantTokens, 1));
            int c = 0;

            // Render the first token, or the first `relevantTokens` if needed.
            if (fromBeginning)
            {
                for (var (next, i) = ValueTuple.Create(AllEnds ? Context.Count : Positions.ElementAt(FirstPosition), 0); i < text.Capacity; ++next, ++i)
                {
                    var nextToken = next < Context.Count ? Context.ElementAt(next) : EndToken;
                    if (Comparer.Equals(nextToken, EndToken))
                    {
                        yield break;
                    }

                    yield return nextToken;
                    text.Add(nextToken);
                }
            }
            else
            {
                int pick = fromBeginning ? FirstPosition : picker(Context.Count + 1);
                if (pick < 0 || pick > Context.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                }

                int first = pick == Context.Count ? Context.Count : Positions.ElementAt(pick);
                var firstToken = first < Context.Count ? Context.ElementAt(first) : EndToken;
                if (Comparer.Equals(firstToken, EndToken))
                {
                    yield break;
                }

                yield return firstToken;
                text.Add(firstToken);
            }

            // Loop rendering until over.
            while (true)
            {
                // Declare:
                int p; // the first position (index of `Positions`) of the most recent `relevantTokens` tokens rendered;                 0 if `relevantTokens == 0`
                int n; // the number of the most recent `relevantTokens` tokens rendered occurances in `Tokens`;                         `Tokens.Count` + 1 if `relevantTokens == 0`
                int d; // the distance (in number of tokens) between the first relevant token and the next to render (`relevantTokens`); 0 if `relevantTokens == 0`
                    // until `relevantTokens` tokens have not yet been rendered, `text.Count` is used as the number of relevant tokens, i. e. all rendered tokens are relevant

                // Find the values according to `relevantTokens`.
                if (relevantTokens == 0)
                {
                    p = 0;
                    n = Context.Count + 1;
                    d = 0;
                }
                else
                {
                    (p, n) = FindPositionIndexAndCount(Comparer, Context, Positions, text, c);
                    d = text.Count; // note that `text` shall never hold more than `relevantTokens` tokens
                }

                // Render the next token.

                int pick = picker(n);
                if (pick < 0 || pick >= n)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                }

                int next = Positions.ElementAt(p + pick) + d;
                var nextToken = next < Context.Count ? Context.ElementAt(next) : EndToken;
                if (Comparer.Equals(nextToken, EndToken))
                {
                    yield break;
                }

                if (text.Count < text.Capacity)
                {
                    yield return nextToken;
                    text.Add(nextToken); 
                }
                else
                {
                    yield return nextToken;
                    text[c] = nextToken;
                    c = (c + 1) % text.Count; // <-- cyclicity of list `text`
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text from <see cref="Context" />.
        ///     </para>
        ///
        ///     <para>
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen internally; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="random">(Pseudo-)Random number generator.</param>
        /// <param name="fromBeginning">If <c>true</c>, <paramref name="picker" /> function is not called to choose the first max(<paramref name="relevantTokens" />, 1) tokens, but the beginning of the pen's context is chosen instead; otherwise the first token is chosen by immediately calling <paramref name="picker" /> function.</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="relevantTokens" /> is (strictly) negative.</exception>
        /// <remarks>
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         If no specific <see cref="System.Random" /> object or seed should be used, <see cref="Render(Int32, Boolean)" /> method may be invoked instead.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it (via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc.). Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>subcase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Func{Int32, Int32}, Boolean)" />
        /// <seealso cref="Render(Int32, Boolean)" />
        public IEnumerable<String?> Render(int relevantTokens, System.Random random, bool fromBeginning = false) =>
            Render(relevantTokens, n => random.Next(n), fromBeginning);

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text from <see cref="Context" />.
        ///     </para>
        ///
        ///     <para>
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first max(<paramref name="relevantTokens" />, 1) tokens are chosen internally; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of an internal <see cref="System.Random" /> object (<see cref="System.Random" />). Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of the internal <see cref="System.Random" /> object. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        ///
        ///     
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="fromBeginning">If <c>true</c>, <paramref name="picker" /> function is not called to choose the first max(<paramref name="relevantTokens" />, 1) tokens, but the beginning of the pen's context is chosen instead; otherwise the first token is chosen by immediately calling <paramref name="picker" /> function.</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="relevantTokens" /> is (strictly) negative.</exception>
        /// <remarks>
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         Calling this method is essentially the same (reproducibility aside) as calling <see cref="Render(Int32, Random, Boolean)" /> by providing an internal <see cref="System.Random" /> object (<see cref="System.Random" />) as the parameter <c>random</c>. If no specific <see cref="System.Random" /> object or seed should be used, this method will suffice.
        ///     </para>
        ///
        ///     <para>
        ///         The query returned is not run until enumerating it (via explicit calls to <see cref="IEnumerable{T}.GetEnumerator" /> method, a <c>foreach</c> loop, a call to <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc.). Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.
        ///     </para>
        ///
        ///     <para>
        ///         It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as a <see cref="List{T}" />, or concatenated together into a string to avoid memory errors. This may be done by calling <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.
        ///     </para>
        ///
        ///     <para>
        ///         The query shall immediately stop, without rendering any tokens, if:
        ///     </para>
        ///
        ///     <list type="number">
        ///         <listheader>
        ///             <term>
        ///                 case
        ///             </term>
        ///             <description>
        ///                 description
        ///             </description>
        ///         </listheader>
        ///         <item>
        ///             <term>
        ///                 no tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an empty enumerable of tokens,
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 all ending tokens
        ///             </term>
        ///             <description>
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>subcase</em> of the first case),
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>
        ///                 by choice
        ///             </term>
        ///             <description>
        ///                 a <em>successor</em> of the last token or an ending token is picked first.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <seealso cref="Render(Int32, Func{Int32, Int32}, Boolean)" />
        /// <seealso cref="Render(Int32, Random, Boolean)" />
        public IEnumerable<String?> Render(int relevantTokens, bool fromBeginning = false) =>
            Render(relevantTokens, RandomPicker, fromBeginning);
    }
}
