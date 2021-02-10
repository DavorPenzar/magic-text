# ```ShatteringOptions``` class

Options for ```ITokeniser.Shatter(StreamReader, ShatteringOptions?)``` and ```ITokeniser.ShatterAsync(StreamReader, ShatteringOptions?)``` methods.

## Properties

### IgnoreEmptyTokens

```Boolean``` &ndash; If ```true```, empty tokens shall be ignored. Default is ```false```.

#### Remarks

Empty tokens are defined by the implementing class of [```ITokeniser```](ITokeniser.md), but usually these include ```null```s and possibly other strings yielding ```true``` when checked via ```String.IsNullOrEmpty(String)``` or ```String.IsNullOrWhiteSpace(String)``` methods.

### IgnoreLineEnds

```Boolean``` &ndash; If ```true```, line ends shall produce no token; otherwise they shall be represented by a [```LineEndToken```](#lineendtoken). Default is ```false```.

### IgnoreEmptyLines

```Boolean``` &ndash; If ```true```, mpty lines shall be ignored; otherwised they shall be represented by an [```EmptyLineToken```](#emptylinetoken). Default is ```false```.

#### Remarks

Empty lines are considered those lines not producing any token (after token filtering if [```IgnoreEmptyTokens```](#ignoreemptytokens) is ```true```) except for ```EmptyLineToken```.

### LineEndToken

```String?``` &ndash; Token to represent line ends. Default is ```Environment.NewLine```.

### EmptyLineToken

```String?``` &ndash; Token to represent empty lines. Default is ```String.Empty```.
