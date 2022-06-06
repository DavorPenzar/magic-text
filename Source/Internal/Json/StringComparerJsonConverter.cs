using MagicText.Internal.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Internal.Json
{
    internal sealed class StringComparerJsonConverter : JsonConverter<StringComparer>, IEquatable<StringComparerJsonConverter>, IEquatable<JsonConverter<StringComparer>>, IEquatable<JsonConverter>
    {
        delegate void JsonConverterWrite<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
        delegate Object? JsonConverterRead(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

        private static class CustomStringComparerEncapsulationPropertyNames
        {
            public const string TypeName = "Type";
            public const string ValueName = "Value";
        }

        [return: MaybeNull, NotNullIfNotNull("name")]
        private static String? DoNotConvertName([AllowNull] String? name) =>
            name;

        public static Boolean Equals(StringComparerJsonConverter? left, StringComparerJsonConverter? right) =>
            left is null || right is null ? Object.ReferenceEquals(left, right) : left.Equals(right);

        public static Boolean operator ==(StringComparerJsonConverter? left, StringComparerJsonConverter? right) =>
            Equals(left, right);

        public static Boolean operator !=(StringComparerJsonConverter? left, StringComparerJsonConverter? right) =>
            !Equals(left, right);

        private static void SkipComments(ref Utf8JsonReader reader)
        {
            while (reader.TokenType == JsonTokenType.Comment)
            {
                reader.Read();
            }
        }

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

        private static StringComparison ReadStringComparison(ref Utf8JsonReader reader, JsonSerializerOptions? options)
        {
            StringComparison value;

            SkipComments(ref reader);

            if (options?.GetConverter(typeof(StringComparison)) is JsonConverter<StringComparison> converter)
            {
                value = converter.Read(ref reader, typeof(StringComparison), options);
            }
            else
            {
                value = (StringComparison)reader.GetInt32();
            }

            //reader.Read();

            return value;
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

        private static void WriteStringComparison(Utf8JsonWriter writer, String name, StringComparison value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(StringComparison)) is JsonConverter<StringComparison> converter)
            {
                writer.WritePropertyName(name);
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteNumber(name, (Int32)value);
        }

        private static void WriteStringComparisonValue(Utf8JsonWriter writer, StringComparison value, JsonSerializerOptions? options)
        {
            if (options?.GetConverter(typeof(StringComparison)) is JsonConverter<StringComparison> converter)
            {
                converter.Write(writer, value, options);

                return;
            }

            writer.WriteNumberValue((Int32)value);
        }

        private static StringComparer? ReadCustomStringComparer(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions? options)
        {
            Func<String, String> convertName = options?.PropertyNamingPolicy is null ? new Func<String, String>(DoNotConvertName!) : new Func<String, String>(options.PropertyNamingPolicy.ConvertName);

            SkipComments(ref reader);

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            reader.Read();

            StringComparer propertyNameComparer = (options is null || !options.PropertyNameCaseInsensitive) ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;

            SkipComments(ref reader);

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            if (!propertyNameComparer.Equals(reader.GetString() ?? throw new JsonException(), convertName(CustomStringComparerEncapsulationPropertyNames.TypeName)))
            {
                throw new JsonException();
            }
            reader.Read();

            SkipComments(ref reader);

            Type valueType = Type.GetType(ReadString(ref reader, options));

            SkipComments(ref reader);

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            if (!propertyNameComparer.Equals(reader.GetString() ?? throw new JsonException(), convertName(CustomStringComparerEncapsulationPropertyNames.ValueName)))
            {
                throw new JsonException();
            }
            reader.Read();

            JsonConverter jsonConverter = options?.GetConverter(valueType) ?? throw new InvalidOperationException();
            Type converterType = typeof(JsonConverter<>).MakeGenericType(valueType);
            if (!converterType.IsAssignableFrom(jsonConverter.GetType()))
            {
                throw new InvalidOperationException();
            }

            JsonConverterRead conversionMethod = (JsonConverterRead)Delegate.CreateDelegate(typeof(JsonConverterRead), jsonConverter, converterType.GetMethod(nameof(JsonConverter<Object>.Read), new Type[] { typeof(Utf8JsonReader).MakeByRefType(), typeof(Type), typeof(JsonSerializerOptions) }));

            SkipComments(ref reader);

            StringComparer? value = (StringComparer)conversionMethod.Invoke(ref reader, valueType, options)!;
            reader.Read();

            SkipComments(ref reader);

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return value;
        }

        private static void WriteCustomStringComparer(Utf8JsonWriter writer, StringComparer value, JsonSerializerOptions? options)
        {
            Func<String, String> convertName = options?.PropertyNamingPolicy is null ? new Func<String, String>(DoNotConvertName!) : new Func<String, String>(options.PropertyNamingPolicy.ConvertName);

            Type valueType = value.GetType();

            JsonConverter jsonConverter = options?.GetConverter(valueType) ?? throw new InvalidOperationException();
            Type converterType = typeof(JsonConverter<>).MakeGenericType(valueType);
            if (!converterType.IsAssignableFrom(jsonConverter.GetType()))
            {
                throw new InvalidOperationException();
            }

            MethodInfo conversionMethod = converterType.GetMethod(nameof(JsonConverter<Object>.Write), new Type[] { typeof(Utf8JsonWriter), valueType, typeof(JsonSerializerOptions) });
            
            writer.WriteStartObject();

            WriteString(writer, convertName(CustomStringComparerEncapsulationPropertyNames.TypeName), valueType.AssemblyQualifiedName, options);
            
            writer.WritePropertyName(convertName(CustomStringComparerEncapsulationPropertyNames.ValueName));
            conversionMethod.Invoke(jsonConverter, new Object[] { writer, value, options });

            writer.WriteEndObject();
        }

        public StringComparerJsonConverter() : base()
        {
        }

        public override StringComparer? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Skip comments.
            SkipComments(ref reader);

            // Return `null` if a `null` JSON token is found.
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            // If a JSON object is found, read it as a custom string comparer.
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return ReadCustomStringComparer(ref reader, typeToConvert, options);
            }

            // Read the comparison type.
            StringComparison comparisonType = ReadStringComparison(ref reader, options);

            // Return a string comparer corresponding to the comparison type.
#if NETSTANDARD2_0
            return StringComparerExtensions.GetComparerFromComparison(comparisonType);
#else
            return StringComparer.FromComparison(comparisonType);
#endif // NETSTANDARD2_0
        }

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
                WriteCustomStringComparer(writer, value, options);
            }
        }

        public override Int32 GetHashCode() =>
            GetType().GetHashCode();

        public Boolean Equals(StringComparerJsonConverter? other) =>
            !(other is null) && other.GetType().Equals(GetType());

        public Boolean Equals(JsonConverter<StringComparer>? other) =>
            !(other is null) && other.GetType().Equals(GetType());

        public Boolean Equals(JsonConverter? other) =>
            !(other is null) && other.GetType().Equals(GetType());

        public override Boolean Equals(Object? obj) =>
            !(obj is null) && obj.GetType().Equals(GetType());
    }
}
