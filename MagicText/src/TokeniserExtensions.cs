using MagicText.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Provides auxiliary extension methods for the <see cref="ITokeniser" /> interface.</summary>
    [CLSCompliant(true)]
    public static class TokeniserExtensions
    {
        private const string TokeniserNullErrorMessage = "Tokeniser cannot be null.";
        private const string TextNullErrorMessage = "Input string cannot be null.";
        private const string InputNullErrorMessage = "Input stream cannot be null.";
#if NETSTANDARD2_0
        private const string EncodingNullErrorMessage = "Stream's byte encoding cannot be null.";
#endif // NETSTANDARD2_0
        private const string InvalidStreamErrorMessage = "Cannot read from the input stream.";

#if NETSTANDARD2_0
        /// <summary>The default buffer size for reading from <see cref="Stream" />s.</summary>
        /// <remarks>
        ///     <para>The default buffer size is 1024 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and -1 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private const int DefaultBufferSize = 0x400;
#else
        /// <summary>The default buffer size for reading from <see cref="Stream" />s.</summary>
        /// <remarks>
        ///     <para>The default buffer size is 1024 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and -1 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private const int DefaultBufferSize = -1;
#endif // NETSTANDARD2_0

        private static readonly Encoding? _defaultEncoding;

#if NETSTANDARD2_0
        /// <summary>Gets the default <see cref="Encoding" /> for reading <see cref="Char" />s from and writing <see cref="Char" />s to <see cref="Stream" />s as text resources.</summary>
        /// <returns>The default <see cref="Encoding" />.</returns>
        /// <remarks>
        ///     <para>The default <see cref="Encoding" /> is <see cref="Encoding.UTF8" /> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and <c>null</c> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private static Encoding DefaultEncoding => _defaultEncoding!;
#else
        /// <summary>Gets the default <see cref="Encoding" /> for reading <see cref="Char" />s from and writing <see cref="Char" />s to <see cref="Stream" />s as text resources.</summary>
        /// <returns>The default <see cref="Encoding" />.</returns>
        /// <remarks>
        ///     <para>The default <see cref="Encoding" /> is <see cref="Encoding.UTF8" /> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and <c>null</c> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private static Encoding? DefaultEncoding => _defaultEncoding;
#endif // NETSTANDARD2_0

        /// <summary>Initialises static fields.</summary>
        static TokeniserExtensions()
        {
#if NETSTANDARD2_0
            _defaultEncoding = Encoding.UTF8;
#else
            _defaultEncoding = null;
#endif // NETSTANDARD2_0
        }

#if NETSTANDARD2_0

        /// <summary>Creates a <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="stream" /></c>.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with <see cref="TokeniserExtensions" />' internal default default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class regarding buffer size; however, different policies for detecting byte order marks (BOM) and leaving the <c><paramref name="stream" /></c> open are used.</para>
        ///     <para>Byte order marks are not looked for at the beginning of the <c><paramref name="stream" /></c>.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream, Encoding encoding) =>
            new StreamReader(stream: stream, encoding: encoding, detectEncodingFromByteOrderMarks: false, bufferSize: DefaultBufferSize, leaveOpen: true);

#else

        /// <summary>Creates a <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="stream" /></c>.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with <see cref="TokeniserExtensions" />' internal default default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class regarding buffer size; however, different policies for detecting byte order marks (BOM) and leaving the <c><paramref name="stream" /></c> open are used.</para>
        ///     <para>Byte order marks are not looked for at the beginning of the <c><paramref name="stream" /></c>.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream, Encoding? encoding) =>
            new StreamReader(stream: stream, encoding: encoding, detectEncodingFromByteOrderMarks: false, bufferSize: DefaultBufferSize, leaveOpen: true);

#endif // NETSTANDARD2_0

        /// <summary>Creates a <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with <see cref="TokeniserExtensions" />' internal default default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class regarding <see cref="Encoding" /> and buffer size; however, different policies for detecting byte order marks (BOM) and leaving the <c><paramref name="stream" /></c> open are used.</para>
        ///     <para>Byte order marks are not looked for at the beginning of the <c><paramref name="stream" /></c>.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream) =>
            CreateDefaultStreamReader(stream, DefaultEncoding);

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering <c><paramref name="text" /></c>.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="text" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArray(ITokeniser, String, ShatteringOptions)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        /// </remarks>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            using TextReader textReader = new StringReader(text);
            foreach (String? token in tokeniser.Shatter(textReader, options))
            {
                yield return token;
            }
        }

