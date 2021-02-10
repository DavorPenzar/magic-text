# ```Tokeniser``` Class

Static class with convenient extension methods for instances of [```ITokeniser```](ITokeniser.md) interface.

## Methods

### Shatter(this ITokeniser, String, Encoding[, ShatteringOptions? = null])

Shatter ```text``` decoded by ```encoding``` into tokens synchronously.

Signature:

```csharp
static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)

```

#### Parameters

1.  ```tokeniser```: [```ITokeniser```](ITokeniser.md) &ndash; Tokeniser used for shattering.
2.  ```text```: ```String``` &ndash; Input text.
3.  ```encoding```: ```Encoding``` &ndash; Encoding used for decoding ```text``` via ```Encoding.GetBytes(String)``` method.
4.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```IEnumerable<String?>``` &ndash; Enumerable of tokens (in the order they were read) read from ```text```.

### Shatter(this ITokeniser, String[, ShatteringOptions? = null])

Shatter ```text``` decoded by ```Encoding.Default``` into tokens synchronously.

Signature:

```csharp
static IEnumerable<String?> Shatter(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)

```

Decoding is done by calling ```Encoding.Default.GetBytes(String)``` method with ```text``` passed as the parameter.

#### Parameters

1.  ```tokeniser```: [```ITokeniser```](ITokeniser.md) &ndash; Tokeniser used for shattering.
2.  ```text```: ```String``` &ndash; Input text.
3.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```IEnumerable<String?>``` &ndash; Enumerable of tokens (in the order they were read) read from ```text```.

### ShatterAsync(this ITokeniser, String, Encoding[, ShatteringOptions? = null])

Shatter ```text``` decoded by ```encoding``` into tokens asynchronously.

Signature:

```csharp
static Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)

```

#### Parameters

1.  ```tokeniser```: [```ITokeniser```](ITokeniser.md) &ndash; Tokeniser used for shattering.
2.  ```text```: ```String``` &ndash; Input text.
3.  ```encoding```: ```Encoding``` &ndash; Encoding used for decoding ```text``` via ```Encoding.GetBytes(String)``` method.
4.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```Task<IEnumerable<String?>>``` &ndash; Task whose result is enumerable of tokens (in the order they were read) read from ```input```.

### ShatterAsync(this ITokeniser, String[, ShatteringOptions? = null])

Shatter ```text``` decoded by ```Encoding.Default``` into tokens asynchronously.

Signature:

```csharp
static Task<IEnumerable<String?>> ShatterAsync(this ITokeniser tokeniser, string text, Encoding encoding, ShatteringOptions? options = null)

```

Decoding is done by calling ```Encoding.Default.GetBytes(String)``` method with ```text``` passed as the parameter.

#### Parameters

1.  ```tokeniser```: [```ITokeniser```](ITokeniser.md) &ndash; Tokeniser used for shattering.
2.  ```text```: ```String``` &ndash; Input text.
3.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```Task<IEnumerable<String?>>``` &ndash; Task whose result is enumerable of tokens (in the order they were read) read from ```input```.

## Remarks

See [*Remarks* of ```ITokeniser``` interface](ITokeniser.md#remarks-2).
