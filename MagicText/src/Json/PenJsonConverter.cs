using MagicText.Internal.Extensions;
using MagicText.Internal.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Json
{
    /// <summary>Provides methods to convert <see cref="Pen" />s to <a href="http://json.org/json-en.html"><em>JSON</em></a> elements and <em>vice-versa</em>.</summary>
    /// <remarks>
    ///     <para>This is the default <see cref="JsonConverter{T}" /> for the <see cref="Pen" /> class. It is optimised to avoid time-expensive initialisation of <see cref="Pen" />s using the usual non-copy constructors (e. g. <see cref="Pen.Pen(IEnumerable{String}, StringComparer, String, Boolean)" />, <see cref="Pen.Pen(IEnumerable{String}, StringComparison, String, Boolean)" />) on deserialisation. Do not manually alter or create serialised <a href="http://json.org/json-en.html"><em>JSON</em></a> data and deserialise it back to a <see cref="Pen" /> instance. Any manipulation of the serialised <a href="http://json.org/json-en.html"><em>JSON</em></a> data may result in <see cref="Pen" />s with unexpected behaviour. Using such <see cref="Pen" />s may even cause uncaught exceptions not explicitly documented for the methods of the <see cref="Pen" /> class.</para>
    ///     <para>Automatically, a default <see cref="PenJsonConverter" /> (as initialised by the default <see cref="PenJsonConverter()" /> constructor) may only serialise and deserialise <see cref="Pen" />s whose <see cref="Pen.Comparer" /> property is one of the following <em>default</em> <see cref="StringComparer" />s: <see cref="StringComparer.InvariantCulture" />, <see cref="StringComparer.InvariantCultureIgnoreCase" />, <see cref="StringComparer.Ordinal" /> and <see cref="StringComparer.OrdinalIgnoreCase" />. Additionally, <see cref="StringComparer" />s with a custom <see cref="JsonConverter{T}" />, where the generic type parameter <c>T</c> is <strong>exactly</strong> the <see cref="StringComparer" />'s type, may be automatically handled if the <see cref="JsonConverter{T}" /> is either registered to the <see cref="StringComparer" />'s type (via the <see cref="JsonConverterAttribute" />) or passed to the serialisation/deserialisation method through <see cref="JsonSerializerOptions" /> (via the <see cref="JsonSerializerOptions.Converters" /> property). In all other cases please implement a <see cref="JsonConverter{T}" /> that accepts <strong>any <see cref="StringComparer" /></strong> (i. e. set the generic type parameter <c>T</c> to the abstract <see cref="StringComparer" /> class) and pass the <see cref="JsonConverter{T}" /> instance either to the <see cref="PenJsonConverter(JsonConverter{StringComparer})" /> constructor or to the serialisation/deserialisation method through <see cref="JsonSerializerOptions" />. Any potential <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s passed through <see cref="JsonSerializerOptions" /> overrides the one passed to the <see cref="PenJsonConverter(JsonConverter{StringComparer})" /> constructor.</para>
    ///     <para>When relying on the automatic <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s, keep the following in mind:</para>
    ///     <list type="number">
    ///         <item>Providing a custom <see cref="JsonConverter{T}" /> of <see cref="StringComparison" />s through <see cref="JsonSerializerOptions" /> which serialises a <see cref="StringComparison" /> value into a <a href="http://json.org/json-en.html"><em>JSON</em></a> object (enclosed in curly braces) may cause unexpected behaviour. As <see cref="StringComparison" /> is merely an enumeration type, the automatic <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s expects it to be serialised into a primitive value, e. g. a number (the integral numeric value of the enumeration constant) or a <see cref="String" /> (the name of the enumeration constant).</item>
    ///         <item>If the automatic <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s did not recognise the <see cref="StringComparer" /> as one of the <em>default</em> <see cref="StringComparer" />s (see above) and had to utilise a more specific <see cref="JsonConverter{T}" /> of the actual <see cref="StringComparer" />'s <see cref="Type" /> for the serialisation, the order of properties <strong>matters</strong>. In such cases the automatic <see cref="JsonConverter" /> creates two properties: one indicating the <see cref="StringComparer" />'s <see cref="Type" /> and the other representing the serialised <see cref="StringComparer" />'s value. The <see cref="Type" /> property <strong>must</strong> appear first when deserialising the <a href="http://json.org/json-en.html"><em>JSON</em></a>—otherwise the <see cref="JsonConverter" /> would not know how to read/interpret/deserialise the other property. Obviously, the <see cref="Type" /> is framework/language-specific even though there are other <a href="http://en.wikipedia.org/wiki/Object-oriented_programming">object-oriented programming languages</a>. Consequently, interpreting the resulting <a href="http://json.org/json-en.html"><em>JSON</em></a> in a different programming language/framework is meaningless.</item>
    ///     </list>
    ///     <para><strong>Nota bene.</strong> All private methods (static or instance members) are intended for <strong>internal use only</strong> and therefore do not make unnecessary checks of the parameters.</para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class PenJsonConverter : JsonConverter<Pen>
    {
        private const string ComparerJsonConverterNullErrorMessage = "String somparer JSON converter cannot be null.";

        /// <summary>Reads a <see cref="StringComparer" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="defaultConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s (if no specific is passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="StringComparer" />.</param>
        /// <returns>The <see cref="StringComparer" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="StringComparer" />.</para>
        /// </remarks>
        private static StringComparer? ReadStringComparer(ref Utf8JsonReader reader, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options, Boolean read = true)
        {
            StringComparer? value;

            JsonConversionHelper.SkipComments(ref reader);

            JsonConverter<StringComparer> converter;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            value = converter.Read(ref reader, typeof(StringComparer), options!);

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Writes a <see cref="StringComparer" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="defaultConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s (if no specific is passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        private static void WriteStringComparer(Utf8JsonWriter writer, String name, StringComparer value, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options)
        {
            JsonConverter<StringComparer> converter;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            writer.WritePropertyName(name);
            converter.Write(writer, value, options!);
        }

        /// <summary>Writes a <see cref="StringComparer" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="defaultConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s (if no specific is passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        private static void WriteStringComparerValue(Utf8JsonWriter writer, StringComparer value, JsonConverter<StringComparer> defaultConverter, JsonSerializerOptions? options)
        {
            JsonConverter<StringComparer> converter;
            if (options is null || !options.Converters.ContainsType(out converter!))
            {
                converter = defaultConverter;
            }

            converter.Write(writer, value, options!);
        }

        /// <summary>Reads a <see cref="Pen" />'s property.</summary>
        /// <param name="properties">The <see cref="IDictionary{TKey, TValue}" /> of read properties.</param>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="propertyNames">The <see cref="IReadOnlyDictionary{TKey, TValue}" /> of appropriately converted property names (in respect of the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="propertyNameComparer">The <see cref="StringComparer" /> to use for appropriately comparing property names (in respect of the <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="defaultComparerConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s (if no specific is passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <exception cref="JsonException">An unexpected <a href="http://json.org/json-en.html"><em>JSON</em></a> token is encountered (by type or value).</exception>
        /// <remarks>
        ///     <para>The read property's value is inserted to the <c><paramref name="properties" /></c> and not returned.</para>
        /// </remarks>
        private static void ReadProperty(IDictionary<String, Object?> properties, ref Utf8JsonReader reader, IReadOnlyDictionary<String, String> propertyNames, StringComparer propertyNameComparer, JsonConverter<StringComparer> defaultComparerConverter, JsonSerializerOptions? options)
        {
            String propertyName;

            // Assert and read the property name.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.PropertyName, reader.TokenType));
            }
            propertyName = reader.GetString() ?? throw new JsonException(JsonConversionHelper.PropertyNameReadingFailErrorMessage);
            reader.Read();

            // Check if the property has already been defined.
            if (properties.ContainsKey(propertyName))
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.DuplicatePropertyDefinitionErrorMessage, propertyName));
            }

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Read the property value and add it to `properties`.
            if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Interned)]))
            {
                properties[nameof(Pen.Interned)] = JsonConversionHelper.ReadBoolean(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Comparer)]))
            {
                properties[nameof(Pen.Comparer)] = ReadStringComparer(ref reader, defaultComparerConverter, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Index)]))
            {
                properties[nameof(Pen.Index)] = JsonConversionHelper.ReadInt32Array(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.Context)]))
            {
                properties[nameof(Pen.Context)] = JsonConversionHelper.ReadStringArray(ref reader, options);
            }
            else if (propertyNameComparer.Equals(propertyName, propertyNames[nameof(Pen.SentinelToken)]))
            {
                properties[nameof(Pen.SentinelToken)] = JsonConversionHelper.ReadString(ref reader, options);
            }
            else
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.InvalidPropertyErrorMessage, propertyName));
            }
        }

        /// <summary>Writes all of the <see cref="Pen" /> <c><paramref name="value" /></c>'s properties.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value whose properties to write.</param>
        /// <param name="defaultComparerConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s (if no specific is passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <c><paramref name="options" /></c>).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        private static void WriteProperties(Utf8JsonWriter writer, Pen value, JsonConverter<StringComparer> defaultComparerConverter, JsonSerializerOptions? options)
        {
            Func<String, String> convertName = JsonConversionHelper.GetPropertyNameConversionPolicy(options);

            JsonConversionHelper.WriteBoolean(writer, convertName(nameof(Pen.Interned)), value.Interned, options);
            WriteStringComparer(writer, convertName(nameof(Pen.Comparer)), value.Comparer, defaultComparerConverter, options);
            JsonConversionHelper.WriteStringArray(writer, convertName(nameof(Pen.Context)), value.Context?.AsBuffer()!, options);
            JsonConversionHelper.WriteInt32Array(writer, convertName(nameof(Pen.Index)), value.Index?.AsBuffer(), options);
            JsonConversionHelper.WriteString(writer, convertName(nameof(Pen.SentinelToken)), value.SentinelToken, options);
        }

        /// <summary>Deconstructs a <see cref="Pen" />'s property <see cref="IReadOnlyDictionary{TKey, TValue}" />.</summary>
        /// <param name="properties">The <see cref="IReadOnlyDictionary{TKey, TValue}" /> of properties.</param>
        /// <param name="interned">The policy of interning all non-<c>null</c> tokens from the <see cref="Pen.Context" />.</param>
        /// <param name="comparer">The <see cref="StringComparer" /> used for comparing tokens.</param>
        /// <param name="index">The index of entries in the <see cref="Pen.Context" /> sorted ascendingly.</param>
        /// <param name="context">The reference token context.</param>
        /// <param name="sentinelToken">The ending token.</param>
        private static void DeconstructProperties(IReadOnlyDictionary<String, Object?> properties, out Boolean interned, out StringComparer? comparer, out IReadOnlyList<Int32>? index, out IReadOnlyList<String?>? context, out String? sentinelToken)
        {
            interned = properties.TryGetValue(nameof(Pen.Interned), out Object? internedObject) && (Boolean)internedObject!;
            comparer = properties.TryGetValue(nameof(Pen.Comparer), out Object? comparerObject) ? (StringComparer)comparerObject! : null;
            if (properties.TryGetValue(nameof(Pen.Index), out Object? indexObject))
            {
                List<Int32> indexList = new List<Int32>((IEnumerable<Int32>)indexObject!);
                indexList.TrimExcess();

                index = indexList.AsReadOnly();
            }
            else
            {
                index = null;
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
            else
            {
                context = null;
            }
            sentinelToken = properties.TryGetValue(nameof(Pen.SentinelToken), out Object? sentinelTokenObject) ? (String)sentinelTokenObject! : null;
        }

        private readonly JsonConverter<StringComparer> _comparerJsonConverter;

        /// <summary>Gets the default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s.</summary>
        /// <returns>The internal default <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s.</returns>
        /// <remarks>
        ///     <para>Any <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s passed through the <see cref="JsonSerializerOptions.Converters" /> property of the <see cref="JsonSerializerOptions" /> passed to a conversion method (<see cref="Read(ref Utf8JsonReader, Type, JsonSerializerOptions)" /> or <see cref="Write(Utf8JsonWriter, Pen, JsonSerializerOptions)" />) overrides this <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s.</para>
        /// </remarks>
        private JsonConverter<StringComparer> ComparerJsonConverter => _comparerJsonConverter;

        /// <summary>Creates a <see cref="PenJsonConverter" />.</summary>
        /// <param name="comparerConverter">The default <see cref="JsonConverter{T}" /> for converting <see cref="StringComparer" />s to use.</param>
        /// <exception cref="ArgumentException">The <c><paramref name="comparerConverter" /></c> parameter is <c>null</c>.</exception>
        public PenJsonConverter(JsonConverter<StringComparer> comparerConverter) : base()
        {
            _comparerJsonConverter = comparerConverter ?? throw new ArgumentException(nameof(comparerConverter), ComparerJsonConverterNullErrorMessage);
        }

        /// <summary>Creates a default <see cref="PenJsonConverter" />.</summary>
        public PenJsonConverter() : this(StringComparerJsonConverter.Instance)
        {
        }

        /// <summary>Reads and converts the <a href="http://json.org/json-en.html"><em>JSON</em></a> to a <see cref="Pen" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="typeToConvert">The type to convert (<see cref="Pen" />).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The converted <see cref="Pen" /> value.</returns>
        /// <exception cref="JsonException">An unexpected <a href="http://json.org/json-en.html"><em>JSON</em></a> token is encountered (by type or value).</exception>
        public override Pen Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Declare `Pen`'s properties.
            Boolean interned;
            StringComparer? comparer;
            IReadOnlyList<Int32>? index;
            IReadOnlyList<String?>? context;
            String? sentinelToken;

            // Return `null` in case of a `null`.
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null!;
            }

            // Read `Pen`'s properties.
            {
                // Assert and read the beginning of an object.
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.StartObject, reader.TokenType));
                }
                reader.Read();

                // Get the appropriate property name comparer and conversion policy.
                StringComparer propertyNameComparer = JsonConversionHelper.GetPropertyNameComparer(options);
                Func<String, String> convertName = JsonConversionHelper.GetPropertyNameConversionPolicy(options);

                // Initialise a dictionary mapping raw property names to the converted.
                Dictionary<String, String> propertyNames = new Dictionary<String, String>(5)
                {
                    {  nameof(Pen.Interned), convertName(nameof(Pen.Interned)) },
                    {  nameof(Pen.Comparer), convertName(nameof(Pen.Comparer)) },
                    {  nameof(Pen.Index), convertName(nameof(Pen.Index)) },
                    {  nameof(Pen.Context), convertName(nameof(Pen.Context)) },
                    {  nameof(Pen.SentinelToken), convertName(nameof(Pen.SentinelToken)) }
                };

                // Initialise an empty dictionary of property values.
                Dictionary<String, Object?> properties = new Dictionary<String, Object?>(5, propertyNameComparer);

                // Read properties.
                while (true)
                {
                    // Skip the comments.
                    JsonConversionHelper.SkipComments(ref reader);

                    // If the end of the object is reached, stop.
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    // Read the current property.
                    ReadProperty(properties, ref reader, propertyNames, propertyNameComparer, ComparerJsonConverter, options);
                }

                // Deconstruct properties from the dictionary.
                DeconstructProperties(properties, out interned, out comparer, out index, out context, out sentinelToken);
            }

            // Create and return the read `Pen`.
            return new Pen(interned, comparer!, index!, context!, sentinelToken);
        }

        /// <summary>Writes the specified <see cref="Pen" /> <c><paramref name="value" /></c> as <a href="http://json.org/json-en.html"><em>JSON</em></a>.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The <see cref="Pen" /> value to convert to <a href="http://json.org/json-en.html"><em>JSON</em></a>.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
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
