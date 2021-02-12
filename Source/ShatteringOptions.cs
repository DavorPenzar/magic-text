using System;
using System.IO;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Options for <see cref="ITokeniser.Shatter(StreamReader, ShatteringOptions?)" /> and <see cref="ITokeniser.ShatterAsync(StreamReader, ShatteringOptions?)" /> methods.
    ///     </para>
    /// </summary>
    public class ShatteringOptions
    {
        /// <value>Indicator if empty tokens should be ignore. Default is <c>false</c>.</value>
        /// <remarks>
        ///     <para>
        ///         Exact implementations may define what exactly an <em>empty</em> token means, but usually this would be a <c>null</c> or a string yielding <c>true</c> when checked via <see cref="String.IsNullOrEmpty(String)" /> or <see cref="String.IsNullOrWhiteSpace(String)" /> method.
        ///     </para>
        /// </remarks>
        public Boolean IgnoreEmptyTokens { get; set; } = false;

        /// <value>Indicator if empty lines should not produce any tokens. If <c>false</c>, they should be represented by <see cref="LineEndToken" />s. Default is <c>false</c>.</value>
        public Boolean IgnoreLineEnds { get; set; } = false;

        /// <value>Inidcator if empty lines hould be ignored. If <c>true</c>, they should not produce even <see cref="LineEndToken" />; if <c>false</c>, they should be represented by <see cref="EmptyLineToken" />s. Default is <c>false</c>.</value>
        /// <remarks>
        ///     <para>
        ///         Empty lines should be considered those lines that produce no tokens. This should be checked <strong>after</strong> filtering empty tokens out from the line if <see cref="IgnoreEmptyTokens" /> is <c>true</c>.
        ///     </para>
        /// </remarks>
        public Boolean IgnoreEmptyLines { get; set; } = false;

        /// <value>Token to represent the line end. Default is <see cref="Environment.NewLine" />.</value>
        /// <remarks>
        ///     <para>
        ///         Line ends should be considered both the new line character (CR, LF and CRLF) and the end of the input. If a line is discarded as empty (if <see cref="IgnoreEmptyLines" /> is <c>true</c>), it should not produce a <see cref="LineEndToken" />.
        ///     </para>
        /// </remarks>
        public String? LineEndToken { get; set; } = Environment.NewLine;

        /// <value>Token to represent an empty line. Default is <see cref="String.Empty" />.</value>
        /// <remarks>
        ///     <para>
        ///         If a line does not produce any token (after ignoring empty tokens if <see cref="IgnoreEmptyTokens" /> is <c>true</c>) but should not be discarded (if <see cref="IgnoreEmptyLines" /> is <c>false</c>), it should be represented by <see cref="EmptyLineToken" />. This should be done even if <see cref="EmptyLineToken" /> would be considered an empty token and if empty tokens should be ignored (if <see cref="IgnoreEmptyTokens" /> is <c>true</c>).
        ///     </para>
        /// </remarks>
        public String? EmptyLineToken { get; set; } = String.Empty;
    }
}
