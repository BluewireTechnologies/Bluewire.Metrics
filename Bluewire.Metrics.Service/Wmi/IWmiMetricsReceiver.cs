using Metrics;

namespace Bluewire.Metrics.Service.Wmi
{
    public interface IWmiMetricsReceiver
    {
        void AddString(string context, string name, string value);
        void AddNumber(string context, string name, double value, Unit unit, MetricTags tags);
    }
}
