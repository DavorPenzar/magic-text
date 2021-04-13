#   MagicText

[*.NET Standard*](http://docs.microsoft.com/en-gb/dotnet/standard/net-standard) library written in [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) for generating random text.

**Author**: Davor Penzar (April 2021)

##  Quick Info

The library provides simple interfaces and classes for tokenising existing text blocks and generating new ones upon the extracted tokens. The library is written in [*C# 8*](http://docs.microsoft.com/en-gb/dotnet/csharp/whats-new/csharp-8) compliant with [*.NET Standard 2.1*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md), without the need for any additional [*NuGet* packages](http://nuget.org/).

Tokens extracted from a text block are usually words + punctuation + white spaces, or single characters. It should not be considered a good practice to mix *apples and oranges*, i. e. to have some tokens in form of complete words, while others as single characters (except for words such as the pronoun *I* or the article *a*, for example, in English). Both tokenisation policies mentioned are implemented in the librabry, while additional policies may be obtained by:

1.  passing desired [```ShatteringOptions```](Source/ShatteringOptions.cs) to tokenisation methods,
2.  constructing a [```RegexTokeniser```](Source/RegexTokeniser.cs) with a custom regular expression break pattern and potentially with a transformation method,
3.  implementing a custom extension of [```LineByLineTokeniser```](Source/LineByLineTokeniser.cs) abstract class or implementing complete [```ITokeniser```](Source/ITokeniser.cs) interface.

Once extracted, the collection of tokens (in the order as read from the input) is called ***context*** in the rest of this document. The terminology is inspired by actual context of words in usual forms of text.

### Possible Use Cases

In the author's opinion, the library does not seem to provide any actually useful functionality for applications built using [*.NET*](http://dotnet.microsoft.com/). However, it may be used in self-educational purposes (for understanding some algorithm principles such as text tokenisation, sorting, searching etc.&mdash;although nested classes, private fields, inline documentation and comments are, addmitingly, not very *neat* hear and there, and, as a result, a user-friendly [IDE](http://en.wikipedia.org/wiki/Integrated_development_environment) (such as [*Visual Studio*](http://visualstudio.microsoft.com/)) would help greatly in navigating the source code), for various unit testing (to generate [mock objects](http://en.wikipedia.org/wiki/Mock_object)) or maybe even for implementation of some parts of [*chatbots*](http://en.wikipedia.org/wiki/Chatbot). The main idea, though, behind writing this library was (the author's) self-training in programming [*C#*](http://docs.microsoft.com/en-us/dotnet/csharp/) + [*.NET*](http://dotnet.microsoft.com/) applications through implementing an application for generating random blocks of text *for fun*.

### Algorithm Explanation

The (non-deterministic) *algorithm* for generating text blocks implemented by the library is the following:

1.  A **context** of tokens is set.
2.  Input: *number of relevant tokens* ```n >= 0```.
3.  Do:
    1.  Randomly choose a token from the context.
    2.  Repeat:
        1.  Find all occurances of the ```n``` most recent tokens chosen (if ```n``` tokens have not yet been chosen, substitute it by the number of tokens chosen) as subcollections in the context.
        2.  Randomly choose one of the occurances.
        3.  For the next token, choose the token immediately following the tokens from the chosen occurance. If no token follows the occurance (if the occurance is at the very end of the context), stop the algorithm.
4.  Output: collection of chosen tokens (in the order as chosen).

For example, if the context is acquired by slicing the string *aaaabaaac* at each character, the context is the collection ```{'a', 'a', 'a', 'a', 'b', 'a', 'a', 'a', 'c'}```. Suppose ```n = 3```. A possible line of steps is given bellow:

1.  The letter *a* is chosen.
2.  All occurances of *a* are the following:
    1.  ***a**aaabaaac*
    2.  *a**a**aabaaac*
    3.  *aa**a**abaaac*
    4.  *aaa**a**baaac*
    5.  *aaaab**a**aac*
    6.  *aaaaba**a**ac*
    7.  *aaaabaa**a**c*
3.  Occurance 2 is chosen. Therefore the next token chosen is (also) the letter *a*. Note that the same would happen if any occurance except occurances 4 and 7 were chosen.
4.  The string *aa* (still shorter than ```n == 3``` characters) must now be found. All occurances of the string are the following:
    1.  ***aa**aabaaac*
    2.  *a**aa**abaaac*
    3.  *aa**aa**baaac*
    4.  *aaaab**aa**ac*
    5.  *aaaaba**aa**c*
5.  Occurance 3 is chosen meaning the next token is the letter *b*. This makes the following steps uniquely determined.
6.  The string *aab* (exactly ```n == 3``` characters) must now be found. All occurances of the string are the following:
    1.  *aa**aab**aaac*
7.  The next token chosen is the letter *a*.
8.  The string *aba* (the first letter *a* is discarded because otherwise more than the most recent ```n == 3``` characters would be considered) must now be found. All occurances of the string are the following:
    1.  *aaa**aba**aac*
9.  The next token chosen is the letter *a*.
10. The string *baa* must now be found. All occurances of the string are the following:
    1.  *aaaa**baa**ac*
11. The next token chosen is the letter *a*.
12. The string *aaa* must now be found. All occurances of the string are the following:
    1.  ***aaa**abaaac*
    2.  *a**aaa**baaac*
    3.  *aaaab**aaa**c*
13. Unique determination of steps breaks here. If the letter *a* is chosen, the next state is the same as the state in the previous step (12). If the letter *b* is chosen, the next state is the same as in the step 6. If the letter *c* is chosen, the algorithm is uniquely determined until the end. For the sake of brevity, let us say the letter *c* is chosen, i. e. occurance 3.
14. The string *aac* must now be found. All occurances of the string are the following:
    1.  *aaaaba**aac***
15. No token follows the chosen (actually, the only one possible) occurance. The algorithm stops here.

The steps explained above produce *aabaaac* as the output. Although this is indeed shorter than the input text and also a substring of it, that is not necessarily the case.

A subsequence of 0 tokens is assumed to precede every token. In other words, if ```n == 0``` (if there are no most recent relevant tokens), all tokens are chosen in the same way as the first one: by choosing randomly from the complete context. Moreover, a subsequence of 0 tokens is also assumed to precede the *end of the context*, in the sense that the occurance of the range of 0 tokens following the last token may also be chosen, in which case the algorithm terminates. This can be identified with [null-terminated strings](http://en.wikipedia.org/wiki/Null-terminated_string), such as in [*C* programming language](http://www.iso.org/standard/74528.html), where the null character at the end of the string is a single independent (not *glued together* with its predecessor) token.

##  Code Example

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

These are the lyrics (only the first verse) to the song called [*Firework*](http://youtube.com/watch?v=QGJuMBdaqIw) by the American singer [Katy Perry](http://katyperry.com/). We shall use them because of the [anaphora](http://en.wikipedia.org/wiki/Anaphora_(rhetoric)) present in them, making it a nice short input that may produce many different results.

To generate alternative lyrics, one may use the following code:

```csharp
using MagicText; // <-- namespace of the library
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ...

IEnumerable<String?> tokens;
Pen pen;

using (Stream fileStream = File.OpenRead("Firework.txt"))
using (TextReader fileReader = new StreamReader(fileStream))
{
	ITokeniser tokeniser = new RegexTokeniser();
	tokens = tokeniser.ShatterToList(fileReader, new ShatteringOptions() { IgnoreEmptyTokens = true });
}

pen = new Pen(tokens);

foreach (String? token in pen.Render(4, new Random(1000)).Take(300))
{
	Console.Write(token);
}
Console.WriteLine();

```

The code above outputs:

```
 deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you know that there

```

Alternatively, if [```CharTokeniser```](Source/CharTokeniser.cs) is used instead of [```RegexTokeniser```](Source/RegexTokeniser.cs), the code outputs:

```
 deep?
Six feet under screams, but no one blow from cavin' in?
Do you?
'Cause of cards, one seems to start again?
Do you ever feel, feel like a plastic bag
Drifting to hear a thing
Do you know that there's a spark in you?
'Cause of cards, one blow from cavin' in?
Do you know that there's a spark in

```

**Nota bene.** The results above were obtained by running the code on a 64-bit version ([*x64*](http://en.wikipedia.org/wiki/X86-64)) of the [*.NET 5.0* framework](http://github.com/dotnet/core/blob/master/release-notes/5.0/README.md). Running the code in a different environment may yield different results&mdash;this has not been tested.

##  Remarks

This library should not be used when working with large corpora of context tokens. Objects of class [```Pen```](Source/Pen.cs) store complete context using an in-memory container, rather than reading tokens from external memory or a network resource. The implemented approach is much simpler and faster, but lacks the possibility to work with a large number of tokens. However, logic used in the library may be generalised to implement a more sophisticated programs able to handle storing tokens externally.

##  References

The complete library is customly written by a single author&mdash;me&mdash;but the logic behind it is widely known and used in many applications (at least I have come accross it a few times throughout my student days). As I consider the logic as general knowledge in the field, I did not use any extraneous sources of knowledge for implementing the library and therefore do not feel a moral or any other obligation to cite sources and references.
