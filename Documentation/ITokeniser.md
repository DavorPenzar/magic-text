# ```ITokeniser``` Interface

Interface for tokenising (*shattering* into tokens) input texts.

## Methods

### Shatter(StreamReader, [ShatteringOptions? = null])

Shatter text read from ```input``` into tokens synchronously.

Signature:

```csharp
IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null)

```

#### Parameters

1.  ```input```: ```StreamReader``` &ndash; Stream for reading the input text.
2.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```IEnumerable<String?>``` &ndash; Enumerable of tokens (in the order they were read) read from ```input```.

#### Remarks

The method should ultimately return the same results as ```ShatterAsync(StreamReader, ShatteringOptions?)``` called with the same parameters.

### ShatterAsync(StreamReader, [ShatteringOptions? = null])

Shatter text read from ```input``` into tokens asynchronously.

Signature:

```csharp
Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null)

```

#### Parameters

1.  ```input```: ```StreamReader``` &ndash; Stream for reading the input text.
2.  ```options```: ```ShatteringOptions```, optional &ndash; Shattering options. If ```null```, defaults are used.

#### Returns

```Task<IEnumerable<String?>>``` &ndash; Task whose result is enumerable of tokens (in the order they were read) read from ```input```.

#### Remarks

The method should ultimately return the same results as ```ShatterAsync(StreamReader, ShatteringOptions?)``` called with the same parameters.

## Remarks

When implementing a custom class of ```ITokeniser``` interface, make sure [```Shatter(StreamReader, ShatteringOptions?)```](#shatterstreamreader-shatteringoptions--null) and [```ShatterAsync(StreamReader, ShatteringOptions?)```](#shatterasyncstreamreader-shatteringoptions--null) methods return fully built ```IEnumerable<String?>```s, such as a ```String?[]```, a ```List<String>``` or a ```LinkedList<String>```. If the methods return a query, as when implementing with ```yield return```s, extension methods provided by ```Tokeniser``` static class may not work.
