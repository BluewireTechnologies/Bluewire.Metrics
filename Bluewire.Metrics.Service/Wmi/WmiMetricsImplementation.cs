using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;
using Metrics.MetricData;
using Metrics.Sampling;
using Metrics.Utils;
using Timer = Metrics.Timer;

namespace Bluewire.Metrics.Service.Wmi
{
    public class WmiMetricsImplementation
    {
        private readonly WmiMetricSource source;
        private readonly IWmiMetricProjection[] projections;
        private readonly string name;
        private readonly WmiMetricsDataProvider dataProvider;

        public WmiMetricsImplementation(
            string name,
            WmiMetricsDataProvider dataProvider,
            WmiMetricSource source,
            IWmiMetricProjection[] projections
            )
        {
            this.name = name;
            this.dataProvider = dataProvider;
            this.source = source;
            this.projections = projections;
        }

        public void AttachToContext(AdvancedMetricsContext context, Scheduler scheduler, TimeSpan pollInterval)
        {
            context.AttachContext(name, new WmiMetricsContext(dataProvider, scheduler));
            scheduler.Start(pollInterval, UpdateMetrics);
        }

        private Task UpdateMetrics(CancellationToken token)
        {
            var receiver = new WmiMetricsReceiver(name);
            foreach (var result in source.Fetch())
            {
                foreach (var projection in projections)
                {
                    projection.Project(result, receiver);
                }
                token.ThrowIfCancellationRequested();
            }
            dataProvider.Update(receiver);

            return Task.CompletedTask;
        }

        class WmiMetricsContext : MetricsContext, AdvancedMetricsContext
        {
            private WmiMetricsDataProvider dataProvider;
            private readonly Scheduler scheduler;

            public WmiMetricsContext(WmiMetricsDataProvider dataProvider, Scheduler scheduler)
            {
                this.dataProvider = dataProvider;
                this.scheduler = scheduler;
            }

            public void Dispose()
            {
                scheduler.Stop();
                scheduler.Dispose();
                if (dataProvider != null)
                {
                    dataProvider = null;
                    this.ContextShuttingDown?.Invoke(this, EventArgs.Empty);
                }
            }

            MetricsContext MetricsContext.Context(string contextName) { throw new NotSupportedException(); }
            MetricsContext MetricsContext.Context(string contextName, Func<string, MetricsContext> contextCreator) { throw new NotSupportedException(); }

            public void ShutdownContext(string contextName)
            {
            }
            public AdvancedMetricsContext Advanced => this;
            public MetricsDataProvider DataProvider => dataProvider;

            public void CompletelyDisableMetrics()
            {
                Dispose();
                this.ContextDisabled?.Invoke(this, EventArgs.Empty);
            }

            public void ResetMetricsValues()
            {
                dataProvider?.Reset();
            }

            public void WithCustomMetricsBuilder(MetricsBuilder metricsBuilder) { }

            public event EventHandler ContextShuttingDown;
            public event EventHandler ContextDisabled;

            void MetricsContext.Gauge(string name, Func<double> valueProvider, Unit unit, MetricTags tags) { throw new NotSupportedException(); }
            void MetricsContext.PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit, MetricTags tags) { throw new NotSupportedException(); }
            Counter MetricsContext.Counter(string name, Unit unit, MetricTags tags) { throw new NotSupportedException(); }
            Meter MetricsContext.Meter(string name, Unit unit, TimeUnit rateUnit, MetricTags tags) { throw new NotSupportedException(); }
            Histogram MetricsContext.Histogram(string name, Unit unit, SamplingType samplingType, MetricTags tags) { throw new NotSupportedException(); }
            Timer MetricsContext.Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) { throw new NotSupportedException(); }

            void AdvancedMetricsContext.Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit, MetricTags tags) { throw new NotSupportedException(); }
            Counter AdvancedMetricsContext.Counter<T>(string name, Unit unit, Func<T> builder, MetricTags tags) { throw new NotSupportedException(); }
            Meter AdvancedMetricsContext.Meter<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit, MetricTags tags) { throw new NotSupportedException(); }
            Histogram AdvancedMetricsContext.Histogram<T>(string name, Unit unit, Func<T> builder, MetricTags tags) { throw new NotSupportedException(); }
            Histogram AdvancedMetricsContext.Histogram(string name, Unit unit, Func<Reservoir> builder, MetricTags tags) { throw new NotSupportedException(); }
            Timer AdvancedMetricsContext.Timer<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) { throw new NotSupportedException(); }
            Timer AdvancedMetricsContext.Timer(string name, Unit unit, Func<HistogramImplementation> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) { throw new NotSupportedException(); }
            Timer AdvancedMetricsContext.Timer(string name, Unit unit, Func<Reservoir> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) { throw new NotSupportedException(); }
            bool AdvancedMetricsContext.AttachContext(string contextName, MetricsContext context) { throw new NotSupportedException(); }
        }
    }
}
