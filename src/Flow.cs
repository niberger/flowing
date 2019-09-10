using System;
using System.Reactive.Linq;

namespace Flowing
{
    public interface IFlow<out T>
    {
        IObservable<IFlowState<T>> StateObs { get; }
    }

    internal class Flow<T> : IFlow<T>
    {
        internal Flow(IObservable<IFlowState<T>> stateObs) => StateObs = stateObs;

        public IObservable<IFlowState<T>> StateObs { get; }
    }

    public static class Flow
    {
        /// <summary>
        ///     create a basic flow, always equal to the given value
        /// </summary>
        public static IFlow<T> Return<T>(T value) => new Flow<T>(Observable.Return(new Value<T>(value)));

        /// <summary>
        ///     create a basic flow, always in error state
        /// </summary>
        public static IFlow<T> Error<T>(Exception error) => new Flow<T>(Observable.Return(new Error<T>(error)));

        /// <summary>
        ///     create a basic flow, always in pending state
        /// </summary>
        public static IFlow<T> Pending<T>() => new Flow<T>(Observable.Return(new Pending<T>()));

        public static IObservable<T> ToObservable<T>(this IFlow<T> flow) =>
            flow.StateObs.OfType<Value<T>>().Select(val => val.Val);

        internal static IFlow<T> Flatten<T>(this IFlowState<IFlow<T>> state)
        {
            switch (state)
            {
                case Value<IFlow<T>> val:
                    return val.Val;
                case Error<IFlow<T>> err:
                    return Error<T>(err.Ex);
                default:
                    return Pending<T>();
            }
        }

        /// <summary>
        /// Transform a flow of flow into a flattened flow.
        /// Only the last states of the most recent flow are pulsed in the result.
        /// Each time a new flow is received, unsubscribe from the older flow.
        /// </summary>
        public static IFlow<T> Flatten<T>(this IFlow<IFlow<T>> flow) => new Flow<T>(flow.StateObs
            .Select(state => state.Flatten().StateObs).Switch().DistinctUntilChanged());

        /// <summary>
        /// Apply a function to each values pulsed in the flow and recover from exception pulsed.
        /// </summary>
        public static IFlow<TResult> Select<TSource, TResult>(this IFlow<TSource> flow,
            Func<TSource, TResult> valueSelector,
            Func<Exception, TResult> errorSelector) =>
            new Flow<TResult>(flow.StateObs.Select(t => t.Select(valueSelector, errorSelector)));

        /// <summary>
        /// Apply a function to each values pulsed in the flow.
        /// The exceptions and the pending state are kept intact.
        /// </summary>
        public static IFlow<TResult>
            Select<TSource, TResult>(this IFlow<TSource> flow, Func<TSource, TResult> selector) =>
            new Flow<TResult>(flow.StateObs.Select(t => t.Select(selector)));

        public static IFlow<TResult> SelectMany<TSource, TResult>(this IFlow<TSource> source,
            Func<TSource, IFlow<TResult>> resultSelector) =>
            source.Select(resultSelector).Flatten();

        public static IFlow<TResult> SelectMany<TSource, TFlow, TResult>(this IFlow<TSource> source,
            Func<TSource, IFlow<TFlow>> flowSelector, Func<TSource, TFlow, TResult> resultSelector) =>
            source.SelectMany(s => flowSelector(s).Select(t => resultSelector(s, t)));

        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext, Action<Exception> onError,
            Action onPending)
        {
            void StateObsOnNext(IFlowState<T> s)
            {
                switch (s)
                {
                    case Value<T> val:
                        onNext(val.Val);
                        break;
                    case Error<T> err:
                        onError(err.Ex);
                        break;
                    case Pending<T> _:
                        onPending();
                        break;
                }
            }

            return flow.StateObs.Subscribe(StateObsOnNext);
        }

        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext, Action<Exception> onError) =>
            flow.Subscribe(onNext, onError, () => { });

        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext) =>
            flow.Subscribe(onNext, ex => throw ex, () => { });
    }
}
