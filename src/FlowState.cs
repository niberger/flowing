using System;

namespace Flowing
{
    public interface IFlowState<out T>
    {
    }

    public class Value<T> : IFlowState<T>
    {
        public Value(T val)
        {
            Val = val;
        }

        public T Val { get; }
    }

    public class Error<T> : IFlowState<T>
    {
        public Exception Ex;

        public Error(Exception ex)
        {
            Ex = ex;
        }
    }

    public class Pending<T> : IFlowState<T>
    {
        //all Pending<T> are the same
        public override bool Equals(object obj)
        {
            return obj is Pending<T>;
        }

        public override int GetHashCode()
        {
            return typeof(T).GetHashCode();
        }
    }

    public static class FlowState
    {
        public static IFlowState<TResult> SelectMany<TSource, TResult>(this IFlowState<TSource> state,
            Func<TSource, IFlowState<TResult>> selector)
        {
            return state.SelectMany(selector, e => new Error<TResult>(e));
        }

        public static IFlowState<TResult> SelectMany<TSource, TFlow, TResult>(this IFlowState<TSource> source,
            Func<TSource, IFlowState<TFlow>> flowSelector, Func<TSource, TFlow, TResult> resultSelector)
        {
            return source.SelectMany(s => flowSelector(s).Select(t => resultSelector(s, t)));
        }

        public static IFlowState<TResult> SelectMany<TSource, TResult>(this IFlowState<TSource> state,
            Func<TSource, IFlowState<TResult>> valueSelector,
            Func<Exception, IFlowState<TResult>> errorSelector)
        {
            switch (state)
            {
                case Value<TSource> val:
                    try
                    {
                        return valueSelector(val.Val);
                    }
                    catch (Exception e)
                    {
                        return new Error<TResult>(e);
                    }

                case Error<TSource> err:
                    try
                    {
                        return errorSelector(err.Ex);
                    }
                    catch (Exception e)
                    {
                        return new Error<TResult>(e);
                    }

                default:
                    return new Pending<TResult>();
            }
        }

        public static IFlowState<TResult> Select<TSource, TResult>(this IFlowState<TSource> state,
            Func<TSource, TResult> valueSelector,
            Func<Exception, TResult> errorSelector)
        {
            return state.SelectMany(t => new Value<TResult>(valueSelector(t)),
                e => new Value<TResult>(errorSelector(e)));
        }

        public static IFlowState<TResult> Select<TSource, TResult>(this IFlowState<TSource> state,
            Func<TSource, TResult> selector)
        {
            return state.SelectMany(t => new Value<TResult>(selector(t)));
        }

        public static IFlowState<T> Flatten<T>(this IFlowState<IFlowState<T>> state)
        {
            return state.SelectMany(t => t);
        }
    }
}
