namespace Bluewire.Metrics.Service.Wmi
{
    public interface IWmiMetricProjection
    {
        void Project(IManagementObjectAccessor objects, IWmiMetricsReceiver receiver);
    }
}
