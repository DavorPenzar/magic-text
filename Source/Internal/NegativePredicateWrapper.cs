using System;

namespace MagicText.Internal
{
    /// <summary>Exposes the negation of a simple (one argument) predicate.</summary>
    /// <typeparam name="T">The type of the argument of the predicate.</typeparam>
    internal class NegativePredicateWrapper<T> : Object
    {
        private const string PositivePredicateNullErrorMessage = "Positive predicate cannot be null.";

        private static readonly Func<T, Boolean> _defaultPositivePredicate;

        /// <summary>Gets the default positive predicate to wrap by a default <see cref="NegativePredicateWrapper{T}" />.</summary>
        /// <returns>The default positive predicate.</returns>
        /// <remarks>
        ///     <para>The <see cref="DefaultPositivePredicate" /> always evaluates arguments as <c>false</c>.</para>
        ///     <para>This property is intended for internal purposes only, to be used in <em>default</em> cases.</para>
        /// </remarks>
        protected static Func<T, Boolean> DefaultPositivePredicate => _defaultPositivePredicate;

        /// <summary>Initialises static fields.</summary>
        static NegativePredicateWrapper()
        {
            _defaultPositivePredicate = PredicateAlwaysFalse;
        }

        /// <summary>Always evaluates the argument as <c>false</c>.</summary>
        /// <param name="_">The parameter to evaluate. This parameter is unused.</param>
        /// <returns>Always <c>false</c>.</returns>
        /// <remarks>
        ///     <para>This method is intended for internal purposes only, to be used in <em>default</em> cases.</para>
        /// </remarks>
        protected static Boolean PredicateAlwaysFalse(T _) =>
            false;

        private readonly Func<T, Boolean> _positivePredicate;

        /// <summary>Gets the predicate that is negated through the <see cref="EvaluateNegation(T)" /> method.</summary>
        /// <returns>The internal wrapped predicate.</returns>
        public Func<T, Boolean> PositivePredicate => _positivePredicate;

        /// <summary>Creates a negative wrapper of the <c><paramref name="positivePredicate" /></c>.</summary>
        /// <param name="positivePredicate">The predicate that is negated through the <see cref="EvaluateNegation(T)" /> method.</param>
        /// <exception cref="ArgumentNullException">The parameter <c><paramref name="positivePredicate" /></c> is <c>null</c>.</exception>
        public NegativePredicateWrapper(Func<T, Boolean> positivePredicate) : base()
        {
            _positivePredicate = positivePredicate ?? throw new ArgumentNullException(nameof(positivePredicate), PositivePredicateNullErrorMessage);
        }

        /// <summary>Creates a default negative wrapper of a positive predicate.</summary>
        /// <remarks>
        ///     <para>The resulting <see cref="NegativePredicateWrapper{T}" /> shall always return <c>true</c> when evaluating parameters via the <see cref="EvaluateNegation(T)" /> method. In other words, the underlying <see cref="PositivePredicate" /> always evaluates parameters as <c>false</c>.</para>
        /// </remarks>
        public NegativePredicateWrapper() : this(DefaultPositivePredicate)
        {
        }

        /// <summary>Negates the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" /> delegate.</summary>
        /// <param name="arg">The parameter to evaluate.</param>
        /// <returns>The <see cref="Boolean" /> negation (<c>true</c> to <c>false</c> and vice versa) of the evaluation of the <c><paramref name="arg" /></c> via the <see cref="PositivePredicate" /> delegate. Simply put, the method returns <c>!<see cref="PositivePredicate" />(<paramref name="arg" />)</c>.</returns>
        /// <remarks>
        ///     <para>The exceptions thrown by the <see cref="PositivePredicate" /> delegate call are not caught.</para>
        /// </remarks>
        public Boolean EvaluateNegation(T arg) =>
            !PositivePredicate(arg);
    }
}
