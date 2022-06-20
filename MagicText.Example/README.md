#   MagicText &ndash; Example

`MagicText.Example` is a [*.NET Core*](http://github.com/dotnet/core) example console project written in [*C#*](http://docs.microsoft.com/en-gb/dotnet/csharp/) for demonstrating how to tokenise text from an online resource and generate new text upon the extracted tokens. It primarily demonstrates a possible use case of the [`MagicText`](http://github.com/DavorPenzar/magic-text/) library.

##  Setup

After downloading the project, it is ready to run. Originally, it targets the [*.NET 6* framework](http://github.com/dotnet/core/blob/main/6.0/README.md), but this can be easily changed to as low as [*.NET Core 2.0*](http://github.com/dotnet/core/blob/main/2.0/README.md)) without changing the source code (other than the [`TargetFramework`](http://docs.microsoft.com/en-gb/dotnet/core/project-sdk/msbuild-props#targetframework) property in the [`MagicText.Example.csproj` project file](MagicText.Example.csproj)).

### Application Settings

Additionally, some application settings may be changed via the [`appsettings.json` settings file](appsettings.json) without changing the source code, but first learn about the intended use of the application before altering its settings. Read [*Usage* section](#Usage) first, then [*`appsettings.json` Schema* section](#appsettings.json-schema) next.

##  Usage

The app should be run from the console to which it shall also output the generated text (unless the output is redirected). Briefly explained, the app:

1.  downloads text from a web source then shatters it into tokens,
2.  constructs a [`Pen`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/Pen.cs) from the extracted tokens, and
3.  renders and prints a newly generated random text.

More detailed:

1.  To download the original text, the app **sends a [*HTTP* GET](http://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol#Request_methods) request** without any special [headers](http://en.wikipedia.org/wiki/List_of_HTTP_header_fields) (e. g. authentication). Having received a [successful response](http://en.wikipedia.org/wiki/List_of_HTTP_status_codes#2xx_success), the app decodes the received bytes using a predefined [character encoding](http://en.wikipedia.org/wiki/Character_encoding) and simultaneously **shatters the text using a [`RegexSplitTokeniser`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/RegexSplitTokeniser.cs)**.
2.  After the web resource is read until the end and all tokens are extracted, the communication with the source is closed. **Only then (all tokens fully constructed and in memory) is a [`Pen`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/Pen.cs) constructed**&mdash;once a response is received from the resource, any potential subsequent changes made on the web source are not reflected in the app.
3.  Finally, when the [`Pen`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/Pen.cs) is constructed, a **limited number of random text tokens are generated**. The tokens are first concatenated into a single [`System.String`](http://docs.microsoft.com/en-gb/dotnet/api/system.string), and then the [`System.String`](http://docs.microsoft.com/en-gb/dotnet/api/system.string) is printed to the console. **If the number of tokens is too large for the resulting [`System.String`](http://docs.microsoft.com/en-gb/dotnet/api/system.string) to fit in the memory, the app may crash**.

Without changing the source code, the behaviour explained above cannot be changed. However, settings such as the web resource address, the [`RegexSplitTokeniser`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/RegexSplitTokeniser.cs) pattern, or the maximal number of tokens to generate, may easily be set/altered. See [*`appsettings.json` Schema* section](#appsettings.json-schema) to learn how.

##  [`appsettings.json`](appsettings.json) Schema

The app uses [`Microsoft.Extensions.Configuration.IConfiguration`](http://docs.microsoft.com/en-gb/dotnet/api/microsoft.extensions.configuration.iconfiguration) to read settings at startup. It actually reads in the following order (subsequent definition overrides a previous one):

1.  environment variables,
2.  [`appsettings.json` file](appsettings.json),
3.  `appsettings.{DOTNETCORE_ENVIRONMENT}.json` file (e. g. [`appsettings.Development.json` file](appsettings.json)) if the `DOTNETCORE_ENVIRONMENT` environment variable is set and the file exists,
4.  command line arguments.

The app settings shall only be described for the [`appsettings.json`](appsettings.json) and `appsettings.{DOTNETCORE_ENVIRONMENT}.json` files&mdash;to set the settings using another [`Microsoft.Extensions.Configuration.IConfigurationProvider`](http://docs.microsoft.com/en-gb/dotnet/api/microsoft.extensions.configuration.iconfigurationprovider), use the same keys and data types following the [`Microsoft.Extensions.Configuration.IConfigurationProvider`'s](http://docs.microsoft.com/en-gb/dotnet/api/microsoft.extensions.configuration.iconfigurationprovider) syntax.

### Non-App Settings

The following keys are not app-specific and are used to configure third-party services used by the app:

*   `$schema` &ndash; the [*JSON*](http://json.org/) schema of the file (ignore in other [`Microsoft.Extensions.Configuration.IConfigurationProvider`s](http://docs.microsoft.com/en-gb/dotnet/api/microsoft.extensions.configuration.iconfigurationprovider)),
*   `ConnectionStrings` &ndash; connection strings for database connections (unused),
*   `Serilog` &ndash; configuration of [Serilog](http://serilog.net/) logging.

### App Settings

The following keys are app-specific and are used to configure internal app's services:

*   `Text` (object) &ndash; settings for the original text source and the random text generator:
    *   `WebSource` (object) &ndash; settings for the original text cource:
        *   `BaseAddress` (string) &ndash; base address (see [`System.Net.Http.HttpClient.BaseAddress`](http://docs.microsoft.com/en-gb/dotnet/api/system.net.http.httpclient.baseaddress)),
        *   `RequestUri` (string) &ndash; request [URI](http://en.wikipedia.org/wiki/Uniform_Resource_Identifier) (see [`System.Net.Http.HttpRequestMessage.RequestUri`](http://docs.microsoft.com/en-gb/dotnet/api/system.net.http.httprequestmessage.requesturi)),
        *   `Encoding` (string) &ndash; encoding name (see [*List of Encodings*](http://docs.microsoft.com/en-gb/dotnet/api/system.text.encoding#list-of-encodings)),
    *   `RandomGenerator` (object) &ndash; settings for the random text generator:
        *   `Seed` (number) &ndash; random seed (see [`System.Random.Random(System.Int32)`](http://docs.microsoft.com/en-gb/dotnet/api/system.random.-ctor))
        *   `RelevantTokens` (number) &ndash; the number of (most recent) relevant tokens,
        *   `FromPosition` (number|null) &ndash; zero-indexed position of the first generated token in the original context,
        *   `MaxTokens` (number) &ndash; the maximal number of tokens to generate,
*   `Tokeniser` (object) &ndash; settings for the [`RegexSplitTokeniser`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/RegexSplitTokeniser.cs) used for text tokenisation:
    *   `Pattern` (string) &ndash; [regular expression](http://en.wikipedia.org/wiki/Regular_expression) separator pattern,
    *   `Options` (number|string) &ndash; [`System.Text.RegularExpressions.RegexOptions`](http://docs.microsoft.com/en-gb/dotnet/api/system.text.regularexpressions.regexoptions),
*   `ShatteringOptions` (object) &ndash; [`ShatteringOptions`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/ShatteringOptions.cs) used for text tokenisation,
*   `Pen` (object) &ndash; settings for the [`Pen`](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/Pen.cs) used for text generation:
    *   `ComparisonType` (number|string) &ndash; [`System.StringComparison`](http://docs.microsoft.com/en-gb/dotnet/api/system.stringcomparison) type,
    *   `SentinelToken` (string) &ndash; ending token,
    *   `Intern` (boolean) &ndash; whether or not to intern tokens using the [`System.String.Intern(System.String)` method](http://docs.microsoft.com/en-gb/dotnet/api/system.string.intern).

Most, if not all, fields may be omitted. Omitted values are usually substituted by the corresponding type's default value (e. g. 0 for number types), with a few notable exceptions:

*   `Text:WebSource:Encoding`: if omitted or `null`, [*UTF-8*](http://en.wikipedia.org/wiki/UTF-8) is used,
*   `Text:RandomGenerator:FromPosition`: `null` is the default,
*   `Text:RandomGenerator:MaxTokens`: [`System.Int32.MaxValue`](http://docs.microsoft.com/en-gb/dotnet/api/system.int32.maxvalue) (2^32 &minus; 1 = 2147483647) is the default,
*   `Tokeniser:Pattern`: `RegexSplitTokeniser.DefaultInclusivePattern` field of the [`RegexSplitTokeniser` class](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/RegexSplitTokeniser.cs) is the default,
*   `Tokeniser:Options`: `RegexTokeniser.DefaultOptions` field of the [`RegexTokeniser` class](http://github.com/DavorPenzar/magic-text/blob/main/MagicText/src/RegexTokeniser.cs) is the default,
*   `Pen:ComparisonType` &ndash; [`System.StringComparison.Ordinal`](http://docs.microsoft.com/en-gb/dotnet/api/system.stringcomparison#system-stringcomparison-ordinal) is the default.

**Nota bene.** To avoid problems (including exceptions) caused by [Issue #36510: *Null Configuration Elements Deserialized as Empty Strings*](http://github.com/dotnet/runtime/issues/36510), all `null` and empty [`System.String`s](http://docs.microsoft.com/en-gb/dotnet/api/system.string) read from the [`Microsoft.Extensions.Configuration.IConfiguration`](http://docs.microsoft.com/en-gb/dotnet/api/microsoft.extensions.configuration.iconfiguration) are treated as `null`s. As a result, `Tokeniser:Pattern` and `Pen:SentinelToken` cannot be set to a empty [`System.String`s](http://docs.microsoft.com/en-gb/dotnet/api/system.string) (`""` or [`System.String.Empty`](http://docs.microsoft.com/en-gb/dotnet/api/system.string.empty)).
