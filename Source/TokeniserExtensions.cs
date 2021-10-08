using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Provides auxiliary extension methods for the instances of the <see cref="ITokeniser" /> interface.</summary>
    /// <seealso cref="ITokeniser" />
    [CLSCompliant(true)]
    public static class TokeniserExtensions
    {
        private const string TokeniserNullErrorMessage = "Tokeniser cannot be null.";
        private const string TextNullErrorMessage = "Input string cannot be null.";
        private const string InputNullErrorMessage = "Input stream cannot be null.";
        private const string InvalidStreamErrorMessage = "Cannot read from the input stream.";

        /// <summary>The default buffer size for reading from <see cref="Stream" />s.</summary>
        /// <remarks>
        ///     <para>The default buffer size is 1024 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and -1 in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private const int DefaultBufferSize =
#if NETSTANDARD2_0
            1024;
#else
            -1;
#endif // NETSTANDARD2_0

        /// <summary>The default policy of detecting byte order marks at the beginning of files (<see cref="Stream" />s).</summary>
        /// <remarks>
        ///     <para>The default policy of detecting byte order marks for this library is <c>false</c>. This is different from the standard library defaults, where <c>true</c> is the default.</para>
        /// </remarks>
        private const bool DefaultDetectEncodingFromByteOrderMarks = false;

        /// <summary>The default policy of leaving <see cref="Stream" />s open when disposing <see cref="StreamReader" />s.</summary>
        /// <remarks>
        ///     <para>The default policy of leaving <see cref="Stream" />s open for this library is <c>true</c>. This is different from the standard library defaults, where <c>false</c> is the default.</para>
        /// </remarks>
        private const bool DefaultLeaveOpen = true;

        private static readonly Encoding? _defaultEncoding;

        /// <summary>Gets the default <see cref="Encoding" /> for reading <see cref="Char" />s from and writing <see cref="Char" />s to <see cref="Stream" />s as text resources.</summary>
        /// <returns>The default <see cref="Encoding" />.</returns>
        /// <remarks>
        ///     <para>The default <see cref="Encoding" /> is <see cref="Encoding.UTF8" /> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md"><em>.NET Standard 2.0</em></a> and <c>null</c> in <a href="http://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.1.md"><em>.NET Standard 2.1</em></a>.</para>
        /// </remarks>
        private static Encoding? DefaultEncoding => _defaultEncoding;

        /// <summary>Initialises static fields.</summary>
        static TokeniserExtensions()
        {
#if NETSTANDARD2_0
            _defaultEncoding = Encoding.UTF8;
#else
            _defaultEncoding = null;
#endif // NETSTANDARD2_0
        }

        /// <summary>Creates a <see cref="StreamReader" /> for reading <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="stream" /></c>.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream, Encoding? encoding) =>
            new StreamReader(stream: stream, encoding: encoding, detectEncodingFromByteOrderMarks: DefaultDetectEncodingFromByteOrderMarks, bufferSize: DefaultBufferSize, leaveOpen: DefaultLeaveOpen);

        /// <summary>Creates a <see cref="StreamReader" /> for reading <c><paramref name="stream" /></c>.</summary>
        /// <param name="stream">The <see cref="Stream" /> from which to read data.</param>
        /// <returns>A <see cref="StreamReader" /> for reading from the <c><paramref name="stream" /></c> with default settings.</returns>
        /// <remarks>
        ///     <para>The method is intended for the internal use only and therefore does not make unnecessary checks of the parameters.</para>
        ///     <para>The <see cref="TokeniserExtensions" />' internal default settings are used for construction of the <see cref="StreamReader" />. These settings should coincide with the actual defaults of the <see cref="StreamReader" /> class.</para>
        ///     <para>Disposing of the <see cref="StreamReader" /> will neither dispose nor close the <c><paramref name="stream" /></c>.</para>
        ///     <para>The exceptions thrown by the <see cref="StreamReader(Stream, Encoding, Boolean, Int32, Boolean)" /> constructor are not caught.</para>
        /// </remarks>
        private static StreamReader CreateDefaultStreamReader(Stream stream) =>
            CreateDefaultStreamReader(stream: stream, encoding: DefaultEncoding);

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="text" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
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

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
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

            TextReader inputReader;
            try
            {
                inputReader = CreateDefaultStreamReader(input, encoding);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input), ex);
            }

            using (inputReader)
            {
                foreach (String? token in tokeniser.Shatter(inputReader, options))
                {
                    yield return token;
                }
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" /> extension method or the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is merely a query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IEnumerable<String?> Shatter(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null)
        {
            foreach (String? token in Shatter(tokeniser, input, DefaultEncoding, options))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <remarks>
        ///     <para>The returned enumerable is a fully-built container and is therefore safe to enumerate even after disposing the <c><paramref name="input" /></c>. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(input, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null.</c></exception>
        /// <remarks>
        ///     <para>The returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="Shatter(ITokeniser, String, ShatteringOptions?)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, String text, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(text, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatteringOptions" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>(tokeniser.Shatter(input, encoding, options));
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <returns>The list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" /> extension method or the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>The returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        public static IList<String?> ShatterToList(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null) =>
            ShatterToList(tokeniser, input, DefaultEncoding, options);

        /// <summary>Shatters <c><paramref name="text" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> may be used to cancel the shattering process without extra caution.</para>
        ///     <para>The parameter <c><paramref name="continueTasksOnCapturedContext" /></c> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <c><paramref name="text" /></c>. Providing <c>true</c> as <c><paramref name="continueTasksOnCapturedContext" /></c> indeed passes the value to the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may result in negative consequences. The parameter is exposed only to maintain consistency of method signatures and calls.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="text" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
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
            await foreach (String? token in tokeniser.ShatterAsync(textReader, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), TextNullErrorMessage);
            }

            TextReader inputReader;
            try
            {
                inputReader = CreateDefaultStreamReader(input, encoding);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(InvalidStreamErrorMessage, nameof(input), ex);
            }

            using (inputReader)
            {
                await foreach (String? token in tokeniser.ShatterAsync(inputReader, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    yield return token;
                }
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into tokens asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" /> extension method or the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The returned asynchronous enumerable is merely an asynchronous query for enumerating tokens (also known as <em>deferred execution</em>) to allow simultaneously reading and enumerating tokens from the <c><paramref name="input" /></c>. If a fully built container is needed, consider using the <see cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" /> extension method instead to improve performance.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async IAsyncEnumerable<String?> ShatterAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            await foreach (String? token in ShatterAsync(tokeniser, input, DefaultEncoding, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
            {
                yield return token;
            }
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="TextReader" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> thrown by cancelling the shattering operation via the <c><paramref name="cancellationToken" /></c> is not caught but is propagated to the original caller; otherwise the <see cref="OperationCanceledException" /> is caught and it merely terminates the shattering operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The task that represents the asynchronous shattering operation. Its value of the <see cref="Task{TResult}.Result" /> property is the list of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null.</c></exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, if the <c><paramref name="input" /></c> is a <see cref="StreamReader" />, data having already been read from the underlying <see cref="Stream" /> may be irrecoverable when cancelling the operation. The parameter <c><paramref name="throwExceptionOnCancellation" /></c> is provided primarily to allow recovering as much data as possible, although extracted tokens might not be identical to original, raw data.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>false</c> and the encapsulated shattering operation is cancelled via the <c><paramref name="cancellationToken" /></c>, this only terminates the shattering operation. No <see cref="OperationCanceledException" /> is then being thrown and the tokens having already been extracted from the <c><paramref name="input" /></c> are still returned in a <see cref="List{T}" />. Thus the <c><paramref name="throwExceptionOnCancellation" /></c> may be used to control the shattering operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, TextReader input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean throwExceptionOnCancellation = true, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            try
            {
                await foreach (String? token in tokeniser.ShatterAsync(input, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    tokens.Add(token);
                }
            }
            catch (OperationCanceledException) when (!throwExceptionOnCancellation)
            {
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters <c><paramref name="text" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="text">The input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> thrown by cancelling the shattering operation via the <c><paramref name="cancellationToken" /></c> is not caught but is propagated to the original caller; otherwise the <see cref="OperationCanceledException" /> is caught and it merely terminates the shattering operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The task that represents the asynchronous shattering operation. Its value of the <see cref="Task{TResult}.Result" /> property is the list of tokens (in the order they were read) read from the <c><paramref name="text" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="text" /></c> is <c>null.</c></exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>Since <see cref="String" />s are immutable and the encapsulated <see cref="StringReader" /> is not available outside of the method, the <c><paramref name="cancellationToken" /></c> may be used to cancel the shattering process without extra caution. The parameter <c><paramref name="throwExceptionOnCancellation" /></c> is provided primarily to maintain consistency of method signatures and calls.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>false</c> and the encapsulated shattering operation is cancelled via the <c><paramref name="cancellationToken" /></c>, this only terminates the shattering operation. No <see cref="OperationCanceledException" /> is then being thrown and the tokens having already been extracted from the <c><paramref name="text" /></c> are still returned in a <see cref="List{T}" />. Thus the <c><paramref name="throwExceptionOnCancellation" /></c> may be used to control the shattering operation.</para>
        ///     <para>The parameter <c><paramref name="continueTasksOnCapturedContext" /></c> should always be set to <c>false</c> as every context has reading access to all <see cref="String" />s, including <c><paramref name="text" /></c>. Providing <c>true</c> as <c><paramref name="continueTasksOnCapturedContext" /></c> indeed passes the value to the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call, which may result in negative consequences. The parameter is exposed only to maintain consistency of method signatures and calls.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" /> method call (notably the <see cref="ArgumentNullException" />) are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, String text, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean throwExceptionOnCancellation = true, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            try
            {
                await foreach (String? token in tokeniser.ShatterAsync(text, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    tokens.Add(token);
                }
            }
            catch (OperationCanceledException) when (!throwExceptionOnCancellation)
            {
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="encoding">The <see cref="Encoding" /> to use to read <see cref="Char" />s from the <c><paramref name="input" /></c>.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> thrown by cancelling the shattering operation via the <c><paramref name="cancellationToken" /></c> is not caught but is propagated to the original caller; otherwise the <see cref="OperationCanceledException" /> is caught and it merely terminates the shattering operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation. The parameter <c><paramref name="throwExceptionOnCancellation" /></c> is provided to allow recovering as much data as possible, although extracted tokens might not be identical to original, raw data.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>false</c> and the encapsulated shattering operation is cancelled via the <c><paramref name="cancellationToken" /></c>, this only terminates the shattering operation. No <see cref="OperationCanceledException" /> is then being thrown and the tokens having already been extracted from the <c><paramref name="input" /></c> are still returned in a <see cref="List{T}" />. Thus the <c><paramref name="throwExceptionOnCancellation" /></c> may be used to control the shattering operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, Stream input, Encoding? encoding, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean throwExceptionOnCancellation = true, Boolean continueTasksOnCapturedContext = false)
        {
            if (tokeniser is null)
            {
                throw new ArgumentNullException(nameof(tokeniser), TokeniserNullErrorMessage);
            }

            List<String?> tokens = new List<String?>();
            try
            {
                await foreach (String? token in tokeniser.ShatterAsync(input, encoding, options, cancellationToken, continueTasksOnCapturedContext).WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext))
                {
                    tokens.Add(token);
                }
            }
            catch (OperationCanceledException) when (!throwExceptionOnCancellation)
            {
            }
            tokens.TrimExcess();

            return tokens;
        }

        /// <summary>Shatters text read from the <c><paramref name="input" /></c> into a token list asynchronously.</summary>
        /// <param name="tokeniser">The tokeniser used for shattering.</param>
        /// <param name="input">The <see cref="Stream" /> from which the input text is read.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults (<see cref="ShatteringOptions.Default" />) are used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="throwExceptionOnCancellation">If <c>true</c>, the <see cref="OperationCanceledException" /> thrown by cancelling the shattering operation via the <c><paramref name="cancellationToken" /></c> is not caught but is propagated to the original caller; otherwise the <see cref="OperationCanceledException" /> is caught and it merely terminates the shattering operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s (e. g. the <see cref="TextReader.ReadAsync(Char[], Int32, Int32)" />, <see cref="TextReader.ReadLineAsync()" /> and <see cref="IAsyncEnumerator{T}.MoveNextAsync()" /> method calls) should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> method, the <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension method etc.).</param>
        /// <returns>The asynchronous enumerable of tokens (in the order they were read) read from the <c><paramref name="input" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="tokeniser" /></c> is <c>null</c>. The parameter <c><paramref name="input" /></c> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <c><paramref name="input" /></c> is not readable.</exception>
        /// <exception cref="OperationCanceledException">The operation is cancelled via the <c><paramref name="cancellationToken" /></c> and the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>true</c>.</exception>
        /// <remarks>
        ///     <para>The <see cref="StreamReader" /> used in the method for reading from the <c><paramref name="input" /></c> is constructed with the standard library default <see cref="Encoding" /> and buffer size, but other custom library defaults are used: byte order marks are not looked for at the beginning of the <c><paramref name="input" /></c> and the <c><paramref name="input" /></c> is left open. To control the settings of the <see cref="StreamReader" />, use the <see cref="TokeniserExtensions.Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" /> extension method or the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> method with a custom <see cref="StreamReader" /> instead.</para>
        ///     <para>The <c><paramref name="input" /></c> is neither disposed of nor closed in the method. This must be done manually <strong>after</strong> retrieving the tokens.</para>
        ///     <para>Although the method accepts a <c><paramref name="cancellationToken" /></c> to support cancelling the operation, this should be used with caution. For instance, data having already been read from the <see cref="Stream" /> <c><paramref name="input" /></c> may be irrecoverable when cancelling the operation. The parameter <c><paramref name="throwExceptionOnCancellation" /></c> is provided to allow recovering as much data as possible, although extracted tokens might not be identical to original, raw data.</para>
        ///     <para>If the <c><paramref name="throwExceptionOnCancellation" /></c> is <c>false</c> and the encapsulated shattering operation is cancelled via the <c><paramref name="cancellationToken" /></c>, this only terminates the shattering operation. No <see cref="OperationCanceledException" /> is then being thrown and the tokens having already been extracted from the <c><paramref name="input" /></c> are still returned in a <see cref="List{T}" />. Thus the <c><paramref name="throwExceptionOnCancellation" /></c> may be used to control the shattering operation.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous shattering process. However, in some cases only the original context might have reading access to the resource provided by the <c><paramref name="input" /></c>, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>The ultimately returned enumerable is a fully-built container. However, as such it is impossible to enumerate it before the complete reading and shattering process is finished.</para>
        ///     <para>The exceptions thrown by the <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> method call are not caught.</para>
        /// </remarks>
        /// <seealso cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, TextReader, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToListAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean, Boolean)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, TextReader, ShatteringOptions?)" />
        /// <seealso cref="ShatterToList(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, Stream, Encoding?, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="ShatterAsync(ITokeniser, String, ShatteringOptions?, CancellationToken, Boolean)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, Stream, Encoding?, ShatteringOptions?)" />
        /// <seealso cref="Shatter(ITokeniser, String, ShatteringOptions?)" />
        /// <seealso cref="ShatteringOptions" />
        public static async Task<IList<String?>> ShatterToListAsync(this ITokeniser tokeniser, Stream input, ShatteringOptions? options = null, CancellationToken cancellationToken = default, Boolean throwExceptionOnCancellation = true, Boolean continueTasksOnCapturedContext = false) =>
            await ShatterToListAsync(tokeniser, input, DefaultEncoding, options, cancellationToken, throwExceptionOnCancellation, continueTasksOnCapturedContext).ConfigureAwait(continueTasksOnCapturedContext);
    }
}
