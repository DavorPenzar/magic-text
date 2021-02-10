# Documentation

The library provides two functionalities: text tokenisation and text generation. Hence the documentation shall be divided into these two parts.

## Text Tokenisation

Tokenisation of input texts is called *shattering* the text (into tokens) throughout the rest of the documentation. The term was chosen to avoid code ambiguity with some other, more sophisticated and more common packages used in projects referencing this library.

### Interfaces

1.  [```ITokeniser```](ITokeniser.md) &ndash; Interface for shattering input texts into tokens.

### Classes

1.  [```ShatteringOptions```](ShatteringOptions.md) &ndash; Options for ```ITokeniser.Shatter(StreamReader, ShatteringOptions?)``` and ```ITokeniser.ShatterAsync(StreamReader, ShatteringOptions?)``` methods.
2.  [```Tokeniser```](Tokeniser.md) &ndash; Static class with convenient extension methods for instances of ```ITokeniser``` interface.
3.  [```CharTokeniser```](SharTokeniser.md) &ndash; Tokeniser which shatters text at each character.
4.  [```RegexTokeniser```](RegexTokeniser.md) &ndash; Tokeniser which shatters text at specific regular expression pattern matches.

## Text Generation

### Classes

1.  [```Pen```](Pen.md) &ndash; Random text generator.
