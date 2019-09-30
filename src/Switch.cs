using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flowing
{
    public static class SwitchExtensions
    {
        private static bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant)
            => potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;

        private static IFlow<IGroupedFlow<Type, T>> GroupBySelectedTypes<T>(IFlow<T> flow, IEnumerable<Type> selectedTypes)
            => flow.GroupBy(x => selectedTypes.First(selectedType => IsSameOrSubclass(selectedType, x.GetType())));

        public static IFlow<TResult> Switch<TSource, T1, TResult>(this IFlow<TSource> flow,
            Func<IFlow<T1>, IFlow<TResult>> selector,
            Func<IFlow<TSource>, IFlow<TResult>> defaultSelector)
            where T1 : class, TSource
        {
            var groups = GroupBySelectedTypes(flow, new[] {typeof(T1), typeof(TSource)});
            return groups.Select(group =>
            {
                if (group.Key == typeof(T1))
                {
                    return selector(group.Select(x => x as T1));
                }

                return defaultSelector(group);
            }).Flatten();
        }

        public static IFlow<TResult> Switch<TSource, T1, T2, TResult>(this IFlow<TSource> flow,
            Func<IFlow<T1>, IFlow<TResult>> firstSelector,
            Func<IFlow<T2>, IFlow<TResult>> secondSelector,
            Func<IFlow<TSource>, IFlow<TResult>> defaultSelector)
            where T1 : class, TSource
            where T2 : class, TSource
        {
            var groups = GroupBySelectedTypes(flow, new[] { typeof(T1), typeof(T2), typeof(TSource) });
            return groups.Select(group =>
            {
                if (group.Key == typeof(T1))
                {
                    return firstSelector(group.Select(x => x as T1));
                }
                if (group.Key == typeof(T2))
                {
                    return secondSelector(group.Select(x => x as T2));
                }
                return defaultSelector(group);
            }).Flatten();
        }

        public static IFlow<TResult> Switch<TSource, T1, T2, T3, TResult>(this IFlow<TSource> flow,
            Func<IFlow<T1>, IFlow<TResult>> firstSelector,
            Func<IFlow<T2>, IFlow<TResult>> secondSelector,
            Func<IFlow<T2>, IFlow<TResult>> thirdSelector,
            Func<IFlow<TSource>, IFlow<TResult>> defaultSelector)
            where T1 : class, TSource
            where T2 : class, TSource
            where T3 : class, TSource
        {
            var groups = GroupBySelectedTypes(flow, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(TSource) });
            return groups.Select(group =>
            {
                if (group.Key == typeof(T1))
                {
                    return firstSelector(group.Select(x => x as T1));
                }
                if (group.Key == typeof(T2))
                {
                    return secondSelector(group.Select(x => x as T2));
                }
                if (group.Key == typeof(T3))
                {
                    return thirdSelector(group.Select(x => x as T3));
                }
                return defaultSelector(group);
            }).Flatten();
        }
    }
}
