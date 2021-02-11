#   RandomText

[*.NET Standard*](http://docs.microsoft.com/en-us/dotnet/standard/net-standard) library for generating random text.

##  Quick Info

The library provides simple interfaces and classes for tokenising existing text blocks and generating new ones upon the extracted tokens. The library is written compliant with [*.NET Standard 2.1*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md), without the need for any additional [*NuGet* packages](http://nuget.org/).

Tokens extracted from a text block are usually words + punctuation + white spaces, or single characters. It should not be considered a good practice to mix *apples and oranges*, i. e. to have some tokens in form of complete words, while others as single characters. Both tokenisation policies mentioned are implemented in the librabry, while additional policies may be obtained by:

1.  passing desired ```ShatteringOptions``` to tokenisation methods,
2.  constructing ```RegexTokeniser``` with custom regular expression break pattern and transformation method,
3.  implementing custom extensions of ```LineByLineTokeniser``` abstract class or implementing a complete ```ITokeniser``` interface.

Once extracted, the collection of tokens (in the order as read from input) is called ***context*** in the rest of this document. The terminology is inspired by actual context of words in usual forms of text.

### Algorithm Explanation

The (non-deterministic) *algorithm* for generating text blocks implemented by the library is the following:

1.  A **context** of tokens is set.
2.  Input: the *number of relevant tokens* ```n```.
3.  Do:
    1.  Randomly choose a token from the context.
    2.  Repeat:
        1.  Find all occurances of the ```n``` most recent tokens chosen (if ```n``` tokens have not yet been chosen, substitute it by the number of tokens chosen) as subcollections in the context.
        2.  Randomly choose one of the occurances.
        3.  For the next token, choose the token immediately following the tokens from the chosen occurance. If no token follows the occurance, stop the algorithm.
4.  Output: collection of chosen tokens (in the order as chosen).

For example, if the context is acquired by slicing the string *aaaabaaac* at each character, the context is the collection ```{'a', 'a', 'a', 'a', 'b', 'a', 'a', 'a', 'c'}```. Suppose ```n = 3```. A possible line of steps is given bellow:

1.  The letter *a* is chosen.
2.  All occurances of *a* are the following (tokens following the occurance are also shown):
    1.  ***a**aaabaaac*
    2.  ***a**aabaaac*
    3.  ***a**abaaac*
    4.  ***a**baaac*
    5.  ***a**aac*
    6.  ***a**ac*
    7.  ***a**c*
3.  Occurance 2 is chosen. Therefore the next token chosen is (also) the letter *a*. Note that the same would happen if any occurance except occurances 4 and 7 were chosen.
4.  The string *aa* (still shorter than ```n == 3``` characters) must now be found. All occurances of the string are the following:
    1.  ***aa**aabaaac*
    2.  ***aa**abaaac*
    3.  ***aa**baaac*
    4.  ***aa**ac*
    5.  ***aa**c*
5.  Occurance 3 is chosen meaning the next token is the letter *b*. This makes the following steps uniquely determined.
6.  The string *aab* (exactly ```n == 3``` characters) must now be found. All occurances of the string are the following:
    1.  ***aab**aaac*
7.  The next token chosen is the letter *a*.
8.  The string *aba* (the first letter *a* is discarded because otherwise more than the most recent ```n == 3``` characters would be considered) must now be found. All occurances of the string are the following:
    1.  ***aba**aac*
9.  The next token chosen is the letter *a*.
10. The string *baa* must now be found. All occurances of the string are the following:
    1.  ***baa**ac*
11. The next token chosen is the letter *a*.
12. The string *aaa* must now be found. All occurances of the string are the following:
    1.  ***aaa**abaaac*
    2.  ***aaa**baaac*
    3.  ***aaa**c*
13. Unique determination of steps breaks here. If the letter *a* is chosen, the next state is the same as the state in the previous step (12). If the letter *b* is chosen, the next state is the same as in the step 6. If the letter *c* is chosen, the algorithm is uniquely determined until the end. For the sake of brevity, let us say the letter *c* is chosen, i. e. occurance 3.
14. The string *aac* must now be found. All occurances of the string are the following:
    1.  ***aac***
15. No token follows the chosen (actually, the only one possible) occurance. The algorithm stops here.

The steps explained above produce *aabaaac* as the output. Although this is indeed shorter than the input text and also a substring of it, this is not necessarily the case.

##  Example

Suppose a file *Firework.txt* exists in the working directory, with the contents:

```
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you know that there's still a chance for you?
'Cause there's a spark in you

```

These are the lyrics (only the first verse) to a song called *Firework* by the American singer Katy Perry. We shall use them because of the anaphora present in them, making it a nice short input that may produce many different results.

To generate alternative lyrics, one may use the following code:

```csharp
IEnumerable<String?> tokens;

using (var fileStream = File.OpenRead("Firework.txt"))
using (var fileReader = new StreamReader(fileStream))
{
    var tokeniser = new RegexTokeniser();
    tokens = tokeniser.Shatter(fileReader).Where(t => !String.IsNullOrEmpty(t)); // remove the empty line from the end of the file
}

var pen = new Pen(tokens);

foreach (var token in pen.Render(2, new Random(2021)).Take(32))
{
    Console.Write(token);
}
Console.WriteLine();

```

The code above outputs:

```
deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you know that

```

Alternatively, if ```CharTokeniser``` is used instead of ```RegexTokeniser```, the code outputs:

```
lready buried deep?
Six feel like a plastic bag
Drifting through there's still a chance for you ever feet under screams, but no one seems to start again?
Do you ever feel like a plastic bag
Drifting to start again?
Do you?
'Cause of cards, one seems to hear a thin
Like a plastic bag
Drifting through

```
