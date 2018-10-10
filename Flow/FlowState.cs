using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;


namespace Flowing
{
    public interface IFlowState<out T>{ }
    public class Value<T>: IFlowState<T>
    {
        public Value(T val)
        {
            Val = val;
        }
        public T Val { get; }
    }
    public class Error<T>: IFlowState<T>
    {
        public Error(Exception ex)
        {
            Ex = ex;
        }
        public Exception Ex;
    }
    public class Pending<T>: IFlowState<T> 
    {
        //all Pending<T> are the same
        public override bool Equals(object obj)
            => obj is Pending<T>;
        public override int GetHashCode()
            => typeof(T).GetHashCode();
    }
    public static class FlowState
    {
        public static IFlowState<S> SelectMany<T,S>(this IFlowState<T> state, Func<T,IFlowState<S>> selector)
            => state.SelectMany(selector, e => new Error<S>(e));

        public static IFlowState<TResult> SelectMany<TSource, TFlow, TResult>(this IFlowState<TSource> source, Func<TSource, IFlowState<TFlow>> flowSelector, Func<TSource, TFlow, TResult> resultSelector)
            => source.SelectMany(s => flowSelector(s).Select(t => resultSelector(s, t)));

        public static IFlowState<S> SelectMany<T,S>(this IFlowState<T> state, Func<T,IFlowState<S>> valueSelector, Func<Exception, IFlowState<S>> errorSelector)
        {
            switch(state)
            {
                case Value<T> val:
                    try
                    {
                        return valueSelector(val.Val);
                    }
                    catch(Exception e)
                    {
                        return new Error<S>(e);
                    }
                case Error<T> err:
                    try
                    {
                        return errorSelector(err.Ex);
                    }
                    catch(Exception e)
                    {
                        return new Error<S>(e);
                    }
                default:
                    return new Pending<S>();
            }
        }
        public static IFlowState<S> Select<T,S>(this IFlowState<T> state, Func<T,S> valueSelector, Func<Exception,S> errorSelector) 
            => state.SelectMany(t => new Value<S>(valueSelector(t)), e => new Value<S>(errorSelector(e)));
        public static IFlowState<S> Select<T,S>(this IFlowState<T> state, Func<T,S> selector) 
            => state.SelectMany(t => new Value<S>(selector(t)));
        public static IFlowState<T> Flatten<T>(this IFlowState<IFlowState<T>> state) 
            => state.SelectMany(t => t);
    }
}