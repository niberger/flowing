using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;


namespace Flowing
{
    public class IFlow<T>
    {
        internal IFlow(){}
        internal IObservable<IFlowState<T>> StateObs { get; set; }
    }
    internal class Flow<T> : IFlow<T>
    {
        internal Flow(IObservable<IFlowState<T>> stateObs)
        {
            StateObs = stateObs;
        }
    }
    public static class Flow
    {
        /// <summary>
        ///     create a basic flow, always equal to the given value
        /// </summary>
        public static IFlow<T> Return<T>(T value) 
            => new Flow<T>(Observable.Return(new Value<T>(value)));
        /// <summary>
        ///     create a basic flow, always in error state
        /// </summary>
        public static IFlow<T> Error<T>(Exception error) 
            => new Flow<T>(Observable.Return(new Error<T>(error)));
        /// <summary>
        ///     create a basic flow, always in pending state
        /// </summary>
        public static IFlow<T> Pending<T>() 
            => new Flow<T>(Observable.Return(new Pending<T>()));
        /// <summary>
        ///     create a flow from an observable of values
        /// </summary>
        public static IFlow<T> ToFlow<T>(this IObservable<T> obs)
        {
            var stateObs = obs.SelectMany(x => (new List<IFlowState<T>>{new Pending<T>(), new Value<T>(x)}).ToObservable());
            return new Flow<T>(stateObs);

        }
        internal static IFlow<T> Flatten<T>(this IFlowState<IFlow<T>> state)
        {
            switch(state)
            {
                case Value<IFlow<T>> val:
                    return val.Val;
                case Error<IFlow<T>> err:
                    return Error<T>(err.Ex);
                default:
                    return Pending<T>();
            }
        }
        public static IFlow<T> Flatten<T>(this IFlow<IFlow<T>> flow) 
            => new Flow<T>(flow.StateObs.Select(state => state.Flatten().StateObs).Switch().DistinctUntilChanged());
        public static IFlow<S> Select<T,S>(this IFlow<T> flow, Func<T,S> valueSelector, Func<Exception,S> errorSelector) 
            => new Flow<S>(flow.StateObs.Select(t => t.Select(valueSelector, errorSelector)));
        public static IFlow<S> Select<T,S>(this IFlow<T> flow, Func<T,S> selector) 
            => new Flow<S>(flow.StateObs.Select(t => t.Select(selector)));
        public static IFlow<TResult> SelectMany<TSource, TResult>(this IFlow<TSource> source, Func<TSource, IFlow<TResult>> resultSelector)
            => source.Select(resultSelector).Flatten();
        public static IFlow<TResult> SelectMany<TSource, TFlow, TResult>(this IFlow<TSource> source, Func<TSource, IFlow<TFlow>> flowSelector, Func<TSource, TFlow, TResult> resultSelector)
            => source.SelectMany(s => flowSelector(s).Select(t => resultSelector(s, t)));
        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext, Action<Exception> onError, Action onPending)
        {
            Action<IFlowState<T>> stateObsOnNext = s => {
                if(s is Value<T> val)
                    onNext(val.Val);
                else if(s is Error<T> err)
                    onError(err.Ex);
                else if(s is Pending<T>)
                    onPending();
            };

            return flow.StateObs.Subscribe(stateObsOnNext);
        }
        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext, Action<Exception> onError) 
            => flow.Subscribe(onNext, onError, () => {});
        public static IDisposable Subscribe<T>(this IFlow<T> flow, Action<T> onNext) 
            => flow.Subscribe(onNext, (ex) => {throw ex;}, () => {});
    }
}