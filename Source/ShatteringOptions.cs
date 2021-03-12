using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Options for <see cref="ITokeniser.Shatter(TextReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(TextReader, ShatteringOptions?)" /> methods.
    ///     </para>
    /// </summary>
    public class ShatteringOptions : IEquatable<ShatteringOptions>, ICloneable
    {
        private const string OtherNullErrorMessage = "Shattering options to copy may not be `null`.";
        protected const string StringComparerNullErrorMessage = "String comparer may not be `null`.";

        public static Boolean operator ==(ShatteringOptions? left, ShatteringOptions? right) =>
            left is null ? right is null : left.Equals(right);

        public static Boolean operator !=(ShatteringOptions? left, ShatteringOptions? right) =>
            !(left == right);

        private bool ignoreEmptyTokens;
        private bool ignoreLineEnds;
        private bool ignoreEmptyLines;

        private String? lineEndToken;
        private String? emptyLineToken;

        /// <summary>
        ///     <para>
        ///         Default is <c>false</c>.
        ///     </para>
        /// </summary>
        /// <returns>Indicator if empty tokens should be ignored: <c>true</c> if ignoring, <c>false</c> otherwise.</returns>
        /// <value>New indicator value.</value>
        /// <remarks>
        ///     <para>
        ///         Actual implementations of <see cref="ITokeniser" /> interface may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a string yielding <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method.
        ///     </para>
        /// </remarks>
        [DefaultValue(false)]
        public Boolean IgnoreEmptyTokens
        {
            get => ignoreEmptyTokens;
            set
            {
                ignoreEmptyTokens = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Default is <c>false</c>.
        ///     </para>
        /// </summary>
        /// <returns>Indicator if line ends should be ignored: <c>true</c> if ignoring, <c>false</c> otherwise.</returns>
        /// <value>New indicator value.</value>
        /// <remarks>
        ///     <para>
        ///         If <c>false</c>, line ends should be represented by <see cref="LineEndToken" />s.
        ///     </para>
        ///
        ///     <para>
        ///         Line ends should be considered both the new line character (CR, LF and CRLF) and the end of the input.
        ///     </para>
        /// </remarks>
        [DefaultValue(false)]
        public Boolean IgnoreLineEnds
        {
            get => ignoreLineEnds;
            set
            {
                ignoreLineEnds = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Default is <c>false</c>.
        ///     </para>
        /// </summary>
        /// <returns>Inidcator if empty lines should be ignored, i. e. not produce any tokens: <c>true</c> if ignoring, <c>false</c> otherwise.</returns>
        /// <value>New indicator value.</value>
        /// <remarks>
        ///     <para>
        ///         If <c>true</c>, empty lines should not produce even <see cref="LineEndToken" />; if <c>false</c>, they should be represented by <see cref="EmptyLineToken" />s.
        ///     </para>
        ///
        ///     <para>
        ///         Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.
        ///     </para>
        /// </remarks>
        [DefaultValue(false)]
        public Boolean IgnoreEmptyLines
        {
            get => ignoreEmptyLines;
            set
            {
                ignoreEmptyLines = true;
            }
        }

        /// <summary>
        ///     <para>
        ///         Default is <see cref="Environment.NewLine" />.
        ///     </para>
        /// </summary>
        /// <returns>Token to represent a line end.</returns>
        /// <value>New token value.</value>
        /// <remarks>
        ///     <para>
        ///         Line ends should be considered both the new line character (CR, LF and CRLF) and the end of the input.
        ///     </para>
        ///
        ///     <para>
        ///         If a line is discarded as empty (if <see cref="IgnoreEmptyLines" /> is <c>true</c>), it should not produce <see cref="LineEndToken" />.
        ///     </para>
        /// </remarks>
        [DefaultValue("\n")] // <-- this may be different from `Environment.NewLine`
        public String? LineEndToken
        {
            get => lineEndToken;
            set
            {
                lineEndToken = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Default is <see cref="String.Empty" />.
        ///     </para>
        /// </summary>
        /// <returns>Token to represent an empty line.</returns>
        /// <value>New token value.</value>
        /// <remarks>
        ///     <para>
        ///         If a line produces no tokens (after ignoring empty tokens if <see cref="IgnoreEmptyTokens" /> is <c>true</c>) but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it should be represented by <see cref="EmptyLineToken" />. This should be done even if <see cref="EmptyLineToken" /> would be considered an empty token and empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>).
        ///     </para>
        /// </remarks>
        [DefaultValue("")]
        public String? EmptyLineToken
        {
            get => emptyLineToken;
            set
            {
                emptyLineToken = value;
            }
        }

        /// <summary>
        ///     <para>
        ///         Create default shattering options.
        ///     </para>
        /// </summary>
        public ShatteringOptions() : this(false, false, false, Environment.NewLine, String.Empty)
        {
        }

        /// <summary>
        ///     <para>
        ///         Copy shattering options.
        ///     </para>
        /// </summary>
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

        /// <summary>
        ///     <para>
        ///         Create specified shattering options.
        ///     </para>
        /// </summary>
        /// <param name="ignoreEmptyTokens">Indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">Indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">Inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">Token to represent a line end.</param>
        /// <param name="emptyLineToken">Token to represent an empty line.</param>
        public ShatteringOptions(Boolean ignoreEmptyTokens, Boolean ignoreLineEnds, Boolean ignoreEmptyLines, String? lineEndToken, String? emptyLineToken)
        {
            this.ignoreEmptyTokens = ignoreEmptyTokens;
            this.ignoreLineEnds = ignoreLineEnds;
            this.ignoreEmptyLines = ignoreEmptyLines;
            this.lineEndToken = lineEndToken;
            this.emptyLineToken = emptyLineToken;
        }

        /// <summary>
        ///     <para>
        ///         Deconstruct shattering options.
        ///     </para>
        /// </summary>
        /// <param name="ignoreEmptyTokens">Indicator if empty tokens should be ignored.</param>
        /// <param name="ignoreLineEnds">Indicator if line ends should be ignored.</param>
        /// <param name="ignoreEmptyLines">Inidcator if empty lines should be ignored.</param>
        /// <param name="lineEndToken">Token to represent a line end.</param>
        /// <param name="emptyLineToken">Token to represent an empty line.</param>
        public void Deconstruct(out Boolean ignoreEmptyTokens, out Boolean ignoreLineEnds, out Boolean ignoreEmptyLines, out String? lineEndToken, out String? emptyLineToken)
        {
            ignoreEmptyTokens = IgnoreEmptyTokens;
            ignoreLineEnds = IgnoreLineEnds;
            ignoreEmptyLines = IgnoreEmptyLines;
            lineEndToken = LineEndToken;
            emptyLineToken = EmptyLineToken;
        }

        /// <summary>
        ///     <para>
        ///         Compute shattering options' hash code.
        ///     </para>
        /// </summary>
        /// <returns>Hash code.</returns>
        public override Int32 GetHashCode()
        {
            int hashCode = 7;

            hashCode = 31 * hashCode + IgnoreEmptyTokens.GetHashCode();
            hashCode = 31 * hashCode + IgnoreLineEnds.GetHashCode();
            hashCode = 31 * hashCode + IgnoreEmptyLines.GetHashCode();
            hashCode = 31 * hashCode + (lineEndToken is null ? 0 : lineEndToken.GetHashCode());
            hashCode = 31 * hashCode + (emptyLineToken is null ? 0 : emptyLineToken.GetHashCode());

            return hashCode;
        }

        /// <summary>
        ///     <para>
        ///         Compare shattering options to another shattering options for equality.
        ///     </para>
        /// </summary>
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

        /// <summary>
        ///     <para>
        ///         Compare shattering options to another shattering options for equality.
        ///     </para>
        /// </summary>
        /// <param name="other">Another instance of <see cref="ShatteringOptions" />.</param>
        /// <returns>If shattering options are equal according to all relevant values, <c>true</c>; <c>false</c>otherwise.</returns>
        public virtual Boolean Equals(ShatteringOptions? other) =>
            Equals(other, EqualityComparer<String?>.Default);

        /// <summary>
        ///     <para>
        ///         Compare shattering options to another <see cref="Object" /> for equality.
        ///     </para>
        /// </summary>
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

        /// <summary>
        ///     <para>
        ///         Clone shattering options.
        ///     </para>
        /// </summary>
        /// <returns>New instance of <see cref="ShatteringOptions" /> with the same values.</returns>
        public virtual Object Clone() =>
            new ShatteringOptions(this);
    }
}