#if NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="encoding" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArray(ITokeniser, Stream, Encoding, ShatteringOptions)" /> extension method instead to improve performance  and to avoid accidentally enumerating the query after disposing/closing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        /// </remarks>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, Stream input, Encoding encoding, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding), EncodingNullErrorMessage);
            }

            if (!input.CanRead)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input));
            }

            using TextReader inputReader = CreateDefaultStreamReader(input, encoding);
            foreach (String? token in tokeniser.Shatter(inputReader, options))
            {
                yield return token;
            }
        }

#else

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArray(ITokeniser, Stream, Encoding, ShatteringOptions)" /> extension method instead to improve performance  and to avoid accidentally enumerating the query after disposing/closing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        /// </remarks>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), InputNullErrorMessage);
            }

            if (!input.CanRead)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input));
            }

            using TextReader inputReader = CreateDefaultStreamReader(input, encoding);
            foreach (String? token in tokeniser.Shatter(inputReader, options))
            {
                yield return token;
            }
        }

#endif // NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size and <see cref="Encoding" />, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArray(ITokeniser, Stream, Encoding, ShatteringOptions)" /> extension method instead to improve performance  and to avoid accidentally enumerating the query after disposing/closing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        /// </remarks>
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null) =>
            Shatter(tokeniser, input, DefaultEncoding, options);

        /// <summary>Shatters text read from the <c><paramref name="inputReader" /></c> into a token <see cref="Array" />.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="inputReader">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="inputReader" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="inputReader" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing the <c><paramref name="inputReader" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is essentially the same as chaining the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method and the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" /> extension method. However, it is still provided as a convenience single-call self-explanatory method.</para>
        /// </remarks>
        public static String?[] ShatterToArray(this ITokeniser tokeniser, TextReader inputReader, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            return tokeniser.Shatter(inputReader, options).AsBuffer();
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token <see cref="Array" />.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering <c><paramref name="text" /></c>.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="text" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The returned <see cref="Array" /> is a fully-built container.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is essentially the same as chaining the <see cref="TokeniserExtensions.Shatter(ITokeniser, String, ShatteringOptions)" /> extension method and the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" /> extension method. However, it is still provided as a convenience single-call self-explanatory method.</para>
        /// </remarks>
        public static String?[] ShatterToArray(this ITokeniser tokeniser, String text, ShatteringOptions? options = null) =>
            Shatter(tokeniser, text, options).AsBuffer();

#if NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" />.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="encoding" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> extension method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is essentially the same as chaining the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding, ShatteringOptions)" /> extension method and the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" /> extension method. However, it is still provided as a convenience single-call self-explanatory method.</para>
        /// </remarks>
        public static String?[] ShatterToArray(this ITokeniser tokeniser, Stream input, Encoding encoding, ShatteringOptions? options = null) =>
            Shatter(tokeniser, input, encoding, options).AsBuffer();

#else

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" />.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> extension method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is essentially the same as chaining the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding, ShatteringOptions)" /> extension method and the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" /> extension method. However, it is still provided as a convenience single-call self-explanatory method.</para>
        /// </remarks>
        public static String?[] ShatterToArray(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null) =>
            Shatter(tokeniser, input, encoding, options).AsBuffer();

#endif // NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" />.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <returns>An <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size and <see cref="Encoding" />, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ShatterToArray(ITokeniser, TextReader, ShatteringOptions)" /> extension method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is essentially the same as chaining the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, ShatteringOptions)" /> extension method and the <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})" /> extension method. However, it is still provided as a convenience single-call self-explanatory method.</para>
        /// </remarks>
        public static String?[] ShatterToArray(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null) =>
            ShatterToArray(tokeniser, input, DefaultEncoding, options);

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering <c><paramref name="text" /></c>.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="text" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> parameter may be used to cancel the shattering operation without extra caution.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the 0 extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        /// </remarks>
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), TextNullErrorMessage);
            }

            using TextReader textReader = new StringReader(text);
            await foreach (String? token in tokeniser.ShatterAsync(textReader, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

#if NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="encoding" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArrayAsync(ITokeniser, Stream, Encoding, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after closing/disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        /// </remarks>
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, Encoding encoding, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), TextNullErrorMessage);
            }
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding), EncodingNullErrorMessage);
            }

            if (!input.CanRead)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input));
            }

            using TextReader inputReader = CreateDefaultStreamReader(input, encoding);
            await foreach (String? token in tokeniser.ShatterAsync(inputReader, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

#else

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArrayAsync(ITokeniser, Stream, Encoding, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after closing/disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        /// </remarks>
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), TextNullErrorMessage);
            }

            if (!input.CanRead)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input));
            }

            using TextReader inputReader = CreateDefaultStreamReader(input, encoding);
            await foreach (String? token in tokeniser.ShatterAsync(inputReader, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

#endif // NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>An asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <a href="http://docs.microsoft.com/en-gb/dotnet/standard/linq/deferred-execution-lazy-evaluation#deferred-execution"><em>deferred execution</em></a>). If a fully built container is needed, consider using the <see cref="ShatterToArrayAsync(ITokeniser, Stream, ShatteringOptions, Boolean, Boolean, Action{OperationCanceledException}, CancellationToken)" /> extension method instead to improve performance and to avoid accidentally enumerating the query after closing/disposing the <c><paramref name="input" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        /// </remarks>
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (String? token in ShatterAsync(tokeniser, input, DefaultEncoding, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="inputReader" /></c> into a token <see cref="Array" /> asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="inputReader">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> is not caught if the shattering operation is cancelled.</param>
        /// <param name="cancellationCallback">If provided (if not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> if the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c> and the shattering operation is cancelled; it is ignored when the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>A task that represents shattering text from the <c><paramref name="inputReader" /></c> into an <see cref="Array" /> of tokens. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="inputReader" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="inputReader" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="inputReader" /></c> is a <see cref="StreamReader" />, the data having already been read from the underlying <see cref="Stream" /> may be irrecoverable after cancelling the operation.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c>, the list of tokens read up until the cancellation is returned if the shattering operation is cancelled. Additionally, if the <c><paramref name="cancellationCallback" /></c> parameter is provided (is not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> without throwing it (unless <c><paramref name="cancellationCallback" /></c> throws it inself�this is out of the scope of this extension method).</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="inputReader" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing the <c><paramref name="inputReader" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is similar to chaining the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method and the <see cref="M:System.Linq.AsyncEnumerable.ToArrayAsync{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource}, System.Threading.CancellationToken)" /> extension method. However, other than being a convenience single-call self-explanatory method, this method provides additional possibilities, such as intercepting the <see cref="OperationCanceledException" /> via the <c><paramref name="throwExceptionOnCancellation" /></c> and <c><paramref name="cancellationCallback" /></c> parameters.</para>
        /// </remarks>
        public static async Task<String?[]> ShatterToArrayAsync(this ITokeniser tokeniser, TextReader inputReader, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, Boolean throwExceptionOnCancellation = true, Action<OperationCanceledException>? cancellationCallback = null, CancellationToken cancellationToken = default)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            Int32 count = 0;
            String?[] tokens = Array.Empty<String>();

            try
            {
                await foreach (String? token in tokeniser.ShatterAsync(inputReader, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    if (count >= tokens.Length)
                    {
                        Buffering.Expand(ref tokens);
                    }

                    tokens[count++] = token;
                }
            }
            catch (OperationCanceledException exception) when (!throwExceptionOnCancellation)
            {
                cancellationCallback?.Invoke(exception);
            }

            Buffering.TrimExcess(ref tokens, count);

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token <see cref="Array" /> asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering <c><paramref name="text" /></c>.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> is not caught if the shattering operation is cancelled.</param>
        /// <param name="cancellationCallback">If provided (if not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> if the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c> and the shattering operation is cancelled; it is ignored when the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>A task that represents shattering <c><paramref name="text" /></c> into an <see cref="Array" /> of tokens. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="text" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> parameter may be used to cancel the shattering operation without extra caution.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c>, the list of tokens read up until the cancellation is returned if the shattering operation is cancelled. Additionally, if the <c><paramref name="cancellationCallback" /></c> parameter is provided (is not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> without throwing it (unless <c><paramref name="cancellationCallback" /></c> throws it inself�this is out of the scope of this extension method).</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is similar to chaining the <see cref="ShatterAsync(ITokeniser, String, ShatteringOptions, Boolean, CancellationToken)" /> extension method and the <see cref="M:System.Linq.AsyncEnumerable.ToArrayAsync{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource}, System.Threading.CancellationToken)" /> extension method. However, other than being a convenience single-call self-explanatory method, this method provides additional possibilities, such as intercepting the <see cref="OperationCanceledException" /> via the <c><paramref name="throwExceptionOnCancellation" /></c> and <c><paramref name="cancellationCallback" /></c> parameters.</para>
        /// </remarks>
        public static async Task<String?[]> ShatterToArrayAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, Boolean throwExceptionOnCancellation = true, Action<OperationCanceledException>? cancellationCallback = null, CancellationToken cancellationToken = default)
        {
            Int32 count = 0;
            String?[] tokens = Array.Empty<String>();

            try
            {
                await foreach (String? token in ShatterAsync(tokeniser, text, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    if (count >= tokens.Length)
                    {
                        Buffering.Expand(ref tokens);
                    }

                    tokens[count++] = token;
                }
            }
            catch (OperationCanceledException exception) when (!throwExceptionOnCancellation)
            {
                cancellationCallback?.Invoke(exception);
            }

            Buffering.TrimExcess(ref tokens, count);

            return tokens;
        }

#if NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" /> asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> is not caught if the shattering operation is cancelled.</param>
        /// <param name="cancellationCallback">If provided (if not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> if the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c> and the shattering operation is cancelled; it is ignored when the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>A task that represents shattering text from the <c><paramref name="input" /></c> into an <see cref="Array" /> of tokens. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>,</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="encoding" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c>, the list of tokens read up until the cancellation is returned if the shattering operation is cancelled. Additionally, if the <c><paramref name="cancellationCallback" /></c> parameter is provided (is not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> without throwing it (unless <c><paramref name="cancellationCallback" /></c> throws it inself�this is out of the scope of this extension method).</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is similar to chaining the <see cref="ShatterAsync(ITokeniser, Stream, Encoding, ShatteringOptions, Boolean, CancellationToken)" /> extension method and the <see cref="M:System.Linq.AsyncEnumerable.ToArrayAsync{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource}, System.Threading.CancellationToken)" /> extension method. However, other than being a convenience single-call self-explanatory method, this method provides additional possibilities, such as intercepting the <see cref="OperationCanceledException" /> via the <c><paramref name="throwExceptionOnCancellation" /></c> and <c><paramref name="cancellationCallback" /></c> parameters.</para>
        /// </remarks>
        public static async Task<String?[]> ShatterToArrayAsync(this ITokeniser tokeniser, Stream input, Encoding encoding, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, Boolean throwExceptionOnCancellation = true, Action<OperationCanceledException>? cancellationCallback = null, CancellationToken cancellationToken = default)
        {
            Int32 count = 0;
            String?[] tokens = Array.Empty<String>();

            try
            {
                await foreach (String? token in ShatterAsync(tokeniser, input, encoding, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    if (count >= tokens.Length)
                    {
                        Buffering.Expand(ref tokens);
                    }

                    tokens[count++] = token;
                }
            }
            catch (OperationCanceledException exception) when (!throwExceptionOnCancellation)
            {
                cancellationCallback?.Invoke(exception);
            }

            Buffering.TrimExcess(ref tokens, count);

            return tokens;
        }

#else

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" /> asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> is not caught if the shattering operation is cancelled.</param>
        /// <param name="cancellationCallback">If provided (if not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> if the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c> and the shattering operation is cancelled; it is ignored when the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>A task that represents shattering text from the <c><paramref name="input" /></c> into an <see cref="Array" /> of tokens. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c>, the list of tokens read up until the cancellation is returned if the shattering operation is cancelled. Additionally, if the <c><paramref name="cancellationCallback" /></c> parameter is provided (is not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> without throwing it (unless <c><paramref name="cancellationCallback" /></c> throws it inself�this is out of the scope of this extension method).</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is similar to chaining the <see cref="ShatterAsync(ITokeniser, Stream, Encoding, ShatteringOptions, Boolean, CancellationToken)" /> extension method and the <see cref="M:System.Linq.AsyncEnumerable.ToArrayAsync{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource}, System.Threading.CancellationToken)" /> extension method. However, other than being a convenience single-call self-explanatory method, this method provides additional possibilities, such as intercepting the <see cref="OperationCanceledException" /> via the <c><paramref name="throwExceptionOnCancellation" /></c> and <c><paramref name="cancellationCallback" /></c> parameters.</para>
        /// </remarks>
        public static async Task<String?[]> ShatterToArrayAsync(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, Boolean throwExceptionOnCancellation = true, Action<OperationCanceledException>? cancellationCallback = null, CancellationToken cancellationToken = default)
        {
            Int32 count = 0;
            String?[] tokens = Array.Empty<String>();

            try
            {
                await foreach (String? token in ShatterAsync(tokeniser, input, encoding, options, continueTasksOnCapturedContext, cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    if (count >= tokens.Length)
                    {
                        Buffering.Expand(ref tokens);
                    }

                    tokens[count++] = token;
                }
            }
            catch (OperationCanceledException exception) when (!throwExceptionOnCancellation)
            {
                cancellationCallback?.Invoke(exception);
            }

            Buffering.TrimExcess(ref tokens, count);

            return tokens;
        }

#endif // NETSTANDARD2_0

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token <see cref="Array" /> asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering text.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">The options to control the shattering behaviour. If <c>null</c>, the defaults are used (<see cref="ShatteringOptions.Default" />)</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s and <see cref="ValueTask" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) is marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> is not caught if the shattering operation is cancelled.</param>
        /// <param name="cancellationCallback">If provided (if not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> if the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c> and the shattering operation is cancelled; it is ignored when the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>true</c>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the shattering operation.</param>
        /// <returns>A task that represents shattering text from the <c><paramref name="input" /></c> into an <see cref="Array" /> of tokens. Its <see cref="Task{TResult}.Result" /> property is the resulting <see cref="Array" /> of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>the <c><paramref name="tokeniser" /></c> parameter is <c>null</c>, or</item>
        ///         <item>the <c><paramref name="input" /></c> parameter is <c>null</c>.</item>
        ///     </list>
        /// </exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks (BOM) are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open after disposing the <see cref="StreamReader" />. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, the data having already been read from the <c><paramref name="input" /></c> may be irrecoverable after cancelling the operation.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> parameter is <c>false</c>, the list of tokens read up until the cancellation is returned if the shattering operation is cancelled. Additionally, if the <c><paramref name="cancellationCallback" /></c> parameter is provided (is not <c>null</c>), it is invoked on the caught <see cref="OperationCanceledException" /> without throwing it (unless <c><paramref name="cancellationCallback" /></c> throws it inself�this is out of the scope of this extension method).</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter is desirable as it may optimise the asynchronous shattering process. However, in some cases the <c><paramref name="tokeniser" /></c>'s logic might be <see cref="SynchronizationContext" /> dependent and/or only the original <see cref="SynchronizationContext" /> might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus the <c><paramref name="continueTasksOnCapturedContext" /></c> parameter should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned <see cref="Array" /> is a fully-built container and is therefore safe to enumerate even after disposing/closing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering operation is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor call and the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> method call are not caught.</para>
        ///
        ///     <h3><a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/"><em>LINQ</em></a> Alternatives</h3>
        ///     <para>This extension method is similar to chaining the <see cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions, Boolean, CancellationToken)" /> extension method and the <see cref="M:System.Linq.AsyncEnumerable.ToArrayAsync{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource}, System.Threading.CancellationToken)" /> extension method. However, other than being a convenience single-call self-explanatory method, this method provides additional possibilities, such as intercepting the <see cref="OperationCanceledException" /> via the <c><paramref name="throwExceptionOnCancellation" /></c> and <c><paramref name="cancellationCallback" /></c> parameters.</para>
        /// </remarks>
        public static async Task<String?[]> ShatterToArrayAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, Boolean continueTasksOnCapturedContext = false, Boolean throwExceptionOnCancellation = true, Action<OperationCanceledException>? cancellationCallback = null, CancellationToken cancellationToken = default) =>
            await ShatterToArrayAsync(tokeniser, input, DefaultEncoding, options, continueTasksOnCapturedContext, throwExceptionOnCancellation, cancellationCallback, cancellationToken).ConfigureAwait(continueTasksOnCapturedContext);
    }
}
