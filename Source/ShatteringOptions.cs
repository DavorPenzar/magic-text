using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace MagicText
{
    /// <summary>Defines the options for the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods and for the extension methods from <see cref="TokeniserExtensions" />.</summary>
    [Serializable]
    public class ShatteringOptions : Object, IEquatable<ShatteringOptions>, ICloneable, ISerializable
    {
        protected const string SerialisationInfoNullErrorMessage = "Serialisation info cannot be null.";
        private const string OtherNullErrorMessage = "Shattering options to copy cannot be null.";
        protected const string StringComparerNullErrorMessage = "String comparer cannot be null.";

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are equal to the <c><paramref name="right" /></c> or not.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public static Boolean operator ==(ShatteringOptions? left, ShatteringOptions? right) =>
            left is null ? right is null : left.Equals(right);

        /// <summary>Indicates whether the <c><paramref name="left" /></c> <see cref="ShatteringOptions" /> are not equal to the <c><paramref name="right" /></c>.</summary>
        /// <param name="left">The left <see cref="ShatteringOptions" /> to compare.</param>
        /// <param name="right">The right <see cref="ShatteringOptions" /> to compare.</param>
        /// <returns>If the <see cref="ShatteringOptions" /> are not equal, <c>true</c>; <c>false</c> otherwise.</returns>
        public static Boolean operator !=(ShatteringOptions? left, ShatteringOptions? right) =>
            !(left == right);

        /// <summary>Gets the default <see cref="ShatteringOptions" /> to use when no options are set.</summary>
        /// <returns>The default <see cref="ShatteringOptions" />.</returns>
        /// <remarks>
        ///     <para>Retrieving the value of this property is <strong>exactly the same</strong> as creating new <see cref="ShatteringOptions" /> using the default <see cref="ShatteringOptions()" /> constructor. The property is provided merely to enable a more readable and explicit code when handling <c>null</c>-options in the implementations of the <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods.</para>
        ///     <para>The property always returns a new reference (to the newly constructed default <see cref="ShatteringOptions" />). Hence changes made to one <see cref="ShatteringOptions" /> returned by the property do not affect the others. Also, <c><see cref="Object" />.ReferenceEquals(<see cref="Default" />, <see cref="Default" />)</c> never evaluates to <c>true</c>, although both <c><see cref="Default" />.Equals(<see cref="Default" />)</c> and <c><see cref="Default" /> == <see cref="Default" /></c> evaluate to <c>true</c> because of the overloads of methods and operators.</para>
        /// </remarks>
        public static ShatteringOptions Default => new ShatteringOptions();

        private Boolean ignoreEmptyTokens;
        private Boolean ignoreLineEnds;
        private Boolean ignoreEmptyLines;

        private String? lineEndToken;
        private String? emptyLineToken;

        /// <summary>Gets or sets the policy of ignoring empty tokens: <c>true</c> if ignoring, <c>false</c> otherwise. The default is <c>false</c>.</summary>
        /// <returns>If empty tokens should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>The new ignoring empty tokens policy value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a <see cref="String" /> yielding <c>true</c> when checked via the <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method calls.</para>
        /// </remarks>
        [DisplayName("Ignore empty tokens")]
        [Display(Name = "Ignore empty tokens", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "IgnoreEmptyTokens", Order = 0)]
        [DefaultValue(false)]
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
        ///     <para>If the value is <c>false</c>, then line ends should be copied or represented by <see cref="LineEndToken" />s.</para>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (CR, LF and CRLF) and/or the end of the input.</para>
        /// </remarks>
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="LineEndToken" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Ignore line ends")]
        [Display(Name = "Ignore line ends", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "IgnoreLineEnds", Order = 1)]
        [DefaultValue(false)]
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
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text spanned between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="LineEndToken" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Ignore empty lines")]
        [Display(Name = "Ignore empty lines", Description = "True if ignoring, false otherwise.", GroupName = "Ignore", ShortName = "IgnoreEmptyLines", Order = 2)]
        [DefaultValue(false)]
        public Boolean IgnoreEmptyLines
        {
            get => ignoreEmptyLines;
            set
            {
                ignoreEmptyLines = value;
            }
        }

        /// <summary>Gets or sets the token to represent a line end. The default is the <see cref="Environment.NewLine" />.</summary>
        /// <returns>The line end token.</returns>
        /// <value>The new line end token value.</value>
        /// <remarks>
        ///     <para>The actual implementations of the <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters and sequences of characters (CR, LF and CRLF) and/or the end of the input. Furthermore, an actual implementation of the <see cref="ITokeniser" /> interface at hand may as well choose to merely copy the line end, and not replace it with a <see cref="LineEndToken" />. The property is given only to allow the standardisation of line ends when shattering text, but not to force it.</para>
        ///     <para>If a line is discarded as empty (when <see cref="IgnoreEmptyLines" /> is <c>true</c>), it might not produce <see cref="LineEndToken" />—it depends on the <see cref="ITokeniser" /> interface implementation used.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Line end token")]
        [Display(Name = "Line end token", Description = "Token to represent a line end.", GroupName = "Tokens", ShortName = "LineEndToken", Order = 3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DefaultValue("\n")] // <-- this may be different from the `System.Environment.NewLine`
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
        ///     <para>If a line is empty but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it might be be represented by a <see cref="EmptyLineToken" />. If empty lines are substituted by <see cref="EmptyLineToken" />s, it should be done even if the <see cref="EmptyLineToken" /> would be considered an empty token and empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>). However, an actual implementation of the <see cref="ITokeniser" /> interface at hand may choose to simply yield 0 tokens for an empty line instead of the <see cref="EmptyLineToken" />. The property is given only to allow using empty lines as special parts of text (such as paragraph breaks), but not to force it.</para>
        ///     <para>A <em>line</em> should be considered a <see cref="String" /> of text between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of the <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />). Of course, a line's border may also be the beginning and/or the end of the text.</para>
        ///     <para>Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering the empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.</para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="LineEndToken" />
        [DisplayName("Empty line token")]
        [Display(Name = "Empty line token", Description = "Token to represent an empty line.", GroupName = "Tokens", ShortName = "EmptyLineToken", Order = 4)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DefaultValue("")]
        public String? EmptyLineToken
        {
            get => emptyLineToken;
            set
            {
                emptyLineToken = value;
            }
        }

        /// <summary>Creates default options.</summary>
        public ShatteringOptions() : this(false, false, false, Environment.NewLine, String.Empty)
        {
        }

        /// <summary>Copies the <c><paramref name="other" /></c> options.</summary>
        /// <param name="other">The <see cref="ShatteringOptions" /> to copy.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="other" /></c> is <c>null</c>.</exception>
        public ShatteringOptions(ShatteringOptions other) :
            this(
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreEmptyTokens,
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreLineEnds,
                other is null ? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage) : other.IgnoreEmptyLines,
                other?.LineEndToken,
                other?.EmptyLineToken
            )
        {
        }

        /// <summary>Creates options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public ShatteringOptions(Boolean ignoreEmptyTokens, Boolean ignoreLineEnds, Boolean ignoreEmptyLines, String? lineEndToken, String? emptyLineToken) : base()
        {
            this.ignoreEmptyTokens = ignoreEmptyTokens;
            this.ignoreLineEnds = ignoreLineEnds;
            this.ignoreEmptyLines = ignoreEmptyLines;
            this.lineEndToken = lineEndToken;
            this.emptyLineToken = emptyLineToken;
        }

        /// <summary>Creates options by retrieving the serialisation <c><paramref name="info" /></c>.</summary>
        /// <param name="info">The <see cref="SerializationInfo" /> from which to read data.</param>
        /// <param name="context">The source of this deserialisation.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="info" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The exceptions thrown by the <c><paramref name="info" /></c>'s methods (most notably the <see cref="SerializationException" /> and <see cref="InvalidCastException" />) are not caught.</para>
        /// </remarks>
        protected ShatteringOptions(SerializationInfo info, StreamingContext context) :
            this(
                info is null ? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage) : info.GetBoolean(nameof(IgnoreEmptyTokens)),
                info is null ? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage) : info.GetBoolean(nameof(IgnoreLineEnds)),
                info is null ? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage) : info.GetBoolean(nameof(IgnoreEmptyLines)),
                info is null ? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage) : info.GetString(nameof(LineEndToken)),
                info is null ? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage) : info.GetString(nameof(EmptyLineToken))
            )
        {
        }

        /// <summary>Deconstructs the current options.</summary>
        /// <param name="ignoreEmptyTokens">The indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">The indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">The inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">The token to represent a line end.</param>
        /// <param name="emptyLineToken">The token to represent an empty line.</param>
        public virtual void Deconstruct(out Boolean ignoreEmptyTokens, out Boolean ignoreLineEnds, out Boolean ignoreEmptyLines, out String? lineEndToken, out String? emptyLineToken)
        {
            ignoreEmptyTokens = IgnoreEmptyTokens;
            ignoreLineEnds = IgnoreLineEnds;
            ignoreEmptyLines = IgnoreEmptyLines;
            lineEndToken = LineEndToken;
            emptyLineToken = EmptyLineToken;
        }

        /// <summary>Returns the hash code of the current options according to the <c><paramref name="stringComparer" /></c>.</summary>
        /// <param name="stringComparer">The <see cref="StringComparer" /> used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" /> according to the <c><paramref name="stringComparer" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        public virtual Int32 GetHashCode(IEqualityComparer<String?> stringComparer)
        {
            if (stringComparer is null)
            {
                throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage);
            }

            Int32 hashCode = 7;

            hashCode = 31 * hashCode + IgnoreEmptyTokens.GetHashCode();
            hashCode = 31 * hashCode + IgnoreLineEnds.GetHashCode();
            hashCode = 31 * hashCode + IgnoreEmptyLines.GetHashCode();
            hashCode = 31 * hashCode + stringComparer.GetHashCode(lineEndToken);
            hashCode = 31 * hashCode + stringComparer.GetHashCode(EmptyLineToken);

            return hashCode;
        }

        /// <summary>Returns the hash code of the current options.</summary>
        /// <returns>The hash code of the current <see cref="ShatteringOptions" />.</returns>
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        public override Int32 GetHashCode() =>
            GetHashCode(EqualityComparer<String?>.Default);

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options according to the <c><paramref name="stringComparer" /></c> or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <param name="stringComparer">The <see cref="StringComparer" /> used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c> according to the <c><paramref name="stringComparer" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode()" />
        public virtual Boolean Equals(ShatteringOptions? other, IEqualityComparer<String?> stringComparer) =>
            stringComparer is null ?
                throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage) :
                Object.ReferenceEquals(this, other) ||
                    (
                        !(other is null) &&
                        IgnoreEmptyTokens == other.IgnoreEmptyTokens &&
                        IgnoreLineEnds == other.IgnoreLineEnds &&
                        IgnoreEmptyLines == other.IgnoreEmptyLines &&
                        stringComparer.Equals(LineEndToken, other.LineEndToken) &&
                        stringComparer.Equals(EmptyLineToken, other.EmptyLineToken)
                    );

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="other" /></c> options or not.</summary>
        /// <param name="other">The other <see cref="ShatteringOptions" /> to compare with these options.</param>
        /// <returns>If the current <see cref="ShatteringOptions" /> are equal to the <c><paramref name="other" /></c>, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode()" />
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        public virtual Boolean Equals(ShatteringOptions? other) =>
            Equals(other, EqualityComparer<String?>.Default);

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> according to the <c><paramref name="stringComparer" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <param name="stringComparer">The <see cref="StringComparer" /> used for comparing <see cref="String" />s for equality and for retrieving <see cref="String" />s' hash codes.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it according to the <c><paramref name="stringComparer" /></c>, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="stringComparer" /></c> is <c>null</c>.</exception>
        /// <seealso cref="Equals(Object?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode()" />
        public virtual Boolean Equals(Object? obj, IEqualityComparer<String?> stringComparer)
        {
            try
            {
                return !(obj is null) && Equals((ShatteringOptions)obj, stringComparer);
            }
            catch (InvalidCastException)
            {
            }

            return false;
        }

        /// <summary>Indicates whether the current options are equal to the <c><paramref name="obj" /></c> or not.</summary>
        /// <param name="obj">The <see cref="Object" /> to compare with these <see cref="ShatteringOptions" />.</param>
        /// <returns>If the <c><paramref name="obj" /></c> is also <see cref="ShatteringOptions" /> or it may be cast to <see cref="ShatteringOptions" /> and the current shattering options are equal to it, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <seealso cref="Equals(Object?, IEqualityComparer{String?})" />
        /// <seealso cref="Equals(ShatteringOptions?)" />
        /// <seealso cref="Equals(ShatteringOptions?, IEqualityComparer{String?})" />
        /// <seealso cref="GetHashCode()" />
        /// <seealso cref="GetHashCode(IEqualityComparer{String?})" />
        public override Boolean Equals(Object? obj) =>
            Equals(obj, EqualityComparer<String?>.Default);

        /// <summary>Populates the serialisation <c><paramref name="info" /></c> with data needed to serialise the current options.</summary>
        /// <param name="info">The <see cref="SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination for this serialisation.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="info" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>The exceptions thrown by the <c><paramref name="info" /></c>'s methods (most notably the <see cref="SerializationException" />) are not caught.</para>
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage);
            }

            info.AddValue(nameof(IgnoreEmptyTokens), IgnoreEmptyTokens);
            info.AddValue(nameof(IgnoreLineEnds), IgnoreLineEnds);
            info.AddValue(nameof(IgnoreEmptyLines), IgnoreEmptyLines);
            info.AddValue(nameof(LineEndToken), LineEndToken, typeof(String));
            info.AddValue(nameof(EmptyLineToken), EmptyLineToken, typeof(String));
        }

        /// <summary>Creates a new <see cref="Object" /> that is a copy of the current options.</summary>
        /// <returns>The new <see cref="ShatteringOptions" /> with the same values.</returns>
        public virtual Object Clone() =>
            new ShatteringOptions(this);
    }
}
