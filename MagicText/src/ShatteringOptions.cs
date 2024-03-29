using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Serialization;

namespace MagicText
{
    /// <summary>Defines options for the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods and for the extension methods from <see cref="TokeniserExtensions" />.</summary>
    [CLSCompliant(true), DataContract, Serializable]
    public sealed class ShatteringOptions : Object, IEquatable<ShatteringOptions>, ICloneable, IExtensibleDataObject
    {
        private const string StringBuilderNullErrorMessage = "String builder cannot be null.";

        /// <summary>Defines <see cref="String" /> constants for building <see cref="String" /> representations of <see cref="ShatteringOptions" />.</summary>
        private static class StringRepresentationConstants
        {
            /// <summary>A single space (<c>" "</c>).</summary>
            public const string Space = " ";

            /// <summary>A single comma (<c>","</c>).</summary>
            public const string Comma = ",";

            /// <summary>A single opening curly bracket (<c>"{"</c>).</summary>
            public const string OpeningBracket = "{";

            /// <summary>A single closing curly bracket (<c>"}"</c>).</summary>
            public const string ClosingBracket = "}";

            /// <summary>The <see cref="String" /> format for displaying named members.</summary>
            /// <remarks>
            ///     <para>The <see cref="NamedMemberFormat" /> is intended to be used as:</para>
            ///     <code>
            ///         <see cref="String" />.Format(<see cref="NamedMemberFormat" />, memberName, memberValue)
            ///     </code>
            ///     <para>In the example above, <c>memberName</c> is a <see cref="String" /> containing the name of the member (for instance, retrieved by the <c>nameof</c> operator), and <c>memberValue</c> is the current actual value of the member.</para>
            ///     <para>The resulting string is of the format <c>{memberName} = {memberValue}</c>, i. e. <c>memberName</c> and <c>memberValue</c> delimited by an equality sign (<c>"="</c>) which is surrounded by spaces (<c>" "</c>).</para>
            /// </remarks>
            [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
            public const string NamedMemberFormat = "{0} = {1}";

            /// <summary>The <see cref="String" /> delimiter for inserting between consecutive member displays.</summary>
            /// <remarks>
            ///     <para>The <see cref="MembersDelimiter" /> is a <see cref="Comma" /> followed by a <see cref="Space" />.</para>
            /// </remarks>
            public const string MembersDelimiter = Comma + Space;
        }

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> <c><paramref name="left" /></c> and <c><paramref name="right" /></c> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public static Boolean Equals(ShatteringOptions? left, ShatteringOptions? right) =>
            left is null || right is null ? Object.ReferenceEquals(left, right) : left.Equals(right);

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> <c><paramref name="left" /></c> and <c><paramref name="right" /></c> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public static Boolean operator ==(ShatteringOptions? left, ShatteringOptions? right) =>
            Equals(left, right);

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are not equal to the <c><paramref name="right" /></c>.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> <c><paramref name="left" /></c> and <c><paramref name="right" /></c> are not equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public static Boolean operator !=(ShatteringOptions? left, ShatteringOptions? right) =>
            !Equals(left, right);

        /// <summary>Gets the default <see cref="ShatteringOptions" /> to use when no options are set.</summary>
        /// <returns>The default <see cref="ShatteringOptions" />.</returns>
        /// <remarks>
        ///     <para>Retrieving the value of this property is <strong>exactly the same</strong> as creating new <see cref="ShatteringOptions" /> using the default <see cref="ShatteringOptions()" /> constructor. The property is provided merely to enable a more readable and explicit code when using default options.</para>
        ///     <para>The property always returns a new reference (to the newly constructed default <see cref="ShatteringOptions" />). Hence changes made to one <see cref="ShatteringOptions" /> returned by the property do not affect the others. Also, <c><see cref="Object" />.ReferenceEquals(<see cref="Default" />, <see cref="Default" />)</c> never evaluates to <c>true</c>, although all <c><see cref="Default" /> == <see cref="Default" /></c>, <c><see cref="ShatteringOptions" />.Equals(<see cref="Default" />, <see cref="Default" />)</c> and <c><see cref="Default" />.Equals(<see cref="Default" />)</c> evaluate to <c>true</c> because of overloads of methods and operators.</para>
        /// </remarks>
        public static ShatteringOptions Default => new ShatteringOptions();

