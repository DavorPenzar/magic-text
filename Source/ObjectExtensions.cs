using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MagicText
{
    /// <summary>Provides useful extension methods used in the library.</summary>
    internal static class ObjectExtensions
    {
        private const string ValuesNullErrorMessage = "The value enumerable may not be `null`.";
        private const string ActionNullErrorMessage = "The action to apply may not be `null`.";

        /// <summary>Retrieves the index of the value <c><paramref name="x" /></c> amongst the <c><paramref name="values" /></c>.</summary>
        /// <param name="values">The enumerable of values amongst which <c><paramref name="x" /></c> should be found.</param>
        /// <param name="x">The value to find.</param>
        /// <param name="comparer">The comparer used for comparing instances of type <typeparamref name="T" /> for equality. If <c>null</c>, the <see cref="EqualityComparer{T}.Default" /> is used.</param>
        /// <returns>The minimal index <c>i</c> such that <c><paramref name="comparer" />.Equals(<paramref name="values" />[i], <paramref name="x" />)</c>, or <c>-1</c> if <c><paramref name="x" /></c> is not found amongst the <c><paramref name="values" /></c> (read the indexer as the <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" /> extension method call).</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="values" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>In order to find <c><paramref name="x" /></c> amongst the <c><paramref name="values" /></c>, the enumerable <c><paramref name="values" /></c> must be enumerated. If and when the first occurance of <c><paramref name="x" /></c> is found, the enumeration is terminated.</para>
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

            Int32 index = -1;

            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                for (Int32 i = 0; enumerator.MoveNext(); ++i)
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

        /// <summary>Retrieves the index of the value <c><paramref name="x" /></c> amongst the <c><paramref name="values" /></c> asynchronously.</summary>
        /// <param name="values">The asynchronous enumerable of values amongst which <c><paramref name="x" /></c> should be found.</param>
        /// <param name="x">The value to find.</param>
        /// <param name="comparer">The comparer used for comparing instances of type <typeparamref name="T" /> for equality. If <c>null</c>, the <see cref="EqualityComparer{T}.Default" /> is used.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s accessible to the method should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> and <see cref="TaskAsyncEnumerableExtensions.ConfigureAwait(IAsyncDisposable, Boolean)" /> extension methods).</param>
        /// <returns>The value task that represents the asynchronous index retrieval operation. The value of the <see cref="ValueTask{TResult}.Result" /> property is the minimal index <c>i</c> such that <c><paramref name="comparer" />.Equals(<paramref name="values" />[i], <paramref name="x" />)</c>, or <c>-1</c> if <c><paramref name="x" /></c> is not found amongst the <c><paramref name="values" /></c> (read the indexer as the <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource}, Int32)" /> extension method call).</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="values" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>In order to find <c><paramref name="x" /></c> amongst the <c><paramref name="values" /></c>, the asynchronous enumerable <c><paramref name="values" /></c> must be enumerated. If and when the first occurance of <c><paramref name="x" /></c> is found, the enumeration is terminated.</para>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous index finding process. However, in some cases only the original context might have the required access right to the resources used (<c><paramref name="values" /></c>), and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        /// </remarks>
        public async static ValueTask<Int32> IndexOfAsync<T>(this IAsyncEnumerable<T> values, T x, IEqualityComparer<T>? comparer = null, CancellationToken cancellationToken = default, Boolean continueTasksOnCapturedContext = false)
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

        /// <summary>Applies the <c><paramref name="action" /></c> on the <c><paramref name="obj" /></c> and returns the <c><paramref name="obj" /></c>.</summary>
        /// <typeparam name="T">The type of the <c><paramref name="obj" /></c> on which the <c><paramref name="action" /></c> should be applied.</typeparam>
        /// <param name="obj">The object on which the <c><paramref name="action" /></c> should be applied.</param>
        /// <param name="action">The action to apply.</param>
        /// <returns>The <c><paramref name="obj" /></c> after having applied the <c><paramref name="action" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="action" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>It is not checked whether the <c><paramref name="obj" /></c> is <c>null</c> or not before passing it to the <c><paramref name="action" /></c>.</para>
        ///     <para>The exceptions thrown by the <c><paramref name="action" /></c> call are not caught.</para>
        /// </remarks>
        public static T ApplyAction<T>(this T obj, Action<T> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action), ActionNullErrorMessage);
            }

            action(obj);

            return obj;
        }

        /// <summary>Applies the <c><paramref name="asyncAction" /></c> on the <c><paramref name="obj" /></c> and returns the <c><paramref name="obj" /></c>.</summary>
        /// <typeparam name="T">The type of the <c><paramref name="obj" /></c> on which the <c><paramref name="asyncAction" /></c> should be applied.</typeparam>
        /// <param name="obj">The object on which the <c><paramref name="asyncAction" /></c> should be applied.</param>
        /// <param name="asyncAction">The asynchronous action to apply.</param>
        /// <param name="continueTasksOnCapturedContext">If <c>true</c>, the continuation of all internal <see cref="Task" />s accessible to the method should be marshalled back to the original context (via the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method). However, this does not affect the internal <see cref="Task" />s of the <c><paramref name="asyncAction" /></c>.</param>
        /// <returns>The task that represents the asynchronous operation of applying the <c><paramref name="asyncAction" /></c> on the <c><paramref name="obj" /></c>. The value of the <see cref="Task{TResult}.Result" /> property is the <c><paramref name="obj" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="asyncAction" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>Usually the default <c>false</c> value of the <c><paramref name="continueTasksOnCapturedContext" /></c> is desirable as it may optimise the asynchronous action applying process. However, in some cases only the original context might have the required access right to the resources used, and thus <c><paramref name="continueTasksOnCapturedContext" /></c> should be set to <c>true</c> to avoid errors.</para>
        ///     <para>It is not checked whether the <c><paramref name="obj" /></c> is <c>null</c> or not before passing it to the <c><paramref name="asyncAction" /></c>.</para>
        ///     <para>The exceptions thrown by the <c><paramref name="asyncAction" /></c> are not caught.</para>
        /// </remarks>
        public static async Task<T> ApplyActionAsync<T>(this T obj, Func<T, Task> asyncAction, Boolean continueTasksOnCapturedContext)
        {
            if (asyncAction is null)
            {
                throw new ArgumentNullException(nameof(asyncAction), ActionNullErrorMessage);
            }

            await asyncAction(obj).ConfigureAwait(continueTasksOnCapturedContext);

            return obj;
        }

        /// <summary>Applies the <c><paramref name="asyncAction" /></c> on the <c><paramref name="obj" /></c> and returns the <c><paramref name="obj" /></c>.</summary>
        /// <typeparam name="T">The type of the <c><paramref name="obj" /></c> on which the <c><paramref name="asyncAction" /></c> should be applied.</typeparam>
        /// <param name="obj">The object on which the <c><paramref name="asyncAction" /></c> should be applied.</param>
        /// <param name="asyncAction">The asynchronous action to apply.</param>
        /// <returns>The task that represents the asynchronous operation of applying the <c><paramref name="asyncAction" /></c> on the <c><paramref name="obj" /></c>. The value of the <see cref="Task{TResult}.Result" /> property is the <c><paramref name="obj" /></c>.</returns>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="asyncAction" /></c> is <c>null</c>.</exception>
        /// <remarks>
        ///     <para>No continutation of any internal <see cref="Task" /> accessible to the method is necessarily marshalled back to the original context (a <c>false</c> value is passed to the <see cref="Task{TResult}.ConfigureAwait(Boolean)" /> extension method). However, this does not include the internal <see cref="Task" />s of the <c><paramref name="asyncAction" /></c>.</para>
        ///     <para>It is not checked whether the <c><paramref name="obj" /></c> is <c>null</c> or not before passing it to the <c><paramref name="asyncAction" /></c>.</para>
        ///     <para>The exceptions thrown by the <c><paramref name="asyncAction" /></c> are not caught.</para>
        /// </remarks>
        public static async Task<T> ApplyActionAsync<T>(this T obj, Func<T, Task> asyncAction) =>
            await ApplyActionAsync(obj, asyncAction, false).ConfigureAwait(false);
    }
}
