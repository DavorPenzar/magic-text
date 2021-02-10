# ```RegexTokeniser``` Class

Tokeniser which shatters text at specific regular expression pattern matches.

Implements:

*  [```ITokeniser```](ITokeniser.md)

Additionally, the tokeniser provides a possibility to transform tokens immediately after extraction of regular expression matches and prior to checking for empty tokens. Initially, the idea was to also use regular expressions for the transformation, but a custom transformation function may be provided instead.

## Constants

### DefaultBreakPattern

```String``` &ndash; The default break pattern.

Value:

```csharp
const String DefaultBreakPattern = @"(\s+|[\.!\?‽¡¿⸘,:;\(\)\[\]\{\}\-—–]+|…)"

```

The pattern matches all non-empty continuous white space groups, punctuation symbol (periods, exclamation marks, question marks, colons, semicolons, brackets and dashes are included, but quotation marks are not) groups and the horizontal ellipsis. The pattern is enclosed in (round) brackets to be captured by ```Regex.Split(String)``` method.

## Static Methods

### CreateReplacementTransformationFunction(String, String[, Boolean = false, Boolean = false, RegexOptions = RegexOptions.None])

Create a transformation function of regular expression based replacement.

Signature:

```csharp
Func<String?, String?> CreateReplacementTransformationFunction(String matchPattern, String replacementPattern, Boolean escapeMatch = false, Boolean escapeReplacement = false, RegexOptions options = RegexOptions.None)

```

#### Parameters

1.  ```matchPattern```: ```String``` &ndash; Regular expression pattern to match.
2.  ```replacementPattern```: ```String``` &ndash; Regular expression pattern for replacement of matches captured by ```matchPattern```.
3.  ```escapeMatch```: ```Boolean```, optional &ndash; If ```true```, ```matchPattern``` is escaped via ```Regex.Escape(String)``` method before usage.
4.  ```escapeReplacement```: ```Boolean```, optional &ndash; If ```true```, ```replacementPattern``` is escaped via ```Regex.Escape(String)``` method before usage.
5.  ```options```: ```RegexOptions```, optional &ndash; Options passed to ```Regex.Regex(String, RegexOptions)``` constructor.

#### Returns

Function that returns ```null``` when passed a ```null```, otherwise performs the regular expression based replacement defined.

## Properties

### BreakPattern

```String``` &ndash; Regular expression break pattern used by the tokeniser.

Shattering a line of text ```line``` by the tokeniser, without transformation, filtering and replacement of empty lines and line ends, is equivalent to calling ```Regex.Split(line, BreakPattern)```.

### Transform

```Func<String?, String?>?``` &ndash; Transformation function used by the tokeniser. A ```null``` reference means no transformation function is used.

Transformation is done on raw regular expression pattern matches, before (potential) filtering of empty tokens.

### Break

```Regex``` (protected) &ndash; Regular expression breaker used by the tokeniser.

## Constructors

### RegexTokeniser()

Create a default ```RegexTokeniser```.

Signature:

```csharp
RegexTokeniser()

```

[```DefaultBreakPattern```](#defaultbreakpattern) is used as the regular expression break pattern ([```BreakPattern```](#breakpattern)) and no transformation function ([```Transform```](#transform)) is used.

### RegexTokeniser(String[, Func<String?, String?>? = null, Boolean = false, RegexOptions = RegexOptions.None])

Create a ```RegexTokeniser``` with provided options.

Signature:

```csharp
RegexTokeniser(String breakPattern, Func<String?, String?>? transform = null, Boolean escape = false, RegexOptions options = RegexOptions.None)

```

#### Parameters

1.  ```breakPattern```: ```String``` &ndash; Regular expression break pattern to use.
2.  ```transform```: ```Func<String?, String?>```, optional &ndash; Optional transformation function. If ```null```, no transformation function is used.
3.  ```escape```: ```Boolean```, optional &ndash; If ```true```, ```breakPattern``` is escaped via ```Regex.Escape(String)``` method before usage.
4.  ```options```: ```RegexOptions```, optional &ndash; Options passed to ```Regex.Regex(String, RegexOptions)``` constructor.

### RegexTokeniser(Regex[, Func<String?, String?>? = null, RegexOptions? = null])

Create a ```RegexTokeniser``` with provided options.

Signature:

```csharp
RegexTokeniser(Regex @break, Func<String?, String?>? transform = null, RegexOptions? alterOptions = null)

```

#### Parameters

1.  ```break```: ```Regex``` &ndash; Regular expression breaker to use.
2.  ```transform```: ```Func<String?, String?>```, optional &ndash; Optional transformation function. If ```null```, no transformation function is used.
4.  ```alterOptions```: ```RegexOptions```, optional &ndash; If ```null```, ```break```'s options are used (no new ```Regex``` is constructed); otherwise options passed to ```Regex.Regex(String, RegexOptions)``` constructor.

#### Remarks

Calling this constructor is essentially the same (performance aside) as calling ```RegexTokeniser.RegexTokeniser(breakPattern: @break.ToString(), transform: transform, options: alterOptions ?? @break.Options)```.

## Methods

### Shatter(StreamReader[, ShatteringOptions? = null])

Shatter text read from ```input``` into tokens synchronously.

Signature:

```csharp
IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null)

```

#### Parameters

1.  ```input```: ```StreamReader``` &ndash; Stream for reading the input text.
2.  ```options```: [```ShatteringOptions```](ShatteringOptions.md), optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```IEnumerable<String?>``` &ndash; Enumerable of tokens (in the order they were read) read from ```input```.

#### Remarks

Each line from the input is split into *raw* tokens via ```Regex.Split(String)``` method (using the internal regular expression breaker ([```Break```](#break)) defined on construction of the tokeniser). If a transformation function ([```Transform```](#transform)) is set, it is then used to transform each *raw* token. The filtering of empty tokens is done **after** the transformation.

Empty tokens are considered those tokens that yield ```true``` when checked via ```String.IsNullOrEmpty(String)```.

### ShatterAsync(StreamReader[, ShatteringOptions? = null])

Shatter text read from ```input``` into tokens asynchronously.

Signature:

```csharp
Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null)

```

#### Parameters

1.  ```input```: ```StreamReader``` &ndash; Stream for reading the input text.
2.  ```options```: [```ShatteringOptions```](ShatteringOptions.md), optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```Task<IEnumerable<String?>>``` &ndash; Task whose result is enumerable of tokens (in the order they were read) read from ```input```.

#### Remarks

Each line from the input is split into *raw* tokens via ```Regex.Split(String)``` method (using the internal regular expression breaker ([```Break```](#break)) defined on construction of the tokeniser). If a transformation function ([```Transform```](#transform)) is set, it is then used to transform each *raw* token. The filtering of empty tokens is done **after** the transformation.

Empty tokens are considered those tokens that yield ```true``` when checked via ```String.IsNullOrEmpty(String)```.

## Remarks

Shattering methods read and process text line-by-line with all CR, LF and CRLF line breaks treated the same. Consequently, as no inner buffer is used, regular expression breaking pattern cannot stretch over a line break, regardless of ```RegexOptions``` passed to the constructor.
