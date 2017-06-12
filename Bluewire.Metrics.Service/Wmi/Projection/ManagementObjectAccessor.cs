using System.Management;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public struct ManagementObjectAccessor : IManagementObjectAccessor
    {
        private readonly ManagementBaseObject instance;

        public ManagementObjectAccessor(ManagementBaseObject instance)
        {
            this.instance = instance;
        }

        public object Get(string fieldName)
        {
            return instance[fieldName];
        }
    }
}
