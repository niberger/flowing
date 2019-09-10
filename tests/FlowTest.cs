using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Flowing;
using Xunit;

namespace Tests
{
    public class FlowTest
    {
        private readonly List<double> _randomList = new List<double>
        {
            1.0,
            3.5,
            Math.PI,
            2.2,
            2.2,
            double.Epsilon,
            double.NegativeInfinity,
            5.6,
            2.2,
            -9.1,
            0.0,
            0.0,
            double.PositiveInfinity,
            3.2
        };

        [Fact]
        public void can_recover_from_exceptions()
        {
            double Func(double x) => x > 0 ? x : throw new NotImplementedException();

            var source = _randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double>();
            flow.Select(Func).Select(x => 0.0, e => e is NotImplementedException ? 0.0 : throw e)
                .Subscribe(x => target.Add(x));
            Assert.Equal(source.Select(x => 0.0), target);
        }

        [Fact]
        public void convert_list_to_flow()
        {
            var source = _randomList;
            var flows = Enumerable.Range(0, 5).Select(i => source.ToObservable().ToFlow());
            var flow = flows.ToFlow();

            var target = new List<IEnumerable<double>>();
            flow.Subscribe(x => target.Add(x));

            Assert.True(target.Last().All(x => x.Equals(source.Last())));
        }

        [Fact]
        public void flow_enumerable()
        {
            var source = _randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double>();
            flow.Subscribe(x => target.Add(x));
            Assert.Equal(source, target);
        }

        [Fact]
        public void flow_one_pending_per_value()
        {
            var source = _randomList;
            var flow = source.ToObservable().ToFlow();
            var complexFlow = from x in flow from y in flow select x + y;
            var counter = 0;
            complexFlow.Subscribe(x => ++counter, e => { }, () => --counter);
            Assert.Equal(0, counter);
        }

        [Fact]
        public void may_flow_exceptions()
        {
            double Func(double x) => x > 0 ? x : throw new NotImplementedException();

            var source = _randomList;
            var flow = source.ToObservable().ToFlow();
            var nValues = 0;
            var nExceptions = 0;
            flow.Select(Func).Subscribe(x => ++nValues, e => ++nExceptions);
            Assert.Equal(nValues + nExceptions, source.Count);
        }

        [Fact]
        public void merge_three_flows()
        {
            var source = _randomList;
            var flow = source.ToObservable().ToFlow();

            var sum = from x in flow
                from y in flow
                from z in flow
                select x + y + z;

            var target = new List<double>();
            sum.Subscribe(x => target.Add(x));

            Assert.Equal(target.Count, source.Count);
            Assert.Equal(target.Last(), 3 * source.Last());
        }

        [Fact]
        public void merge_two_flows()
        {
            var source = _randomList;
            var flow = source.ToObservable().ToFlow();

            var sum = from x in flow
                from y in flow
                select x + y;

            var target = new List<double>();
            sum.Subscribe(x => target.Add(x));

            Assert.Equal(target.Count, source.Count);
            Assert.Equal(target.Last(), 2 * source.Last());
        }

        [Fact]
        public void throw_not_recovered_exceptions()
        {
            double Func(double x) => x > 0 ? x : throw new NotImplementedException();

            var source = _randomList;
            var flow = source.ToObservable().ToFlow();
            var catched = false;
            try
            {
                flow.Select(Func).Subscribe(x => { });
            }
            catch (NotImplementedException)
            {
                catched = true;
            }

            Assert.True(catched);
        }
    }
}
