using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Flowing 
{
    public static class IObsevableExtensions
    {
        /// <summary>
        ///     create a flow from an observable of values
        /// </summary>
        public static IFlow<T> ToFlow<T>(this IObservable<T> obs)
        {
            var stateObs = obs.SelectMany(x => (new List<IFlowState<T>>{new Pending<T>(), new Value<T>(x)}).ToObservable());
            return new Flow<T>(stateObs);

        }
    }
}