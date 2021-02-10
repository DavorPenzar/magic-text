using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RandomText
{
    public class RegexTokeniser : ITokeniser
    {
        public const string DefaultBreakPattern = @"(\s+|[\.!\?‽¡¿⸘,:;\(\)\[\]\{\}\-—–]+|…)";

        public static Func<String?, String?> CreateReplacementTransformationFunction(string matchPattern, string replacementPattern, bool escapeMatch = false, bool escapeReplacement = false, RegexOptions options = RegexOptions.None)
        {
            var replace = new Regex(escapeMatch ? Regex.Escape(matchPattern) : matchPattern, options);
            if (escapeReplacement)
            {
                replacementPattern = Regex.Escape(replacementPattern);
            }

            return t => t is null ? null : replace.Replace(t, replacementPattern);
        }

        private readonly Regex _break = new Regex(DefaultBreakPattern, RegexOptions.None);
        private readonly Func<String?, String?>? _transform = null;

        protected Regex Break => _break;

        public String BreakPattern => _break.ToString();
        public Func<String?, String?>? Transform => _transform;

        public RegexTokeniser()
        {
        }

        public RegexTokeniser(String breakPattern, Func<String?, String?>? transform = null, bool escape = false, RegexOptions options = RegexOptions.None)
        {
            _break = new Regex(escape ? Regex.Escape(breakPattern) : breakPattern, options);
            _transform = transform;
        }

        public RegexTokeniser(Regex @break, Func<String?, String?>? transform = null, RegexOptions? alterOptions = null)
        {
            _break = alterOptions is null ? @break : new Regex(@break.ToString(), (RegexOptions)alterOptions!);
            _transform = transform;
        }

        private void ShatterLine(List<String?> tokens, String line, ShatteringOptions options)
        {
            if (!options.IgnoreLineEnds && tokens.Any())
            {
                tokens.Add(options.LineEndToken);
            }

            IEnumerable<String?> lineTokens = Break.Split(line);
            if (!(Transform is null))
            {
                lineTokens = lineTokens.Select(t => Transform(t));
            }
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

                ShatterLine(tokens, line, options);
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

                ShatterLine(tokens, line, options);
            }

            tokens.TrimExcess();

            return tokens;
        }
    }
}
