using MagicText.Internal.Extensions;
using System;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Internal.Json
{
    /// <summary>Provides methods to convert <see cref="StringComparer" />s to <a href="http://json.org/json-en.html"><em>JSON</em></a> elements and <em>vice-versa</em>.</summary>
    /// <remarks>
    ///     <para>This is a singleton class. <see cref="StringComparerJsonConverter" />s cannot be initialised, use the singleton <see cref="Instance" /> static property instead.</para>
    ///     <para><see cref="StringComparerJsonConverter" />s may only serialise and deserialise the following <em>default</em> <see cref="StringComparer" />s: <see cref="StringComparer.Ordinal" />, <see cref="StringComparer.OrdinalIgnoreCase" />, <see cref="StringComparer.InvariantCulture" /> and <see cref="StringComparer.InvariantCultureIgnoreCase" />. Additionally, <see cref="StringComparer" />s with a custom <see cref="JsonConverter{T}" />, where the generic type parameter <c>T</c> is <strong>exactly</strong> the <see cref="StringComparer" />'s type, may be automatically handled if the <see cref="JsonConverter{T}" /> is either registered to the <see cref="StringComparer" />'s type (via the <see cref="JsonConverterAttribute" />) or passed to the serialisation/deserialisation method through <see cref="JsonSerializerOptions" /> (via the <see cref="JsonSerializerOptions.Converters" /> property). In all other cases please use a different (custom) <see cref="JsonConverter{T}" /> of <see cref="StringComparer" />s.</para>
    ///     <para>When using <see cref="StringComparerJsonConverter" />, keep in mind the following:</para>
    ///     <list type="number">
    ///         <item>Providing a custom <see cref="JsonConverter{T}" /> of <see cref="StringComparison" />s through <see cref="JsonSerializerOptions" /> which serialises a <see cref="StringComparison" /> value into a <a href="http://json.org/json-en.html"><em>JSON</em></a> object (enclosed in curly braces) may cause unexpected behaviour. As <see cref="StringComparison" /> is merely an enumeration type, the <see cref="StringComparerJsonConverter" /> expects it to be serialised into a primitive value, e. g. a number (the integral numeric value of the enumeration constant) or a <see cref="String" /> (the name of the enumeration constant).</item>
    ///         <item>If the <see cref="StringComparerJsonConverter" /> did not recognise the <see cref="StringComparer" /> as one of the <em>default</em> <see cref="StringComparer" />s (see above) and had to utilise a more specific <see cref="JsonConverter{T}" /> of the actual <see cref="StringComparer" />'s <see cref="Type" /> for the serialisation, the order of properties <strong>is relevant</strong>. In such cases the <see cref="StringComparerJsonConverter" /> creates two properties: one indicating the <see cref="StringComparer" />'s <see cref="Type" /> and the other representing the serialised <see cref="StringComparer" />'s value. The <see cref="Type" /> property <strong>must</strong> appear first when deserialising the <a href="http://json.org/json-en.html"><em>JSON</em></a>—otherwise the <see cref="StringComparerJsonConverter" /> would not know how to read/interpret/deserialise the other property. Obviously, the <see cref="Type" /> is framework/language-specific even though there are other <a href="http://en.wikipedia.org/wiki/Object-oriented_programming">object-oriented programming languages</a>. Consequently, interpreting the resulting <a href="http://json.org/json-en.html"><em>JSON</em></a> in a different programming language/framework is meaningless.</item>
    ///     </list>
    ///     <para><strong>Nota bene.</strong> All private methods (static or instance members) are intended for <strong>internal use only</strong> and therefore do not make unnecessary checks of the parameters.</para>
    /// </remarks>
    internal sealed class StringComparerJsonConverter : JsonConverter<StringComparer>
    {
        private const string ComparerJsonConverterNullErrorMessage = "String somparer JSON converter cannot be null.";
        private const string MissingComparerJsonConverterFormatErrorMessage = "No JSON converter is registered for string comparers of type {0}.";
        private const string InvalidComparerJsonConverterTypeFormatErrorMessage = "Cannot derive a standard JSON converter of type {1} from JSON converter type {0}.";

        /// <summary>Provides property names for custom <a href="http://json.org/json-en.html"><em>JSON</em></a> serialisation of <see cref="StringComparer" />s.</summary>
        private static class CustomStringComparerEncapsulationPropertyNames
        {
            /// <summary>Name of the property representing the <see cref="Type" /> of the <see cref="StringComparer" />.</summary>
            public const string TypeName = "Type";
            
            /// <summary>Name of the property representing the actual serialised value of the <see cref="StringComparer" />.</summary>
            public const string ValueName = "Value";
        }

        private static readonly StringComparerJsonConverter _instance;

        /// <summary>Gets the singleton instance of the <see cref="StringComparerJsonConverter" />.</summary>
        /// <returns>The singleton instace of the <see cref="StringComparerJsonConverter" />.</returns>
        public static StringComparerJsonConverter Instance => _instance;

        /// <summary>Initiallises static fields.</summary>
        static StringComparerJsonConverter()
        {
            _instance = new StringComparerJsonConverter();
        }

        /// <summary>Reads a <see cref="StringComparison" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="StringComparison" /> value read from the <c><paramref name="reader" /></c>.</returns>
        /// <remarks>
        ///     <para>The method reads the token from the <c><paramref name="reader" /></c> but does not move its position to the next token.</para>
        /// </remarks>
        private static StringComparison ReadStringComparison(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            StringComparison value;

            if (options?.GetConverter(typeof(StringComparison)) is JsonConverter<StringComparison> converter)
            {
                value = converter.Read(ref reader, typeof(StringComparison), options);
            }
            else
            {
                value = (StringComparison)reader.GetInt32();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a customly serialised (encapsulated) <see cref="StringComparer" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="StringComparer" /> encapsulation.</param>
        /// <returns>The <see cref="StringComparer" /> read from the <c><paramref name="reader" /></c>.</returns>
        /// <exception cref="JsonException">An unexpected <a href="http://json.org/json-en.html"><em>JSON</em></a> token is encountered (by type or value).</exception>
        /// <exception cref="InvalidOperationException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>a <see cref="JsonConverter{T}" /> of the <see cref="StringComparer" />'s <see cref="Type" /> was not provided or</item>
        ///         <item>failed to utilise the provided <see cref="JsonConverter" />.</item>
        ///     </list>
        /// </exception>
        private static StringComparer? ReadCustomStringComparer(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            // Return `null` in case of a `null`.
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (read)
                {
                    reader.Read();
                }

                return null;
            }

            // Assert and read the beginning of an object.
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.StartObject, reader.TokenType));
            }
            reader.Read();

            // Get the appropriate property name comparer and conversion policy.
            StringComparer propertyNameComparer = JsonConversionHelper.GetPropertyNameComparer(options);
            Func<String, String> convertName = JsonConversionHelper.GetPropertyNameConversionPolicy(options);

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Assert and read the property name. It must be equal to `CustomStringComparerEncapsulationPropertyNames.TypeName`.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.PropertyName, reader.TokenType));
            }
            if (!propertyNameComparer.Equals(reader.GetString() ?? throw new JsonException(JsonConversionHelper.PropertyNameReadingFailErrorMessage), convertName(CustomStringComparerEncapsulationPropertyNames.TypeName)))
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedPropertyNameErrorMessage, CustomStringComparerEncapsulationPropertyNames.TypeName, reader.GetString()));
            }
            reader.Read();

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Get the type of the `StringComparer`.
            Type valueType = TypeJsonConverter.Instance.Read(ref reader, typeof(Type), options!)!;

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Assert and read the property name. It must be equal to `CustomStringComparerEncapsulationPropertyNames.ValueName`.
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.PropertyName, reader.TokenType));
            }
            if (!propertyNameComparer.Equals(reader.GetString() ?? throw new JsonException(), convertName(CustomStringComparerEncapsulationPropertyNames.ValueName)))
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedPropertyNameErrorMessage, CustomStringComparerEncapsulationPropertyNames.ValueName, reader.GetString()));
            }
            reader.Read();

            // Get the `JsonConverter` of the `valueType` from the `options`.
            JsonConverter jsonConverter = options?.GetConverter(valueType) ?? throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MissingComparerJsonConverterFormatErrorMessage, valueType.FullName));
            Type converterType = typeof(JsonConverter<>).MakeGenericType(valueType);
            if (!converterType.IsAssignableFrom(jsonConverter.GetType()))
            {
                throw new InvalidOperationException(String.Format(InvalidComparerJsonConverterTypeFormatErrorMessage, jsonConverter.GetType().FullName, converterType.FullName));
            }

            // Get the JSON conversion method `Read` from the `jsonConverter`.
            JsonConversionHelper.JsonConverterRead conversionMethod = (JsonConversionHelper.JsonConverterRead)Delegate.CreateDelegate(typeof(JsonConversionHelper.JsonConverterRead), jsonConverter, converterType.GetMethod(nameof(JsonConverter<Object>.Read), new Type[] { typeof(Utf8JsonReader).MakeByRefType(), typeof(Type), typeof(JsonSerializerOptions) }));

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Read the `StringComparer` value.
            StringComparer? value = (StringComparer)conversionMethod.Invoke(ref reader, valueType, options)!;
            reader.Read();

            // Skip comments.
            JsonConversionHelper.SkipComments(ref reader);

            // Assert the end of the object.
            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, JsonConversionHelper.UnexpectedTokenFormatErrorMessage, JsonTokenType.EndObject, reader.TokenType));
            }

            if (read)
            {
                reader.Read();
            }

            // Return the read `StringComparer`.
            return value;
        }

        /// <summary>Writes a <see cref="StringComparison" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        private static void WriteStringComparisonValue(Utf8JsonWriter writer, StringComparison value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(StringComparison)) is JsonConverter<StringComparison> converter)
            {
                converter.Write(writer, value, options);
            }
            else
            {
                writer.WriteNumberValue((Int32)value);
            }
        }

        /// <summary>Writes a customly serialised (encapsulated) <see cref="StringComparer" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <exception cref="InvalidOperationException">
        ///     <para>Either:</para>
        ///     <list type="number">
        ///         <item>a <see cref="JsonConverter{T}" /> of the <see cref="StringComparer" />'s <see cref="Type" /> was not provided or</item>
        ///         <item>failed to utilise the provided <see cref="JsonConverter" />.</item>
        ///     </list>
        /// </exception>
        private static void WriteCustomStringComparerValue(Utf8JsonWriter writer, StringComparer value, JsonSerializerOptions? options)
        {
            // Get the appropriate property name conversion policy.
            Func<String, String> convertName = JsonConversionHelper.GetPropertyNameConversionPolicy(options);

            // Get the `Type` of the `StringComparer`.
            Type valueType = value.GetType();

            // Get the `JsonConverter` of the `valueType` from the `options`.
            JsonConverter jsonConverter = options?.GetConverter(valueType) ?? throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, MissingComparerJsonConverterFormatErrorMessage, valueType.FullName));
            Type converterType = typeof(JsonConverter<>).MakeGenericType(valueType);
            if (!converterType.IsAssignableFrom(jsonConverter.GetType()))
            {
                throw new InvalidOperationException(String.Format(InvalidComparerJsonConverterTypeFormatErrorMessage, jsonConverter.GetType().FullName, converterType.FullName));
            }

            // Get the JSON conversion method `Write` from the `jsonConverter`.
            MethodInfo conversionMethod = converterType.GetMethod(nameof(JsonConverter<Object>.Write), new Type[] { typeof(Utf8JsonWriter), valueType, typeof(JsonSerializerOptions) });
            
            // Write the beginning of the object.
            writer.WriteStartObject();

            // Write the `valueType`.
            writer.WritePropertyName(CustomStringComparerEncapsulationPropertyNames.TypeName);
            TypeJsonConverter.Instance.Write(writer, valueType, options!);

            // Write the `StringComparer`.
            writer.WritePropertyName(convertName(CustomStringComparerEncapsulationPropertyNames.ValueName));
            conversionMethod.Invoke(jsonConverter, new Object[] { writer, value, options });

            // Write the end of the object.
            writer.WriteEndObject();
        }

        /// <summary>Creates a default <see cref="StringComparerJsonConverter" />.</summary>
        private StringComparerJsonConverter() : base()
        {
        }

        /// <summary>Reads and converts the <a href="http://json.org/json-en.html"><em>JSON</em></a> to a <see cref="StringComparer" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="typeToConvert">The type to convert (<see cref="StringComparer" />).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The converted <see cref="StringComparer" /> value.</returns>
        /// <exception cref="JsonException">An unexpected <a href="http://json.org/json-en.html"><em>JSON</em></a> token is encountered (by type or value).</exception>
        public override StringComparer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Return `null` if a `null` JSON token is found.
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // If a JSON object is found, read it as a custom string comparer.
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return ReadCustomStringComparer(ref reader, options, read: false);
            }

            // Read the comparison type.
            StringComparison comparisonType = ReadStringComparison(ref reader, options, read: false);

            // Return a string comparer corresponding to the comparison type.
#if NETSTANDARD2_0
            return StringComparerExtensions.GetComparerFromComparison(comparisonType);
#else
            return StringComparer.FromComparison(comparisonType);
#endif // NETSTANDARD2_0
        }

        /// <summary>Writes the specified <see cref="StringComparer" /> <c><paramref name="value" /></c> as <a href="http://json.org/json-en.html"><em>JSON</em></a>.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The <see cref="StringComparer" /> value to convert to <a href="http://json.org/json-en.html"><em>JSON</em></a>.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public override void Write(Utf8JsonWriter writer, StringComparer value, JsonSerializerOptions options)
        {
            // Write `null` if the comparer is `null`.
            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            // Detect the comparer's underlying comparison type and proceed accordingly.
            if (value.TryGetComparison(out StringComparison comparisonType))
            {
                // Write the comparison type.
                WriteStringComparisonValue(writer, comparisonType, options);
            }
            else
            {
                // Write the custom string comparer using the special method.
                WriteCustomStringComparerValue(writer, value, options);
            }
        }
    }
}
