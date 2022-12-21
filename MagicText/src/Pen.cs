using MagicText.Internal;
using MagicText.Internal.Extensions;
using MagicText.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace MagicText
{
    /// <summary>Provides methods for (pseudo-)random text generation.</summary>
    /// <remarks>
    ///     <para>If the <see cref="Pen" /> should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable <c>context</c> passed to the constructor. To prevent overflowing from one source to another (e. g. if the last token from the first source is not a contextual predecessor of the first token from the second source), an ending token (<see cref="SentinelToken" />) should be put between the sources' tokens in the final enumerable <c>tokens</c>. Picking an ending token in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, Random, Nullable{Int32})" /> or <see cref="Render(Int32, Nullable{Int32})" /> method calls shall cause the rendering to stop—the same as when a <em>successor</em> of the last entry in tokens is chosen.</para>
    ///     <para>A complete deep copy of the enumerable <c>context</c> (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large. To reduce memory usage and even time consumption, <c>true</c> may be passed as the parameter <c>intern</c> to the constructor; however, other side effects of <see cref="String" /> interning via the <see cref="String.Intern(String)" /> method should be considered as well.</para>
    ///     <para>Changing any of the properties—public or private—breaks the functionality of the <see cref="Pen" />. By doing so, the behaviour of the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods is unexpected and no longer guaranteed.</para>
    ///     <para>To learn about serialisation and deserialisation of <see cref="Pen" />s, see:</para>
    ///     <list type="bullet">
    ///         <item>for serialisation via <see cref="IFormatter" />s, the <see cref="Pen(SerializationInfo, StreamingContext)" /> constructor and the <see cref="GetObjectData(SerializationInfo, StreamingContext)" /> method,</item>
    ///         <item>
    ///             for <a href="http://json.org/json-en.html"><em>JSON</em></a> serialisation
    ///             <list type="bullet">
    ///                 <item>using <a href="http://docs.microsoft.com/en-gb/dotnet/api/system.text.json"><c>System.Text.Json</c> namespace</a> members, the <see cref="PenJsonConverter" /> class,</item>
    ///                 <item>using <a href="http://newtonsoft.com/json">Json.NET</a>, the <see cref="Pen(SerializationInfo, StreamingContext)" /> constructor and the <see cref="GetObjectData(SerializationInfo, StreamingContext)" /> method,</item>
    ///             </list>
    ///         </item>
    ///         <item>for <a href="http://w3.org/XML/"><em>XML</em></a> serialisation, no implementation is provided.</item>
    ///     </list>
    ///     <para><a href="http://json.org/json-en.html"><em>JSON</em></a> serialisation was primarily implemented only for <a href="http://docs.microsoft.com/en-gb/dotnet/api/system.text.json"><c>System.Text.Json</c> namespace</a> <a href="http://en.wikipedia.org/wiki/API">API</a>. However, as described <a href="http://newtonsoft.com/json/help/html/serializationguide.htm#ISerializable">here</a>, <a href="http://newtonsoft.com/json">Json.NET</a> also enables <a href="http://json.org/json-en.html"><em>JSON</em></a> serialisation using the same <see cref="Pen" /> class' constructors and methods as <see cref="IFormatter" />s. On the other hand, <a href="http://docs.microsoft.com/en-gb/dotnet/api/system.text.json"><c>System.Text.Json</c> namespace</a> and <a href="http://newtonsoft.com/json">Json.NET</a> <a href="http://en.wikipedia.org/wiki/API">APIs</a> might not always produce compatible serialised <a href="http://json.org/json-en.html"><em>JSON</em></a> data, i. e. serialising using one <a href="http://json.org/json-en.html"><em>JSON</em></a> serialisation provider and deserialising using the other may fail.</para>
    ///     <para>Note that any manipulation of the serialised data, or even deserialising any data not constructed by/with the documented and recomended serialisers, serialising methods and serialising protocols, may result in <see cref="Pen" />s with unexpected behaviour. Using such <see cref="Pen" />s may even cause uncaught exceptions not explicitly documented for the methods of the <see cref="Pen" /> class.</para>
    /// </remarks>
    [CLSCompliant(true), JsonConverter(typeof(PenJsonConverter)), Serializable]
    public sealed class Pen : Object, ICloneable, ISerializable
    {
        private const string SerialisationInfoNullErrorMessage = "Serialisation info cannot be null.";
        private const string OtherNullErrorMessage = "Pen to copy cannot be null.";
        private const string ComparerNullErrorMessage = "String comparer cannot be null.";
        private const string TokensNullErrorMessage = "Token list cannot be null.";
        private const string IndexNullErrorMessage = "Index cannot be null.";
        private const string SampleNullErrorMessage = "Token sample cannot be null.";
        private const string ContextNullErrorMessage = "Context token enumerable cannot be null.";
        private const string PickerNullErrorMessage = "Picking function cannot be null.";
        private const string RandomNullErrorMessage = "(Pseudo-)Random number generator cannot be null.";
        private const string ComparisonTypeNotSupportedErrorMessage = "The string comparison type passed in is currently not supported.";
        private const string NonSerialisableComparerErrorMessage = "Cannot serialise the pen because the comparer is not serialisable.";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        private const string RelevantTokensOutOfRangeFormatErrorMessage = "Relevant tokens number is out of range. Must be greater than or equal to {0:D}.";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        private const string FromPositionOutOfRangeFormatErrorMessage = "First token index is out of range. Must be greater than or equal to {0:D} and less than or equal to the size of the context ({1:D}).";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        private const string PickOutOfRangeFormatErrorMessage = "Picking function returned a pick out of range. Must return a pick greater than or equal to {0:D} and less than the parameter given ({1:D}); however, if the parameter equals {2:D}, {3:D} must be returned.";
        
        private static readonly Object _locker;

        private static Int32 randomSeed;

        [ThreadStatic]
        private static System.Random? mutableThreadStaticRandom = null;

        /// <summary>Gets the internal locking object of the <see cref="Pen" /> class.</summary>
        /// <returns>The internal locking object.</returns>
        private static Object Locker => _locker;

        /// <summary>Gets or sets the seed for the <see cref="Pen" />'s internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <returns>The current seed.</returns>
        /// <value>The new seed. If the <c>value</c> is less than or equal to 0, the new seed is set to 1.</value>
        /// <remarks>
        ///     <para>The current value does not necessarily indicate the seeding value or the random state of the internal number generator (<see cref="Random" />). The dissonance may appear if the number generator has already been instantiated and used or if the value of the <see cref="RandomSeed" /> has changed.</para>
        /// </remarks>
        private static Int32 RandomSeed
        {
            get
            {
                lock (Locker)
                {
                    return randomSeed;
                }
            }

            set
            {
                lock (Locker)
                {
                    randomSeed = GetRandomSeed(value);
                }
            }
        }

        /// <summary>Gets the <see cref="Pen" />'s static (pseudo-)random number generator.</summary>
        /// <returns>The internal (pseudo-)random number generator for the current thread.</returns>
        /// <remarks>
        ///     <para>The number generator is thread safe (actually, each thread uses its own instance) and instances across multiple threads are seeded differently. However, instances across multiple processes initiated at approximately the same time could be seeded with the same value. Therefore the main purpose of the number generator is to provide a virtually unpredictable (to an unconcerned human user) implementation of the <see cref="Render(Int32, Nullable{Int32})" /> method for a single process without having to provide a custom number generator (a <see cref="Func{T, TResult}" /> function or a <see cref="System.Random" /> instance); no other properties are guaranteed.</para>
        ///     <para>If the code is expected to spread over multiple threads (either explicitly by starting <see cref="Thread" />s or implicitly by programming asynchronously), the reference returned by the <see cref="Random" /> property should not be stored in an outside variable and used later. Reference the property <see cref="Random" /> directly (or indirectly via the <see cref="RandomNext()" /> method or one of its overloads) in such scenarios.</para>
        /// </remarks>
        private static System.Random Random
        {
            get
            {
                lock (Locker)
                {
                    if (mutableThreadStaticRandom is null)
                    {
                        unchecked
                        {
                            mutableThreadStaticRandom = new System.Random(RandomSeed++);
                        }
                    }

                    return mutableThreadStaticRandom;
                }
            }
        }

        /// <summary>Initialises static fields.</summary>
        static Pen()
        {
            _locker = new Object();

            randomSeed = GetRandomSeed((Int32)((1073741827L * AssemblyInfo.InitialisationTimePoint.Ticks + 1073741789L) & (Int64)Int32.MaxValue));
        }

        /// <summary>Gets a strictly positive random seed value constructed from the raw <see cref="Int32" /> <c><paramref name="value" /></c>.</summary>
        /// <param name="value">The original <see cref="Int32" /> value.</param>
        /// <returns>The strictly positive random seed value defined by the raw <c><paramref name="value" /></c>.</returns>
        /// <remarks>
        ///     <para>The strictly positive random seed value is obtained by taking the maximum of <c><paramref name="value" /></c>, its bitwise complement, and 1.</para>
        /// </remarks>
        private static Int32 GetRandomSeed(Int32 value)
        {
            unchecked
            {
                return Math.Max(Math.Max(value, ~value), 1);
            }
        }

        /// <summary>Initialises variables representing a <se cref="Pen" /> instance's fields.</summary>
        /// <param name="context">The input tokens.</param>
        /// <param name="comparer">The <see cref="StringComparer" /> used by the <see cref="Pen" />.</param>
        /// <param name="sentinelToken">The ending token.</param>
        /// <param name="intern">Whether or not to intern non-<c>null</c> tokens from the <c><paramref name="context" /></c> (via the <see cref="String.Intern(String)" /> method) when being copied into the <c><paramref name="contextField" /></c>.</param>
        /// <param name="internedField">Destination for the <see cref="Interned" />'s underlying field.</param>
        /// <param name="comparerField">Destination for the <see cref="Comparer" />'s underlying field.</param>
        /// <param name="indexField">Destination for the <see cref="Index" />'s underlying field.</param>
        /// <param name="contextField">Destination for the <see cref="Context" />'s underlying field.</param>
        /// <param name="sentinelTokenField">Destination for the <see cref="SentinelToken" />'s underlying field.</param>
        /// <remarks>
        ///     <para><strong>Nota bene.</strong> The method is intended for <strong>internal use only</strong> to be used in the <see cref="Pen(IEnumerable{String}, StringComparer, String, Boolean)" /> and <see cref="Pen(IEnumerable{String}, StringComparison, String, Boolean)" /> constructors, and therefore does not make unnecessary checks of the parameters.</para>
        /// </remarks>
        private static void InitialiseFields(IEnumerable<String?> context, StringComparer comparer, String? sentinelToken, Boolean intern, out Boolean internedField, out StringComparer comparerField, out IReadOnlyList<Int32> indexField, out IReadOnlyList<String?> contextField, out String? sentinelTokenField)
        {
            // Copy the interning policy, the comparer and the ending token.
            internedField = intern;
            comparerField = comparer;
            sentinelTokenField = internedField ? StringExtensions.InternNullable(sentinelToken) : sentinelToken;

            // Copy the context.
            {
                List<String?> contextList = new List<String?>(internedField ? context.Select(StringExtensions.InternNullable) : context);
                contextList.TrimExcess();
                contextField = contextList.AsReadOnly();
            }

            // Find the sorting positions of tokens in the context.
            {
                List<Int32> indexList = new List<Int32>(Enumerable.Range(0, contextField.Count));
                indexList.Sort(new IndexComparer(comparerField, contextField));
                indexList.TrimExcess();
                indexField = indexList.AsReadOnly();
            }
        }

        /// <summary>Generates a non-negative (pseudo-)random integer using the internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <returns>A (pseudo-)random integer that is greater than or equal to 0 but less than <see cref="Int32.MaxValue" />.</returns>
        private static Int32 RandomNext() =>
            Random.Next();

        /// <summary>Generates a non-negative (pseudo-)random integer using the internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. The <c><paramref name="maxValue" /></c> must be greater than or equal to 0.</param>
        /// <returns>A (pseudo-)random integer that is greater than or equal to 0 but less than <c><paramref name="maxValue" /></c>. However, if <c><paramref name="maxValue" /></c> equals 0, 0 is returned.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="System.Random.Next(Int32)" /> method are not caught.</para>
        /// </remarks>
        private static Int32 RandomNext(Int32 maxValue) =>
            Random.Next(maxValue);

        /// <summary>Generates a non-negative (pseudo-)random integer using the internal (pseudo-)random number generator (<see cref="Random" />).</summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number to be generated. The <c><paramref name="maxValue" /></c> must be greater than or equal to the <c><paramref name="minValue" /></c>.</param>
        /// <returns>A (pseudo-)random integer that is greater than or equal to <c><paramref name="minValue" /></c> and less than <c><paramref name="maxValue" /></c>. However, if <c><paramref name="maxValue" /></c> equals <c><paramref name="minValue" /></c>, that value is returned.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="System.Random.Next(Int32)" /> method are not caught.</para>
        /// </remarks>
        private static Int32 RandomNext(Int32 minValue, Int32 maxValue) =>
            Random.Next(minValue, maxValue);

        /// <summary>Compares a subrange of <c><paramref name="tokens" /></c> with the sample of tokens <c><paramref name="sampleCycle" /></c>.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used for comparing.</param>
        /// <param name="tokens">The list of tokens whose subrange is compared to the <c><paramref name="sampleCycle" /></c>.</param>
        /// <param name="sampleCycle">The cyclical sample list of tokens. The list represents the range <c>{ <paramref name="sampleCycle" />[<paramref name="cycleStart" />], <paramref name="sampleCycle" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sampleCycle" />[<paramref name="sampleCycle" />.Count - 1], <paramref name="sampleCycle" />[0], ..., <paramref name="sampleCycle" />[<paramref name="cycleStart" /> - 1] }</c>.</param>
        /// <param name="i">The starting index of the subrange of <c><paramref name="tokens" /></c> to compare. The subrange <c>{ <paramref name="tokens" />[i], <paramref name="tokens" />[i + 1], ..., <paramref name="tokens" />[min(i + <paramref name="sampleCycle" />.Count - 1, <paramref name="tokens" />.Count - 1)] }</c> is used.</param>
        /// <param name="cycleStart">The starting index of the <c><paramref name="sampleCycle" /></c>.</param>
        /// <returns>A signed integer that indicates the comparison of the values of the subrange from the <c><paramref name="tokens" /></c> starting from <c><paramref name="i" /></c> and the <c><paramref name="sampleCycle" /></c>. A negative value indicates the <em>less than</em> relation, a positive value indicates the <em>greater than</em> relation, and 0 indicates the <em>equal to</em> relation between the <c><paramref name="cycleStart" /></c> and the subrange from <c><paramref name="tokens" />.</c></returns>
        /// <remarks>
        ///     <para>The values from the subrange of <c><paramref name="tokens" /></c> and the <c><paramref name="sampleCycle" /></c> are compared in order by calling the <see cref="StringComparer.Compare(String, String)" /> method of the <c><paramref name="comparer" /></c>. If a comparison yields a non-zero value, it is returned. If the subrange of <c><paramref name="tokens" /></c> is shorter (in the number of tokens) than the <c><paramref name="sampleCycle" /></c> but all of its tokens compare equal to corresponding tokens from the beginning of the <c><paramref name="sampleCycle" /></c>, a negative number is returned. If all tokens compare equal and the subrange of <c><paramref name="tokens" /></c> is the same length (in the number of tokens) as the <c><paramref name="sampleCycle" /></c>, 0 is returned.</para>
        ///     <para><strong>Nota bene.</strong> The method is intended for <strong>internal use only</strong>, and therefore does not make unnecessary checks of the parameters.</para>
        /// </remarks>
        private static Int32 CompareRange(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<String?> sampleCycle, Int32 i, Int32 cycleStart)
        {
            Int32 c = 0;

            {
                Int32 j;

                // Compare the tokens.
                for (/* [`i` is set in function call,] */ j = 0; i < tokens.Count && j < sampleCycle.Count; ++i, ++j)
                {
                    c = comparer.Compare(tokens[i], sampleCycle[(cycleStart + j) % sampleCycle.Count]);
                    if (c != 0)
                    {
                        break;
                    }
                }

                // If `tokens` have reached the end, but the `sampleCycle` has not, consider the subrange of `tokens` smaller.
                if (c == 0 && i == tokens.Count && j < sampleCycle.Count)
                {
                    c = -1;
                }
            }

            return c;
        }

        /// <summary>Finds the first index and the number of occurrences of the <c><paramref name="sampleCycle" /></c> amongst <c><paramref name="tokens" /></c>.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used for comparing.</param>
        /// <param name="tokens">The list of tokens whose subrange is compared to the <c><paramref name="sampleCycle" /></c>.</param>
        /// <param name="index">The positional ordering of <c><paramref name="tokens" /></c> in respect of the <c><paramref name="comparer" /></c>.</param>
        /// <param name="sampleCycle">The cyclical sample list of tokens to find. The list represents the range <c>{ <paramref name="sampleCycle" />[<paramref name="cycleStart" />], <paramref name="sampleCycle" />[<paramref name="cycleStart" /> + 1], ..., <paramref name="sampleCycle" />[<paramref name="sampleCycle" />.Count - 1], <paramref name="sampleCycle" />[0], ..., <paramref name="sampleCycle" />[<paramref name="cycleStart" /> - 1] }</c>.</param>
        /// <param name="cycleStart">The starting index of the <c><paramref name="sampleCycle" /></c>.</param>
        /// <param name="count">The total number of occurrences of the <c><paramref name="sampleCycle" /></c> amongst <c><paramref name="tokens" /></c>.</param>
        /// <param name="bounds">If set, it defines the initial lower and upper bound of a matching position index.</param>
        /// <returns>The minimal index <c>i</c> such that an occurrence of the <c><paramref name="sampleCycle" /></c> begins at <c><paramref name="tokens" />[<paramref name="index" />[i]]</c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="comparer" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="tokens" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="sampleCycle" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <c><paramref name="bounds" /></c> parameter should be used only if the range of a possible position index match is known. For instance, if it is known that the <c><paramref name="sampleCycle" /></c> occurs between <c><paramref name="index" />[m]</c> and <c><paramref name="index" />[n]</c> inclusively (at least one of <c>{ <paramref name="tokens" />[<paramref name="index" />[m]], <paramref name="tokens" />[<paramref name="index" />[m + 1]], ..., <paramref name="tokens" />[<paramref name="index" />[n]] }</c> is the start of the <c><paramref name="sampleCycle" /></c>'s occurance), where <c>m</c> and <c>n</c> are some legal and valid integers, then <c>(m, n)</c> may be passed to the method call as <c><paramref name="bounds" /></c>. The most common scenario for this would probably be when the position index and count of a shorter sample, which is at the very beginning of the actual <c><paramref name="sampleCycle" /></c>, is known: e. g. the position index and count of <c>{ "lorem", " " }</c> in the search for <c>{ "lorem", " ", "ipsum" }</c>.</para>
        ///     <para>It is sufficient that <c><paramref name="bounds" /></c> (if provided) cover at least one matching position index, even if not all of them are covered. Consequently, all matches are still going to be identified. However, if no position index is covered but the <c><paramref name="sampleCycle" /></c> exists amongst <c><paramref name="tokens" /></c>, the match shall not be found.</para>
        ///     <para>Because of performance reasons, the implementation of the method <em>assumes</em> the following without checking:</para>
        ///     <list type="number">
        ///         <item>
        ///             <term><c><paramref name="index" /></c> legality</term>
        ///             <description><c><paramref name="index" /></c> is of the same length as <c><paramref name="tokens" /></c>, all of its values are legal indices for the <c><paramref name="tokens" /></c> and each of its values appears only once (in short, the <c><paramref name="index" /></c> is a permutation of the sequence <c>{ 0, 1, ..., <paramref name="tokens" />.Count - 1 }</c>),</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="cycleStart" /></c> legality</term>
        ///             <description><c><paramref name="cycleStart" /></c> is a legal index for the <c><paramref name="sampleCycle" /></c> (i. e. a value in the sequence <c>{ 0, 1, ..., <paramref name="sampleCycle" />.Count - 1 }</c>).,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="index" /></c> validity</term>
        ///             <description><c><paramref name="index" /></c> indeed sorts <c><paramref name="tokens" /></c> ascendingly in respect of the <c><paramref name="comparer" /></c>,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="bounds" /></c> legality &amp; validity</term>
        ///             <description>if <c><paramref name="bounds" /></c> are set—let us denote them <c>(low, high)</c>—then all inequalities in the chained expression <c>0 &lt;= low &lt;= high &lt;= <paramref name="tokens" />.Count</c> are true,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="sampleCycle" /></c> existence</term>
        ///             <description>the <c><paramref name="sampleCycle" /></c> exists amongst <c><paramref name="tokens" /></c> (when compared by the <c><paramref name="comparer" /></c>).</description>
        ///         </item>
        ///     </list>
        ///     <para>If any of the first four assumptions is incorrect, the behaviour of the method is undefined (even the <see cref="ArgumentOutOfRangeException" /> might be thrown and not caught when calling an <see cref="IReadOnlyList{T}.this[Int32]" /> indexer). If the last assumption is incorrect, the returned index shall point to the position at which the <c><paramref name="sampleCycle" /></c>'s position should be inserted to retain the sorted order but the number of occurrences (<c><paramref name="count" /></c>) shall be 0. However, if <c><paramref name="bounds" /></c> are set but the <c><paramref name="sampleCycle" /></c> is not found in the specified range, the latter may not be true.</para>
        /// </remarks>
        private static Int32 FindPositionIndexAndCount(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<Int32> index, IReadOnlyList<String?> sampleCycle, Int32 cycleStart, out Int32 count, Nullable<ValueTuple<Int32, Int32>> bounds = default)
        {
            if (comparer is null)
            {
                throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            }
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens), TokensNullErrorMessage);
            }
            if (index is null)
            {
                throw new ArgumentNullException(nameof(index), IndexNullErrorMessage);
            }
            if (sampleCycle is null)
            {
                throw new ArgumentNullException(nameof(sampleCycle), SampleNullErrorMessage);
            }

            // Binary search...

            // Initialise the lower, upper and middle positions.
            Int32 l;
            Int32 h;
            if (bounds.HasValue)
            {
                l = bounds.Value.Item1;
                h = bounds.Value.Item2;
            }
            else
            {
                l = 0;
                h = tokens.Count;
            }

            // Loop until found.
            {
                Int32 m;
                Int32 c;

                while (l < h)
                {
                    m = (l + h) >> 1; // (l + h) / 2

                    // Compare the ranges.
                    c = CompareRange(comparer, tokens, sampleCycle, index[m], cycleStart);

                    // Break the loop or update the positions.
                    if (c < 0)
                    {
                        l = m + 1;
                    }
                    else if (c > 0)
                    {
                        h = m;
                    }
                    else
                    {
                        l = m;
                        h = m;

                        break;
                    }
                }
            }

            // Find the minimal position index `l` and the maximal index `h` of occurrences of the `sampleCycle` amongst the `tokens`.
            while (l > 0 && CompareRange(comparer, tokens, sampleCycle, index[l - 1], cycleStart) == 0)
            {
                --l;
            }
            while (h <= tokens.Count && CompareRange(comparer, tokens, sampleCycle, h < tokens.Count ? index[h] : tokens.Count, cycleStart) == 0)
            {
                ++h;
            }

            // Return the computed values.

            count = h - l;

            return l;
        }

        /// <summary>Finds the first index and the number of occurrences of the <c><paramref name="sample" /></c> amongst <c><paramref name="tokens" /></c>.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used for comparing.</param>
        /// <param name="tokens">The list of tokens whose subrange is compared to the <c><paramref name="sample" /></c>.</param>
        /// <param name="index">The positional ordering of <c><paramref name="tokens" /></c> in respect of the <c><paramref name="comparer" /></c>.</param>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <param name="count">The total number of occurrences of the <c><paramref name="sample" /></c> amongst <c><paramref name="tokens" /></c>.</param>
        /// <param name="bounds">If set, it defines the initial lower and upper bound of a matching position index.</param>
        /// <returns>The minimal index <c>i</c> such that an occurrence of the <c><paramref name="sample" /></c> begins at <c><paramref name="tokens" />[<paramref name="index" />[i]]</c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="comparer" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="tokens" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="sample" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <c><paramref name="bounds" /></c> parameter should be used only if the range of a possible position index match is known. For instance, if it is known that the <c><paramref name="sample" /></c> occurs between <c><paramref name="index" />[m]</c> and <c><paramref name="index" />[n]</c> inclusively (at least one of <c>{ <paramref name="tokens" />[<paramref name="index" />[m]], <paramref name="tokens" />[<paramref name="index" />[m + 1]], ..., <paramref name="tokens" />[<paramref name="index" />[n]] }</c> is the start of the <c><paramref name="sample" /></c>'s occurance), where <c>m</c> and <c>n</c> are some legal and valid integers, then <c>(m, n)</c> may be passed to the method call as <c><paramref name="bounds" /></c>. The most common scenario for this would probably be when the position index and count of a shorter sample, which is at the very beginning of the actual <c><paramref name="sample" /></c>, is known: e. g. the position index and count of <c>{ "lorem", " " }</c> in the search for <c>{ "lorem", " ", "ipsum" }</c>.</para>
        ///     <para>It is sufficient that <c><paramref name="bounds" /></c> (if provided) cover at least one matching position index, even if not all of them are covered. Consequently, all matches are still going to be identified. However, if no position index is covered but the <c><paramref name="sample" /></c> exists amongst <c><paramref name="tokens" /></c>, the match shall not be found.</para>
        ///     <para>Because of performance reasons, the implementation of the method <em>assumes</em> the following without checking:</para>
        ///     <list type="number">
        ///         <item>
        ///             <term><c><paramref name="index" /></c> legality</term>
        ///             <description><c><paramref name="index" /></c> is of the same length as <c><paramref name="tokens" /></c>, all of its values are legal indices for the <c><paramref name="tokens" /></c> and each of its values appears only once (in short, the <c><paramref name="index" /></c> is a permutation of the sequence <c>{ 0, 1, ..., <paramref name="tokens" />.Count - 1 }</c>),</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="index" /></c> validity</term>
        ///             <description><c><paramref name="index" /></c> indeed sorts <c><paramref name="tokens" /></c> ascendingly in respect of the <c><paramref name="comparer" /></c>,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="bounds" /></c> legality &amp; validity</term>
        ///             <description>if <c><paramref name="bounds" /></c> are set—let us denote them <c>(low, high)</c>—then all inequalities in the chained expression <c>0 &lt;= low &lt;= high &lt;= <paramref name="tokens" />.Count</c> are true,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="sample" /></c> existence</term>
        ///             <description>the <c><paramref name="sample" /></c> exists amongst <c><paramref name="tokens" /></c> (when compared by the <c><paramref name="comparer" /></c>).</description>
        ///         </item>
        ///     </list>
        ///     <para>If any of the first three assumptions is incorrect, the behaviour of the method is undefined (even the <see cref="ArgumentOutOfRangeException" /> might be thrown and not caught when calling an <see cref="IReadOnlyList{T}.this[Int32]" /> indexer). If the last assumption is incorrect, the returned index shall point to the position at which the <c><paramref name="sample" /></c>'s position should be inserted to retain the sorted order but the number of occurrences (<c><paramref name="count" /></c>) shall be 0. However, if <c><paramref name="bounds" /></c> are set but the <c><paramref name="sample" /></c> is not found in the specified range, the latter may not be true.</para>
        /// </remarks>
        private static Int32 FindPositionIndexAndCount(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<Int32> index, IEnumerable<String?> sample, out Int32 count, Nullable<ValueTuple<Int32, Int32>> bounds = default) =>
            FindPositionIndexAndCount(comparer, tokens, index, sample is null ? throw new ArgumentNullException(nameof(sample), SampleNullErrorMessage) : sample.AsReadOnlyList(), 0, out count, bounds);

        /// <summary>Finds the first index and the number of occurrences of the <c><paramref name="token" /></c> amongst <c><paramref name="tokens" /></c>.</summary>
        /// <param name="comparer">The <see cref="StringComparer" /> used for comparing.</param>
        /// <param name="tokens">The list of tokens whose subrange is compared to the <c><paramref name="token" /></c>.</param>
        /// <param name="index">The positional ordering of <c><paramref name="tokens" /></c> in respect of the <c><paramref name="comparer" /></c>.</param>
        /// <param name="token">The token to find.</param>
        /// <param name="count">The total number of occurrences of the <c><paramref name="token" /></c> amongst <c><paramref name="tokens" /></c>.</param>
        /// <param name="bounds">If set, it defines the initial lower and upper bound of a matching position index.</param>
        /// <returns>The minimal index <c>i</c> such that an occurrence of the <c><paramref name="token" /></c> is <c><paramref name="tokens" />[<paramref name="index" />[i]]</c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="comparer" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="tokens" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <c><paramref name="bounds" /></c> parameter should be used only if the range of a possible position index match is known. For instance, if it is known that the <c><paramref name="token" /></c> occurs between <c><paramref name="index" />[m]</c> and <c><paramref name="index" />[n]</c> inclusively (at least one of <c>{ <paramref name="tokens" />[<paramref name="index" />[m]], <paramref name="tokens" />[<paramref name="index" />[m + 1]], ..., <paramref name="tokens" />[<paramref name="index" />[n]] }</c> is an occurance of the <c><paramref name="token" /></c>), where <c>m</c> and <c>n</c> are some legal and valid integers, then <c>(m, n)</c> may be passed to the method call as <c><paramref name="bounds" /></c>.</para>
        ///     <para>It is sufficient that <c><paramref name="bounds" /></c> (if provided) cover at least one matching position index, even if not all of them are covered. Consequently, all matches are still going to be identified. However, if no position index is covered but the <c><paramref name="token" /></c> exists amongst <c><paramref name="tokens" /></c>, the match shall not be found.</para>
        ///     <para>Because of performance reasons, the implementation of the method <em>assumes</em> the following without checking:</para>
        ///     <list type="number">
        ///         <item>
        ///             <term><c><paramref name="index" /></c> legality</term>
        ///             <description><c><paramref name="index" /></c> is of the same length as <c><paramref name="tokens" /></c>, all of its values are legal indices for the <c><paramref name="tokens" /></c> and each of its values appears only once (in short, the <c><paramref name="index" /></c> is a permutation of the sequence <c>{ 0, 1, ..., <paramref name="tokens" />.Count - 1 }</c>),</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="index" /></c> validity</term>
        ///             <description><c><paramref name="index" /></c> indeed sorts <c><paramref name="tokens" /></c> ascendingly in respect of the <c><paramref name="comparer" /></c>,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="bounds" /></c> legality &amp; validity</term>
        ///             <description>if <c><paramref name="bounds" /></c> are set—let us denote them <c>(low, high)</c>—then all inequalities in the chained expression <c>0 &lt;= low &lt;= high &lt;= <paramref name="tokens" />.Count</c> are true,</description>
        ///         </item>
        ///         <item>
        ///             <term><c><paramref name="token" /></c> existence</term>
        ///             <description>the <c><paramref name="token" /></c> exists amongst <c><paramref name="tokens" /></c> (when compared by the <c><paramref name="comparer" /></c>).</description>
        ///         </item>
        ///     </list>
        ///     <para>If any of the first three assumptions is incorrect, the behaviour of the method is undefined (even the <see cref="ArgumentOutOfRangeException" /> might be thrown and not caught when calling an <see cref="IReadOnlyList{T}.this[Int32]" /> indexer). If the last assumption is incorrect, the returned index shall point to the position at which the <c><paramref name="token" /></c>'s position should be inserted to retain the sorted order but the number of occurrences (<c><paramref name="count" /></c>) shall be 0. However, if <c><paramref name="bounds" /></c> are set but the <c><paramref name="token" /></c> is not found in the specified range, the latter may not be true.</para>
        /// </remarks>
        private static Int32 FindPositionIndexAndCount(StringComparer comparer, IReadOnlyList<String?> tokens, IReadOnlyList<Int32> index, String? token, out Int32 count, Nullable<ValueTuple<Int32, Int32>> bounds = default) =>
            FindPositionIndexAndCount(comparer, tokens, index, Enumerable.Repeat(token, 1), out count, bounds);

        [XmlIgnore, JsonIgnore, NonSerialized]
        private readonly Boolean _interned;

        [XmlIgnore, JsonIgnore, NonSerialized]
        private readonly StringComparer _comparer;

        [XmlIgnore, JsonIgnore, NonSerialized]
        private readonly IReadOnlyList<Int32> _index;

        [XmlIgnore, JsonIgnore, NonSerialized]
        private readonly IReadOnlyList<String?> _context;

        [XmlIgnore, JsonIgnore, NonSerialized]
        private readonly String? _sentinelToken;

        /// <summary>Gets the policy of interning all non-<c>null</c> tokens from the <see cref="Context" />, as well as the ending token (<see cref="SentinelToken" />): <c>true</c> if interning, <c>false</c> otherwise.</summary>
        /// <returns>If all tokens in the <see cref="Context" />, as well as the ending token (<see cref="SentinelToken" />), are interned, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <remarks>
        ///     <para>Tokens are interned using the <see cref="String.Intern(String)" /> method.</para>
        ///     <para>If the <see cref="Context" /> is empty and the ending token (<see cref="SentinelToken" />) is <c>null</c>, the value of the <see cref="Interned" /> property may still be both <c>true</c> and <c>false</c>. It actually depends on the value of the <c>intern</c> parameter in the constructor.</para>
        /// </remarks>
        public Boolean Interned => _interned;

        /// <summary>Gets the <see cref="StringComparer" /> used by the pen for comparing tokens.</summary>
        /// <returns>The internal <see cref="StringComparer" />.</returns>
        internal StringComparer Comparer => _comparer;

        /// <summary>Gets the index of entries in the <see cref="Context" /> sorted ascendingly (their sorting positions): if <c>i &lt; j</c>, then <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]]) &lt;= 0</c>.</summary>
        /// <returns>The sorting positions of tokens in the <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>The order is actually determined by the complete sequence of tokens, and not just by single tokens. For instance, if <c>i != j</c>, but <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]]) == 0</c> (<c><see cref="Comparer" />.Equals(<see cref="Context" />[<see cref="Index" />[i]], <see cref="Context" />[<see cref="Index" />[j]])</c>), then the result of <c><see cref="Comparer" />.Compare(<see cref="Context" />[<see cref="Index" />[i] + 1], <see cref="Context" />[<see cref="Index" />[j] + 1])</c> is used; if it also evaluates to 0, <c><see cref="Index" />[i] + 2</c> and <c><see cref="Index" />[j] + 2</c> are checked, and so on. The first position to reach the end (when <c>max(<see cref="Index" />[i] + n, <see cref="Index" />[j] + n) == <see cref="Context" />.Count</c> for a non-negative integer <c>n</c>), if all previous positions compared equal, is considered smaller. Hence the <see cref="Index" /> defines a total (linear) lexicographic ordering of the <see cref="Context" /> in respect of the <see cref="Comparer" />, which may be useful in search algorithms, such as the binary search, for finding the position of any non-empty finite sequence of tokens.</para>
        ///     <para>The position(s) of ending tokens' (<see cref="SentinelToken" />) indices, if there are any in the <see cref="Context" />, are not fixed nor guaranteed. For instance, they are not necessarily at the beginning or the end of <see cref="Index" />. If the ending token is <c>null</c> or an empty <see cref="String" /> (<see cref="String.Empty" />), then ending tokens shall be compared less than any other tokens and their indices shall be <em>pushed</em> to the beginning, provided there are no <c>null</c>s in <see cref="Context" /> in the latter case. But, generally speaking, ending tokens' indices are determined by the <see cref="Comparer" /> and the values of other tokens in <see cref="Context" />.</para>
        /// </remarks>
        internal IReadOnlyList<Int32> Index => _index;

        /// <summary>Gets the reference token context used by the pen.</summary>
        /// <returns>The reference context.</returns>
        /// <remarks>
        ///     <para>The order of tokens is kept as provided in the constructor.</para>
        /// </remarks>
        public IReadOnlyList<String?> Context => _context;

        /// <summary>Gets the ending token of the pen.</summary>
        /// <returns>The ending token.</returns>
        /// <remarks>
        ///     <para>The ending token (<see cref="SentinelToken" />), or any other token comparing equal to it in respect of the <see cref="Comparer" />, shall never be rendered.</para>
        ///     <para>If the ending token is <c>null</c>, it <strong>does not</strong> mean that no token is considered an ending token. It simply means that <c>null</c>s are considered ending tokens. Moreover, the ending token <strong>cannot</strong> be ignored (not used)—if no token should be considered an ending token, set the ending token to a value not appearing in the <see cref="Context" />. Such value may be found via an adaptation of the <a href="http://en.wikipedia.org/wiki/Cantor%27s_diagonal_argument">Cantor's diagonal method</a>. Note, however, that comparing long <see cref="String" />s is expensive in time resources, and therefore <em>short</em> ending tokens should be preferred: <c>null</c>, <c><see cref="String.Empty" /></c>, <c>"\0"</c> etc. (depending on the values in the <see cref="Context" />).</para>
        /// </remarks>
        public String? SentinelToken => _sentinelToken;

        /// <summary>Creates a pen.</summary>
        /// <param name="context">The input tokens. All random text shall be generated based on the <c><paramref name="context" /></c>: both by picking only tokens from the <c><paramref name="context" /></c> and by using the order of tokens from it.</param>
        /// <param name="comparer">The <see cref="StringComparer" /> used by the <see cref="Pen" />. Tokens shall be compared (e. g. for equality) by the <c><paramref name="comparer" /></c>. If <c>null</c>, <see cref="StringComparer.Ordinal" /> is used.</param>
        /// <param name="sentinelToken">The ending token.</param>
        /// <param name="intern">If <c>true</c>, non-<c>null</c> tokens from the <c><paramref name="context" /></c> shall be interned (via the <see cref="String.Intern(String)" /> method) when being copied into the internal <see cref="Pen" />'s container <see cref="Context" />.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="context" /></c> parameter is <c>null</c>.</exception>
        public Pen(IEnumerable<String?> context, StringComparer? comparer = null, String? sentinelToken = null, Boolean intern = false) : base()
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context), ContextNullErrorMessage);
            }

            InitialiseFields(context, comparer ?? StringComparer.Ordinal, sentinelToken, intern, out _interned, out _comparer, out _index, out _context, out _sentinelToken);
        }

        /// <summary>Creates a pen.</summary>
        /// <param name="context">The input tokens. All random text shall be generated based on the <c><paramref name="context" /></c>: both by picking only from the <c><paramref name="context" /></c> and by using the order from it.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies how <see cref="System.String" />s should be compared. Tokens shall be compared (e. g. for equality) by the <see cref="StringComparer" /> specified by the <c><paramref name="comparisonType" /></c>.</param>
        /// <param name="sentinelToken">The ending token.</param>
        /// <param name="intern">If <c>true</c>, non-<c>null</c> tokens from the <c><paramref name="context" /></c> shall be interned (via the <see cref="String.Intern(String)" /> method) when being copied into the internal <see cref="Pen" />'s container <see cref="Context" />.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="context" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="comparisonType" /></c> is not a supported <see cref="StringComparison" /> value.</exception>
        public Pen(IEnumerable<String?> context, StringComparison comparisonType, String? sentinelToken = null, Boolean intern = false) : base()
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context), ContextNullErrorMessage);
            }

            StringComparer comparer;
            try
            {
#if NETSTANDARD2_1_OR_GREATER
                comparer = StringComparer.FromComparison(comparisonType);
#else
                comparer = StringComparerExtensions.GetComparerFromComparison(comparisonType);
#endif // NETSTANDARD2_1_OR_GREATER
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException(ComparisonTypeNotSupportedErrorMessage, nameof(comparisonType), exception);
            }

            InitialiseFields(context, comparer, sentinelToken, intern, out _interned, out _comparer, out _index, out _context, out _sentinelToken);
        }

        /// <summary>Creates an empty pen.</summary>
        /// <remarks>
        ///     <para><strong>Nota bene.</strong> This constructor was introduced only to allow some operations that would be impossible if a parameterless constructor did not exist. It is strongly advised not to use this constructor because the resulting <see cref="Pen" /> is useless—objects of the <see cref="Pen" /> class are immutable and an empty <see cref="Context" /> cannot be used for any text generation or corpus analysis.</para>
        /// </remarks>
        public Pen() : this(Enumerable.Empty<String?>())
        {
        }

        /// <summary>Copies the <c><paramref name="other" /></c> pen with a custom interning policy.</summary>
        /// <param name="other">The <see cref="Pen" /> to copy.</param>
        /// <param name="intern">If <c>true</c>, non-<c>null</c> tokens from the <c><paramref name="other" /></c>'s <see cref="Context" /> shall be interned (via the <see cref="String.Intern(String)" /> method) when being copied.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="other" /></c> parmeter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If <c><paramref name="intern" /></c> is <c>false</c> or the same as the <c><paramref name="other" /></c>'s <see cref="Interned" /> property, the new <see cref="Pen" /> shall be a shallow copy of the <c><paramref name="other" /></c>. Otherwise the copy shall be partly shallow (for those fields that are not affected by the interning policy) and partly deep (for those fields that require token copying and interning).</para>
        /// </remarks>
        public Pen(Pen other, Boolean intern) : base()
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other), OtherNullErrorMessage);
            }

            // Copy the interning policy and the comparer.
            _interned = intern;
            _comparer = other.Comparer;

            // Copy the ending token and the `Context`.
            if (Interned && !other.Interned)
            {
                _sentinelToken = StringExtensions.InternNullable(other.SentinelToken);

                {
                    List<String?> contextList = new List<String?>(other.Context.Select(StringExtensions.InternNullable));
                    contextList.TrimExcess();
                    _context = contextList.AsReadOnly();
                }
            }
            else
            {
                _sentinelToken = other.SentinelToken;

                _context = other.Context;
            }

            // Copy the `Index`.
            _index = other.Index;
        }

        /// <summary>Copies the <c><paramref name="other" /></c> pen.</summary>
        /// <param name="other">The <see cref="Pen" /> to copy.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="other" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The new <see cref="Pen" /> shall be a shallow copy of the <c><paramref name="other" /></c>.</para>
        ///     <para>Calling this constructor is essentially the same as calling the <see cref="Pen(Pen, Boolean)" /> constructor as:</para>
        ///     <code>
        ///         <see cref="Pen" />(<paramref name="other" />, <paramref name="other" />.Interned)
        ///     </code>
        /// </remarks>
        public Pen(Pen other) : this(other!, other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.Interned)
        {
        }

        /// <summary>Creates a pen by retrieving the serialisation <c><paramref name="info" /></c>.</summary>
        /// <param name="info">The <see cref="SerializationInfo" /> from which to read data.</param>
        /// <param name="context">The source of this deserialisation.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="info" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the original <see cref="Pen" />'s tokens are all interned (<see cref="Interned" />), the deserialised <see cref="Pen" />'s tokens are also going to be interned. To avoid this, use the <see cref="Pen(Pen, Boolean)" /> constructor before the serialisation to create a new <see cref="Pen" /> with a different interning policy.</para>
        ///     <para>The <see cref="Pen" />'s <see cref="Comparer" /> property might not always be fully (de)serialisable. Amongst standard <see cref="StringComparer" />s, only <see cref="StringComparer.InvariantCulture" />, <see cref="StringComparer.InvariantCultureIgnoreCase" />, <see cref="StringComparer.Ordinal" /> and <see cref="StringComparer.OrdinalIgnoreCase" /> may be serialised/deserialised. Otherwise, if a custom <see cref="StringComparer" /> is used, the <see cref="SerializableAttribute" /> must be applied to its <see cref="Type" /> and/or it must implement the <see cref="ISerializable" /> interface.</para>
        ///     <para>Because of performance reasons, no value is checked when deserialising data from the <c><paramref name="info" /></c>—it is assumed that all values are <em>legal</em> and <em>valid</em>. Deserialising data retrieved by actually serialising a <see cref="Pen" /> shall result in a valid <see cref="Pen" /> equivalent to the original (provided the <see cref="StringComparer" /> was successfully serialised and deserialised); any other deserialisation would probably fail or result in a <see cref="Pen" /> with unexpected behaviour.</para>
        ///     <para>The exceptions thrown by the <c><paramref name="info" /></c>'s <see cref="SerializationInfo" /> methods are not caught.</para>
        ///     <para><strong>Nota bene.</strong> Only member properties are serialised/deserialised. Random state of the internal (pseudo-)random number generator, which is used in the <see cref="Pen.Render(Int32, Nullable{Int32})" /> method, is not serialised/deserialised.</para>
        /// </remarks>
        private Pen(SerializationInfo info, StreamingContext context) : base()
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage);
            }

            // Deserialise the interning policy.
            _interned = info.GetBoolean(nameof(Interned));

            // Deserialise the `Comparer`.
            {
                StringComparer comparer;
                try
                {
                    Type comparerType = Type.GetType(info.GetString(nameof(StringComparer)));
                    comparer = (StringComparer)info.GetValue(nameof(Comparer), comparerType);
                }
                catch (Exception exception) when (exception is SerializationException || exception is ArgumentNullException || exception is ArgumentException || exception is InvalidCastException)
                {
                    StringComparison comparisonType = (StringComparison)info.GetValue(nameof(Comparer), typeof(StringComparison));
#if NETSTANDARD2_1_OR_GREATER
                    comparer = StringComparer.FromComparison(comparisonType);
#else
                    comparer = StringComparerExtensions.GetComparerFromComparison(comparisonType);
#endif // NETSTANDARD2_1_OR_GREATER
                }
                if (comparer is null)
                {
                    throw new InvalidOperationException(ComparerNullErrorMessage);
                }

                _comparer = comparer;
            }

            // Deserialise the `Index`.
            {
                Int32[] indexArray = (Int32[])info.GetValue(nameof(Index), typeof(Int32[]));
                if (indexArray is null)
                {
                    throw new InvalidOperationException(IndexNullErrorMessage);
                }

                List<Int32> indexList = new List<Int32>(indexArray);
                indexList.TrimExcess();
                _index = indexList.AsReadOnly();
            }

            // Deserialise the `Context`.
            {
                String?[] contextArray = (String[])info.GetValue(nameof(Context), typeof(String[]));
                if (contextArray is null)
                {
                    throw new InvalidOperationException(ContextNullErrorMessage);
                }

                List<String?> contextList = new List<String?>(Interned ? contextArray.Select(StringExtensions.InternNullable) : contextArray);
                contextList.TrimExcess();
                _context = contextList.AsReadOnly();
            }

            // Deserialise the ending token.
            {
                String? sentinelToken = info.GetString(nameof(SentinelToken));
                _sentinelToken = Interned ? StringExtensions.InternNullable(sentinelToken) : sentinelToken;
            }
        }

        /// <summary>Creates a pen from externally initialised values.</summary>
        /// <param name="interned">The interning policy.</param>
        /// <param name="comparer">The <see cref="StringComparer" />.</param>
        /// <param name="index">The sorting index of the <c><paramref name="context" /></c> in respect of the <c><paramref name="comparer" /></c>.</param>
        /// <param name="context">The token context.</param>
        /// <param name="sentinelToken">The ending token.</param>
        /// <remarks>
        ///     <para><strong>Nota bene.</strong> This constructor is intended for <strong>internal use only</strong> (hence the access modifier <em><c>internal</c></em>). It is supposed to be used for <em>XML</em> and <em>JSON</em> serialisation and deserialisation of instances of class <see cref="Pen" />.</para>
        /// </remarks>
        internal Pen(Boolean interned, StringComparer comparer, IReadOnlyList<Int32> index, IReadOnlyList<String?> context, String? sentinelToken) : base()
        {
            _interned = interned;
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer), ComparerNullErrorMessage);
            _context = context ?? throw new ArgumentNullException(nameof(context), ContextNullErrorMessage);
            _index = index ?? throw new ArgumentNullException(nameof(index), IndexNullErrorMessage);
            _sentinelToken = sentinelToken;
        }

        /// <summary>Finds the positions of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>A collection of positions in the <see cref="Context" /> at which all of the occurrences of the <c><paramref name="sample" /></c> begin.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>For each position <c>p</c> in the returned collection, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. All such positions are contained in the returned collection (no position is disregarded). Therefore, if the returned collection is empty, the <c><paramref name="sample" /></c> does not occur in the <see cref="Context" />.</para>
        ///     <para>When enumerated, the returned collection of positions is unordered (no particular order of positions is guaranteed).</para>
        ///     <para>The method always returns a newly constructed collection of positions, even if the <c><paramref name="sample" /></c> is the same between two calls. Moreover, changes made to the returned collection affect neither the state of the <see cref="Pen" /> nor any other collection of positions returned by the method, past or future.</para>
        /// </remarks>
        public ICollection<Int32> PositionsOf(IEnumerable<String?> sample)
        {
            if (sample is null)
            {
                throw new ArgumentNullException(nameof(sample), SampleNullErrorMessage);
            }

            Int32 p = FindPositionIndexAndCount(Comparer, Context, Index, sample, out Int32 n);
            Int32 P = p + n;

#if NETSTANDARD2_1_OR_GREATER
            HashSet<Int32> positions = new HashSet<Int32>(n);
#else
            HashSet<Int32> positions = new HashSet<Int32>();
#endif // NETSTANDARD2_1_OR_GREATER
            for (Int32 i = p; i < P; ++i)
            {
                positions.Add(i < Context.Count ? Index[i] : Context.Count);
            }
            positions.TrimExcess();

            return positions;
        }

        /// <summary>Finds the positions of an empty token sample in the <see cref="Context" />.</summary>
        /// <returns>A collection of positions in the <see cref="Context" /> at which all of the occurrences of an empty token sample begin.</returns>
        /// <remarks>
        ///     <para>An empty token sample is assumed to exist at all positions in the <see cref="Context" />, including the value of its <see cref="IReadOnlyCollection{T}.Count" /> property.</para>
        ///     <para>When enumerated, the returned collection of positions is unordered (no particular order of positions is guaranteed).</para>
        ///     <para>The method always returns a newly constructed collection of positions. Moreover, changes made to the returned collection affect neither the state of the <see cref="Pen" /> nor any other collection of positions returned by the method, past or future.</para>
        /// </remarks>
        public ICollection<Int32> PositionsOf() =>
            PositionsOf(Enumerable.Empty<String?>());

        /// <summary>Finds the positions of the <c><paramref name="token" /></c> in the <see cref="Context" />.</summary>
        /// <param name="token">The token to find.</param>
        /// <returns>A collection of all positions in the <see cref="Context" /> at which the <c><paramref name="token" /></c> occurs.</returns>
        /// <remarks>
        ///     <para>For each position <c>p</c> in the returned collection, the <c><see cref="Context" />[p]</c> equals the <c><paramref name="token" /></c> in respect of the <see cref="Comparer" />. All such positions are contained in the returned collection (no position is disregarded). Therefore, if the returned collection is empty, the <c><paramref name="token" /></c> does not occur in the <see cref="Context" />.</para>
        ///     <para>When enumerated, the returned collection of positions is unordered (no particular order of positions is guaranteed).</para>
        ///     <para>The method always returns a newly constructed collection of positions, even if the <c><paramref name="token" /></c> is the same between two calls. Moreover, changes made to the returned collection affect neither the state of the <see cref="Pen" /> nor any other collection of positions returned by the method, past or future.</para>
        /// </remarks>
        public ICollection<Int32> PositionsOf(String? token) =>
            PositionsOf(Enumerable.Repeat(token, 1));

        /// <summary>Finds the positions of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>A collection of positions in the <see cref="Context" /> at which all of the occurrences of the <c><paramref name="sample" /></c> begin.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>For each position <c>p</c> in the returned collection, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. All such positions are contained in the returned collection (no position is disregarded). Therefore, if the returned collection is empty, the <c><paramref name="sample" /></c> does not occur in the <see cref="Context" />.</para>
        ///     <para>When enumerated, the returned collection of positions is unordered (no particular order of positions is guaranteed).</para>
        ///     <para>The method always returns a newly constructed collection of positions, even if the <c><paramref name="sample" /></c> is the same between two calls. Moreover, changes made to the returned collection affect neither the state of the <see cref="Pen" /> nor any other collection of positions returned by the method, past or future.</para>
        /// </remarks>
        public ICollection<Int32> PositionsOf(params String?[] sample) =>
            PositionsOf((IEnumerable<String?>)sample);

        /// <summary>Finds the first position of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the minimal position in the <see cref="Context" /> at which an occurrence of the <c><paramref name="sample" /></c> begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>p</c> is the returned position and <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the first occurrence of the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>first</em> if the value of the position <c>p</c> is minimal.</para>
        ///     <para>Unlike the <see cref="String.IndexOf(Char)" />, <see cref="Array.IndexOf{T}(T[], T)" />, <see cref="List{T}.IndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="sample" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 FirstPositionOf(IEnumerable<String?> sample)
        {
            if (sample is null)
            {
                throw new ArgumentNullException(nameof(sample), SampleNullErrorMessage);
            }

            Int32 p = FindPositionIndexAndCount(Comparer, Context, Index, sample, out Int32 n);

            if (n == 0)
            {
                return Context.Count;
            }

            Int32 P = p + n;

            Int32 minPosition = Int32.MaxValue;
            for (Int32 i = p; i < P; ++i)
            {
                if (Index[i] < minPosition)
                {
                    minPosition = Index[i];
                }
            }

            return minPosition;
        }

        /// <summary>Finds the first position of an empty token sample in the <see cref="Context" />.</summary>
        /// <returns>If an empty token sample is found in the <see cref="Context" />, the minimal position in the <see cref="Context" /> at which an occurrence of an empty token sample begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>An empty token sample is assumed to exist at all positions in the <see cref="Context" />, including the value of its <see cref="IReadOnlyCollection{T}.Count" /> property. An occurrence is considered <em>first</em> if the value of the position <c>p</c> is minimal.</para>
        /// </remarks>
        public Int32 FirstPositionOf() =>
            FirstPositionOf(Enumerable.Empty<String?>());

        /// <summary>Finds the first position of the <c><paramref name="token" /></c> in the <see cref="Context" />.</summary>
        /// <param name="token">The token to find.</param>
        /// <returns>If the <c><paramref name="token" /></c> is found in the <see cref="Context" />, the minimal position in the <see cref="Context" /> at which it occurs is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>If the <c><paramref name="token" /></c> is found in the <see cref="Context" />, the <c><see cref="Context" />[p]</c>, where <c>p</c> is the returned position, corresponds to the first occurrence of the <c><paramref name="token" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>first</em> if the value of the position <c>p</c> is minimal.</para>
        ///     <para>Unlike the <see cref="String.IndexOf(Char)" />, <see cref="Array.IndexOf{T}(T[], T)" />, <see cref="List{T}.IndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="token" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 FirstPositionOf(String? token) =>
            FirstPositionOf(Enumerable.Repeat(token, 1));

        /// <summary>Finds the first position of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the minimal position in the <see cref="Context" /> at which an occurrence of the <c><paramref name="sample" /></c> begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>p</c> is the returned position and <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the first occurrence of the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>first</em> if the value of the position <c>p</c> is minimal.</para>
        ///     <para>Unlike the <see cref="String.IndexOf(Char)" />, <see cref="Array.IndexOf{T}(T[], T)" />, <see cref="List{T}.IndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="sample" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 FirstPositionOf(params String?[] sample) =>
            FirstPositionOf((IEnumerable<String?>)sample);

        /// <summary>Finds the last position of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the maximal position in the <see cref="Context" /> at which an occurrence of the <c><paramref name="sample" /></c> begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>p</c> is the returned position and <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the last occurrence of the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>last</em> if the value of the position <c>p</c> is maximal.</para>
        ///     <para>Unlike the <see cref="String.LastIndexOf(Char)" />, <see cref="Array.LastIndexOf{T}(T[], T)" />, <see cref="List{T}.LastIndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="sample" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 LastPositionOf(IEnumerable<String?> sample)
        {
            if (sample is null)
            {
                throw new ArgumentNullException(nameof(sample), SampleNullErrorMessage);
            }

            Int32 p = FindPositionIndexAndCount(Comparer, Context, Index, sample, out Int32 n);

            if (n == 0)
            {
                return Context.Count;
            }

            Int32 P = p + n;

            Int32 maxPosition = Int32.MinValue;
            for (Int32 i = p; i < P; ++i)
            {
                if (Index[i] > maxPosition)
                {
                    maxPosition = Index[i];
                }
            }

            return maxPosition;
        }

        /// <summary>Finds the last position of an empty token sample in the <see cref="Context" />.</summary>
        /// <returns>If an empty token sample is found in the <see cref="Context" />, the maximal position in the <see cref="Context" /> at which an occurrence of an empty token sample begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>An empty token sample is assumed to exist at all positions in the <see cref="Context" />, including the value of its <see cref="IReadOnlyCollection{T}.Count" /> property. An occurrence is considered <em>last</em> if the value of the position <c>p</c> is maximal.</para>
        /// </remarks>
        public Int32 LastPositionOf() =>
            LastPositionOf(Enumerable.Empty<String?>());

        /// <summary>Finds the last position of the <c><paramref name="token" /></c> in the <see cref="Context" />.</summary>
        /// <param name="token">The token to find.</param>
        /// <returns>If the <c><paramref name="token" /></c> is found in the <see cref="Context" />, the maximal position in the <see cref="Context" /> at which it occurs is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <remarks>
        ///     <para>If the <c><paramref name="token" /></c> is found in the <see cref="Context" />, the <c><see cref="Context" />[p]</c>, where <c>p</c> is the returned position, corresponds to the last occurrence of the <c><paramref name="token" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>last</em> if the value of the position <c>p</c> is maximal.</para>
        ///     <para>Unlike the <see cref="String.LastIndexOf(Char)" />, <see cref="Array.LastIndexOf{T}(T[], T)" />, <see cref="List{T}.LastIndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="token" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 LastPositionOf(String? token) =>
            LastPositionOf(Enumerable.Repeat(token, 1));

        /// <summary>Finds the last position of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the maximal position in the <see cref="Context" /> at which an occurrence of the <c><paramref name="sample" /></c> begins is returned; otherwise the total number of tokens in the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>If the <c><paramref name="sample" /></c> is found in the <see cref="Context" />, the collection <c>{ <see cref="Context" />[p], <see cref="Context" />[p + 1], ..., <see cref="Context" />[p + n] }</c>, where <c>p</c> is the returned position and <c>n</c> is the length of the <c><paramref name="sample" /></c>, corresponds to the last occurrence of the <c><paramref name="sample" /></c> in respect of the <see cref="Comparer" />. An occurrence is considered <em>last</em> if the value of the position <c>p</c> is maximal.</para>
        ///     <para>Unlike the <see cref="String.LastIndexOf(Char)" />, <see cref="Array.LastIndexOf{T}(T[], T)" />, <see cref="List{T}.LastIndexOf(T)" /> etc. methods, the method <strong>does not</strong> return -1 if the <c><paramref name="sample" /></c> is not found, but instead returns the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property). This way the value returned by the method may be used as the parameter <c>fromPosition</c> in the <see cref="Render(Int32, Func{Int32, Int32}, Nullable{Int32})" />, <see cref="Render(Int32, System.Random, Nullable{Int32})" /> and <see cref="Render(Int32, Nullable{Int32})" /> methods to achieve a somewhat expected result (no tokens shall be rendered) without causing any exceptions.</para>
        /// </remarks>
        public Int32 LastPositionOf(params String?[] sample) =>
            LastPositionOf((IEnumerable<String?>)sample);

        /// <summary>Finds the total number of occurrences of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>The total number of occurrences of the <c><paramref name="sample" /></c> in the <see cref="Context" />. A value of 0 indicates that no occurance is found.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The occurances are searched in respect of the <see cref="Comparer" />.</para>
        ///     <para>Calling this method is slightly faster and more memory efficient than counting the number of elements in the collection returned by the <see cref="PositionsOf(IEnumerable{String})" /> method (even if using the <see cref="ICollection{T}.Count" /> property) because no actual collection needs to be created. However, the values are ultimately the same—for instance, prefer the <see cref="PositionsOf(IEnumerable{String})" /> method if both the positions and their quantity is needed.</para>
        /// </remarks>
        public Int32 Count(IEnumerable<String?> sample)
        {
            if (sample is null)
            {
                throw new ArgumentNullException(nameof(sample), SampleNullErrorMessage);
            }

            FindPositionIndexAndCount(Comparer, Context, Index, sample, out Int32 count);

            return count;
        }

        /// <summary>Finds the total number of occurrences of an empty token sample in the <see cref="Context" />.</summary>
        /// <returns>The total number of occurrences of an empty token sample in the <see cref="Context" />. A value of 0 indicates that no occurance is found.</returns>
        /// <remarks>
        ///     <para>An empty token sample is assumed to exist at all positions in the <see cref="Context" />, including the value of its <see cref="IReadOnlyCollection{T}.Count" /> property. An occurrence is considered <em>first</em> if the value of the position <c>p</c> is minimal.</para>
        ///     <para>Calling this method is slightly faster and more memory efficient than counting the number of elements in the collection returned by the <see cref="PositionsOf()" /> method (even if using the <see cref="ICollection{T}.Count" /> property) because no actual collection needs to be created. However, the values are ultimately the same—for instance, prefer the <see cref="PositionsOf()" /> method if both the positions and their quantity is needed.</para>
        /// </remarks>
        public Int32 Count() =>
            Count(Enumerable.Empty<String?>());

        /// <summary>Finds the total number of occurrences of the <c><paramref name="token" /></c> in the <see cref="Context" />.</summary>
        /// <param name="token">The token to find.</param>
        /// <returns>The total number of occurrences of the <c><paramref name="token" /></c> in the <see cref="Context" />. A value of 0 indicates that no occurance is found.</returns>
        /// <remarks>
        ///     <para>The occurances are searched in respect of the <see cref="Comparer" />.</para>
        ///     <para>Calling this method is slightly faster and more memory efficient than counting the number of elements in the collection returned by the <see cref="PositionsOf(String)" /> method (even if using the <see cref="ICollection{T}.Count" /> property) because no actual collection needs to be created. However, the values are ultimately the same—for instance, prefer the <see cref="PositionsOf(String)" /> method if both the positions and their quantity is needed.</para>
        /// </remarks>
        public Int32 Count(String? token) =>
            Count(Enumerable.Repeat(token, 1));

        /// <summary>Finds the total number of occurrences of the <c><paramref name="sample" /></c> in the <see cref="Context" />.</summary>
        /// <param name="sample">The sample enumerable of tokens to find.</param>
        /// <returns>The total number of occurrences of the <c><paramref name="sample" /></c> in the <see cref="Context" />. A value of 0 indicates that no occurance is found.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="sample" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The occurances are searched in respect of the <see cref="Comparer" />.</para>
        ///     <para>Calling this method is slightly faster and more memory efficient than counting the number of elements in the collection returned by the <see cref="PositionsOf(String[])" /> method (even if using the <see cref="ICollection{T}.Count" /> property) because no actual collection needs to be created. However, the values are ultimately the same—for instance, prefer the <see cref="PositionsOf(String[])" /> method if both the positions and their quantity is needed.</para>
        /// </remarks>
        public Int32 Count(params String?[] sample) =>
            Count((IEnumerable<String?>)sample);

        /// <summary>Renders (generates) a block of text from the <see cref="Context" />.</summary>
        /// <param name="relevantTokens">The number of (most recent) relevant tokens. The value must be greater than or equal to 0.</param>
        /// <param name="picker">The random number generator. When passed an integer <c>n</c> (greater than or equal to 0) as the argument, it should return an integer greater than or equal to 0 but (strictly) less than <c>n</c>; if <c>n</c> equals 0, 0 should be returned.</param>
        /// <param name="fromPosition">If set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached) without calling the <c><paramref name="picker" /></c> function; otherwise the first token is chosen by immediately calling the <c><paramref name="picker" /></c> function. The value must be greater than or equal to 0 but less than or equal to the total number of tokens in the <see cref="Context" />.</param>
        /// <returns>An enumerable of tokens generated from the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="picker" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item><c><paramref name="relevantTokens" /></c> is less than 0,</item>
        ///         <item><c><paramref name="fromPosition" /></c> is less than 0 or greater than the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property), or</item>
        ///         <item>the <c><paramref name="picker" /></c> function returns a value outside the legal range.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>If <c><paramref name="fromPosition" /></c> is set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen accordingly; otherwise they are chosen by calling the <c><paramref name="picker" /></c> function. Each consecutive token is chosen by observing the most recent <c><paramref name="relevantTokens" /></c> tokens (or the number of generated tokens if <c><paramref name="relevantTokens" /></c> tokens have not yet been generated) and choosing one of the possible successors by calling the <c><paramref name="picker" /></c> function. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.</para>
        ///     <para>An extra copy of <c><paramref name="relevantTokens" /></c> tokens is kept when generating new tokens. Memory errors may occur if the parameter <c><paramref name="relevantTokens" /></c> is too large.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). The query returned is not run until enumerating it, such as via explicit calls to the <see cref="IEnumerable{T}.GetEnumerator()" /> method, a <c>foreach</c> loop, a call to the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. If <c><paramref name="picker" /></c> is not a deterministic function, two distinct enumerators over the query may return different results.</para>
        ///     <para>It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as the <see cref="Array" /> or <see cref="List{T}" /> of <see cref="String" />s, or concatenated together into a <see cref="String" /> to avoid memory errors. This may be done by calling the <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.</para>
        ///     <para>The enumeration of tokens shall immediately stop, without rendering any tokens, if:</para>
        ///     <list type="number">
        ///         <item>the <see cref="Context" /> is empty,</item>
        ///         <item>all tokens in the <see cref="Context" /> are ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case), or</item>
        ///         <item>a <em>successor</em> of the last token or an ending token is picked first, which may be manually triggered (regardless of the <c><paramref name="picker" /></c> function) by passing the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property) as the value of the parameter <c><paramref name="fromPosition" /></c>.</item>
        ///     </list>
        /// </remarks>
        public IEnumerable<String?> Render(Int32 relevantTokens, Func<Int32, Int32> picker, Nullable<Int32> fromPosition = default)
        {
            if (picker is null)
            {
                throw new ArgumentNullException(nameof(picker), PickerNullErrorMessage);
            }
            if (relevantTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relevantTokens), relevantTokens, String.Format(CultureInfo.CurrentCulture, RelevantTokensOutOfRangeFormatErrorMessage, 0));
            }

            // Initialise the list of the `relevantTokens` most recent tokens and its first position (the list will be cyclical after rendering `relevantTokens` tokens).
            List<String?> text = new List<String?>(Math.Max(relevantTokens, 1));
            Int32 c = 0;

            // Render the first token, or the deterministically defined first `relevantTokens` if needed.
            if (fromPosition.HasValue)
            {
                if (fromPosition.Value < 0 || fromPosition.Value > Context.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(fromPosition), fromPosition.Value, String.Format(CultureInfo.CurrentCulture, FromPositionOutOfRangeFormatErrorMessage, 0, Context.Count));
                }

                Int32 next;
                Int32 i;
                for ((next, i) = ValueTuple.Create(fromPosition.Value, 0); i < text.Capacity; ++next, ++i)
                {
                    String? nextToken = next < Context.Count ? Context[next] : SentinelToken;
                    if (Comparer.Equals(nextToken, SentinelToken))
                    {
                        yield break;
                    }

                    yield return nextToken;
                    text.Add(nextToken);
                }
            }
            else
            {
                Int32 pick = picker(Context.Count + 1);
                if (pick < 0 || pick > Context.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), pick, String.Format(CultureInfo.CurrentCulture, PickOutOfRangeFormatErrorMessage, 0, Context.Count + 1, 0, 0));
                }

                Int32 first = pick < Context.Count ? Index[pick] : Context.Count;
                String? firstToken = first < Context.Count ? Context[first] : SentinelToken;
                if (Comparer.Equals(firstToken, SentinelToken))
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
                Int32 p; // the first position (index of `Positions`) of the most recent `relevantTokens` tokens rendered;                 0 if `relevantTokens == 0`
                Int32 n; // the number of the most recent `relevantTokens` tokens rendered occurrences in `Tokens`;                         `Tokens.Count` + 1 if `relevantTokens == 0`
                Int32 d; // the distance (in number of tokens) between the first relevant token and the next to render (`relevantTokens`); 0 if `relevantTokens == 0`
                    // until `relevantTokens` tokens have not yet been rendered, `text.Count` is used as the number of relevant tokens, i. e. all rendered tokens are relevant

                // Find the values according to the `relevantTokens`.
                if (relevantTokens == 0)
                {
                    p = 0;
                    n = Context.Count + 1;
                    d = 0;
                }
                else
                {
                    p = FindPositionIndexAndCount(Comparer, Context, Index, text, c, out n);
                    d = text.Count; // note that `text` shall never hold more than `relevantTokens` tokens
                }

                // Render the next token.

                Int32 pick = picker(n);
                if (pick < 0 || pick >= Math.Max(n, 1)) // actually, `n` should never be 0
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), pick, String.Format(CultureInfo.CurrentCulture, PickOutOfRangeFormatErrorMessage, 0, n, 0, 0));
                }
                pick += p;

                Int32 next = pick < Context.Count ? Index[pick] + d : Context.Count;
                String? nextToken = next < Context.Count ? Context[next] : SentinelToken;
                if (Comparer.Equals(nextToken, SentinelToken))
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
                    c = (c + 1) % text.Count; // <-- cyclicity of the list `text`
                }
            }
        }

        /// <summary>Renders (generates) a block of text from the <see cref="Context" />.</summary>
        /// <param name="relevantTokens">The number of (most recent) relevant tokens. The value must be greater than or equal to 0.</param>
        /// <param name="random">The (pseudo-)random number generator.</param>
        /// <param name="fromPosition">If set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached) without using the <c><paramref name="random" /></c>; otherwise the first token is chosen by immediately calling the <see cref="System.Random.Next(Int32)" /> method of the <c><paramref name="random" /></c>. The value must be greater than or equal to 0 but less than or equal to the total number of tokens in the <see cref="Context" />.</param>
        /// <returns>An enumerable of tokens generated from the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="random" /></c> parameter is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item><c><paramref name="relevantTokens" /></c> is less than 0, or</item>
        ///         <item><c><paramref name="fromPosition" /></c> is less than 0 or greater than the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property).</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>If no specific <see cref="System.Random" /> instance or seed should be used, the <see cref="Render(Int32, Nullable{Int32})" /> method could be used instead. The <c><paramref name="random" /> is used for choosing tokens to render.</c></para>
        ///     <para>If <c><paramref name="fromPosition" /></c> is set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen accordingly; otherwise they are chosen by calling the <see cref="System.Random.Next(Int32)" /> method of the <c><paramref name="random" /></c>. Each consecutive token is chosen by observing the most recent <c><paramref name="relevantTokens" /></c> tokens (or the number of generated tokens if <c><paramref name="relevantTokens" /></c> tokens have not yet been generated) and choosing one of the possible successors by calling the <see cref="System.Random.Next(Int32)" /> method of the <c><paramref name="random" /></c>. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.</para>
        ///     <para>An extra copy of <c><paramref name="relevantTokens" /></c> tokens is kept when generating new tokens. Memory errors may occur if the parameter <c><paramref name="relevantTokens" /></c> is too large.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). The query returned is not run until enumerating it, such as via explicit calls to the <see cref="IEnumerable{T}.GetEnumerator()" /> method, a <c>foreach</c> loop, a call to the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.</para>
        ///     <para>It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as the <see cref="Array" /> or <see cref="List{T}" /> of <see cref="String" />s, or concatenated together into a <see cref="String" /> to avoid memory errors. This may be done by calling the <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.</para>
        ///     <para>The enumeration of tokens shall immediately stop, without rendering any tokens, if:</para>
        ///     <list type="number">
        ///         <item>the <see cref="Context" /> is empty,</item>
        ///         <item>all tokens in the <see cref="Context" /> are ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case), or</item>
        ///         <item>a <em>successor</em> of the last token or an ending token is picked first, which may be manually triggered (regardless of the <c><paramref name="random" /></c>) by passing the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property) as the value of the parameter <c><paramref name="fromPosition" /></c>.</item>
        ///     </list>
        /// </remarks>
        public IEnumerable<String?> Render(Int32 relevantTokens, System.Random random, Nullable<Int32> fromPosition = default) =>
            random is null ? throw new ArgumentNullException(nameof(random), RandomNullErrorMessage) : Render(relevantTokens, random.Next, fromPosition);

        /// <summary>Renders (generates) a block of text from the <see cref="Context" />.</summary>
        /// <param name="relevantTokens">The number of (most recent) relevant tokens. The value must be greater than or equal to 0.</param>
        /// <param name="fromPosition">If set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen as <c>{ <see cref="Context" />[<paramref name="fromPosition" />], <see cref="Context" />[<paramref name="fromPosition" /> + 1], ..., <see cref="Context" />[<paramref name="fromPosition" /> + max(<paramref name="relevantTokens" />, 1) - 1] }</c> (or fewer if the index exceeds its limitation or an ending token is reached); otherwise the first token is chosen (pseudo-)randomly. The value must be greater than or equal to 0 but less than or equal to the total number of tokens in the <see cref="Context" />.</param>
        /// <returns>An enumerable of tokens generated from the <see cref="Context" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item><c><paramref name="relevantTokens" /></c> is less than 0, or</item>
        ///         <item><c><paramref name="fromPosition" /></c> is less than 0 or greater than the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property).</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The method is essentially the same as the <see cref="Render(Int32, Random, Nullable{Int32})" /> method, but uses an internal, thread-safe <see cref="System.Random" /> instance.</para>
        ///     <para>If <c><paramref name="fromPosition" /></c> is set, the first max(<c><paramref name="relevantTokens" /></c>, 1) tokens are chosen accordingly; otherwise they are chosen (pseudo-)randomly. Each consecutive token is chosen by observing the most recent <c><paramref name="relevantTokens" /></c> tokens (or the number of generated tokens if <c><paramref name="relevantTokens" /></c> tokens have not yet been generated) and choosing one of the next one (pseudo-)randomly. The process is repeated until the <em>successor</em> of the last token would be chosen or until the ending token (<see cref="SentinelToken" />) is chosen—the ending tokens are not rendered.</para>
        ///     <para>An extra copy of <c><paramref name="relevantTokens" /></c> tokens is kept when generating new tokens. Memory errors may occur if the parameter <c><paramref name="relevantTokens" /></c> is too large.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). The query returned is not run until enumerating it, such as via explicit calls to the <see cref="IEnumerable{T}.GetEnumerator()" /> method, a <c>foreach</c> loop, a call to the <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})" /> extension method etc. Since the point of <see cref="System.Random" /> class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.</para>
        ///     <para>It is advisable to manually set the upper bound of tokens to render if they are to be stored in a container, such as the <see cref="Array" /> or <see cref="List{T}" /> of <see cref="String" />s, or concatenated together into a <see cref="String" /> to avoid memory errors. This may be done by calling the <see cref="Enumerable.Take{TSource}(IEnumerable{TSource}, Int32)" /> extension method or by iterating a loop with a counter.</para>
        ///     <para>The enumeration of tokens shall immediately stop, without rendering any tokens, if:</para>
        ///     <list type="number">
        ///         <item>the <see cref="Context" /> is empty,</item>
        ///         <item>all tokens in the <see cref="Context" /> are ending tokens (mathematically speaking, this is a <em>supercase</em> of the first case), or</item>
        ///         <item>a <em>successor</em> of the last token or an ending token is picked first, which may be manually triggered by passing the total number of tokens in the <see cref="Context" /> (its <see cref="IReadOnlyCollection{T}.Count" /> property) as the value of the parameter <c><paramref name="fromPosition" /></c>.</item>
        ///     </list>
        /// </remarks>
        public IEnumerable<String?> Render(Int32 relevantTokens, Nullable<Int32> fromPosition = default) =>
            Render(relevantTokens, RandomNext, fromPosition); // not `Render(relevantTokens, Random, fromPosition)` to avoid accessing the thread-static (pseudo-)random number generator (`Pen.Random`) from multiple threads if the returned query (enumerable) is enumerated from multiple threads

        /// <summary>Populates the serialisation <c><paramref name="info" /></c> with data needed to serialise the current pen.</summary>
        /// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination for this serialisation.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="info" /></c> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Comparer" /> is non-serialisable.</exception>
        /// <remarks>
        ///     <para>If the current <see cref="Pen" />'s tokens are all interned (<see cref="Interned" />), the deserialised <see cref="Pen" />'s tokens are also going to be interned. To avoid this, use the <see cref="Pen(Pen, Boolean)" /> constructor before the serialisation to create a new <see cref="Pen" /> with a different interning policy.</para>
        ///     <para>The <see cref="Pen" />'s <see cref="Comparer" /> property might not always be fully (de)serialisable. Amongst standard <see cref="StringComparer" />s, only <see cref="StringComparer.InvariantCulture" />, <see cref="StringComparer.InvariantCultureIgnoreCase" />, <see cref="StringComparer.Ordinal" /> and <see cref="StringComparer.OrdinalIgnoreCase" /> may be serialised/deserialised. Otherwise, if a custom <see cref="StringComparer" /> is used, the <see cref="SerializableAttribute" /> must be applied to its <see cref="Type" /> and/or it must implement the <see cref="ISerializable" /> interface.</para>
        ///     <para>The exceptions thrown by the <c><paramref name="info" /></c>'s <see cref="SerializationInfo" /> methods are not caught.</para>
        ///     <para><strong>Nota bene.</strong> Only member properties are serialised/deserialised. Random state of the internal (pseudo-)random number generator, which is used in the <see cref="Pen.Render(Int32, Nullable{Int32})" /> method, is not serialised/deserialised.</para>
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage);
            }

            // Serialise the interning policy.
            info.AddValue(nameof(Interned), Interned);

            // Serialise the `Comparer`.
            if (Comparer.TryGetComparison(out StringComparison comparisonType))
            {
                info.AddValue(nameof(Comparer), comparisonType);
            }
            else if (Comparer is ISerializable || Comparer.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Any())
            {
                info.AddValue(nameof(StringComparer), Comparer.GetType().AssemblyQualifiedName);
                info.AddValue(nameof(Comparer), Comparer);
            }
            else
            {
                throw new InvalidOperationException(NonSerialisableComparerErrorMessage);
            }

            // Serialise the `Index`.
            {
                Int32[] indexArray = new Int32[Index.Count];
                for (Int32 i = 0; i < Context.Count; ++i)
                {
                    indexArray[i] = Index[i];
                }
                info.AddValue(nameof(Index), indexArray, typeof(Int32[]));
            }

            // Serialise the `Context`.
            {
                String?[] contextArray = new String?[Context.Count];
                for (Int32 i = 0; i < Context.Count; ++i)
                {
                    contextArray[i] = Context[i];
                }
                info.AddValue(nameof(Context), contextArray, typeof(String[]));
            }

            // Serialise the ending token.
            info.AddValue(nameof(SentinelToken), SentinelToken, typeof(String));
        }

        /// <summary>Creates a new <see cref="Pen" /> that is a copy of the current pen.</summary>
        /// <returns>A new <see cref="Pen" /> with the same values.</returns>
        public Pen Clone() =>
            new Pen(this);

        /// <summary>Creates a new <see cref="Object" /> that is a copy of the current pen.</summary>
        /// <returns>A new <see cref="Pen" /> with the same values.</returns>
        Object ICloneable.Clone() =>
            Clone();
    }
}
