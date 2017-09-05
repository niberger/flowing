using System;
using Xunit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Flowing;

namespace Tests
{
    public class FlowTest
    {
        IEnumerable<double> randomList = new List<double> {1.0, 3.5, Math.PI, 2.2, 2.2, Double.Epsilon, Double.NegativeInfinity, 5.6, 2.2, -9.1, 0.0, 0.0, Double.PositiveInfinity, 3.2};
        IEnumerable<double> randomListWithoutRepetition => randomList.Distinct();

        [Fact]
        public void flow_enumerable_without_repetition()
        {
            IEnumerable<double> source = randomListWithoutRepetition;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Subscribe(x => target.Add(x));
            Assert.Equal(source, target);
        }

        [Fact]
        public void flow_enumerable_with_repetition()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Subscribe(x => target.Add(x));
            Assert.Equal(source, target);
        }

        [Fact]
        public void flow_one_pending_per_value()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            var complexFlow = from x in flow from y in flow select x + y;
            int counter = 0;
            complexFlow.Subscribe(x => ++counter, e => {}, () => --counter);
            Assert.Equal(0, counter);
        }

        [Fact]
        public void may_flow_exceptions()
        {
            Func<double,double> f = x => (x > 0)? x : throw new NotImplementedException();

            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            int n_values = 0;
            int n_exceptions = 0;
            flow.Select(f).Subscribe(x => ++n_values, e => ++n_exceptions);
            Assert.Equal(n_values + n_exceptions, source.Count());
        }

        [Fact]
        public void can_recover_from_exceptions()
        {
            Func<double,double> f = x => (x > 0)? x : throw new NotImplementedException();
            
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Select(f).Select(x => 0.0, e => (e is NotImplementedException)? 0.0 : throw e).Subscribe(x => target.Add(x));
            Assert.Equal(source.Select(x => 0.0), target);
        }

        [Fact]
        public void throw_not_recovered_exceptions()
        {
            Func<double,double> f = x => (x > 0)? x : throw new NotImplementedException();

            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            bool catched = false;
            try
            {
                flow.Select(f).Subscribe(x => {});
            }
            catch(NotImplementedException)
            {
                catched = true;
            }
            Assert.True(catched);
        }

        [Fact]
        public void merge_two_flows()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();

            var sum = from x in flow
                from y in flow
                select x + y;
            
            var target = new List<double> {};
            sum.Subscribe(x => target.Add(x));

            Assert.Equal(target.Count(), source.Count());
            Assert.Equal(target.Last(), 2*source.Last());
        }

        [Fact]
        public void merge_three_flows()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();

            var sum = from x in flow
                from y in flow
                from z in flow
                select x + y + z;
            
            var target = new List<double> {};
            sum.Subscribe(x => target.Add(x));

            Assert.Equal(target.Count(), source.Count());
            Assert.Equal(target.Last(), 3*source.Last());
        }
    }
}
