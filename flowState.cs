using System;
using System.Reactive;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;


namespace Flowing
{
    public interface IFlowState<T>{ }
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
    public class Pending<T>: IFlowState<T> {}
    public static class FlowState
    {
        public static IFlowState<S> SelectMany<T,S>(this IFlowState<T> state, Func<T,IFlowState<S>> selector)
        {
            switch(state)
            {
                case Value<T> val:
                    return selector(val.Val);
                case Error<T> err:
                    return new Error<S>(err.Ex);
                default:
                    return new Pending<S>();
            }
        }
        public static IFlowState<S> Select<T,S>(this IFlowState<T> state, Func<T,S> selector) => state.SelectMany(t => new Value<S>(selector(t)));
        public static IFlowState<T> Flatten<T>(this IFlowState<IFlowState<T>> state) => state.SelectMany(t => t);
    }
}