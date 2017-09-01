using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace Flowing
{
    class Program
    {
        static IFlow<long> Interval(TimeSpan period) =>  new Flow<long>(Observable.Interval(period).Select(l => new Value<long>(l)));
        static long Sum(long x, long y) => x+y;
        static void Main(string[] args)
        {
            var fx = Interval(TimeSpan.FromMilliseconds(100));
            var fy = Interval(TimeSpan.FromMilliseconds(110));
            Console.WriteLine("Hello World!");
            var sum = fx.SelectMany(x => fy.Select(y => Sum(x,y)));
            sum.StateObs.Subscribe(z => {if(z is Value<long> val) Console.WriteLine(val.Val);});
            Thread.Sleep(100000);
        }
    }
}
