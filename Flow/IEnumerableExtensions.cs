using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Flowing 
{
    public static class IEnumerableExtensions
    {
        public static IFlow<IEnumerable<T>> ToFlow<T>(this IEnumerable<IFlow<T>> flowCollection)
        {
            Func<IFlow<IEnumerable<T>>, IFlow<T>, IFlow<IEnumerable<T>>> innerConcat = (IFlow<IEnumerable<T>> collFlow, IFlow<T> flow)
                => from coll in collFlow
                    from val in flow
                    select coll.Concat(new List<T>{val});
            return flowCollection.Aggregate(Flow.Return<IEnumerable<T>>(new T[]{}), innerConcat);
        }
    }
}