        /// <summary>Returns a <see cref="String" /> that represents the <c><paramref name="options" /></c>.</summary>
        /// <param name="options">The <see cref="ShatteringOptions" /> to represent as a <see cref="String" />.</param>
        /// <returns>If the <c><paramref name="options" /></c> are non-<c>null</c>, a <see cref="String" /> that represents the <c><paramref name="options" /></c> is returned; otherwiset the empty <see cref="String" /> (<see cref="String.Empty" />) is returned.</returns>
        /// <remarks>
        ///     <para>Although the <see cref="ShatteringOptions" /> class is not a <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/"><em>C#</em></a> <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c></a>, its <see cref="String" /> representation imitates default <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c>s'</a> <see cref="Object.ToString()" /> method. Consequently, the <see cref="PrintMembers(StringBuilder)" />, <see cref="ToString()" /> and <see cref="ToString(ShatteringOptions)" /> methods behave similarly to methods described <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display">here</a> and <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#printmembers-formatting-in-derived-records">here</a>.</para>
        /// </remarks>
        public static String ToString([AllowNull] ShatteringOptions? options) =>
            options is null ? String.Empty : options.ToString();

        [XmlIgnore, JsonIgnore]
        private Boolean ignoreEmptyTokens;

        [XmlIgnore, JsonIgnore]
        private Boolean ignoreLineEnds;

        [XmlIgnore, JsonIgnore]
        private Boolean ignoreEmptyLines;

        [XmlIgnore, JsonIgnore]
        private String? lineEndToken;

        [XmlIgnore, JsonIgnore]
        private String? emptyLineToken;

        [XmlIgnore, JsonIgnore, NonSerialized]
        private ExtensionDataObject extensionData;

