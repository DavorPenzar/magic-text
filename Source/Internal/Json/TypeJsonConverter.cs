using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagicText.Internal.Json
{
    /// <summary>Provides methods to convert <see cref="Type" />s to <a href="http://json.org/json-en.html"><em>JSON</em></a> elements and <em>vice-versa</em>.</summary>
    /// <remarks>
    ///     <para>This is a singleton class. <see cref="TypeJsonConverter" />s cannot be initialised, use the singleton <see cref="Instance" /> static property instead.</para>
    ///     <para>Obviously, the <see cref="Type" /> is framework/language-specific even though there are other <a href="http://en.wikipedia.org/wiki/Object-oriented_programming">object-oriented programming languages</a>. Consequently, interpreting the resulting <a href="http://json.org/json-en.html"><em>JSON</em></a> in a different programming language/framework is meaningless.</para>
    ///     <para><strong>Nota bene.</strong> All private methods (static or instance members) are intended for <strong>internal use only</strong> and therefore do not make unnecessary checks of the parameters.</para>
    /// </remarks>
    internal sealed class TypeJsonConverter : JsonConverter<Type>
    {
        private static readonly TypeJsonConverter _instance;

        /// <summary>Gets the singleton instance of the <see cref="TypeJsonConverter" />.</summary>
        /// <returns>The singleton instace of the <see cref="TypeJsonConverter" />.</returns>
        public static TypeJsonConverter Instance => _instance;

        /// <summary>Initiallises static fields.</summary>
        static TypeJsonConverter()
        {
            _instance = new TypeJsonConverter();
        }

        /// <summary>Creates a default <see cref="StringComparerJsonConverter" />.</summary>
        private TypeJsonConverter() : base()
        {
        }

        /// <summary>Reads and converts the <a href="http://json.org/json-en.html"><em>JSON</em></a> to a <see cref="StringComparer" /> value.</summary>
        /// <param name="reader">The <see cref="Utf8JsonReader" /> to read.</param>
        /// <param name="typeToConvert">The type to convert (<see cref="StringComparer" />).</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        /// <returns>The converted <see cref="StringComparer" /> value.</returns>
        /// <exception cref="JsonException">An unexpected <a href="http://json.org/json-en.html"><em>JSON</em></a> token is encountered (by type or value).</exception>
        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            return Type.GetType(JsonConversionHelper.ReadString(ref reader, options, read: false));
        }

        /// <summary>Writes the specified <see cref="StringComparer" /> <c><paramref name="value" /></c> as <a href="http://json.org/json-en.html"><em>JSON</em></a>.</summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter" /> to which to write.</param>
        /// <param name="value">The <see cref="StringComparer" /> value to convert to <a href="http://json.org/json-en.html"><em>JSON</em></a>.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions" /> to use.</param>
        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();

                return;
            }

            JsonConversionHelper.WriteStringValue(writer, value.AssemblyQualifiedName, options);
        }
    }
}
