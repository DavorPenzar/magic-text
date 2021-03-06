using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Random text generator.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the pen should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable <c>context</c> passed to the constructor. To prevent overflowing from one source to another (e. g. if the last token from the first source is not a contextual predecessor of the first token from the second source), an ending token (<see cref="EndToken" />) should be put between the sources' tokens in the final enumerable <c>tokens</c>. Choosing an ending token in <see cref="Render(Int32, Func{Int32, Int32}, Boolean)" />, <see cref="Render(Int32, Random, Boolean)" /> or <see cref="Render(Int32, Boolean)" /> methods shall cause the rendering to stop—the same as when a successor of the last entry in tokens is chosen.
    ///     </para>
    ///
    ///     <para>
    ///         A complete deep copy of enumerable <c>context</c> (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large. To reduce memory usage and even time consumption, <c>true</c> may be passed as parameter <c>intern</c> to constructor(s); however, be aware of other side effects of string interning via <see cref="String.Intern(String)" /> method.
    ///     </para>
    ///
    ///     <para>
    ///         Changing any of the properties—public or protected—breaks the functionality of the pen. By doing so, behaviour of <see cref="Render(Int32, Func{Int32, Int32}, Boolean)" />, <see cref="Render(Int32, Random, Boolean)" /> and <see cref="Render(Int32, Boolean)" /> methods is unexpected and no longer guaranteed.
    ///     </para>
    /// </remarks>
    public class Pen
    {
        private const string ValuesNullErrorMessage = "Values' enumerable may not be `null`.";
        private const string ContextNullErrorMessage = "Context tokens' enumerable may not be `null`.";
        private const string PickerNullErrorMessage = @"""Random"" number generator function (picker function) may not be `null`.";
        private const string RandomNullErrorMessage = "(Pseudo-)Random number generator may not be `null`.";
        private const string RelevantTokensOutOfRangeErrorMessage = "Number of relevant tokens must be non-negative (greater than or equal to 0).";
        private const string PickOutOfRangeErrorMessage = "Picker function must return an integer from [0, n) including 0 (even if `n == 0`).";

        private static readonly Object _Locker;
        private static Int32 _RandomSeed;

        [ThreadStatic]
        private static System.Random? _Random;

        /// <returns>Internal locking object of <see cref="Pen" /> class.</returns>
        private static Object Locker => _Locker;

        /// <returns>Seed for the internal (pseudo-)random number generator (<see cref="Random" />).</returns>
        /// <value>New seed. If <c>value</c> is less than or equal to <c>0</c>, new seed is set to <c>1</c>.</value>
        /// <remarks>
        ///     <para>
        ///         The current value does not necessarily indicate the seeding value or the random state of the internal number generator. The dissonance may appear if the number generator has already been instantiated and used or if the value of <see cref="RandomSeed" /> has changed.
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

        /// <returns>Internal (pseudo-)random number generator.</returns>
        /// <remarks>
        ///     <para>
        ///         The number generator is thread safe (actually, each thread has its own instance) and instances across multiple threads are seeded differently. However, instancess across multiple processes initiated at approximately the same time could be seeded with the same value. Therefore the main purpose of the number generator is to provide a virtually unpredictable (to an unconcerned human user) implementation of <see cref="Render(Int32, Boolean)" /> for a single process without having to provide a custom number generator (a <see cref="Func{T, TResult}" /> function or a <see cref="System.Random" /> object); no other properties are guaranteed.
        ///     </para>
        ///
        ///     <para>
        ///         If the code is expected to spread over multiple threads (either explicitly by starting <see cref="Thread" /> instances or implicitly by programming asynchronously), reference returned by <see cref="Random" /> property should not be stored in an outside variable and used later. Directly reference property <see cref="Random" /> in such scenarios.
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

                    return _Random;
                }
            }
        }

        /// <summary>
        ///     <para>
        ///         Initialise static fields.
        ///     </para>
        /// </summary>
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
        /// <param name="values">Enumerable of values amongst which <paramref name="x" /> should be found. Even if <paramref name="values" /> are comparable, the enumerable does not have to be sorted.</param>
        /// <param name="x">Value to find.</param>
        /// <param name="comparer">Comparer used for comparing instances of type <typeparamref name="T" /> for equality. If <c>null</c>, <see cref="EqualityComparer{T}.Default" /> is used.</param>
        /// <returns>Minimal index <c>i</c> such that <c><paramref name="values" />[i] == <paramref name="x" /></c>, or <c>-1</c> if <paramref name="x" /> is not found amongst <paramref name="values" /> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" /> method calls).</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="values" /> is <c>null</c>.</exception>
        protected static Int32 IndexOf<T>(IEnumerable<T> values, T x, IEqualityComparer<T>? comparer = null)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values), ValuesNullErrorMessage);
            }

            if (comparer is null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            int index = -1;

            for (var (en, i) = ValueTuple.Create(values.GetEnumerator(), 0); en.MoveNext(); ++i)
            {
                if (comparer.Equals(en.Current, x))
                {
                    index = i;

                    break;
                }
            }

            return index;
        }

        /// <summary>
        ///     <para>
        ///         Compare a subrange of <paramref name="tokens" /> with a sample of tokens <paramref name="sample" /> in respect of <paramref name="comparer" />.
        ///     </para>
        /// </summary>
        /// <param name="comparer">String comparer used for comparing.</param>
        /// <param name="tokens">List of tokens whose subrange is compared to <paramref name="sample" />.</param>
        /// <param name="sample">Cyclical sample list of tokens. The list represents range <c>{ <paramref name="sample" />[<paramref name="cycleStart" />], <paramref name="sample" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sample" />[<paramref name="sample" />.Count - 1], <paramref name="sample" />[0], ..., <paramref name="sample" />[<paramref name="cycleStart" /> - 1] }</c>. The list is treated as <strong>read-only</strong> in the function (it is not changed).</param>
        /// <param name="i">Starting index of the subrange from <paramref name="tokens" /> to compare. The subrange <c>{ <paramref name="tokens" />[i], <paramref name="tokens" />[i + 1], ..., <paramref name="tokens" />[min(i + <paramref name="sample" />.Count - 1, <paramref name="tokens" />.Count - 1)] }</c> is used.</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sample" />.</param>
        /// <returns>A signed integer that indicates the relative values of subrange from <paramref name="tokens" /> starting from <paramref name="i" /> and cyclical sample <paramref name="sample" />.</returns>
        /// <remarks>
        ///     <para>
        ///         Values from the subrange of <paramref name="tokens" /> and <paramref name="sample" /> are compared in order by calling <see cref="StringComparer.Compare(String, String)" /> method on <paramref name="comparer" />. If a comparison yields a non-zero value, it is returned. If the subrange from <paramref name="tokens" /> is shorter (in the number of tokens) than <paramref name="sample" /> but all of its tokens compare equal to corresponding tokens from the beginning of <paramref name="sample" />, a negative number is returned. If all tokens compare equal and the subrange from <paramref name="tokens" /> is the same length (in the number of tokens) as <paramref name="sample" />, <c>0</c> is returned.
        ///     </para>
        /// </remarks>
        private static Int32 CompareRange(StringComparer comparer, ReadOnlyCollection<String?> tokens, IList<String?> sample, Int32 i, Int32 cycleStart)
        {
            int c = 0;

            int j;

            for (/* [`i` is set in function call,] */ j = 0; i < tokens.Count && j < sample.Count; ++i, ++j)
            {
                c = comparer.Compare(tokens[i], sample[(cycleStart + j) % sample.Count]);
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
        /// <param name="sample">Cyclical sample list of tokens to find. The list represents range <c>{ <paramref name="sample" />[<paramref name="cycleStart" />], <paramref name="sample" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sample" />[<paramref name="sample" />.Count - 1], <paramref name="sample" />[0], ..., <paramref name="sample" />[<paramref name="cycleStart" /> - 1] }</c>.  The list is treated as <strong>read-only</strong> in the function (it is not changed).</param>
        /// <param name="cycleStart">Starting index of the cycle in <paramref name="sample" />.</param>
        /// <returns>The minimal index <c>i</c> such that an occurance of <paramref name="sample" /> begins at <c><paramref name="tokens" />[<paramref name="positions" />[i]]</c> and the total number of its occurances amongst <paramref name="tokens" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The implementation of the method assumes <paramref name="sample" /> actually exists (as compared by <paramref name="comparer" />) amongst <paramref name="tokens" /> and that <paramref name="positions" /> indeed sort <paramref name="tokens" /> ascendingly in respect of <paramref name="comparer" />. If the former is not true, the returned index shall point to the position at which <paramref name="t" />'s position should be inserted to retain the sorted order and the number of occurances shall be 0; if the latter is not true, the behaviour of the method is undefined.
        ///     </para>
        /// </remarks>
        private static ValueTuple<Int32, Int32> FindPositionIndexAndCount(StringComparer comparer, ReadOnlyCollection<String?> tokens, ReadOnlyCollection<Int32> positions, IList<String?> sample, Int32 cycleStart)
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
                    int c = CompareRange(comparer, tokens, sample, positions[m], cycleStart);

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
            while (l > 0 && CompareRange(comparer, tokens, sample, positions[l - 1], cycleStart) == 0)
            {
                --l;
            }
            while (h < tokens.Count && CompareRange(comparer, tokens, sample, positions[h], cycleStart) == 0)
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

        /// <returns>String comparer used by the pen for comparing tokens.</returns>
        protected StringComparer Comparer => _comparer;

        /// <returns>Sorting positions of entries in <see cref="Context" />: if <c>i &lt; j</c>, then <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Positions" />[i]], <see cref="Context" />[<see cref="Positions" />[j]]) &lt;= 0</c>.</returns>
        /// <remarks>
        ///     <para>
        ///         The order is actually determined by the complete sequence of tokens, and not just by single tokens. For instance, if <c>i != j</c>, but <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Positions" />[i]], <see cref="Context" />[<see cref="Positions" />[j]]) == 0</c> (<c><see cref="Comparer" />.Equals(<see cref="Context" />[<see cref="Positions" />[i]], <see cref="Context" />[<see cref="Positions" />[j]])</c>), then the result of <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Positions" />[i] + 1], <see cref="Context" />[<see cref="Positions" />[j] + 1])</c> is used; if it also evaluates to <c>0</c>, <c><see cref="Positions" />[i] + 2</c> and <c><see cref="Positions" />[j] + 2</c> are checked, and so on. The first position to reach the end (when <c>max(<see cref="Positions" />[i] + n, <see cref="Positions" />[j] + n) == <see cref="Context" />.Count</c> for a non-negative integer <c>n</c>), if all previous positions compared equal, is considered less. Hence <see cref="Positions" /> define a total (linear) lexicographic ordering of <see cref="Context" /> in respect of <see cref="Comparer" />, which may be useful in search algorithms, such as binary search, for finding the position of any non-empty finite sequence of tokens.
        ///     </para>
        ///
        ///     <para>
        ///         The position(s) of ending tokens (<see cref="EndToken" />), if there are any in <see cref="Context" />, are not fixed nor guaranteed (for instance, their positions are not necessarily at the beginning or the end of <see cref="Positions" />). If the ending token is a <c>null</c> or an empty string (<see cref="String.Empty" />), then ending tokens shall be compared less than any other tokens, provided there are no <c>null</c>s in <see cref="Context" /> in the latter case. But, generally speaking, ending tokens' positions are determined by <see cref="Comparer" /> and the values of other tokens in <see cref="Context" />.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        protected ReadOnlyCollection<Int32> Positions => _positions;

        /// <returns>Position (index of <see cref="Positions" />) of the first non-ending token (<see cref="EndToken" />) in <see cref="Context" />. If such a token does not exist, the value is the total number of elements in <see cref="Context" /> (<see cref="IReadOnlyCollection{T}.Count" />).</returns>
        /// <remarks>
        ///     <para>
        ///         This position index points to the position of the <strong>actual</strong> first non-ending token (<see cref="EndToken" />) in <see cref="Context" />, even though there may exist other tokens comparing equal to it in respect of <see cref="Comparer" />. Hence <c>{ <see cref="Positions" />[<see cref="FirstPosition" />], <see cref="Positions" />[<see cref="FirstPosition" />] + 1, ..., <see cref="Positions" />[<see cref="FirstPosition" />] + n, ... }</c> enumerates <see cref="Context" /> from the beginning by ignoring potential initial ending tokens.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Context" />
        /// <seealso cref="Comparer" />
        /// <seealso cref="EndToken" />
        /// <seealso cref="Positions" />
        protected Int32 FirstPosition => _firstPosition;

        /// <returns>Tokens of the pen.</returns>
        /// <remarks>
        ///     <para>
        ///         The order of tokens is kept as provided in the constructor.
        ///     </para>
        /// </remarks>
        public ReadOnlyCollection<String?> Context => _context;

        /// <returns>Ending token of the pen.</returns>
        /// <remarks>
        ///     <para>
        ///         This token (or any other comparing equal to it by <see cref="Comparer" />) shall never be rendered.
        ///     </para>
        /// </remarks>
        /// <seealso cref="Comparer" />
        public String? EndToken => _endToken;

        /// <returns>Indicator of all tokens in <see cref="Context" /> being equal to <see cref="EndToken" /> (as compared by <see cref="Comparer" />): <c>true</c> if equal, <c>false</c> otherwise.</returns>
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
        /// </summary>
        /// <param name="context">Input tokens. Random text shall be generated based on <paramref name="context" />: both by picking only from <paramref name="context" /> and by using the order of tokens in <paramref name="context" />.</param>
        /// <param name="endToken">Ending token. See <em>Remarks</em> of <see cref="Pen" /> for clarification.</param>
        /// <param name="comparer">String comparer. Tokens shall be compared (e. g. for equality) by <paramref name="comparer" />. If <c>null</c>, <see cref="StringComparer.Ordinal" /> is used.</param>
        /// <param name="intern">If <c>true</c>, tokens from <paramref name="context" /> shall be interned (via <see cref="String.Intern(String)" /> method) when being copied into the internal pen's container (<see cref="Context" />).</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="context" /> is <c>null</c>.</exception>
        public Pen(IEnumerable<String?> context, String? endToken = null, StringComparer? comparer = null, Boolean intern = false)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context), ContextNullErrorMessage);
            }

            // Copy comparer and ending token.
            _comparer = comparer ?? StringComparer.Ordinal;
            _endToken = !intern || endToken is null ? endToken : String.Intern(endToken);

            // Copy context.
            {
                if (intern)
                {
                    context = context.Select(t => t is null ? null : String.Intern(t));
                }
                var contextList = new List<String?>(context);
                contextList.TrimExcess();
                _context = contextList.AsReadOnly();
            }


            // Find sorting positions of tokens in context.
            {
                var positionsList = new List<Int32>(Enumerable.Range(0, Context.Count));
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
                            var t1 = Context[i];
                            var t2 = Context[j];

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

                        // Finally, compare indices (the greater index has reached the end of `Context`, implying a shorter (sub)sequence).
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
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen internally; otherwise they are chosen by calling <see cref="picker" /> function. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <paramref name="picker" /> function. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="picker">Random number generator. When passed an integer <c>n</c> (<c>&gt;= 0</c>) as the argument, it should return an integer from range [0, max(<c>n</c>, 1)), i. e. greater than or equal to 0 but (strictly) less than max(<c>n</c>, 1).</param>
        /// <param name="fromBeginning">If <c>true</c>, <paramref name="picker" /> function is not called to choose the first max(<paramref name="relevantTokens" />, 1) tokens, but the beginning of the pen's context is chosen instead; otherwise the first token is chosen by immediately calling <paramref name="picker" /> function.</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="picker" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative. Function <paramref name="picker" /> returns a value outside of the legal range.</exception>
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
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
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
        public virtual IEnumerable<String?> Render(Int32 relevantTokens, Func<Int32, Int32> picker, Boolean fromBeginning = false)
        {
            if (picker is null)
            {
                throw new ArgumentNullException(nameof(picker), PickerNullErrorMessage);
            }

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
                for (var (next, i) = ValueTuple.Create(AllEnds ? Context.Count : Positions[FirstPosition], 0); i < text.Capacity; ++next, ++i)
                {
                    var nextToken = next < Context.Count ? Context[next] : EndToken;
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

                int first = pick == Context.Count ? Context.Count : Positions[pick];
                var firstToken = first < Context.Count ? Context[first] : EndToken;
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

                int next = Positions[p + pick] + d;
                var nextToken = next < Context.Count ? Context[next] : EndToken;
                if (Comparer.Equals(nextToken, EndToken))
                {
                    yield break;
                }

                yield return nextToken;
                if (text.Count < text.Capacity)
                {
                    text.Add(nextToken); 
                }
                else
                {
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
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen internally; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of <paramref name="random" />. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="random">(Pseudo-)Random number generator.</param>
        /// <param name="fromBeginning">If <c>true</c>, <paramref name="random" /> is not used to choose the first max(<paramref name="relevantTokens" />, 1) tokens, but the beginning of the pen's context is chosen instead; otherwise the first token is chosen by immediately calling <see cref="System.Random.Next(Int32)"/> method on <paramref name="random" />.</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="random" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative.</exception>
        /// <remarks>
        ///     <para>
        ///         An extra copy of <paramref name="relevantTokens"/> tokens is kept when generating new tokens. Memory errors may occur if <paramref name="relevantTokens"/> is too large.
        ///     </para>
        ///
        ///     <para>
        ///         If no specific <see cref="System.Random" /> object or seed should be used, <see cref="Render(Int32, Boolean)" /> method may be called instead.
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
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
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
        public virtual IEnumerable<String?> Render(Int32 relevantTokens, System.Random random, Boolean fromBeginning = false)
        {
            if (random is null)
            {
                throw new ArgumentNullException(nameof(random), RandomNullErrorMessage);
            }

            return Render(relevantTokens, random.Next, fromBeginning);
        }

        /// <summary>
        ///     <para>
        ///         Render (generate) a block of text from <see cref="Context" />.
        ///     </para>
        ///
        ///     <para>
        ///         If <paramref name="fromBeginning" /> is <c>true</c>, the first <c>max(<paramref name="relevantTokens" />, 1)</c> tokens are chosen internally; otherwise they are chosen by calling <see cref="System.Random.Next(Int32)" /> method of an internal <see cref="System.Random" /> object (<see cref="System.Random" />). Each consecutive token is chosen by observing the most recent <paramref name="relevantTokens" /> tokens (or the number of generated tokens if <paramref name="relevantTokens" /> tokens have not yet been generated) and choosing one of the possible successors by calling <see cref="System.Random.Next(Int32)" /> method of the internal <see cref="System.Random" /> object. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="EndToken" />) is chosen—the ending tokens are not rendered.
        ///     </para>
        /// </summary>
        /// <param name="relevantTokens">Number of (most recent) relevant tokens.</param>
        /// <param name="fromBeginning">If <c>true</c>, the beginning of the pen's context is chosen for the first <paramref name="relevantTokens" /> tokens; otherwise the first token is chosen by immediately calling <see cref="System.Random.Next(Int32)"/> method on the internal (pseudo-)random number generator (<see cref="Random" />).</param>
        /// <returns>A query for rendering tokens.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Parameter <paramref name="relevantTokens" /> is (strictly) negative.</exception>
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
        ///                 the pen was constructed with an enumerable consisting only of ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case),
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
        public virtual IEnumerable<String?> Render(Int32 relevantTokens, Boolean fromBeginning = false) =>
            Render(relevantTokens, Random, fromBeginning);
    }
}