        /// <summary>Gets or sets the policy of ignoring empty tokens: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If empty tokens should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring empty tokens policy value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a <see cref="String" /> yielding <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method calls.</para>
        /// </remarks>
        [DisplayName("Ignore empty tokens"), Display(Name = "Ignore empty tokens", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Empty tokens", Order = 1), Editable(true, AllowInitialValue = true), DefaultValue(false), DataMember(Order = 1)]
        public Boolean IgnoreEmptyTokens
        {
            get => ignoreEmptyTokens;
            set
            {
                ignoreEmptyTokens = value;
            }
        }

        /// <summary>Gets or sets the policy of ignoring line ends: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If line ends should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring line ends policy value.</value>
        /// <remarks>
        ///     <para>If the value is <c>false</c>, line ends should be copied or represented by <see cref="LineEndToken" />s.</para>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (<a href="http://en.wikipedia.org/wiki/Newline#Representation">CR, LF and CRLF</a>) and/or the end of the input.</para>
        /// </remarks>
        [DisplayName("Ignore line ends"), Display(Name = "Ignore line ends", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Line ends", Order = 2), Editable(true, AllowInitialValue = true), DefaultValue(false), DataMember(Order = 2)]
        public Boolean IgnoreLineEnds
        {
            get => ignoreLineEnds;
            set
            {
                ignoreLineEnds = value;
            }
        }

        /// <summary>Gets or sets the policy of ignoring empty lines: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If empty lines should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring empty lines policy value.</value>
        /// <remarks>
        ///     <para>If the value is <c>true</c>, empty lines might not produce even <see cref="LineEndToken" />s (it depends on the <see cref="ITokeniser" /> interface implementation used); if <c>false</c>, they should be represented by <see cref="EmptyLineToken" />s.</para>
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text spanned between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the very beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        [DisplayName("Ignore empty lines"), Display(Name = "Ignore empty lines", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "Empty lines", Order = 3), Editable(true, AllowInitialValue = true), DefaultValue(false), DataMember(Order = 3)]
        public Boolean IgnoreEmptyLines
        {
            get => ignoreEmptyLines;
            set
            {
                ignoreEmptyLines = value;
            }
        }

        /// <summary>Gets or sets the token to represent a line end. The default is <see cref="Environment.NewLine" />.</summary>
        /// <returns>The line end token.</returns>
        /// <value>The new line end token value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (<a href="http://en.wikipedia.org/wiki/Newline#Representation">CR, LF and CRLF</a>) and/or the end of the input. Furthermore, an actual implementation of the <see cref="ITokeniser" /> interface at hand may as well choose to merely copy the line end and not replace it with a <see cref="LineEndToken" />. The property is given only to allow the standardisation of line ends when shattering text, but not to force it.</para>
        ///     <para>If a line is discarded as empty (when <see cref="IgnoreEmptyLines" /> is <c>true</c>), it might not produce <see cref="LineEndToken" />—it depends on the <see cref="ITokeniser" /> interface implementation used.</para>
        /// </remarks>
        [DataType(DataType.MultilineText), DisplayName("Line end token"), Display(Name = "Line end token", Description = "Token to represent a line end.", GroupName = "Tokens", ShortName = "Line end", Order = 4), DisplayFormat(ConvertEmptyStringToNull = false, HtmlEncode = true), Editable(true, AllowInitialValue = true)]
        [DefaultValue("\n")] // <-- this may be different from the `System.Environment.NewLine`
        [DataMember(IsRequired = false, Order = 4)]
        public String? LineEndToken
        {
            get => lineEndToken;
            set
            {
                lineEndToken = value;
            }
        }

        /// <summary>Gets or sets the token to represent an empty line. The default is the <see cref="String.Empty" />.</summary>
        /// <returns>The empty line token.</returns>
        /// <value>The new empty line token value.</value>
        /// <remarks>
        ///     <para>If a line is empty but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it might be represented by a <see cref="EmptyLineToken" />. If empty lines are substituted by <see cref="EmptyLineToken" />s, it should be done even if the <see cref="EmptyLineToken" /> would be considered an empty token and empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>). However, an actual implementation of the <see cref="ITokeniser" /> interface at hand may choose to simply yield 0 tokens for an empty line instead of the <see cref="EmptyLineToken" />. The property is given only to allow using empty lines as special parts of text (such as paragraph breaks), but not to force it.</para>
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the very beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        [DataType(DataType.MultilineText), DisplayName("Empty line token"), Display(Name = "Empty line token", Description = "Token to represent an empty line.", GroupName = "Tokens", ShortName = "Empty line", Order = 5), DisplayFormat(ConvertEmptyStringToNull = false, HtmlEncode = true), Editable(true, AllowInitialValue = true), DefaultValue(""), DataMember(IsRequired = false, Order = 5)]
        public String? EmptyLineToken
        {
            get => emptyLineToken;
            set
            {
                emptyLineToken = value;
            }
        }

        /// <summary>Gets or sets the structure that contains extra data.</summary>
        /// <returns>An <see cref="ExtensionDataObject" /> which contains data that is not recognised as belonging to the data contract.</returns>
        /// <value>The new <see cref="ExtensionDataObject" /> that contains data not recognised as belonging to the data contract.</value>
        ExtensionDataObject IExtensibleDataObject.ExtensionData
        {
            get => extensionData;
            set
            {
                extensionData = value;
            }
        }

        /// <summary>Creates options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public ShatteringOptions(Boolean ignoreEmptyTokens, Boolean ignoreLineEnds, Boolean ignoreEmptyLines, String? lineEndToken, String? emptyLineToken) : base()
        {
            this.ignoreEmptyTokens = ignoreEmptyTokens;
            this.ignoreLineEnds = ignoreLineEnds;
            this.ignoreEmptyLines = ignoreEmptyLines;
            this.lineEndToken = lineEndToken;
            this.emptyLineToken = emptyLineToken;

            extensionData = null!;
        }

        /// <summary>Creates default options.</summary>
        public ShatteringOptions() : this(false, false, false, Environment.NewLine, String.Empty)
        {
        }

        /// <summary>Copies the <c><paramref name="other" /></c> options.</summary>
        /// <param name="other">The <see cref="ShatteringOptions" /> to copy.</param>
        /// <remarks>
        ///     <para>If <c><paramref name="other" /></c> is <c>null</c>, the defaults are used instead of throwing an <see cref="ArgumentNullException" />. This is in line with handling <c>null</c> options in other parts of the project (e. g. in the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions, Boolean, CancellationToken)" /> methods).</para>
        /// </remarks>
        public ShatteringOptions(ShatteringOptions other) :
            this(
                !(other is null) && other.IgnoreEmptyTokens,
                !(other is null) && other.IgnoreLineEnds,
                !(other is null) && other.IgnoreEmptyLines,
                other is null ? Environment.NewLine : other.LineEndToken,
                other is null ? String.Empty : other.EmptyLineToken
            )
        {
        }

        /// <summary>Deconstructs the current options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The indicator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public void Deconstruct(out Boolean ignoreEmptyTokens, out Boolean ignoreLineEnds, out Boolean ignoreEmptyLines, out String? lineEndToken, out String? emptyLineToken)
        {
            ignoreEmptyTokens = IgnoreEmptyTokens;
            ignoreLineEnds = IgnoreLineEnds;
            ignoreEmptyLines = IgnoreEmptyLines;
            lineEndToken = LineEndToken;
            emptyLineToken = EmptyLineToken;
        }

        /// <summary>Returns the hash code of the current options.</summary>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" />.</returns>
        public override Int32 GetHashCode()
        {
            EqualityComparer<String> stringEqualityComparer = EqualityComparer<String>.Default;

            Int32 hashCode = 7;

            hashCode = 31 * hashCode + IgnoreEmptyTokens.GetHashCode();
            hashCode = 31 * hashCode + IgnoreLineEnds.GetHashCode();
            hashCode = 31 * hashCode + IgnoreEmptyLines.GetHashCode();

            try
            {
                hashCode = 31 * hashCode + stringEqualityComparer.GetHashCode(LineEndToken!);
            }
            catch (ArgumentNullException) when (LineEndToken is null)
            {
                hashCode *= 31;
            }

            try
            {
                hashCode = 31 * hashCode + stringEqualityComparer.GetHashCode(EmptyLineToken!);
            }
            catch (ArgumentNullException) when (EmptyLineToken is null)
            {
                hashCode *= 31;
            }

            return hashCode;
        }

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        public Boolean Equals(ShatteringOptions? other)
        {
            IEqualityComparer<String> stringEqualityComparer = EqualityComparer<String>.Default;

            return Object.ReferenceEquals(this, other) ||
                (
                    !(other is null) &&
                    IgnoreEmptyTokens == other.IgnoreEmptyTokens &&
                    IgnoreLineEnds == other.IgnoreLineEnds &&
                    IgnoreEmptyLines == other.IgnoreEmptyLines &&
                    stringEqualityComparer.Equals(LineEndToken!, other.LineEndToken!) &&
                    stringEqualityComparer.Equals(EmptyLineToken!, other.EmptyLineToken!)
                );
        }

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it, <c>true</c>; <c>false</c>otherwise.</returns>
        public override Boolean Equals(Object? obj)
        {
            ShatteringOptions? other;
            try
            {
                other = (ShatteringOptions)obj!;
            }
            catch (InvalidCastException)
            {
                return false;
            }

            return Equals(other);
        }

        /// <summary>Prints members to the <c><paramref name="stringBuilder" /></c>.</summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder" /> to which the members are printed.</param>
        /// <returns>If any member is actually printed to the <c><paramref name="stringBuilder" /></c>, <c>true</c>; false otherwise (no member was printed, the <c><paramref name="stringBuilder" /></c> is unaffected).</returns>
        /// <exception cref="ArgumentNullException">The <c><paramref name="stringBuilder" /></c> parameter is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>Although the <see cref="ShatteringOptions" /> class is not a <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/"><em>C#</em></a> <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c></a>, its <see cref="String" /> representation imitates default <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c>s'</a> <see cref="Object.ToString()" /> method. Consequently, the <see cref="PrintMembers(StringBuilder)" />, <see cref="ToString()" /> and <see cref="ToString(ShatteringOptions)" /> methods behave similarly to methods described <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display">here</a> and <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#printmembers-formatting-in-derived-records">here</a>.</para>
        /// </remarks>
        private Boolean PrintMembers(StringBuilder stringBuilder)
        {
            if (stringBuilder is null)
            {
                throw new ArgumentNullException(nameof(stringBuilder), StringBuilderNullErrorMessage);
            }

            {
                Dictionary<String, String?> memberRepresentations = new Dictionary<String, String?>(5);
                {
                    memberRepresentations[nameof(IgnoreEmptyTokens)] = IgnoreEmptyTokens.ToString();
                    memberRepresentations[nameof(IgnoreLineEnds)] = IgnoreLineEnds.ToString();
                    memberRepresentations[nameof(IgnoreEmptyLines)] = IgnoreEmptyLines.ToString();
                    memberRepresentations[nameof(LineEndToken)] = LineEndToken?.ToString();
                    memberRepresentations[nameof(EmptyLineToken)] = EmptyLineToken?.ToString();
                }
#if NETSTANDARD2_1_OR_GREATER
                memberRepresentations.TrimExcess();
#endif // NETSTANDARD2_1_OR_GREATER

                stringBuilder.AppendFormat(StringRepresentationConstants.NamedMemberFormat, nameof(IgnoreEmptyTokens), memberRepresentations[nameof(IgnoreEmptyTokens)]);
                stringBuilder.Append(StringRepresentationConstants.MembersDelimiter);
                stringBuilder.AppendFormat(StringRepresentationConstants.NamedMemberFormat, nameof(IgnoreLineEnds), memberRepresentations[nameof(IgnoreLineEnds)]);
                stringBuilder.Append(StringRepresentationConstants.MembersDelimiter);
                stringBuilder.AppendFormat(StringRepresentationConstants.NamedMemberFormat, nameof(IgnoreEmptyLines), memberRepresentations[nameof(IgnoreEmptyLines)]);
                stringBuilder.Append(StringRepresentationConstants.MembersDelimiter);
                stringBuilder.AppendFormat(StringRepresentationConstants.NamedMemberFormat, nameof(LineEndToken), memberRepresentations[nameof(LineEndToken)]);
                stringBuilder.Append(StringRepresentationConstants.MembersDelimiter);
                stringBuilder.AppendFormat(StringRepresentationConstants.NamedMemberFormat, nameof(EmptyLineToken), memberRepresentations[nameof(EmptyLineToken)]);
            }

            return true;
        }

        /// <summary>Returns a <see cref="String" /> that represents the current options.</summary>
        /// <returns>A <see cref="String" /> that represents the current <see cref="ShatteringOptions" />.</returns>
        /// <remarks>
        ///     <para>Although the <see cref="ShatteringOptions" /> class is not a <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/"><em>C#</em></a> <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c></a>, its <see cref="String" /> representation imitates default <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record"><c>record</c>s'</a> <see cref="Object.ToString()" /> method. Consequently, the <see cref="PrintMembers(StringBuilder)" />, <see cref="ToString()" /> and <see cref="ToString(ShatteringOptions)" /> methods behave similarly to methods described <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#built-in-formatting-for-display">here</a> and <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/language-reference/builtin-types/record#printmembers-formatting-in-derived-records">here</a>.</para>
        /// </remarks>
        public override String ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(GetType().Name ?? nameof(ShatteringOptions));
            stringBuilder.Append(StringRepresentationConstants.Space);
            stringBuilder.Append(StringRepresentationConstants.OpeningBracket);
            stringBuilder.Append(StringRepresentationConstants.Space);
            if (PrintMembers(stringBuilder))
            {
                stringBuilder.Append(StringRepresentationConstants.Space);
            }
            stringBuilder.Append(StringRepresentationConstants.ClosingBracket);

            return stringBuilder.ToString();
        }

        /// <summary>Creates new <see cref="ShatteringOptions" /> that are a copy of the current options.</summary>
        /// <returns>A new <see cref="ShatteringOptions" /> with the same values.</returns>
        public ShatteringOptions Clone() =>
            new ShatteringOptions(this);

        /// <summary>Creates a new <see cref="Object" /> that is a copy of the current options.</summary>
        /// <returns>A new <see cref="ShatteringOptions" /> with the same values.</returns>
        Object ICloneable.Clone() =>
            Clone();
    }
}
