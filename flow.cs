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
        public static IFlow<T> Value<T>(T value) => new Flow<T>(Observable.Return(new Value<T>(value)));
        public static IFlow<T> Error<T>(String error) => new Flow<T>(Observable.Return(new Error<T>(error)));
        public static IFlow<T> Pending<T>() => new Flow<T>(Observable.Return(new Pending<T>()));
        public static IFlow<T> Flatten<T>(this IFlowState<IFlow<T>> state)
        {
            switch(state)
            {
                case Value<IFlow<T>> val:
                    return val.Val;
                case Error<IFlow<T>> err:
                    return Error<T>(err.Msg);
                default:
                    return Pending<T>();
            }
        }
        public static IFlow<S> SelectMany<T, S>(this IFlow<T> source, Func<T, IFlow<S>> resultSelector)
        {
            return new Flow<S>(source.StateObs.Select(state => state.Select(resultSelector).Flatten().StateObs).Switch());
        }
        /*public static IFlow<U> SelectMany<T, S, U>(this IFlow<T> source, Func<T, IEnumerable<S>> collectionSelector, Func<S, IFlow<U>> resultSelector)
        {
            return source.Select(x => collectionSelector(x)).Select(c => c.Select(resultSelector))
        }// */
        public static IFlow<S> Select<T,S>(this IFlow<T> flow, Func<T,S> selector) => new Flow<S>(flow.StateObs.Select(t => t.Select(selector)));
        public static IFlow<T> Flatten<T>(this IFlow<IFlow<T>> flow) => new Flow<T>(flow.StateObs.Select(state => state.Flatten().StateObs).Switch()); 
    }
}