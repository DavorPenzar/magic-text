using MagicText.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicText
{
    public class StringSplitTokeniser : LineByLineTokeniser
    {
        private const string SeparatorsNullErrorMessage = "Separators enumerable cannot be null.";
        private const string SeparatorEmptyOrNullErrorMessage = "Separators cannot be null or empty.";

        private readonly StringSplitOptions _options;
        private readonly String[] _separators;

        protected StringSplitOptions Options => _options;
        protected String[] Separators => _separators;

        public StringSplitTokeniser(IEnumerable<String> separators, StringSplitOptions options = StringSplitOptions.None) : base()
        {
            if (separators is null)
            {
                throw new ArgumentNullException(nameof(separators), SeparatorsNullErrorMessage);
            }

            String[] separatorsArray = separators.DistinctSorted(StringComparer.Ordinal);
            if (separatorsArray.Length != 0 && String.IsNullOrEmpty(separatorsArray[0]))
            {
                throw new ArgumentException(SeparatorEmptyOrNullErrorMessage, nameof(separators));
            }

            _separators = separatorsArray;
            _options = options;
        }

        public StringSplitTokeniser() : this(Enumerable.Empty<String>())
        {
        }

        public StringSplitTokeniser(String separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator, 1), options)
        {
        }

        public StringSplitTokeniser(StringSplitOptions options) : this(Enumerable.Empty<String>(), options)
        {
        }

        public StringSplitTokeniser(params String[] separators) : this((IEnumerable<String>)separators)
        {
        }

        public StringSplitTokeniser(IEnumerable<Char> separators, StringSplitOptions options = StringSplitOptions.None) : this(separators?.Select(Char.ToString)!, options)
        {
        }

        public StringSplitTokeniser(Char separator, StringSplitOptions options = StringSplitOptions.None) : this(Enumerable.Repeat(separator.ToString(), 1), options)
        {
        }

        public StringSplitTokeniser(params Char[] separators) : this(separators?.Select(Char.ToString)!)
        {
        }

        protected sealed override IEnumerable<String?> ShatterLine(String line)
        {
            if (line is null)
            {
                throw new ArgumentNullException(nameof(line), LineNullErrorMessage);
            }

            return line.Split(Separators, Options);
        }
    }
}
