# ```Pen``` Class

Random text generator.

## Properties

### Comparer

```StringComparer``` &ndash; String comparer used by the pen for checking for equalities amongst tokens.

### EndToken

```String``` &ndash; Ending token of the pen.

If the ending token is picked in [```Render(Int32, Func<Int32, Int32>)```](#renderint32-funcint32-int32) or [```Render(Int32, Random)```](#renderint32-random) methods, rendering is stopped.

### Tokens

```List<String?>``` (protected) &ndash; Unsorted tokens of the pen. The order of tokens is kept as provided in the constructor.

### Positions

```List<Int32>``` (protected) &ndash; Sorting positions of entries in [```Tokens```](#tokens).

Generally speaking, if ```i < j```, then ```Comparer.Compare(Tokens[Positions[i]], Tokens[Positions[j]]) <= 0```; however, all tokens equal to [```EndToken```](#endtoken) (when compared with [```Comparer```](#comparer)) are considered less than any other token, including ```null```s if ```!Comparer.Equals(EndToken, null)```.

## Constructors

### Pen(IEnumerable<String?>, StringComparer[, String? = null])

Create a ```Pen``` with provided values.

Signature:

```csharp
Pen(IEnumerable<String?> tokens, StringComparer comparer, String? endToken = null)

```

#### Parameters

1.  ```tokens```: ```IEnumerable<String?>``` &ndash; Input tokens. Random text will be generated based on input tokens: both by picking only from the input tokens and by using the order of the input tokens.
2.  ```comparer```: ```StringComparer``` &ndash; String comparer. Equality of tokens shall be checked with ```comparer```.
3.  ```endToken```: ```String```, optional &ndash; Ending token.

### Pen(IEnumerable<String?>[, String? = null])

Create a ```Pen``` with provided values.

Signature:

```csharp
Pen(IEnumerable<String?> tokens, String? endToken = null)

```

```StringComparer.InvariantCulture``` is used as a comparer of tokens (to check for equality).

#### Parameters

1.  ```tokens```: ```IEnumerable<String?>``` &ndash; Input tokens. Random text will be generated based on input tokens: both by picking only from the input tokens and by using the order of the input tokens.
2.  ```endToken```: ```String```, optional &ndash; Ending token.

## Methods

### Render(Int32, Func<Int32, Int32>)

Render (generate) a block of text.

Signature:

```csharp
IEnumerable<String?> Render(Int32 relevantTokens, Func<Int32, Int32> picker)

```

The first token is chosen by calling ```picker``` function. Each consecutive token is chosen by observing the last ```relevantTokens``` tokens (or the number of generated tokens if ```relevantTokens``` tokens have not yet been generated) and choosing one of the possible successors by calling ```picker``` function. The process is repeated until the *successor* of the last token would be chosen or until the ending token ([```EndToken```](#endtoken)) is chosen&mdash;the ending tokens are not rendered.

#### Parameters

1.  ```relevantTokens```: ```Int32``` &ndash; Number of (last) relevant tokens.
2.  ```picker```: ```Func<Int32, Int32>``` &ndash; *Random* number generator. When passed an integer ```n``` (```>= 0```) as the argument, it should return an integer from range [0, max(```n```, 1)), i. e. greater than or equal to 0 but (strictly) less than max(```n```, 1).

#### Returns

A query for rendering tokens.

#### Exceptions

*   ```ArgumentOutOfRangeException```
    *   If ```relevantTokens``` is (strictly) negative.
    *   If ```picker``` returns a value outside of the legal range.

#### Remarks

An extra copy of ```relevantTokens``` tokens is kept when generating new tokens. Memory errors may occur if ```extraTokens``` is too large.

The query returned is not run until enumerating it (via explicit calls to ```IEnumerable<String?>.GetEnumerator()``` method, a ```foreach``` loop, a call to ```Enumerable.ToList(this IEnumerable<String?>)``` or ```Enumerable.ToArray(this IEnumerable<String?>)``` extension methods etc.). If ```picker``` is not a deterministic function, two distinct enumerators over the query may return different results.

It is advisable to manually set the upper bound of tokens to generate if they are to be stored in a container, such as a ```List<String?>```, or concatenated together into a string to avoid memory problems. This can be done by calling ```Enumerable.Take(this IEnumerable<String?>, Int32)``` extension method or by iterating a loop with a counter.

### Render(Int32, Random)

Render (generate) a block of text.

Signature:

```csharp
IEnumerable<String?> Render(Int32 relevantTokens, Random random)

```

The first token is chosen by calling ```Radom.Next(Int32)``` method of ```random``` with an appropriate argument. Each consecutive token is chosen by observing the last ```relevantTokens``` tokens (or the number of generated tokens if ```relevantTokens``` tokens have not yet been generated) and choosing one of the possible successors by calling ```Radom.Next(Int32)``` method of ```random``` with an appropriate argument. The process is repeated until the *successor* of the last token would be chosen or until the ending token ([```EndToken```](#endtoken)) is chosen&mdash;the ending tokens are not rendered.

#### Parameters

1.  ```relevantTokens```: ```Int32``` &ndash; Number of (last) relevant tokens.
2.  ```random```: ```Func<Int32, Int32>``` &ndash; (Pseudo-)Random number generator.

#### Returns

A query for rendering tokens.

#### Exceptions

*   ```ArgumentOutOfRangeException```
    *   If ```relevantTokens``` is (strictly) negative.

#### Remarks

An extra copy of ```relevantTokens``` tokens is kept when generating new tokens. Memory errors may occur if ```extraTokens``` is too large.

The query returned is not run until enumerating it (via explicit calls to ```IEnumerable<String?>.GetEnumerator()``` method, a ```foreach``` loop, a call to ```Enumerable.ToList(this IEnumerable<String?>)``` or ```Enumerable.ToArray(this IEnumerable<String?>)``` extension methods etc.). Since the point of ```Random``` class is to provide a (pseudo-)random number generator, two distinct enumerators over the query may return different results.

It is advisable to manually set the upper bound of tokens to generate if they are to be stored in a container, such as a ```List<String?>```, or concatenated together into a string to avoid memory problems. This can be done by calling ```Enumerable.Take(this IEnumerable<String?>, Int32)``` extension method or by iterating a loop with a counter.

## Remarks

If the pen should choose from tokens from multiple sources, the tokens should be concatenated into a single enumerable ```tokens``` passed to the constructor. To prevent *overflowing* from one source to another (e. g. if the last token from the first source is not a logical predecessor of the first token from the second source), an *ending token* ([```EndToken```](#endtoken)) should be put between the sources' tokens in the final enumerable ```tokens```. Choosing the ending token in [```Render(Int32, Func<Int32, Int32>)```](#renderint32-funcint32-int32) or [```Render(Int32, Random)```](#renderint32-random) methods will cause the rendering to stop&mdash;the same as when a *successor* of the last entry in ```tokens``` is chosen.

A complete deep copy of enumerable ```tokens``` (passed to the constructor) is created and stored by the pen. Memory errors may occur if the number of tokens in the enumerable is too large.

Changing any of the properties&mdash;public or protected&mdash;will break the functionality of the pen. This includes, but is not limited to, manually changing the contents or their order in lists [```Tokens```](#tokens) and [```Positions```](#positions). By doing so, behaviour of functions [```Render(Int32, Func<Int32, Int32>)```](#renderint32-funcint32-int32) and [```Render(Int32, Random)```](#renderint32-random) is unexpected and no longer guaranteed.
