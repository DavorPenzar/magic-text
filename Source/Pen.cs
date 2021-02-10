using System;
using System.Collections.Generic;
using System.Linq;

namespace RandomText
{
    public class Pen
    {
        private const string EmptyTokensErrorMessage = "Cannot work with an empty enumerable of tokens.";
        private const string RelevantTokensOutOfRangeErrorMessage = "Number of relevant tokens must be non-negative (greater than or equal to 0).";
        private const string PickOutOfRangeErrorMessage = "Picker function must return an integer from [0, n) union {0}.";

        private static int FindPosition(StringComparer comparer, List<String?> tokens, List<Int32> positions, String? endToken, String? t)
        {
            bool end = comparer.Equals(t, endToken);

            int l = 0;
            int h = tokens.Count;
            int m;

            while (true)
            {
                m = (l + h) >> 1;

                var t2 = tokens[positions[m]];

                Int32? c = null;
                {
                    bool end2 = comparer.Equals(t2, endToken);
                    if (end && end2)
                    {
                        c = 0;
                    }
                    else if (end)
                    {
                        c = 1;
                    }
                    else if (end2)
                    {
                        c = -1;
                    }
                }
                if (c is null)
                {
                    c = comparer.Compare(t2, t);
                }

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

            while (m > 0 && comparer.Equals(tokens[positions[m - 1]], t))
            {
                --m;
            }

            return m;
        }
        private readonly StringComparer _comparer;
        private readonly List<String?> _tokens;
        private readonly List<Int32> _positions;
        private readonly String? _endToken;

        protected List<String?> Tokens => _tokens;
        protected List<Int32> Positions => _positions;

        public StringComparer Comparer => _comparer;
        public String? EndToken => _endToken;

        public Pen(IEnumerable<String?> tokens, StringComparer comparer, String? endToken = null)
        {
            _comparer = comparer;

            _tokens = new List<String?>(tokens);

            _endToken = endToken;

            _positions = Enumerable.Range(0, _tokens.Count).ToList();
            _positions.Sort(
                (i, j) =>
                {
                    if (i == j)
                    {
                        return 0;
                    }

                    while (i < Tokens.Count && j < Tokens.Count)
                    {
                        var t1 = Tokens[i];
                        var t2 = Tokens[j];

                        {
                            var ends = new Boolean[] { Comparer.Equals(t1, EndToken), Comparer.Equals(t2, EndToken) };
                            if (ends.All(f => f))
                            {
                                return 0;
                            }
                            else if (ends[0])
                            {
                                return -1;
                            }
                            else if (ends[1])
                            {
                                return 1;
                            }
                        }

                        int c = _comparer.Compare(t1, t2);
                        if (c != 0)
                        {
                            return c;
                        }

                        ++i;
                        ++j;
                    }

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

            _tokens.TrimExcess();
            _positions.TrimExcess();

            if (!Tokens.Any())
            {
                throw new ArgumentException(EmptyTokensErrorMessage, nameof(tokens));
            }
        }

        public Pen(IEnumerable<String?> tokens, String? endToken = null) : this(tokens, StringComparer.InvariantCulture, endToken)
        {
        }

        public IEnumerable<String?> Render(int relevantTokens, Func<Int32, Int32> picker)
        {
            if (relevantTokens < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relevantTokens), RelevantTokensOutOfRangeErrorMessage);
            }

            var text = new List<String?>(Math.Max(relevantTokens, 1));

            int c = 0;

            {
                Lazy<Boolean> allEnds = new Lazy<Boolean>(() => Tokens.All(t => Comparer.Equals(t, EndToken)));
                while (!text.Any())
                {
                    int pick = picker(Tokens.Count);
                    if (pick < 0 || pick >= Tokens.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                    }

                    var firstToken = Tokens[Positions[pick]];
                    if (Comparer.Equals(firstToken, EndToken))
                    {
                        if (allEnds.Value)
                        {
                            yield break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    text.Add(Tokens[Positions[pick]]);
                    yield return text[0];
                }
            }

            while (true)
            {
                int p;
                int n;
                int d;

                if (relevantTokens == 0)
                {
                    p = 0;
                    n = Tokens.Count;
                    d = 1;
                }
                else
                {
                    var t = text[c];

                    p = FindPosition(Comparer, Tokens, Positions, EndToken, t);
                    n = 0;

                    while (p + n < Tokens.Count)
                    {
                        int i = Positions[p + n];
                        int k;

                        for (k = 0; i + k < Tokens.Count && k < text.Count; ++k)
                        {
                            if (!Comparer.Equals(Tokens[i + k], text[(c + k) % text.Count]))
                            {
                                break;
                            }
                        }

                        if (k == text.Count)
                        {
                            ++n;
                        }
                        else if (n == 0)
                        {
                            ++p;
                        }
                        else
                        {
                            break;
                        }
                    }

                    d = text.Count;
                }

                int pick = picker(n);
                if (pick < 0 || pick >= Tokens.Count - p)
                {
                    throw new ArgumentOutOfRangeException(nameof(picker), PickOutOfRangeErrorMessage);
                }

                int next = Positions[p + picker(n)] + d;
                var nextToken = next < Tokens.Count ? Tokens[next] : EndToken;
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
                    c = (c + 1) % text.Count;
                }
            }
        }

        public IEnumerable<String?> Render(int relevantTokens, Random random) =>
            Render(relevantTokens, n => random.Next(n));
    }
}
