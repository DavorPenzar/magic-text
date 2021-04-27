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
    /// <summary>Options for <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods and for extension methods from <see cref="TokeniserExtensions" />.</summary>
    [Serializable]
    public class ShatteringOptions : Object, IEquatable<ShatteringOptions>, ICloneable, ISerializable
    {
        protected const string SerialisationInfoNullErrorMessage = "Serialisation info may not be `null`.";
        private const string OtherNullErrorMessage = "Shattering options to copy may not be `null`.";
        protected const string StringComparerNullErrorMessage = "String comparer may not be `null`.";

        public static Boolean operator ==(ShatteringOptions? left, ShatteringOptions? right) =>
            left is null ? Object.ReferenceEquals(left, right) : left.Equals(right);

        public static Boolean operator !=(ShatteringOptions? left, ShatteringOptions? right) =>
            !(left == right);

        /// <summary>Default shattering options to use when none are set.</summary>
        /// <returns>Default shattering options.</returns>
        /// <remarks>
        ///     <para>
        ///         Retrieving the value of this property is <strong>exactly the same</strong> as creating shattering options using <see cref="ShatteringOptions.ShatteringOptions()" /> constructor. The property is provided merely to enable a more readable and explicit code when handling <c>null</c>-options in the implementation of <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?, CancellationToken, Boolean)" /> methods.
        ///     </para>
        ///
        ///     <para>
        ///         The property always returns a new reference (to newly constructed default shattering options). Hence changes made to one options returned by the property do not affect the others. Also, <c><see cref="Object" />.ReferenceEquals(<see cref="Default" />, <see cref="Default" />)</c> never evaluates to <c>true</c>, although <c><see cref="Default" />.Equals(<see cref="Default" />)</c> and <c><see cref="Default" /> == <see cref="Default" /></c> evaluate to <c>true</c> because of the overloads of methods and operators.
        ///     </para>
        /// </remarks>
        public static ShatteringOptions Default => new ShatteringOptions();

        private bool ignoreEmptyTokens;
        private bool ignoreLineEnds;
        private bool ignoreEmptyLines;

        private String? lineEndToken;
        private String? emptyLineToken;

        /// <summary>Policy of ignoring empty tokens: <c>true</c> if ignoring, <c>false</c> otherwise. Default is <c>false</c>.</summary>
        /// <returns>If empty tokens should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>New ignoring empty tokens policy value.</value>
        /// <remarks>
        ///     <para>
        ///         Actual implementations of <see cref="ITokeniser" /> interface may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a string yielding <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method.
        ///     </para>
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

        /// <summary>Policy of ignoring line ends: <c>true</c> if ignoring, <c>false</c> otherwise. Default is <c>false</c>.</summary>
        /// <returns>If line ends should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>New ignoring line ends policy value.</value>
        /// <remarks>
        ///     <para>
        ///         If <c>false</c>, line ends should be copied or represented by <see cref="LineEndToken" />s.
        ///     </para>
        ///
        ///     <para>
        ///         Actual implementations of <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters (CR, LF and CRLF) and/or the end of the input.
        ///     </para>
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

        /// <summary>Policy of ignoring empty lines: <c>true</c> if ignoring, <c>false</c> otherwise. Default is <c>false</c>.</summary>
        /// <returns>If empty lines should be ignored, <c>true</c>; <c>false</c> otherwise.</returns>
        /// <value>New ignoring empty lines policy value.</value>
        /// <remarks>
        ///     <para>
        ///         If <c>true</c>, empty lines might not produce even <see cref="LineEndToken" />s (it depends on the <see cref="ITokeniser" /> interface implementation used); if <c>false</c>, they should be represented by <see cref="EmptyLineToken" />s.
        ///     </para>
        ///
        ///     <para>
        ///         A <em>line</em> should be considered the string of text between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />).
        ///     </para>
        ///
        ///     <para>
        ///         Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.
        ///     </para>
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

        /// <summary>Token to represent a line end. Default is <see cref="Environment.NewLine" />.</summary>
        /// <returns>Line end token.</returns>
        /// <value>New line end token value.</value>
        /// <remarks>
        ///     <para>
        ///         Actual implementations of <see cref="ITokeniser" /> interface may define what exactly a <em>line end</em> means, but usually this would be the new line characters (CR, LF and CRLF) and/or the end of the input. Furthermore, an actual implementation of <see cref="ITokeniser" /> interface at hand may as well choose to merely copy the line end, and not replace it with <see cref="LineEndToken" />. The property is given to allow standardisation of line ends when shattering text, but not to force it.
        ///     </para>
        ///
        ///     <para>
        ///         If a line is discarded as empty (when <see cref="IgnoreEmptyLines" /> is <c>true</c>), it might not produce <see cref="LineEndToken" />—it depends on the <see cref="ITokeniser" /> interface implementation used.
        ///     </para>
        /// </remarks>
        /// <seealso cref="IgnoreLineEnds" />
        /// <seealso cref="IgnoreEmptyLines" />
        /// <seealso cref="EmptyLineToken" />
        [DisplayName("Line end token")]
        [Display(Name = "Line end token", Description = "Token to represent a line end.", GroupName = "Tokens", ShortName = "LineEndToken", Order = 3)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DefaultValue("\n")] // <-- this may be different from `System.Environment.NewLine`
        public String? LineEndToken
        {
            get => lineEndToken;
            set
            {
                lineEndToken = value;
            }
        }

        /// <summary>Token to represent an empty line. Default is <see cref="String.Empty" />.</summary>
        /// <returns>Empty line token.</returns>
        /// <value>New empty line token value.</value>
        /// <remarks>
        ///     <para>
        ///         If a line is empty but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it might be be represented by <see cref="EmptyLineToken" />. If empty lines are substituted by <see cref="EmptyLineToken" />, it should be done even if <see cref="EmptyLineToken" /> would be considered an empty token and empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>). However, an actual implementation of <see cref="ITokeniser" /> interface at hand may choose to simply yield 0 tokens for an empty line instead of <see cref="EmptyLineToken" />. The property is given to allow using empty lines as special breaks (such as paragraph breaks), but not to force it.
        ///     </para>
        ///
        ///     <para>
        ///         A <em>line</em> should be considered the string of text between two consecutive line ends, whereas the definition of a <em>line end</em> is left to the actual implementation of <see cref="ITokeniser" /> interface (v. <see cref="IgnoreLineEnds" /> and <see cref="LineEndToken" />).
        ///     </para>
        ///
        ///     <para>
        ///         Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.
        ///     </para>
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

        /// <summary>Create default shattering options.</summary>
        public ShatteringOptions() : this(false, false, false, Environment.NewLine, String.Empty)
        {
        }

        /// <summary>Copy shattering options.</summary>
        /// <param name="other">Shattering options to copy.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="other" /> is <c>null</c>.</exception>
        public ShatteringOptions(ShatteringOptions other) :
            this(
                other?.IgnoreEmptyTokens ?? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage),
                other?.IgnoreLineEnds ?? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage),
                other?.IgnoreEmptyLines ?? throw new ArgumentNullException(nameof(other), OtherNullErrorMessage),
                other?.LineEndToken,
                other?.EmptyLineToken
            )
        {
        }

        /// <summary>Create specified shattering options.</summary>
        /// <param name="ignoreEmptyTokens">Indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">Indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">Inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">Token to represent a line end.</param>
        /// <param name="emptyLineToken">Token to represent an empty line.</param>
        public ShatteringOptions(Boolean ignoreEmptyTokens, Boolean ignoreLineEnds, Boolean ignoreEmptyLines, String? lineEndToken, String? emptyLineToken) : base()
        {
            this.ignoreEmptyTokens = ignoreEmptyTokens;
            this.ignoreLineEnds = ignoreLineEnds;
            this.ignoreEmptyLines = ignoreEmptyLines;
            this.lineEndToken = lineEndToken;
            this.emptyLineToken = emptyLineToken;
        }

        /// <summary>Construct shattering options by retrieving serialisation info (deserialise the options).</summary>
        /// <param name="info">Serialisation info to read data.</param>
        /// <param name="context">Source of this deserialisation.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="info" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <paramref name="info" />'s methods (most notably <see cref="SerializationException" /> and <see cref="InvalidCastException" />) are not caught.
        ///     </para>
        /// </remarks>
        protected ShatteringOptions(SerializationInfo info, StreamingContext context) :
            this(
                info?.GetBoolean(nameof(IgnoreEmptyTokens)) ?? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage),
                info?.GetBoolean(nameof(IgnoreLineEnds)) ?? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage),
                info?.GetBoolean(nameof(IgnoreEmptyLines)) ?? throw new ArgumentNullException(nameof(info), SerialisationInfoNullErrorMessage),
                info?.GetString(nameof(LineEndToken)),
                info?.GetString(nameof(EmptyLineToken))
            )
        {
        }

        /// <summary>Deconstruct shattering options.</summary>
        /// <param name="ignoreEmptyTokens">Indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">Indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">Inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">Token to represent a line end.</param>
        /// <param name="emptyLineToken">Token to represent an empty line.</param>
        public virtual void Deconstruct(out Boolean ignoreEmptyTokens, out Boolean ignoreLineEnds, out Boolean ignoreEmptyLines, out String? lineEndToken, out String? emptyLineToken)
        {
            ignoreEmptyTokens = IgnoreEmptyTokens;
            ignoreLineEnds = IgnoreLineEnds;
            ignoreEmptyLines = IgnoreEmptyLines;
            lineEndToken = LineEndToken;
            emptyLineToken = EmptyLineToken;
        }

        /// <summary>Compute shattering options' hash code.</summary>
        /// <param name="stringComparer">Comparer used for comparing strings for equality (actually, for retrieving the hash code).</param>
        /// <returns>Hash code of the options.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="stringComparer" /> is <c>null</c>.</exception>
        public virtual Int32 GetHashCode(IEqualityComparer<String?> stringComparer)
        {
            if (stringComparer is null)
            {
                throw new ArgumentNullException(nameof(stringComparer), StringComparerNullErrorMessage);
            }

            int hashCode = 7;

            hashCode = 31 * hashCode + IgnoreEmptyTokens.GetHashCode();
            hashCode = 31 * hashCode + IgnoreLineEnds.GetHashCode();
            hashCode = 31 * hashCode + IgnoreEmptyLines.GetHashCode();
            hashCode = 31 * hashCode + stringComparer.GetHashCode(lineEndToken);
            hashCode = 31 * hashCode + stringComparer.GetHashCode(EmptyLineToken);

            return hashCode;
        }

        /// <summary>Compute shattering options' hash code.</summary>
        /// <returns>Hash code of the options.</returns>
        public override Int32 GetHashCode() =>
            GetHashCode(EqualityComparer<String?>.Default);

        /// <summary>Compare shattering options to another shattering options for equality.</summary>
        /// <param name="other">Another instance of <see cref="ShatteringOptions" />.</param>
        /// <param name="stringComparer">Comparer used for comparing strings for equality.</param>
        /// <returns>If shattering options are equal according to all relevant values, <c>true</c>; <c>false</c>otherwise.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="stringComparer" /> is <c>null</c>.</exception>
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

        /// <summary>Compare shattering options to another shattering options for equality.</summary>
        /// <param name="other">Another instance of <see cref="ShatteringOptions" />.</param>
        /// <returns>If shattering options are equal according to all relevant values, <c>true</c>; <c>false</c>otherwise.</returns>
        public virtual Boolean Equals(ShatteringOptions? other) =>
            Equals(other, EqualityComparer<String?>.Default);

        /// <summary>Compare shattering options to another <see cref="Object" /> for equality.</summary>
        /// <param name="obj">Another <see cref="Object" />.</param>
        /// <returns>If <paramref name="obj" /> is also shattering options and the shattering options are equal according to all relevant values, <c>true</c>; <c>false</c>otherwise.</returns>
        public override bool Equals(Object? obj)
        {
            try
            {
                return !(obj is null) && Equals((ShatteringOptions)obj);
            }
            catch (InvalidCastException)
            {
            }

            return false;
        }

        /// <summary>Populate serialisation info with data needed to serialise the options.</summary>
        /// <param name="info">Serialisation info to populate with data.</param>
        /// <param name="context">Destination for this serialisation.</param>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="info" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Exceptions thrown by <paramref name="info" />'s methods (most notably <see cref="SerializationException" />) are not caught.
        ///     </para>
        /// </remarks>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
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

        /// <summary>Clone shattering options.</summary>
        /// <returns>New instance of <see cref="ShatteringOptions" /> with the same values.</returns>
        public virtual ShatteringOptions Clone() =>
            new ShatteringOptions(this);

        /// <summary>Clone shattering options.</summary>
        /// <returns>Boxed new instance of <see cref="ShatteringOptions" /> with the same values.</returns>
        Object ICloneable.Clone() =>
            Clone();
    }
}
