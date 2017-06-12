using System.Collections.Generic;
using System.Management;
using Bluewire.Metrics.Service.Wmi.Projection;

namespace Bluewire.Metrics.Service.Wmi
{
    public class WmiMetricSource
    {
        private readonly ManagementScope scope;
        private readonly ObjectQuery query;

        public WmiMetricSource(string scope, string source)
        {
            this.scope = new ManagementScope(scope, new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
            this.query = new ObjectQuery($"select * from {source}");
        }

        public IEnumerable<IManagementObjectAccessor> Fetch()
        {
            scope.Connect();
            var searcher = new ManagementObjectSearcher(scope, query);
            using (var results = searcher.Get())
            {
                foreach (var row in results)
                {
                    yield return new ManagementObjectAccessor(row);
                }
            }
        }
    }
}
