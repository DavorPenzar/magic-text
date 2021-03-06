using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MagicText
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
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <returns>Enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The method should return the same enumerable of tokens as <see cref="ShatterAsync(TextReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public IEnumerable<String?> Shatter(TextReader input, ShatteringOptions? options = null);

        /// <summary>
        ///     <para>
        ///         Shatter text read from <paramref name="input" /> into tokens asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="input">Reader for reading the input text.</param>
        /// <param name="options">Shattering options. If <c>null</c>, defaults should be used.</param>
        /// <returns>Task that represents the asynchronous shattering operation. The value of <see cref="Task{TResult}.Result" /> is enumerable of tokens (in the order they were read) read from <paramref name="input" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="input" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         The method should ultimately return the same enumerable of tokens as <see cref="Shatter(TextReader, ShatteringOptions?)" /> method called with the same parameters.
        ///     </para>
        /// </remarks>
        public Task<IEnumerable<String?>> ShatterAsync(TextReader input, ShatteringOptions? options = null);
    }
}
