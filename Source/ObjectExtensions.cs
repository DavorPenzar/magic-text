using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>
    ///     <para>
    ///         Useful extension methods used in the library.
    ///     </para>
    /// </summary>
    internal static class ObjectExtensions
    {
        private const string ValuesNullErrorMessage = "Value enumerable may not be `null`.";
        private const string ActionNullErrorMessage = "Action to apply may not be `null`.";

        /// <summary>
        ///     <para>
        ///         Retrieve the index of a value.
        ///     </para>
        /// </summary>
        /// <param name="values">Enumerable of values amongst which <paramref name="x" /> should be found. Even if <paramref name="values" /> are comparable, the enumerable does not have to be sorted.</param>
        /// <param name="x">Value to find.</param>
        /// <param name="comparer">Comparer used for comparing instances of type <typeparamref name="T" /> for equality. If <c>null</c>, <see cref="EqualityComparer{T}.Default" /> is used.</param>
        /// <returns>Minimal index <c>i</c> such that <c><paramref name="comparer" />.Equals(<paramref name="values" />[i], <paramref name="x" />)</c>, or <c>-1</c> if <paramref name="x" /> is not found amongst <paramref name="values" /> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" /> method calls).</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="values" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         In order to find <paramref name="x" /> amongst <paramref name="values" />, enumerable <paramref name="values" /> must be enumerated. If and when the first occurance of <paramref name="x" /> is found, the enumeration is terminated.
        ///     </para>
        /// </remarks>
        public static Int32 IndexOf<T>(this IEnumerable<T> values, T x, IEqualityComparer<T>? comparer = null)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values), ValuesNullErrorMessage);
            }

            if (comparer is null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            int index = -1;

            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                for (int i = 0; enumerator.MoveNext(); ++i)
                {
                    if (comparer.Equals(enumerator.Current, x))
                    {
                        index = i;

                        break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        ///     <para>
        ///         Retrieve the index of a value asynchronously.
        ///     </para>
        /// </summary>
        /// <param name="values">Asynchronous enumerable of values amongst which <paramref name="x" /> should be found. Even if <paramref name="values" /> are comparable, the enumerable does not have to be sorted.</param>
        /// <param name="x">Value to find.</param>
        /// <param name="comparer">Comparer used for comparing instances of type <typeparamref name="T" /> for equality. If <c>null</c>, <see cref="EqualityComparer{T}.Default" /> is used.</param>
        /// <param name="cancellationToken">Cancellation token passed to <paramref name="values" /> (via <see cref="TaskAsyncEnumerableExtensions.WithCancellation{T}(IAsyncEnumerable{T}, CancellationToken)" /> extension method).</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> and <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension methods).</param>
        /// <returns>Task that represents the operation of retrieving the index. The value of <see cref="Task{TResult}.Result" /> is minimal index <c>i</c> such that <c><paramref name="comparer" />.Equals(<paramref name="values" />[i], <paramref name="x" />)</c>, or <c>-1</c> if <paramref name="x" /> is not found amongst <paramref name="values" /> (read indexers as <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" /> method calls).</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="values" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         In order to find <paramref name="x" /> amongst <paramref name="values" />, enumerable <paramref name="values" /> must be enumerated. If and when the first occurance of <paramref name="x" /> is found, the enumeration is terminated.
        ///     </para>
        ///
        ///     <para>
        ///         Usually the default <c>false</c> value of <paramref name="continueTasksOnCapturedContext" /> is desirable as it may optimise the asynchronous enumeration process. However, in some cases only the original context might have required access rights to used resources (<paramref name="values" />), and thus <paramref name="continueTasksOnCapturedContext" /> should be set to <c>true</c> to avoid errors.
        ///     </para>
        /// </remarks>
        public async static Task<Int32> IndexOfAsync<T>(this IAsyncEnumerable<T> values, T x, IEqualityComparer<T>? comparer = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values), ValuesNullErrorMessage);
            }

            if (comparer is null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            int index = -1;

            await using (ConfiguredCancelableAsyncEnumerable<T>.Enumerator enumerator = values.WithCancellation(cancellationToken).ConfigureAwait(continueTasksOnCapturedContext).GetAsyncEnumerator())
            {
                for (int i = 0; await enumerator.MoveNextAsync(); ++i)
                {
                    if (comparer.Equals(enumerator.Current, x))
                    {
                        index = i;

                        break;
                    }
                }
            }

            return index;
        }

        /// <summary>
        ///     <para>
        ///         Apply <paramref name="action" /> on an object and return it.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of the object on which <paramref name="action" /> should be applied.</typeparam>
        /// <param name="this">Object on which <paramref name="action" /> should be applied.</param>
        /// <param name="action">Action to apply on object <paramref name="this" />.</param>
        /// <returns><paramref name="this" /></returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="action" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         It is not checked whether <paramref name="this" /> is <c>null</c> or not before passing it to <paramref name="action" />.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <paramref name="action" /> are not caught.
        ///     </para>
        /// </remarks>
        public static T ApplyAction<T>(this T @this, Action<T> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action), ActionNullErrorMessage);
            }

            action(@this);

            return @this;
        }

        /// <summary>
        ///     <para>
        ///         Apply asynchronous <paramref name="action" /> on an object and return it.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of the object on which <paramref name="action" /> should be applied.</typeparam>
        /// <param name="this">Object on which <paramref name="action" /> should be applied.</param>
        /// <param name="action">Action to apply on object <paramref name="this" />.</param>
        /// <param name="continueTaskOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s should be marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method).</param>
        /// <returns>Task that represents the operation of applying <paramref name="action" /> on object <paramref name="this" />. The value of <see cref="Task{TResult}.Result" /> is object <paramref name="this" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="action" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         It is not checked whether <paramref name="this" /> is <c>null</c> or not before passing it to <paramref name="action" />.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <paramref name="action" /> are not caught.
        ///     </para>
        /// </remarks>
        public static async Task<T> ApplyActionAsync<T>(this T @this, Func<T, Task> action, Boolean continueTaskOnCapturedContext)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action), ActionNullErrorMessage);
            }

            await action(@this).ConfigureAwait(continueTaskOnCapturedContext);

            return @this;
        }

        /// <summary>
        ///     <para>
        ///         Apply asynchronous <paramref name="action" /> on an object and return it.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of the object on which <paramref name="action" /> should be applied.</typeparam>
        /// <param name="this">Object on which <paramref name="action" /> should be applied.</param>
        /// <param name="action">Action to apply on object <paramref name="this" />.</param>
        /// <returns>Task that represents the operation of applying <paramref name="action" /> on object <paramref name="this" />. The value of <see cref="Task{TResult}.Result" /> is object <paramref name="this" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="action" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         Continutation of all internal <see cref="Task" />s accessible to the method are marshalled back to the original context (via <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method). However, this does not include internal <see cref="Task" />s of <paramref name="action" />.
        ///     </para>
        ///
        ///     <para>
        ///         It is not checked whether <paramref name="this" /> is <c>null</c> or not before passing it to <paramref name="action" />.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <paramref name="action" /> are not caught.
        ///     </para>
        /// </remarks>
        public static async Task<T> ApplyActionAsync<T>(this T @this, Func<T, Task> action) =>
            await ApplyActionAsync(@this, action, true).ConfigureAwait(true);

        /// <summary>
        ///     <para>
        ///         Apply asynchronous <paramref name="action" /> on an object and return it.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">Type of the object on which <paramref name="action" /> should be applied.</typeparam>
        /// <param name="this">Object on which <paramref name="action" /> should be applied.</param>
        /// <param name="action">Action to apply on object <paramref name="this" />.</param>
        /// <returns>Task that represents the operation of applying <paramref name="action" /> on object <paramref name="this" />. The value of <see cref="Task{TResult}.Result" /> is object <paramref name="this" />.</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="action" /> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>
        ///         It is not checked whether <paramref name="this" /> is <c>null</c> or not before passing it to <paramref name="action" />.
        ///     </para>
        ///
        ///     <para>
        ///         Exceptions thrown by <paramref name="action" /> are not caught.
        ///     </para>
        /// </remarks>
        public static async Task<T> ApplyActionAsync<T>(this T @this, Func<T, ConfiguredTaskAwaitable> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action), ActionNullErrorMessage);
            }

            await action(@this);

            return @this;
        }
    }
}
