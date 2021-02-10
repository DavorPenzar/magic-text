# random-text

[*.NET Standard*](http://docs.microsoft.com/en-us/dotnet/standard/net-standard) library for generating random text.

## Quick Info

The library provides simple interfaces and classes for tokenising existing text blocks and generating new ones upon the extracted tokens. The library is written compliant with [*.NET Standard 2.1*](http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md), without the need for any additional [*NuGet*](http://www.nuget.org/) packages.

## Example

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

These are the lyrics (only the first verse) to a song called *Firework* by the American singer Katy Perry. We shall use them because of the anaphora present in them.

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
 feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel like a plastic bag
Drifting through the wind, wanting to start again?
Do you ever feel, feel so paper thin
Like a house of cards, one blow from cavin' in?
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever feel already buried deep?
Six feet under screams, but no one seems to hear a thing
Do you ever

```

Alternatively, if ```CharTokeniser``` is used instead of ```RegexTokeniser```, the code outputs:

```
 for you?
'Cause there's a spark in you know that the wind, wanting to hear a thing
Do you?
'Cause of cards, one blow from cavin' in?
Do you know that there's a spark in you ever feel, feel, feet under screams, but no one blow from cavin' in?
Do you know that the wind, wanting through there's a spar

```
