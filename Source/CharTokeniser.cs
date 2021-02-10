using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RandomText
{
    public class CharTokeniser : ITokeniser
    {
        public CharTokeniser()
        {
        }

        private void ShatterLine(ref List<String?> tokens, String line, ShatteringOptions options)
        {
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            var lineTokens = line.Select(c => c.ToString());
            if (options.IgnoreEmptyTokens)
            {
                lineTokens = lineTokens.Where(t => !String.IsNullOrEmpty(t));
            }
            lineTokens = lineTokens.ToList();

            if (lineTokens.Any())
            {
                tokens.AddRange(lineTokens);
            }
            else if (!options.IgnoreEmptyLines)
            {
                tokens.Add(options.EmptyLineToken);
            }
        }

        public IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null)
        {
            if (options is null)
            {
                options = new ShatteringOptions();
            }

            var tokens = new List<String?>();

            while (true)
            {
                var line = input.ReadLine();
                if (line is null)
                {
                    break;
                }

                ShatterLine(ref tokens, line, options);
            }

            tokens.TrimExcess();

            return tokens;
        }
        public async Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null)
        {
            if (options is null)
            {
                options = new ShatteringOptions();
            }

            var tokens = new List<String?>();

            while (true)
            {
                var line = await input.ReadLineAsync();
                if (line is null)
                {
                    break;
                }

                ShatterLine(ref tokens, line, options);
            }

            tokens.TrimExcess();

            return tokens;
        }
    }
}
