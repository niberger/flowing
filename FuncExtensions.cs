using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Flowing 
{
    public static class FuncExtensions
    {
        public static IFlow<T> ToFlow<T>(this Func<T> f) 
            => Flow.Return(f());
        public static IFlow<TResult> ToFlow<T1, TResult>(this Func<T1, TResult> f, IFlow<T1> x1) 
            => x1.Select(f);
        public static IFlow<TResult> ToFlow<T1, T2, TResult>(this Func<T1, T2, TResult> f, IFlow<T1> x1, IFlow<T2> x2) 
            => from v1 in x1 from v2 in x2 select f(v1, v2);
        public static IFlow<TResult> ToFlow<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> f, IFlow<T1> x1, IFlow<T2> x2, IFlow<T3> x3)
            => from v1 in x1 from v2 in x2 from v3 in x3 select f(v1, v2, v3);
    }

}