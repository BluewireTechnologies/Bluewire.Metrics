using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;
using Metrics.MetricData;
using NUnit.Framework;

namespace Bluewire.Metrics.Specialised.UnitTests
{
    [TestFixture]
    public class MeterRatioMeterTests
    {
        class PingContext : IDisposable
        {
            private MetricsContext context = Metric.Context(Guid.NewGuid().ToString());
            public MockClock Clock { get; } = new MockClock();
            public Meter Numerator { get; }
            public Meter Denominator { get; }
            public Meter Ratio { get; }

            public MeterValue RatioValue => ValueReader.GetCurrentValue(Ratio);

            public PingContext()
            {
                Numerator = new MeterMetric(Clock, Clock.CreateScheduler());
                Denominator = new MeterMetric(Clock, Clock.CreateScheduler());
                Ratio = context.MeterRatioMeter("MeterRatioMeter", "Marks", Numerator, Denominator);
            }

            public void Dispose()
            {
                context?.Dispose();
                context = null;
            }
        }

        [Test]
        public async Task NumeratorMark_With_NoDenominatorMark_Yields_ValueOfNaN()
        {
            var testContext = new PingContext();

            testContext.Numerator.Mark();
            await testContext.Clock.AdvanceOneMinute();

            Assert.That(
                testContext.RatioValue,
                Is.EqualTo(new MeterValue(1, double.NaN, double.NaN, double.NaN, double.NaN, TimeUnit.Seconds, new MeterValue.SetItem[0]))
                    .Using(new RateMeterValueEqualityComparer()));
        }

        [Test]
        public async Task OneNumeratorMark_With_OneDenominatorMark_Yields_ValueOfOne()
        {
            var testContext = new PingContext();

            testContext.Numerator.Mark();
            testContext.Denominator.Mark();
            await testContext.Clock.AdvanceOneMinute();

            Assert.That(
                testContext.RatioValue,
                Is.EqualTo(new MeterValue(1, 1, 1, 1, 1, TimeUnit.Seconds, new MeterValue.SetItem[0]))
                    .Using(new RateMeterValueEqualityComparer()));
        }

        [Test]
        public async Task OneNumeratorMark_With_TwoDenominatorMarks_Yields_ValueOfOneHalf()
        {
            var testContext = new PingContext();

            testContext.Numerator.Mark();
            testContext.Denominator.Mark();
            testContext.Denominator.Mark();
            await testContext.Clock.AdvanceOneMinute();

            Assert.That(
                testContext.RatioValue,
                Is.EqualTo(new MeterValue(1, 0.5, 0.5, 0.5, 0.5, TimeUnit.Seconds, new MeterValue.SetItem[0]))
                    .Using(new RateMeterValueEqualityComparer()));
        }

        [Test]
        public async Task OneNamedNumeratorMark_With_OneMatchingNamedDenominatorMark_AndOneUnnamedDenominatorMark_Yields_NamedValueOfOne()
        {
            var testContext = new PingContext();

            testContext.Numerator.Mark("A");
            testContext.Denominator.Mark();
            testContext.Denominator.Mark("A");
            await testContext.Clock.AdvanceOneMinute();

            var value = testContext.RatioValue.Items.SingleOrDefault(i => i.Item == "A").Value;
            Assert.That(
                value,
                Is.EqualTo(new MeterValue(1, 1, 1, 1, 1, TimeUnit.Seconds, new MeterValue.SetItem[0]))
                    .Using(new RateMeterValueEqualityComparer()));
        }

        [Test]
        public async Task NamedNumeratorMark_With_NoNamedDenominatorMark_Yields_NoValue()
        {
            var testContext = new PingContext();

            testContext.Numerator.Mark("A");
            await testContext.Clock.AdvanceOneMinute();

            var values = testContext.RatioValue.Items.Where(i => i.Item == "A").ToArray();
            Assert.That(values, Is.Empty, values.FirstOrDefault().Value?.MeanRate.ToString());
        }

        [Test]
        public async Task NamedDenominatorMark_With_NoNamedNumeratorMark_Yields_NoValue()
        {
            var testContext = new PingContext();

            testContext.Denominator.Mark("A");
            await testContext.Clock.AdvanceOneMinute();

            var values = testContext.RatioValue.Items.Where(i => i.Item == "A").ToArray();
            Assert.That(values, Is.Empty, values.FirstOrDefault().Value?.MeanRate.ToString());
        }


        [Test]
        public void UsageTest()
        {
            var context = Metric.Context("Test");
            var numerator = context.Meter("Numerator", "Pings");
            var denominator = context.Meter("Denominator", "Pongs");
            var ratio = context.MeterRatioMeter("MeterRatioMeter", "Pings per Pong", numerator, denominator);
        }
    }
}
