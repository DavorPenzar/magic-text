#   MagicText

[`MagicText`](MagicText/) is a [*.NET Standard*](http://docs.microsoft.com/en-gb/dotnet/standard/net-standard) library written in [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) for generating random text. The library's interface is [CLS-compliant](http://docs.microsoft.com/en-gb/dotnet/standard/language-independence#cls-compliance-rules); however, inline documentation is written from a [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) perspective and some code excerpts (e. g. regarding operator overloading) may not be compatible with other [CLI](http://ecma-international.org/publications-and-standards/standards/ecma-335/) languages.

**Author**: Davor Penzar (September 2022)

##  Quick Info

The library provides simple interfaces and classes for tokenising existing text blocks and generating new ones upon the extracted tokens. The library is written in [*C# 8*](http://docs.microsoft.com/en-gb/dotnet/csharp/whats-new/csharp-8) compliant with [*.NET Standard 2.0*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)&mdash;but is actually targeted for newer versions&mdash;with a few additional [*NuGet* packages](http://nuget.org/). However, the packages are included only to enable features otherwise not present in [*.NET Standard 2.0*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md) but already present in [*.NET Standard 2.1*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md) and to add special annotations/attributes to fields, properties, methods and method arguments, which could be omitted without a loss of the main intended functionality provided by the library. Some packages might even be excluded in a [*.NET Standard 2.0*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md) environment; nevertheless, they are included by default for purposes of code consistency.

Tokens extracted from a text block are usually words + punctuation + white spaces, or single characters. It should not be considered a good practice to mix *apples and oranges*, i. e. to have some tokens in the form of complete words, while others as single characters (except for words such as the pronoun *I* or the article *a*, for example, in English). Both tokenisation policies mentioned are implemented in the librabry, while additional policies may be obtained by:

1.  passing the desired [`ShatteringOptions`](MagicText/src/ShatteringOptions.cs) to tokenisation methods,
2.  constructing a [`StringSplitTokeniser`](MagicText/src/StringSplitTokeniser.cs) with the desired separators and options,
3.  constructing a [`RegexMatchesTokeniser`](MagicText/src/RegexMatchesTokeniser.cs) or a [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs) with a custom [regular expression](http://en.wikipedia.org/wiki/Regular_expression) match/separator pattern and potentially with a token extraction/transformation function,
4.  implementing a custom extension of the [`LineShatteringTokeniser`](MagicText/src/LineShatteringTokeniser.cs) abstract class or implementing the complete [`ITokeniser`](MagicText/src/ITokeniser.cs) interface.

Additionally, the library provides a [`SplitLinesTokeniser`](MagicText/src/SplitLinesTokeniser.cs), which treats each line of text as a single token, and a [`RandomTokeniser`](MagicText/src/RandomTokeniser.cs), which chooses token breaking points at *random*. However, the latter is not very useful as it is, obviously, unpredictable ([*nondeterministic*](http://en.wikipedia.org/wiki/Nondeterministic_programming)). It is especially useless for the process of generating new text (v. [*Algorithm Explanation*](#Algorithm-Explanation) and [*Code Example*](#Code-Example)) because the resulting tokens would probably be mutually very different, even if they occur in otherwise *similar* parts of text. For instance, the string [`"lorem ipsum"`](http://en.wikipedia.org/wiki/Lorem_ipsum) may once be tokenised by a [`RandomTokeniser`](MagicText/src/RandomTokeniser.cs) into tokens `{ "lo", "r", "em ipsum", "", "" }`, and another time tokenised by the same [`RandomTokeniser`](MagicText/src/RandomTokeniser.cs) into tokens `{ "lor", "em ", "i", "psum" }`. On the other hand, a default [`StringSplitTokeniser`](MagicText/src/StringSplitTokeniser.cs) would always yield tokens `{ "lorem", "ispum" }`, default [`RegexMatchesTokeniser`](MagicText/src/RegexMatchesTokeniser.cs) and [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs) would always yield tokens `{ "lorem", " ", "ispum" }`, and the [`ToCharsTokeniser`](MagicText/src/ToCharsTokeniser.cs) would always yield tokens `{ "l", "o", ..., "m" }`.

Once extracted, the collection of tokens (in the order as read from the input) is called ***context*** in the rest of this document (the tokenisation process is called ***shattering*** in the library). The terminology of the former is inspired by actual context of words in usual forms of text.

### Possible Use Cases

In the author's opinion, the library does not seem to provide any actually useful functionality for applications built using [*.NET*](http://dotnet.microsoft.com/). However, it may be used in self-educational purposes (for understanding some [algorithm](http://en.wikipedia.org/wiki/Algorithm) principles such as text tokenisation, sorting, searching etc.&mdash;although private fields, targeted framework branching, inline documentation and comments are, addmitingly, not very *neat* hear and there, and, as a result, a powerful user-friendly [IDE](http://en.wikipedia.org/wiki/Integrated_development_environment) (such as [*Visual Studio*](http://visualstudio.microsoft.com/)) would help greatly in navigating the source code), for various unit testing (to generate [mock objects](http://en.wikipedia.org/wiki/Mock_object)) or maybe even for the implementation of some parts of [*chatbots*](http://en.wikipedia.org/wiki/Chatbot). The main idea, though, behind writing this library was (the author's) self-training in programming [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) + [*.NET*](http://dotnet.microsoft.com/) applications through implementing an application for generating random blocks of text *for fun*.

### [Algorithm](http://en.wikipedia.org/wiki/Algorithm) Explanation

The ([*nondeterministic*](http://en.wikipedia.org/wiki/Nondeterministic_programming)) [*algorithm*](http://en.wikipedia.org/wiki/Algorithm) for generating text blocks implemented by the library is the following:

1.  A **context** of tokens is set.
2.  Input: *number of relevant tokens* `n >= 0`.
3.  Do:
    1.  Randomly choose a token from the context.
    2.  Repeat:
        1.  Find all occurrences of the `n` most recent tokens chosen (if `n` tokens have not yet been chosen, substitute it by the number of tokens chosen) as subcollections in the context.
        2.  Randomly choose one of the occurrences.
        3.  For the next token, choose the token immediately following the tokens from the chosen occurrence. If no token follows the occurrence (if the occurrence is at the very end of the context), stop the [algorithm](http://en.wikipedia.org/wiki/Algorithm).
4.  Output: collection of chosen tokens (in the order as chosen).

For example, if the context is acquired by slicing the string *aaaabaaac* at each character, the context is the collection `{ "a", "a", "a", "a", "b", "a", "a", "a", "c" }`. Suppose `n = 3`. A possible line of steps is given bellow:

1.  The letter *a* is chosen.
2.  All occurrences of *a* are the following:
    1.  ***a**aaabaaac*
    2.  *a**a**aabaaac*
    3.  *aa**a**abaaac*
    4.  *aaa**a**baaac*
    5.  *aaaab**a**aac*
    6.  *aaaaba**a**ac*
    7.  *aaaabaa**a**c*
3.  Occurrence 2 is chosen. Therefore the next token chosen is (also) the letter *a*. Note that the same would happen if any occurrence except occurrences 4 and 7 were chosen.
4.  The string *aa* (still shorter than `n == 3` characters) must now be found. All occurrences of the string are the following:
    1.  ***aa**aabaaac*
    2.  *a**aa**abaaac*
    3.  *aa**aa**baaac*
    4.  *aaaab**aa**ac*
    5.  *aaaaba**aa**c*
5.  Occurrence 3 is chosen meaning the next token is the letter *b*. This makes the following steps uniquely determined.
6.  The string *aab* (exactly `n == 3` characters) must now be found. All occurrences of the string are the following:
    1.  *aa**aab**aaac*
7.  The next token chosen is the letter *a*.
8.  The string *aba* (the first letter *a* is discarded because otherwise more than the most recent `n == 3` characters would be considered) must now be found. All occurrences of the string are the following:
    1.  *aaa**aba**aac*
9.  The next token chosen is the letter *a*.
10. The string *baa* must now be found. All occurrences of the string are the following:
    1.  *aaaa**baa**ac*
11. The next token chosen is the letter *a*.
12. The string *aaa* must now be found. All occurrences of the string are the following:
    1.  ***aaa**abaaac*
    2.  *a**aaa**baaac*
    3.  *aaaab**aaa**c*
13. Unique determination of steps breaks here. If the letter *a* is chosen, the next state is the same as the state in the previous step (12). If the letter *b* is chosen, the next state is the same as in the step 6. If the letter *c* is chosen, the [algorithm](http://en.wikipedia.org/wiki/Algorithm) is uniquely determined until the end. For the sake of brevity, let us say the letter *c* is chosen, i. e. occurrence 3.
14. The string *aac* must now be found. All occurrences of the string are the following:
    1.  *aaaaba**aac***
15. No token follows the chosen (actually, the only one possible) occurrence. The [algorithm](http://en.wikipedia.org/wiki/Algorithm) stops here.

The steps explained above produce *aabaaac* as the output. Although this is indeed shorter than the input text and also a substring of it, that is not necessarily the case.

A subsequence of 0 tokens is assumed to precede every token. In other words, if `n == 0` (if there are no most recent relevant tokens), all tokens are chosen in the same way as the first one: by choosing randomly from the complete context. Moreover, a subsequence of 0 tokens is also assumed to precede the *end of the context*, in the sense that the occurrence of the range of 0 tokens following the last token may also be chosen, in which case the [algorithm](http://en.wikipedia.org/wiki/Algorithm) terminates. This can be identified with [null-terminated strings](http://en.wikipedia.org/wiki/Null-terminated_string), such as in [*C* programming language](http://www.iso.org/standard/74528.html), where the [null character](http://en.wikipedia.org/wiki/Null_character) at the end of the string is a single independent (not *glued together* with its predecessor) token.

##  Code Examples

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

using (Stream input = File.OpenRead("Firework.txt"))
{
	ITokeniser tokeniser = new RegexSplitTokeniser();
	tokens = tokeniser.ShatterToArray(input, new ShatteringOptions() { IgnoreEmptyTokens = true });
}

pen = new Pen(tokens);

foreach (String? token in pen.Render(4, new Random(1000)).Take(300))
{
	Console.Write(token);
}
Console.WriteLine();

```

The code above uses the [`Pen`](MagicText/src/Pen.cs) class and the [`ITokeniser`](MagicText/src/ITokeniser.cs) interface (implemented through the [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs) class) provided by the library and outputs:

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

Alternatively, if a [`ToCharsTokeniser`](MagicText/src/ToCharsTokeniser.cs) is used instead of the [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs), the code outputs:

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

**Nota bene.** The results above were obtained by running the code in a 64-bit version ([*x64*](http://en.wikipedia.org/wiki/X86-64)) of the [*.NET 7* framework](http://github.com/dotnet/core/blob/main/7.0/README.md) on [*Microsoft Windows 10*](http://microsoft.com/en-gb/windows/get-windows-10). Running the code in a different environment may yield different results&mdash;this has not been tested.

### Further Examples

The library actually enables some more sophisticated use cases than the simple example demonstrated above. For instance, to asynchronously tokenise text read from the console one could use the following code:

```csharp
using MagicText; // <-- namespace of the library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ...

List<String?> tokens = new List<String?>();

ITokeniser tokeniser = new RegexMatchesTokeniser(matching: RegexMatchesTokeniserDefaultMatchings.WordsOrNonWords);

try
{
	CancellationTokenSource cancellation = new CancellationTokenSource();
	await foreach (String? token in tokeniser.ShatterAsync(input: Console.In, continueTasksOnCapturedContext: false).WithCancellation(cancellation.Token).ConfigureAwait(false))
	{
		if (token == "###")
		{
			cancellation.Cancel();
		}
		tokens.Add(token);
	}
}
catch (OperationCanceledException)
{
	await Console.Out.WriteLineAsync("Cancelled.").ConfigureAwait(false);
}

```

Obviously, this is an *overkill*, as, instead of `cancellation.Cancel()`, one could simply `break` the asynchronous `foreach`-loop; not to mention that reading from the console may be done synchronously (and in the background it actually is, as explained [here](http://docs.microsoft.com/en-gb/dotnet/api/system.console.in#remarks)). However, the example above illustrates the power and possibilities provided by the library which might come useful in other real-life scenarios.

Note that the [`LineShatteringTokeniser`](MagicText/src/LineShatteringTokeniser.cs)&mdash;the base class of the [`ToCharsTokeniser`](MagicText/src/ToCharsTokeniser.cs), [`StringSplitTokeniser`](MagicText/src/StringSplitTokeniser.cs), [`RegexMatchesTokeniser`](MagicText/src/RegexMatchesTokeniser.cs), [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs), [`SplitLinesTokeniser`](MagicText/src/SplitLinesTokeniser.cs) and [`RandomTokeniser`](MagicText/src/RandomTokeniser.cs) classes&mdash;does not necessarily cancel the tokenising operation immediately, meaning that some additional iterations in the `foreach`-loop above may be run even after the breaking `"###"` token is found. However, no new lines will be read from the underlying input ([`System.Console.In`](http://docs.microsoft.com/en-gb/dotnet/api/system.console.in) in the example above) after cancelling the operation. Actually, no additional bytes shall be read appart from those having already been irrecoverably read.

All tokenisers provided by the library tokenise text using [*deferred execution*](http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-example) (therefore similar examples could have been written using any other [`ITokeniser`](MagicText/src/Tokeniser.cs) provided by the library). In fact, this is the recommended behaviour of all classes implementing the [`ITokeniser`](MagicText/src/ITokeniser.cs) interface. Such implementation enables simultaneous tokenising and reading operations, which may come useful when reading from sources such as the console or a network channel. On the other hand, the [`TokeniserExtensions`](MagicText/src/TokeniserExtensions.cs) class provides extension methods for tokenising into arrays (fully built containers instead of the [*deferred execution*](http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-example)), which is useful when reading from strings and read-only text files. If the latter was the default, simultaneous reading and tokenising would be impossible because the input would have to be read and tokenised until the end before accessing any of the tokens.

####    Simple [Information Theory](http://en.wikipedia.org/wiki/Information_theory) Analysis

The [`Pen`](MagicText/src/Pen.cs) class may be used to perform some analysis of the context tokens in a probably more efficient way than simply iterating over its `Context` property and counting values, without creating an auxiliary index (such as the `Index` property of the [`Pen`](MagicText/src/Pen.cs) class). For instance, one could execute a code as the following:

```csharp
using MagicText; // <-- namespace of the library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

// ...

Pen pen;

// initialise the `pen`...

ICollection<String?> tokens = new HashSet<String?>(pen.Context);

IDictionary<String, Double> probabilities = new Dictionary<String, Double>();
IDictionary<String, Double> informationContents = new Dictionary<String, Double>();
Double entropy;

foreach (String? token in tokens)
{
	String? tokenRep = JsonSerializer.Serialize(token);

	Double prob = Convert.ToDouble(pen.Count(token)) / pen.Context.Count;
	Double info = -Math.Log2(prob);

	probabilities.Add(tokenRep, prob);
	informationContents.Add(tokenRep, info);
}

entropy = probabilities.Sum(e => e.Value * informationContent[e.Key]);

foreach (KeyValuePair<String, Double> entry in probabilities.OrderBy(e => e.Value).ThenBy(e => JsonSerializer.Deserialize<String>(e.Key)))
{
	Console.WriteLine("P ({0}) = {1:P2}", entry.Key, entry.Value);
}
Console.WriteLine();

foreach (KeyValuePair<String, Double> entry in informationContents.OrderByDescending(e => e.Value).ThenBy(e => JsonSerializer.Deserialize<String>(e.Key)))
{
	Console.WriteLine("I ({0}) = {1:N4}", entry.Key, entry.Value);
}
Console.WriteLine();

Console.WriteLine("E = {0:N4}", entropy);

```

Using the same initialisation of the `pen` as in the first example, the code would output the following [empirical probabilities (frequencies)](http://en.wikipedia.org/wiki/Empirical_probability) `P (...)`, [information contents](http://en.wikipedia.org/wiki/Information_content) `I (...)` and [entropy](http://en.wikipedia.org/wiki/Entropy_(information_theory)) `E` (only the top and the bottom 3 tokens are displayed):

```
P ("\u0027 ") = 0,68 %
P ("again") = 0,68 %
P ("already") = 0,68 %
...
P ("you") = 4,08 %
P ("\r\n") = 5,44 %
P (" ") = 38,10 %

I ("\u0027 ") = 7,1997
I ("again") = 7,1997
I ("already") = 7,1997
...
I ("you") = 4,6147
I ("\r\n") = 4,1997
I (" ") = 1,3923

E = 4,2892

```

A little more interesting would be an analysis of a longer text, such as the [*Bible*](http://en.wikipedia.org/wiki/Bible). For instance, download the text [here](http://gutenberg.org/ebooks/10) ([*Project Gutenberg*](http://gutenberg.org/)) and rename it to `Bible.txt`, then run the code as below:

```csharp
using MagicText; // <-- namespace of the library
using System;
using System.Collections.Generic;
using System.IO;

// ...

// Initialisation of the `pen`:

IEnumerable<String?> tokens;
Pen pen;

using (Stream input = File.OpenRead("Bible.txt"))
{
	ITokeniser tokeniser = new RegexSplitTokeniser();
	tokens = tokeniser.ShatterToArray(
		input,
		new ShatteringOptions()
		{
			IgnoreEmptyTokens = true,
			IgnoreLineEnds = false,
			IgnoreEmptyLines = true,
			LineEndToken = " "
		}
	);
}

pen = new Pen(context: tokens, comparer: StringComparer.InvariantCultureIgnoreCase);

// Information Theory Analysis:

Double p1 = Convert.ToDouble(pen.Count("Israel")) / pen.Context.Count;
Double p2 = Convert.ToDouble(pen.Count("God", " ", "of", " ")) / (pen.Context.Count - 3);
Double p3 = Convert.ToDouble(pen.Count("God", " ", "of", " ", "Israel")) / (pen.Context.Count - 4);

Console.WriteLine("P (Israel) = {0:P2}", p1);
Console.WriteLine("P (Israel | God of ...) = {0:P2}", p3 / p2);

```

The program above computes the following [probabilities](http://en.wikipedia.org/wiki/Empirical_probability):

1.  of the token `"Israel"` &ndash; `p1`,
2.  of the token [quadrigram](http://en.wikipedia.org/wiki/N-gram) `{ "God", " ", "of", " " }` &ndash; `p2`,
3.  of the token [quinquegram](http://en.wikipedia.org/wiki/N-gram) `{ "God", " ", "of", " ", "Israel" }` &ndash; `p3`,
4.  of the token `"Israel"` under the condition of the token [quadrigram](http://en.wikipedia.org/wiki/N-gram) `{ "God", " ", "of", " " }` ([conditional probability](http://en.wikipedia.org/wiki/Conditional_probability)) &ndash; `p3 / p2`.

It will take some time to finish, but in the end it should output the following values:

```
P (Israel) = 0,15 %
P (Israel | God of ...) = 40,85 %

```

The above [probabilities](http://en.wikipedia.org/wiki/Empirical_probability) mean that the token `"Israel"` appears with only 0,15 % of chance, but, following the tokens `{ "God", " ", "of", " " }`, the [probability](http://en.wikipedia.org/wiki/Empirical_probability) of the next token being `"Israel"` rises up to 40,85 %&mdash;more than 270 times more! Although, it is worth mentioning that token comparison is actually case-insensitive and that some tokens are not really part of the [*Bible*](http://en.wikipedia.org/wiki/Bible), such as the preamble:

```
The Project Gutenberg eBook of The King James Bible

This eBook is for the use of anyone anywhere in the United States and
most other parts of the world at no cost and with almost no restrictions
whatsoever. You may copy it, give it away or re-use it under the terms
of the Project Gutenberg License included with this eBook or online at
www.gutenberg.org. If you are not located in the United States, you
will have to check the laws of the country where you are located before
using this eBook.

Title: The King James Bible

Release Date: August, 1989 [eBook #10]
[Most recently updated: ...]

Language: English

Character set encoding: UTF-8

*** START OF THE PROJECT GUTENBERG EBOOK THE KING JAMES BIBLE ***

```

Also, note that the [information theory](http://en.wikipedia.org/wiki/Information_theory) analysis examples displayed above considered equally both words and word delimiters as tokens. However, when analysing words only, delimiters would have to be disregarded (e. g. when calculating the [empirical probability](http://en.wikipedia.org/wiki/Empirical_probability) of a word). The simplest solution would be to use a [`RegexMatchesTokeniser`](MagicText/src/RegexMatchesTokeniser.cs) which only yields words as tokens, or a [`StringSplitTokeniser`](MagicText/src/StringSplitTokeniser.cs) or [`RegexSplitTokeniser`](MagicText/src/RegexSplitTokeniser.cs) which do not yield delimiters as tokens, but some word [bigrams](http://en.wikipedia.org/wiki/Bigram) and other [*n*-grams](http://en.wikipedia.org/wiki/N-gram) could then be misidentified. For instance, by shattering the sentence *Although I am hungry, people don't seem to care*, the words `"hungry"` and `"people"` would appear as neighbouring therefore generating the [bigram](http://en.wikipedia.org/wiki/Bigram) `{ "hungry", "people" }`, inspite of them clearly being parts of different clauses.

### Even Further Examples

The [`MagicText.Example`](MagicText.Example/) project targeted for [*.NET 7*](http://github.com/dotnet/core/blob/main/7.0/README.md) (but compatible with as low as [*.NET Core 2.0*](http://github.com/dotnet/core/blob/main/2.0/README.md)) is a fully-working project written in [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) that demonstrates how to use online sources to download an original text and use it for the generation of a new, random text.

##  Remarks

This library should not be used when working with large corpora of context. Objects of the [`Pen`](MagicText/src/Pen.cs) class store complete context using an in-memory container, rather than reading tokens from external memory or a network resource. The implemented approach is much simpler and faster, but lacks the possibility to work with a large number of tokens that would not fit in the internal memory all at once. However, logic used in the library may be generalised to implement a more sophisticated programs able to handle storing tokens externally.

Another limitation of the functionality provided by the library is the fact that objects of the [`Pen`](MagicText/src/Pen.cs) class are immutable. Consequently, once a [`Pen`](MagicText/src/Pen.cs) is initialised, its context cannot be updated. Each update applied to the corpus of the tokens (context) requires initialising a new [`Pen`](MagicText/src/Pen.cs), which in turn executes a relatively expensive process of sorting the context. The larger the context, the more expensive the process. The problem is somewhat justified by the fact that text corpora are relatively rarely updated compared to the frequency of other, read-only operations conducted on them. The complete functionality of the [`Pen`](MagicText/src/Pen.cs) class is in fact read-only in terms of the resources used, which is compatible with the intended use.

On the other hand, the [`Pen`](MagicText/src/Pen.cs) class implements the [`System.Runtime.Serialization.ISerializable`](http://docs.microsoft.com/en-gb/dotnet/api/system.runtime.serialization.iserializable) interface and is implemented with the [`System.SerializableAttribute`](http://docs.microsoft.com/en-gb/dotnet/api/system.serializableattribute) attribute. Also, a default [`System.Text.Json.Serialization.JsonConverter<T>`](http://docs.microsoft.com/en-gb/dotnet/api/system.text.json.serialization.jsonconverter-1) to serialise and deserialise [`Pen`](MagicText/src/Pen.cs) instances to/from [*JSON*](http://en.wikipedia.org/wiki/JSON) is provided by the library *out of the box*. In other words, [`Pen`s](MagicText/src/Pen.cs) are serialisable and can be persisted through multiple instances of the application process. This may speed up the application startup as expensive [`Pen`](MagicText/src/Pen.cs) construction may be avoided. Both serialisation techniques even automatically handle serialisation of some standard and custom [`System.StringComparer`](http://docs.microsoft.com/en-gb/dotnet/api/system.stringcomparer) subclasses. However, [*XML*](http://en.wikipedia.org/wiki/XML) serialisation using the standard serialiser ([`System.Xml.Serialization.XmlSerializer`](http://docs.microsoft.com/en-gb/dotnet/api/system.xml.serialization.xmlserializer)) is not supported.

##  Known Issues

1.  Although the [`Microsoft.Bcl.AsyncInterfaces` package](http://nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/5.0.0) is listed as the library's dependency, when referncing the library from a [*.NET Framework*](http://docs.microsoft.com/en-gb/dotnet/framework/) project, the package might have to be explicitly referenced again from the project. Without doing so the code may fail to compile or the application may crash when an asynchronous method provided by the library is encountered.
    *   **Soultion.** Reference the [`Microsoft.Bcl.AsyncInterfaces` package](http://nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/5.0.0) explicitly from your project. If the error still arises, consider updating to a more recent (modern) [*.NET*](http://dotnet.microsoft.com/) version.

##  License

This project is licensed under the terms of the [*GNU General Public License v3*](http://gnu.org/licenses/gpl-3.0.en.html). However, external dependencies might use different licenses.

##  References

The complete library is customly written by a single author&mdash;me&mdash;but the logic behind it is widely known and is used in many applications (at least I have come accross it a few times throughout my student days). As I consider the logic as general knowledge in the field, I did not use any extraneous sources of knowledge for implementing the library and therefore do not feel a moral or any other obligation to cite sources and references other than already mentioned [Katy Perry](http://katyperry.com/) for the lyrics of her song [*Firework*](http://youtube.com/watch?v=QGJuMBdaqIw).
