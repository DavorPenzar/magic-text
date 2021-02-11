using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RandomText
{
    /// <summary>
    ///     <para>
    ///         Interface for tokenising (<em>shattering</em> into tokens) input texts.
    ///     </para>
    /// </summary>
    public interface ITokeniser
    {
        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens synchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Stream for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The method should return the same enumerable of tokens as <see cref="ShatterAsync(StreamReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(StreamReader input, ShatteringOptions? options = null);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Stream for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <returns>Task whose result is enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <remarks>
        ///     <para>
        ///         The method should ultimately return the same enumerable of tokens as <see cref="ShatterAsync(StreamReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public Task<IEnumerable<String?>> ShatterAsync(StreamReader input, ShatteringOptions? options = null);
    }
}
