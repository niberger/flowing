using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Flowing
{
    public static class ObservableExtensions
    {
        /// <summary>
        ///     Create a flow from an observable of values
        /// </summary>
        public static IFlow<T> ToFlow<T>(this IObservable<T> obs)
        {
            var stateObs = obs.SelectMany(x =>
                new List<IFlowState<T>> {new Pending<T>(), new Value<T>(x)}.ToObservable());
            return new Flow<T>(stateObs);
        }
    }
}