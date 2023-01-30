using MagicText.Internal.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Internal.Json
{
    /// <summary>Provides auxiliary delegates and methods for reading/writing <a href="http://json.org/json-en.html"><em>JSON</em></a> objects.</summary>
    /// <remarks>
    ///     <para><strong>Nota bene.</strong> All methods (private or public) are intended for <strong>internal use only</strong>, and therefore do not make unnecessary checks of the parameters.</para>
    /// </remarks>
    internal static class JsonConversionHelper
    {
        public const string PropertyNameReadingFailErrorMessage = "Failed to read property name as a string.";
        public const string UnexpectedPropertyNameErrorMessage = "Expected to read property `{0}´, got `{1}´ instead.";
        public const string DuplicatePropertyDefinitionErrorMessage = "Property `{0}´ is defined multiple times.";
        public const string InvalidPropertyErrorMessage = "Property `{0}´ is not valid (unrecognised).";

        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        public const string UnexpectedTokenFormatErrorMessage = "Expected to read token {0}, got {1} instead.";

        /// <summary>Represents the <see cref="JsonConverter{T}.Read(ref Utf8JsonReader, Type, JsonSerializerOptions)" /> method.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The converted value.</returns>
        public delegate Object? JsonConverterRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        /// <summary>Represents the <see cref="JsonConverter{T}.Write(Utf8JsonWriter, T, JsonSerializerOptions)" /> method.</summary>
        /// <typeparam name="T">The type of objects or values handled by the <see cref="JsonConverter{T}" />.</typeparam>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to convert to <a href="http://json.org/json-en.html"><em>JSON</em></a>.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public delegate void JsonConverterWrite<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options);

        /// <summary>Represents a function for simple reading of a value from <a href="http://json.org/json-en.html"><em>JSON</em></a>.</summary>
        /// <typeparam name="T">The type of the value to read.</typeparam>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the function actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The value of type <c><typeparamref name="T" /></c> read from the <c><paramref name="reader" /></c>.</returns>
        public delegate T ReadValue<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true);

        /// <summary>Represents a function for simple writing of a value to <a href="http://json.org/json-en.html"><em>JSON</em></a>.</summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public delegate void WriteValue<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions? options);

        /// <summary>Returns the unaltered <a href="http://json.org/json-en.html"><em>JSON</em></a> property <c><paramref name="name" /></c>.</summary>
        /// <param name="name"><a href="http://json.org/json-en.html"><em>JSON</em></a> property name (not) to convert.</param>
        /// <returns>The original value of <c><paramref name="name" /></c> (the same exact reference). If <c><paramref name="name" /></c> is <c>null</c>, <c>null</c> is returned.</returns>
        [return: MaybeNull, NotNullIfNotNull("name")]
        private static String? DoNotConvertName([AllowNull] String? name) =>
            name;

        /// <summary>Returns the appropriate <see cref="StringComparer" /> of <a href="http://json.org/json-en.html"><em>JSON</em></a> property names.</summary>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The <see cref="StringComparer" /> that appropriately compares <a href="http://json.org/json-en.html"><em>JSON</em></a> property names as set by the <see cref="JsonSerializerOptions.PropertyNameCaseInsensitive" /> property of the <c><paramref name="options" /></c>.</returns>
        public static StringComparer GetPropertyNameComparer(JsonSerializerOptions? options) =>
            (options is null || !options.PropertyNameCaseInsensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;

        /// <summary>Returns the appropriate <a href="http://json.org/json-en.html"><em>JSON</em></a> property naming policy conversion.</summary>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The function that converts a raw <a href="http://json.org/json-en.html"><em>JSON</em></a> property name to the appropriate format as set by the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property of the <c><paramref name="options" /></c>.</returns>
        public static Func<String, String> GetPropertyNameConversionPolicy(JsonSerializerOptions? options) =>
            options?.PropertyNamingPolicy is null ? new Func<String, String>(DoNotConvertName!) : new Func<String, String>(options.PropertyNamingPolicy.ConvertName);

        /// <summary>Reads through <a href="http://json.org/json-en.html"><em>JSON</em></a> comments.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <remarks>
        ///     <para>The method reads through all consecutive comment tokens until a non-comment token is reached and sets the <c><paramref name="reader" /></c>'s position to the next non-comment token. If no comment is found, the <c><paramref name="reader" /></c>'s position remains unchanged.</para>
        /// </remarks>
        public static void SkipComments(ref Utf8JsonReader reader)
        {
            while (reader.TokenType == JsonTokenType.Comment)
            {
                reader.Read();
            }
        }

        /// <summary>Reads a <see cref="Boolean" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Boolean" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Boolean ReadBoolean(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
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

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="Byte" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Byte" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Byte ReadByte(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Byte value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Byte)) is JsonConverter<Byte> converter)
            {
                value = converter.Read(ref reader, typeof(Byte), options);
            }
            else
            {
                value = reader.GetByte();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads an <see cref="Int16" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Int16" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int16 ReadInt16(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Int16 value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Int16)) is JsonConverter<Int16> converter)
            {
                value = converter.Read(ref reader, typeof(Int16), options);
            }
            else
            {
                value = reader.GetInt16();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads an <see cref="Int32" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Int32" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int32 ReadInt32(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
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

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads an <see cref="Int64" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Int64" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int64 ReadInt64(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Int64 value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Int64)) is JsonConverter<Int64> converter)
            {
                value = converter.Read(ref reader, typeof(Int64), options);
            }
            else
            {
                value = reader.GetInt64();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="Single" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Single" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Single ReadSingle(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Single value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Single)) is JsonConverter<Single> converter)
            {
                value = converter.Read(ref reader, typeof(Single), options);
            }
            else
            {
                value = reader.GetSingle();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="Double" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Double" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Double ReadDouble(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Double value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Double)) is JsonConverter<Double> converter)
            {
                value = converter.Read(ref reader, typeof(Double), options);
            }
            else
            {
                value = reader.GetDouble();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="Decimal" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Decimal" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Decimal ReadDecimal(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Decimal value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Decimal)) is JsonConverter<Decimal> converter)
            {
                value = converter.Read(ref reader, typeof(Decimal), options);
            }
            else
            {
                value = reader.GetDecimal();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="Char" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="Char" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static Char ReadChar(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
        {
            Char value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(Char)) is JsonConverter<Char> converter)
            {
                value = converter.Read(ref reader, typeof(Char), options);
            }
            else
            {
                value = reader.GetString()!.Single();
            }

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads a <see cref="String" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the token from the <c><paramref name="reader" /></c> and sets its position to the next token.</param>
        /// <returns>The <see cref="String" /> value read from the <c><paramref name="reader" /></c>.</returns>
        public static String? ReadString(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true)
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

            if (read)
            {
                reader.Read();
            }

            return value;
        }

        /// <summary>Reads an <see cref="Array" /> of item type <c><typeparamref name="T" /></c> value.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="readValue">The function to use to read a value of type <c><typeparamref name="T" /></c>.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of item type <c><typeparamref name="T" /></c> value read from the <c><paramref name="reader" /></c>.</returns>
        public static T[]? ReadArray<T>(ref Utf8JsonReader reader, JsonSerializerOptions? options, ReadValue<T> readValue, Boolean read = true)
        {
            // Declare the array.
            T[] array;

            // Skip comments.
            SkipComments(ref reader);

            // Convert using a converter if provided.
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                array = converter.Read(ref reader, typeof(T[]), options)!;

                // Read until the next token if needed.
                if (read)
                {
                    reader.Read();
                }

                return array;
            }

            // Return `null` in case of a `null`.
            if (reader.TokenType == JsonTokenType.Null)
            {
                // Read until the next token if needed.
                if (read)
                {
                    reader.Read();
                }

                return null;
            }

            // Assert and read the beginning of an array.
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException(String.Format(CultureInfo.CurrentCulture, UnexpectedTokenFormatErrorMessage, JsonTokenType.StartArray, reader.TokenType));
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
                    // Read until the next token if needed.
                    if (read)
                    {
                        reader.Read();
                    }

                    break;
                }

                // Expand the array if needed.
                if (size == array.Length)
                {
                    ArrayLengthManipulation.Expand(ref array);
                }

                // Read the current item.
                array[size++] = readValue(ref reader, options, true);
            }

            // Trim the array.
            ArrayLengthManipulation.TrimExcess(ref array, size);

            // Return the array.
            return array;
        }

        /// <summary>Reads an <see cref="Array" /> of <see cref="Boolean" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Boolean" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Boolean[]? ReadBooleanArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadBoolean, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Byte" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Byte" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Byte[]? ReadByteArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadByte, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Int16" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Int16" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int16[]? ReadInt16Array(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadInt16, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Int32" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Int32" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int32[]? ReadInt32Array(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadInt32, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Int64" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Int64" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Int64[]? ReadInt64Array(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadInt64, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Single" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Single" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Single[]? ReadSingleArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadSingle, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Double" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Double" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Double[]? ReadDoubleArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadDouble, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Decimal" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Decimal" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Decimal[]? ReadDecimalArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadDecimal, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="Char" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="Char" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static Char[]? ReadCharArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray(ref reader, options, ReadChar, read);

        /// <summary>Reads an <see cref="Array" /> of <see cref="String" />s value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="read">If <c>true</c>, the method actually reads the tokens from the <c><paramref name="reader" /></c> and sets its position to the next token after the <see cref="Array" />.</param>
        /// <returns>The <see cref="Array" /> of <see cref="String" />s value read from the <c><paramref name="reader" /></c>.</returns>
        public static String[]? ReadStringArray(ref Utf8JsonReader reader, JsonSerializerOptions? options, Boolean read = true) =>
            ReadArray<String>(ref reader, options, ReadString!, read)!;

        /// <summary>Writes a <see cref="Boolean" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteBoolean(Utf8JsonWriter writer, String name, Boolean value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Boolean)) is JsonConverter<Boolean> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteBoolean(name, value);
            }
        }

        /// <summary>Writes a <see cref="Boolean" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteBooleanValue(Utf8JsonWriter writer, Boolean value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Boolean)) is JsonConverter<Boolean> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteBooleanValue(value);
            }
        }

        /// <summary>Writes a <see cref="Byte" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteByte(Utf8JsonWriter writer, String name, Byte value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Byte)) is JsonConverter<Byte> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes a <see cref="Byte" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteByteValue(Utf8JsonWriter writer, Byte value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Byte)) is JsonConverter<Byte> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes an <see cref="Int16" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt16(Utf8JsonWriter writer, String name, Int16 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int16)) is JsonConverter<Int16> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes an <see cref="Int16" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt16Value(Utf8JsonWriter writer, Int16 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int16)) is JsonConverter<Int16> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes an <see cref="Int32" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt32(Utf8JsonWriter writer, String name, Int32 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int32)) is JsonConverter<Int32> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes an <see cref="Int32" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt32Value(Utf8JsonWriter writer, Int32 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int32)) is JsonConverter<Int32> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes an <see cref="Int64" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt64(Utf8JsonWriter writer, String name, Int64 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int64)) is JsonConverter<Int64> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes an <see cref="Int64" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt64Value(Utf8JsonWriter writer, Int64 value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Int64)) is JsonConverter<Int64> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes a <see cref="Single" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteSingle(Utf8JsonWriter writer, String name, Single value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Single)) is JsonConverter<Single> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes a <see cref="Single" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteSingleValue(Utf8JsonWriter writer, Single value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Single)) is JsonConverter<Single> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes a <see cref="Double" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteDouble(Utf8JsonWriter writer, String name, Double value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Double)) is JsonConverter<Double> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes a <see cref="Double" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteDoubleValue(Utf8JsonWriter writer, Double value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Double)) is JsonConverter<Double> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes a <see cref="Decimal" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteDecimal(Utf8JsonWriter writer, String name, Decimal value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Decimal)) is JsonConverter<Decimal> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumber(name, value);
            }
        }

        /// <summary>Writes a <see cref="Decimal" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteDecimalValue(Utf8JsonWriter writer, Decimal value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Decimal)) is JsonConverter<Decimal> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }

        /// <summary>Writes a <see cref="Char" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteChar(Utf8JsonWriter writer, String name, Char value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Char)) is JsonConverter<Char> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteString(name, value.ToString());
            }
        }

        /// <summary>Writes a <see cref="Char" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteCharValue(Utf8JsonWriter writer, Char value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(Char)) is JsonConverter<Char> converter)
            {
                converter.Write(writer, value, options!);
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        /// <summary>Writes a <see cref="String" /> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteString(Utf8JsonWriter writer, String name, String? value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(String)) is JsonConverter<String> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value!, options!);
            }
            else
            {
                writer.WriteString(name, value);
            }
        }

        /// <summary>Writes a <see cref="String" /> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteStringValue(Utf8JsonWriter writer, String? value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(String)) is JsonConverter<String> converter)
            {
                converter.Write(writer, value!, options!);
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }

        /// <summary>Writes an <see cref="Array" /> of item type <c><typeparamref name="T" /></c> property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="writeValue">The function to use to write a value of type <c><typeparamref name="T" /></c>.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteArray<T>(Utf8JsonWriter writer, String name, T[]? value, JsonSerializerOptions? options, WriteValue<T> writeValue)
        {
            // Convert using a converter if provided.
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value!, options!);

                return;
            }

            // Write `null` in case of a `null`.
            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            // Write the name and the beginning of the array.
            writer.WriteStartArray(name);

            // Write the array.
            foreach (T item in value)
            {
                writeValue(writer, item, options!);
            }

            // Write the end of the array.
            writer.WriteEndArray();
        }

        /// <summary>Writes an <see cref="Array" /> of item type <c><typeparamref name="T" /></c> value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <param name="writeValue">The function to use to write a value of type <c><typeparamref name="T" /></c>.</param>
        public static void WriteArrayValue<T>(Utf8JsonWriter writer, T[]? value, JsonSerializerOptions? options, WriteValue<T> writeValue)
        {
            // Convert using a converter if provided.
            if (options?.GetConverter(typeof(T[])) is JsonConverter<T[]> converter)
            {
                converter.Write(writer, value!, options!);

                return;
            }

            // Write `null` in case of a `null`.
            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            // Write the beginning of the array.
            writer.WriteStartArray();

            // Write the array.
            foreach (T item in value)
            {
                writeValue(writer, item, options!);
            }

            // Write the end of the array.
            writer.WriteEndArray();
        }

        /// <summary>Writes an <see cref="Array" /> of <see cref="Boolean" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteBooleanArray(Utf8JsonWriter writer, String name, Boolean[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteBooleanValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Boolean" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteBooleanArrayValue(Utf8JsonWriter writer, Boolean[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteBooleanValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Byte" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteByteArray(Utf8JsonWriter writer, String name, Byte[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteByteValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Byte" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteByteArrayValue(Utf8JsonWriter writer, Byte[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteByteValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int16" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt16Array(Utf8JsonWriter writer, String name, Int16[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteInt16Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int16" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt16ArrayValue(Utf8JsonWriter writer, Int16[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteInt16Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int32" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt32Array(Utf8JsonWriter writer, String name, Int32[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteInt32Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int32" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt32ArrayValue(Utf8JsonWriter writer, Int32[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteInt32Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int64" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteInt64Array(Utf8JsonWriter writer, String name, Int64[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteInt64Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Int64" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteInt64ArrayValue(Utf8JsonWriter writer, Int64[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteInt64Value);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Single" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteSingleArray(Utf8JsonWriter writer, String name, Single[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteSingleValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Single" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteSingleArrayValue(Utf8JsonWriter writer, Single[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteSingleValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Double" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteDoubleArray(Utf8JsonWriter writer, String name, Double[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteDoubleValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Double" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteDoubleArrayValue(Utf8JsonWriter writer, Double[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteDoubleValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Decimal" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteDecimalArray(Utf8JsonWriter writer, String name, Decimal[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteDecimalValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Decimal" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteDecimalArrayValue(Utf8JsonWriter writer, Decimal[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteDecimalValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Char" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteCharArray(Utf8JsonWriter writer, String name, Char[]? value, JsonSerializerOptions? options) =>
            WriteArray(writer, name, value, options, WriteCharValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="Char" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteCharArrayValue(Utf8JsonWriter writer, Char[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue(writer, value, options, WriteCharValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="String" />s property.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <remarks>
        ///     <para>The method uses the raw <c><paramref name="name" /></c> without conversion. For specific naming policies set via the <see cref="JsonSerializerOptions.PropertyNamingPolicy" /> property, convert the name prior to passing it to this method.</para>
        /// </remarks>
        public static void WriteStringArray(Utf8JsonWriter writer, String name, String[]? value, JsonSerializerOptions? options) =>
            WriteArray<String>(writer, name, value!, options, WriteStringValue);

        /// <summary>Writes an <see cref="Array" /> of <see cref="String" />s value.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public static void WriteStringArrayValue(Utf8JsonWriter writer, String[]? value, JsonSerializerOptions? options) =>
            WriteArrayValue<String>(writer, value!, options, WriteStringValue);
    }
}
