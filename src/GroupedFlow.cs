using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace Flowing
{

    public interface IGroupedFlow<out TKey, out TSource> : IFlow<TSource>
    {
        /// <summary>Gets the common key.</summary>
        TKey Key { get; }
    }

    internal class GroupedFlow<TKey, TSource> : IGroupedFlow<TKey, TSource>
    {
        public GroupedFlow(TKey key, IObservable<IFlowState<TSource>> stateObs)
        {
            Key = key;
            StateObs = stateObs;
        }

        public TKey Key { get; }

        public IObservable<IFlowState<TSource>> StateObs { get; }
    }

    public static class GroupedFlow
    {
        public static IFlow<IGroupedFlow<TKey, TSource>> GroupBy<TKey, TSource>(this IFlow<TSource> flow, Func<TSource, TKey> keySelector)
        {
            var groupedStateObs = flow.StateObs
                .GroupBy(key => key.Select(keySelector))
                .Select(groupedObservable => groupedObservable.Key.Select(k => new GroupedFlow<TKey, TSource>(k, groupedObservable)));
            return new Flow<IGroupedFlow<TKey, TSource>>(groupedStateObs);
        }

    }
}
