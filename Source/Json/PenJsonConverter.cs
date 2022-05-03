using MagicText.Internal.Extensions;
using MagicText.Internal.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Json
{
    /// <summary>Provides methods to convert <see cref="Pen" />s to <a href="http://json.org/json-en.html"><em>JSON</em></a> objects and <em>vice-versa</em>.</summary>
    /// <remarks>
    ///     <para>This is the default <see cref="JsonConverter{T}" /> for the <see cref="Pen" /> class. It is optimised to avoid time-expensive initialisation of <see cref="Pen" />s using the usual non-copy constructors (e. g. <see cref="Pen.Pen(IEnumerable{String}, StringComparer, String, Boolean)" />, <see cref="Pen.Pen(IEnumerable{String}, StringComparison, String, Boolean)" />) on deserialisation. Do not manually alter or create serialised <a href="http://json.org/json-en.html"><em>JSON</em></a> data and deserialise it back to a <see cref="Pen" /> instance. Any manipulation of the serialised <a href="http://json.org/json-en.html"><em>JSON</em></a> data may result in <see cref="Pen" />s with unexpected behaviour. Using such <see cref="Pen" />s may even cause uncaught exceptions not explicitly documented for the methods of the <see cref="Pen" /> class.</para>
    ///     <para>Automatically, a default <see cref="PenJsonConverter" /> (as initialised by the default <see cref="PenJsonConverter()" /> constructor) may only serialise and deserialise <see cref="Pen" />s whose <see cref="Pen.Comparer" /> property is one of the following <see cref="StringComparer" />s: <see cref="StringComparer.Ordinal" />, <see cref="StringComparer.OrdinalIgnoreCase" />, <see cref="StringComparer.InvariantCulture" /> and <see cref="StringComparer.InvariantCultureIgnoreCase" />. Additionally, <see cref="StringComparer" />s with a custom <see cref="JsonConverter{T}" />, where the generic type parameter <c>T</c> is <strong>exactly</strong> the <see cref="StringComparer" />'s type, may be automatically handled if the <see cref="JsonConverter{T}" /> is either registered to the <see cref="StringComparer" />'s type (via the <see cref="JsonConverterAttribute" />) or passed to the serialisation/deserialisation method through <see cref="JsonSerializerOptions" />. In all other cases please implement a <see cref="JsonConverter{T}" /> that accepts <strong>any <see cref="StringComparer" /></strong> (i. e. set the generic type parameter <c>T</c> to the abstract <see cref="StringComparer" /> class) and pass the <see cref="JsonConverter{T}" /> instance either to the <see cref="PenJsonConverter(JsonConverter{StringComparer})" /> constructor or to the serialisation/deserialisation method through <see cref="JsonSerializerOptions" />. Any <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s passed through <see cref="JsonSerializerOptions" /> overrides the one passed to the <see cref="PenJsonConverter(JsonConverter{StringComparer})" /> constructor.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class PenJsonConverter : JsonConverter<Pen>
    {
        private delegate T ReadValue<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options);
        private delegate void WriteValue<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions? options);

        /// <summary>Returns the unaltered <a href="http://json.org/json-en.html"><em>JSON</em></a> property <c><paramref name="name" /></c>.</summary>
        /// <param name="name"><a href="http://json.org/json-en.html"><em>JSON</em></a> property name (not) to convert.</param>
        /// <returns>The original value of <c><paramref name="name" /></c> (the same exact reference). If <c><paramref name="name" /></c> is <c>null</c>, <c>null</c> is returned.</returns>
        [return: MaybeNull, NotNullIfNotNull("name")]
        private static String? DoNotConvertName([AllowNull] String? name) =>
            name;

        /// <summary>Reads through <a href="http://json.org/json-en.html"><em>JSON</em></a> comments.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <remarks>
        ///     <para>The method reads through all consecutive comment tokens until a non-comment token is reached and sets the <c><paramref name="reader" /></c>'s position to the next non-comment token. If no comment is found, the <c><paramref name="reader" /></c>'s position remains unchanged.</para>
        /// </remarks>
        private static void SkipComments(ref Utf8JsonReader reader)
        {
            while (reader.TokenType == JsonTokenType.Comment)
            {
                reader.Read();
            }
        }

        /// <summary>Reads a <see cref="Boolean" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="Boolean" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</para>
        /// </remarks>
        private static Boolean ReadBoolean(ref Utf8JsonReader reader, JsonSerializerOptions? options)
        {
            Boolean value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Boolean)) is JsonConverter<Boolean> converter)
            {
                value = converter.Read(ref reader, typeof(Boolean), options);
            }
            else
            {
                value = reader.GetBoolean();
            }

            reader.Read();

            return value;
        }

        /// <summary>Reads an <see cref="Int32" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="Int32" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</para>
        /// </remarks>
        private static Int32 ReadInt32(ref Utf8JsonReader reader, JsonSerializerOptions? options)
        {
            Int32 value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Int32)) is JsonConverter<Int32> converter)
            {
                value = converter.Read(ref reader, typeof(Int32), options);
            }
            else
            {
                value = reader.GetInt32();
            }

            reader.Read();

            return value;
        }

        /// <summary>Reads a <see cref="String" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="String" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</para>
        /// </remarks>
        private static String? ReadString(ref Utf8JsonReader reader, JsonSerializerOptions? options)
        {
            String? value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(String)) is JsonConverter<String> converter)
            {
                value = converter.Read(ref reader, typeof(String), options);
            }
            else
            {
                value = reader.GetString();
            }

            reader.Read();

            return value;
        }

        /// <summary>Reads an <see cref="Array" /> of item type <c><typeparamref name="T" /></c> value.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="readValue">The function to use to read a value of type <c><typeparamref name="T" /></c>.</param>
        /// <returns>The <see cref="Array" /> of item type <c><typeparamref name="T" /></c> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</para>
        /// </remarks>
        private static T[]? ReadArray<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options, ReadValue<T> readValue)
        {
            T[] array;

            // Skip comments.
            SkipComments(ref reader);

            // Convert using a converter if provided.
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                array = converter.Read(ref reader, typeof(T[]), options)!;
                reader.Read();

                return array;
            }

            // Return `null` in case of a `null`.
            if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Read();

                return null;
            }

            // Assert and read the beginning of an array.
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException();
            }
            reader.Read();

            // Initialise an empty array.
            Int32 size = 0;
            array = Array.Empty<T>();

            // Read array.
            while (true)
            {
                // Skip comments.
                SkipComments(ref reader);

                // If the end of the array is reached, read it and stop.
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    reader.Read();

                    break;
                }

                // Expand the array if needed.
                if (size == array.Length)
                {
                    Buffering.Expand(ref array);
                }

                // Read the current item.
                array[size++] = readValue(ref reader, options);
            }

            // Trim the array.
            Buffering.TrimExcess(ref array, size);

            // Return the array.
            return array;
        }

        /// <summary>Reads an <see cref="Array" /> of <see cref="Boolean" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Boolean" />s value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</para>
        /// </remarks>
        private static Boolean[]? ReadBooleanArray(ref Utf8JsonReader reader, JsonSerializerOptions? options) =>
            ReadArray(ref reader, options, ReadBoolean);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Int32" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Int32" />s value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</para>
        /// </remarks>
        private static Int32[]? ReadInt32Array(ref Utf8JsonReader reader, JsonSerializerOptions? options) =>
            ReadArray(ref reader, options, ReadInt32);

        /// <summary>Reads an <see cref="Array" /> of <see cref="String" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="Array" /> of <see cref="String" />s value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</para>
        /// </remarks>
        private static String[]? ReadStringArray(ref Utf8JsonReader reader, JsonSerializerOptions? options) =>
            ReadArray<String>(ref reader, options, ReadString!)!;

        /// <summary>Reads a <see cref="StringComparer" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="defaultConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="StringComparer" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="StringComparer" />.</para>
        /// </remarks>
        private static StringComparer? ReadStringComparer(ref Utf8JsonReader reader, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options)
        {
            StringComparer? value;

            SkipComments(ref reader);

            JsonConverter<StringComparer> converter = null!;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            value = converter.Read(ref reader, typeof(StringComparer), options!);
            reader.Read();

            return value;
        }

        private static void WriteBoolean(Utf8JsonWriter writer, String name, Boolean value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Boolean)) is JsonConverter<Boolean> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteBoolean(name, value);
        }

        private static void WriteBooleanValue(Utf8JsonWriter writer, Boolean value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Boolean)) is JsonConverter<Boolean> converter)
            {
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteBooleanValue(value);
        }

        private static void WriteInt32(Utf8JsonWriter writer, String name, Int32 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int32)) is JsonConverter<Int32> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteNumber(name, value);
        }

        private static void WriteInt32Value(Utf8JsonWriter writer, Int32 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int32)) is JsonConverter<Int32> converter)
            {
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteNumberValue(value);
        }

        private static void WriteString(Utf8JsonWriter writer, String name, String? value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(String)) is JsonConverter<String> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value!, options);

                return;
            }

            writer.WriteString(name, value);
        }

        private static void WriteStringValue(Utf8JsonWriter writer, String? value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(String)) is JsonConverter<String> converter)
            {
                converter.Write(writer, value!, options);

                return;
            }

            writer.WriteStringValue(value);
        }

        private static void WriteArray<T>(Utf8JsonWriter writer, String name, T[]? value, JsonSerializerOptions? options, WriteValue<T> writeValue)
        {
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value!, options);

                return;
            }

            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            writer.WriteStartArray(name);

            foreach (T item in value)
            {
                writeValue(writer, item, options);
            }

            writer.WriteEndArray();
        }

        private static void WriteArrayValue<T>(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions? options, WriteValue<T> writeValue)
        {
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                converter.Write(writer, value!, options);

                return;
            }

            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            writer.WriteStartArray();

            foreach (T item in value)
            {
                writeValue(writer, item, options);
            }

            writer.WriteEndArray();
        }

        private static void WriteBooleanArray(Utf8JsonWriter writer, String name, Boolean[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteBooleanValue);

        private static void WriteBooleanArrayValue(Utf8JsonWriter writer, Boolean[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteBooleanValue);

        private static void WriteInt32Array(Utf8JsonWriter writer, String name, Int32[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteInt32Value);

        private static void WriteInt32ArrayValue(Utf8JsonWriter writer, Int32[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteInt32Value);

        private static void WriteStringArray(Utf8JsonWriter writer, String name, String?[]? value, JsonSerializerOptions? options) =>
            WriteArray<String>(writer, name, value!, options, WriteStringValue);

        private static void WriteStringArrayValue(Utf8JsonWriter writer, String?[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue<String>(writer, value!, options, WriteStringValue);

        private static void WriteStringComparer(Utf8JsonWriter writer, String name, StringComparer value, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options)
        {
            JsonConverter<StringComparer> converter = null!;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            writer.WritePropertyName(name);
            converter.Write(writer, value, options!);
        }

        private static void WriteStringComparerValue(Utf8JsonWriter writer, StringComparer value, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options)
        {
            JsonConverter<StringComparer> converter = null!;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            converter.Write(writer, value, options!);
        }

        private static void ReadProperty(ref Dictionary<String, Object?> properties, ref Utf8JsonReader reader, IReadOnlyDictionary<String, String> propertyNames, StringComparer propertyNameComparer, JsonConverter<StringComparer> defaultComparerConverter, JsonSerializerOptions? options)
        {
            // Skip comments.
            SkipComments(ref reader);

            String propertyName;

            // Assert and read the property name.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            propertyName = reader.GetString() ?? throw new JsonException();
            reader.Read();

            // Check if the property has already been defined.
            if (properties.ContainsKey(propertyName))
            {
                throw new JsonException();
            }

            // Read the property value and add it to `properties`.
            if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Interned)]))
            {
                properties[nameof(Pen.Interned)] = ReadBoolean(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Comparer)]))
            {
                properties[nameof(Pen.Comparer)] = ReadStringComparer(ref reader, defaultComparerConverter, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Index)]))
            {
                properties[nameof(Pen.Index)] = ReadInt32Array(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Context)]))
            {
                properties[nameof(Pen.Context)] = ReadStringArray(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.SentinelToken)]))
            {
                properties[nameof(Pen.SentinelToken)] = ReadString(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.AllSentinels)]))
            {
                properties[nameof(Pen.AllSentinels)] = ReadBoolean(ref reader, options);
            }
            else
            {
                throw new JsonException();
            }
        }

        private static void WriteProperties(Utf8JsonWriter writer, Pen value, JsonConverter<StringComparer> defaultComparerConverter, JsonSerializerOptions? options)
        {
            Func<String, String> convertName = options?.PropertyNamingPolicy is null ? new Func<String, String>(DoNotConvertName!) : new Func<String, String>(options.PropertyNamingPolicy.ConvertName);

            WriteBoolean(writer, convertName(nameof(Pen.Interned)), value.Interned, options);
            WriteStringComparer(writer, convertName(nameof(Pen.Comparer)), value.Comparer, defaultComparerConverter, options);
            WriteStringArray(writer, convertName(nameof(Pen.Context)), value.Context?.AsBuffer(), options);
            WriteInt32Array(writer, convertName(nameof(Pen.Index)), value.Index?.AsBuffer(), options);
            WriteString(writer, convertName(nameof(Pen.SentinelToken)), value.SentinelToken, options);
            WriteBoolean(writer, convertName(nameof(Pen.AllSentinels)), value.AllSentinels, options);
        }

        private static void DeconstructProperties(IReadOnlyDictionary<String, Object?> properties, out Boolean interned, out StringComparer? comparer, out IReadOnlyList<Int32>? index, out IReadOnlyList<String?>? context, out String? sentinelToken, out Boolean allSentinels)
        {
            interned = false;
            comparer = null;
            index = null;
            context = null;
            sentinelToken = null;
            allSentinels = false;

            if (properties.TryGetValue(nameof(Pen.Interned), out Object? internedObject))
            {
                interned = (Boolean)internedObject!;
            }
            if (properties.TryGetValue(nameof(Pen.Comparer), out Object? comparerObject))
            {
                comparer = (StringComparer)comparerObject!;
            }
            if (properties.TryGetValue(nameof(Pen.Index), out Object? indexObject))
            {
                List<Int32> contextList = new List<Int32>((IEnumerable<Int32>)indexObject!);
                contextList.TrimExcess();

                index = contextList.AsReadOnly();
            }
            if (properties.TryGetValue(nameof(Pen.Context), out Object? contextObject))
            {
                List<String?> contextList;
                {
                    IEnumerable<String?> contextEnumerable = (IEnumerable<String?>)contextObject!;
                    contextList = new List<String?>(interned ? contextEnumerable.Select(StringExtensions.InternNullable) : contextEnumerable);
                }
                contextList.TrimExcess();

                context = contextList.AsReadOnly();
            }
            if (properties.TryGetValue(nameof(Pen.SentinelToken), out Object? sentinelTokenObject))
            {
                sentinelToken = (String)sentinelTokenObject!;
            }
            if (properties.TryGetValue(nameof(Pen.AllSentinels), out Object? allSentinelsObject))
            {
                allSentinels = (Boolean)allSentinelsObject!;
            }
        }

        private const string ComparerJsonConverterNullErrorMessage = "String somparer JSON converter cannot be null.";

        private readonly JsonConverter<StringComparer> _comparerJsonConverter;

        private JsonConverter<StringComparer> ComparerJsonConverter => _comparerJsonConverter;

        public PenJsonConverter(JsonConverter<StringComparer> comparerConverter) : base()
        {
            _comparerJsonConverter = comparerConverter ?? throw new ArgumentException(nameof(comparerConverter), ComparerJsonConverterNullErrorMessage);
        }

        public PenJsonConverter() : this(new StringComparerJsonConverter())
        {
        }

        public override Pen? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Func<String, String> convertName = options?.PropertyNamingPolicy is null ? new Func<String, String>(DoNotConvertName!) : new Func<String, String>(options.PropertyNamingPolicy.ConvertName);
            
            SkipComments(ref reader);

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            Boolean interned;
            StringComparer? comparer;
            IReadOnlyList<Int32>? index;
            IReadOnlyList<String?>? context;
            String? sentinelToken;
            Boolean allSentinels;

            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }
                reader.Read();

                StringComparer propertyNameComparer = (options is null || !options.PropertyNameCaseInsensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;

                Dictionary<String, String> propertyNames = new Dictionary<String, String>(6)
                {
                    {  nameof(Pen.Interned), convertName(nameof(Pen.Interned)) },
                    {  nameof(Pen.Comparer), convertName(nameof(Pen.Comparer)) },
                    {  nameof(Pen.Index), convertName(nameof(Pen.Index)) },
                    {  nameof(Pen.Context), convertName(nameof(Pen.Context)) },
                    {  nameof(Pen.SentinelToken), convertName(nameof(Pen.SentinelToken)) },
                    {  nameof(Pen.AllSentinels), convertName(nameof(Pen.AllSentinels)) }
                };

                Dictionary<String, Object?> properties = new Dictionary<String, Object?>(6, propertyNameComparer);

                while (true)
                {
                    SkipComments(ref reader);

                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    ReadProperty(ref properties, ref reader, propertyNames, propertyNameComparer, ComparerJsonConverter, options);
                }

                DeconstructProperties(properties, out interned, out comparer, out index, out context, out sentinelToken, out allSentinels);
            }

            return new Pen(interned, comparer!, index!, context!, sentinelToken, allSentinels);
        }

        public override void Write(Utf8JsonWriter writer, Pen value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            writer.WriteStartObject();

            WriteProperties(writer, value, ComparerJsonConverter, options);

            writer.WriteEndObject();
        }
    }
}
