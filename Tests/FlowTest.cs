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
        public void FlowEnumerableWithoutRepetition()
        {
            IEnumerable<double> source = randomListWithoutRepetition;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Subscribe(x => target.Add(x));
            Assert.Equal(source, target);
        }

        [Fact]
        public void FlowEnumerableWithRepetition()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Subscribe(x => target.Add(x));
            Assert.Equal(source, target);
        }

        [Fact]
        public void FlowOnePendingPerValue()
        {
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            int counter = 0;
            flow.Subscribe(x => ++counter, e => {}, () => --counter);
            Assert.Equal(counter, 0);
        }

        [Fact]
        public void MayFlowExceptions()
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
        public void CanRecoverFromExceptions()
        {
            Func<double,double> f = x => (x > 0)? x : throw new NotImplementedException();
            
            IEnumerable<double> source = randomList;
            var flow = source.ToObservable().ToFlow();
            var target = new List<double> {};
            flow.Select(f).Select(x => 0.0, e => (e is NotImplementedException)? 0.0 : throw e).Subscribe(x => target.Add(x));
            Assert.Equal(source.Select(x => 0.0), target);
        }

        [Fact]
        public void ThrowNotRecoveredExceptions()
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
    }
}
