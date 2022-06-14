using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MagicText.Internal
{
    /// <summary>Exposes the negation of a simple (one argument) predicate.</summary>
    /// <typeparam name="T">The type of the argument of the predicate.</typeparam>
    /// <remarks>
    ///     <para>Predicates are expressed through <see cref="Func{T, TResult}" /> <c>delegate</c>s that return a <see cref="Boolean" /> rather than through built-in <see cref="Predicate{T}" /> <c>delegate</c>s. This is because of the intended use of the <see cref="NegativePredicateWrapper{T}" /> as a parameter to <a href="http://docs.microsoft.com/en-gb/dotnet/csharp/programming-guide/concepts/linq/">LINQ</a> methods such as the <see cref="Enumerable.Any{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, Boolean})" /> and <see cref="Enumerable.Where{TSource}(System.Collections.Generic.IEnumerable{TSource}, Func{TSource, Boolean})" /> methods.</para>
    /// </remarks>
    internal class NegativePredicateWrapper<T> : Object
    {
        private const string PositivePredicateNullErrorMessage = "Positive predicate cannot be null.";
        
        /// <summary>Wraps a <see cref="NegativePredicateWrapper{T}" /> around a predicate.</summary>
        /// <param name="func">The predicate that is wrapped by the resulting <see cref="NegativePredicateWrapper{T}" />.</param>
        /// <returns>A <see cref="NegativePredicateWrapper{T}" /> around the <c><paramref name="func" /></c>.</returns>
        /// <remarks>
        ///     <para>The conversion creates and returns a new <see cref="NegativePredicateWrapper{T}" /> around the <c><paramref name="func" /></c>.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("func")]
        public static explicit operator NegativePredicateWrapper<T>([AllowNull] Func<T, Boolean> func) =>
            func is null ? null! : new NegativePredicateWrapper<T>(func);

        /// <summary>Retrieves the negation of the predicate wrapped by a <see cref="NegativePredicateWrapper{T}" />.</summary>
        /// <param name="negativePredicateWrapper">The <see cref="NegativePredicateWrapper{T}" /> around a positive predicate.</param>
        /// <returns>The negation of the predicate wrapped by the <c><paramref name="negativePredicateWrapper" /></c>.</returns>
        /// <remarks>
        ///     <para>The conversion is essentially the same as simply using the <see cref="NegativePredicate" /> property of the <c><paramref name="negativePredicateWrapper" /></c>.</para>
        /// </remarks>
        [return: MaybeNull, NotNullIfNotNull("negativePredicateWrapper")]
        public static implicit operator Func<T, Boolean>([AllowNull] NegativePredicateWrapper<T> negativePredicateWrapper) =>
            negativePredicateWrapper is null ? null! : negativePredicateWrapper.NegativePredicate;

        private readonly Func<T, Boolean> _positivePredicate;
        private readonly Func<T, Boolean> _negativePredicate;

        /// <summary>Gets the predicate that is negated through the <see cref="InvokeNegatively(T)" /> method.</summary>
        /// <returns>The internal wrapped predicate.</returns>
        public Func<T, Boolean> PositivePredicate => _positivePredicate;

        /// <summary>Gets the predicate that is a negation of the <see cref="PositivePredicate" />.</summary>
        /// <returns>A negation of the internal wrapped predicate (<see cref="PositivePredicate" />).</returns>
        /// <remarks>
        ///     <para>This <c>delegate</c> merely encapsulates the <see cref="InvokeNegatively(T)" /> method.</para>
        /// </remarks>
        public Func<T, Boolean> NegativePredicate => _negativePredicate;

        /// <summary>Creates a negative wrapper of the <c><paramref name="positivePredicate" /></c>.</summary>
        /// <param name="positivePredicate">The predicate that is negated through the <see cref="InvokeNegatively(T)" /> method.</param>
        /// <exception cref="ArgumentNullException">The <c><paramref name="positivePredicate" /></c> parameter is <c>null</c>.</exception>
        public NegativePredicateWrapper(Func<T, Boolean> positivePredicate) : base()
        {
            _positivePredicate = positivePredicate ?? throw new ArgumentNullException(nameof(positivePredicate), PositivePredicateNullErrorMessage);
            _negativePredicate = InvokeNegatively;
        }

        /// <summary>Negates the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" />.</summary>
        /// <param name="arg">The parameter to evaluate.</param>
        /// <returns>The <see cref="Boolean" /> negation (<c>true</c> to <c>false</c> and vice versa) of the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" />. Simply put, the method returns <c>!<see cref="PositivePredicate" />(<paramref name="arg" />)</c>.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="PositivePredicate" /> delegate call are not caught.</para>
        /// </remarks>
        public Boolean InvokeNegatively(T arg) =>
            !PositivePredicate(arg);
    }
}
