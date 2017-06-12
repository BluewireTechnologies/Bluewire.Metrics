namespace Bluewire.Metrics.Service.Wmi
{
    public interface IManagementObjectAccessor
    {
        object Get(string fieldName);
    }
}
