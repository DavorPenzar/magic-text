using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MagicText.Example
{
    public static class Program
    {
        public const int ExitSuccess = 0;
        public const int ExitFailure = -1;

        static Program()
        {
        }

        public static async Task<Int32> Main(String[] args)
        {
            String inputPath = (args is null || args.Length == 0) ? Path.Combine("Resources", "Bible.txt") : args[0];

            IEnumerable<String?> tokens;
            Pen pen;

            Console.WriteLine($"Input file: {inputPath}");

            try
            {
                tokens = await GetTokensAsync(inputPath);
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException)
            {
                await Console.Error.WriteLineAsync(exception.ToString());

                return ExitFailure;
            }

            pen = GetPen(tokens);

            Console.WriteLine($"Total tokens: {pen.Context.Count:D}");
            Console.WriteLine();

            GenerateText(pen);

            PrintOutputDelimiter();

            KeyValuePair<String, Double>[] probabilities;
            KeyValuePair<String, Double>[] informationContents;
            Double entropy;

            (probabilities, informationContents, entropy) = AnalyseTextInformation(pen);

            PrintOutputDelimiter();

            InspectEmpiricalProbabilities(pen, probabilities, informationContents);

            return ExitSuccess;
        }

        private static void PrintOutputDelimiter()
        {
            Console.WriteLine();
            Console.WriteLine("***");
            Console.WriteLine();
        }

        private static async Task<IEnumerable<String?>> GetTokensAsync(String inputPath)
        {
            IEnumerable<String?> tokens;

            Stream inputStream = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.None);
            await using (inputStream)
            {
                ITokeniser tokeniser = new CharTokeniser();
                tokens = await tokeniser.ShatterToArrayAsync(input: inputStream, options: new ShatteringOptions() { IgnoreEmptyTokens = true }).ConfigureAwait(false);
            }

            return tokens;
        }

        private static Pen GetPen(IEnumerable<String?> tokens) =>
            new Pen(context: tokens, intern: true);

        private static void GenerateText(Pen pen)
        {
            Console.WriteLine("GENERATED RANDOM TEXT");
            Console.WriteLine("========= ====== ====");
            Console.WriteLine();

            foreach (String? token in pen.Render(4, new Random(1000)).Take(300))
            {
                Console.Write(token);
            }
            Console.WriteLine();
        }

        private static ValueTuple<KeyValuePair<String, Double>[], KeyValuePair<String, Double>[], Double> AnalyseTextInformation(Pen pen)
        {
            static void PrintInformationTheoryData(KeyValuePair<String, Double>[] data, String entryStringFormat)
            {
                using (IEnumerator<KeyValuePair<String, Double>> enumerator = ((IEnumerable<KeyValuePair<String, Double>>)data).GetEnumerator())
                {
                    Boolean skipped = false;

                    for (Int32 i = 0; i < data.Length && enumerator.MoveNext(); ++i)
                    {
                        if (i >= 3 && i + 3 < data.Length)
                        {
                            if (!skipped)
                            {
                                Console.WriteLine("...");
                                skipped = true;
                            }

                            continue;
                        }

                        Console.WriteLine(entryStringFormat, enumerator.Current.Key, enumerator.Current.Value);
                    }
                }
            }

            Console.WriteLine("INFORMATION CONTENTS AND ENTROPY");
            Console.WriteLine("=========== ======== === =======");
            Console.WriteLine();

            HashSet<String?> tokenCollection = new HashSet<String?>(pen.Context);
            tokenCollection.TrimExcess();

            Dictionary<String, Double> probabilities = new Dictionary<String, Double>();
            Dictionary<String, Double> informationContents = new Dictionary<String, Double>();
            Double entropy;

            foreach (String? token in tokenCollection)
            {
                String? tokenRep = JsonSerializer.Serialize(token);

                Double prob = Convert.ToDouble(pen.Count(token)) / pen.Context.Count;
                Double info = -Math.Log2(prob);

                probabilities.Add(tokenRep, prob);
                informationContents.Add(tokenRep, info);
            }
            probabilities.TrimExcess();
            informationContents.TrimExcess();

            entropy = probabilities.Select(e => e.Value * informationContents[e.Key]).Sum();

            KeyValuePair<String, Double>[] orderedProbabilities = probabilities.OrderBy(e => e.Value).ThenBy(e => JsonSerializer.Deserialize<String>(e.Key)).ToArray();
            KeyValuePair<String, Double>[] orderedInformationContents = informationContents.OrderByDescending(e => e.Value).ThenBy(e => JsonSerializer.Deserialize<String>(e.Key)).ToArray();

            PrintInformationTheoryData(orderedProbabilities, "P ({0}) = {1:P2}");
            Console.WriteLine();

            PrintInformationTheoryData(orderedInformationContents, "I ({0}) = {1:N4}");
            Console.WriteLine();

            Console.WriteLine($"E = {entropy:N4}");

            return ValueTuple.Create(orderedProbabilities, orderedInformationContents, entropy);
        }

        private static void InspectEmpiricalProbabilities(Pen pen, KeyValuePair<String, Double>[] probabilities, KeyValuePair<String, Double>[] informationContents)
        {
            Console.WriteLine("EMPIRICAL PROBABILITIES");
            Console.WriteLine("========= =============");
            Console.WriteLine();

            Dictionary<String, Double> unorderedInformationContents = new Dictionary<String, Double>(informationContents);
            unorderedInformationContents.TrimExcess();

            String? predecessor = pen.SentinelToken;
            String? token = probabilities.Length switch
            {
                0 => pen.SentinelToken,
                _ => JsonSerializer.Deserialize<String>(probabilities[probabilities.Length >> 1].Key)
            };

            HashSet<String?> predecessorCandidates = new HashSet<String?>(pen.PositionsOf(token).Select(i => i != 0 ? pen.Context[i - 1] : pen.SentinelToken).Where(t => t != pen.SentinelToken));
            predecessorCandidates.TrimExcess();

            Double info = Double.NegativeInfinity;
            foreach (String? candidate in predecessorCandidates)
            {
                if (unorderedInformationContents[JsonSerializer.Serialize(candidate)] > info)
                {
                    predecessor = candidate;
                }
            }

            Double p1 = Convert.ToDouble(pen.Count(token)) / pen.Context.Count;
            Double p2 = Convert.ToDouble(pen.Count(predecessor)) / pen.Context.Count;
            Double p3 = Convert.ToDouble(pen.Count(predecessor, token)) / (pen.Context.Count - 1);

            Console.WriteLine($"P ({token}) = {p1:P2}");
            Console.WriteLine($"P ({token} | {predecessor}) = {p3 / p2:P2}");
        }
    }
}
