# ```CharTokeniser``` Class

Tokeniser which shatters text at each character.

Implements:

*   [```ITokeniser```](#ITokeniser.md)

## Constructors

### CharTokeniser()

Instantiate a ```CharTokeniser```.

Signature:

```csharp
CharTokeniser()

```

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

Empty tokens are considered those characters that yield ```true``` when converted to strings via ```Char.ToString()``` method and checked via ```String.IsNullOrEmpty(String)``` method.

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

Empty tokens are considered those characters that yield ```true``` when converted to strings via ```Char.ToString()``` method and checked via ```String.IsNullOrEmpty(String)``` method.

## Remarks

Shattering methods read and process text line-by-line with all CR, LF and CRLF line breaks treated the same.
