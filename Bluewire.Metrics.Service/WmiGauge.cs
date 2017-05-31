using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metrics.Core;
using System.Management;

namespace Bluewire.Metrics.Service
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

        public WmiMetricSnapshot Fetch()
        {
            scope.Connect();
            var searcher = new ManagementObjectSearcher(scope, query);
            var results = searcher.Get();
            return new WmiMetricSnapshot(results);
        }
    }

    public class WmiMetricSnapshot
    {
        private readonly ManagementObjectCollection objects;

        public WmiMetricSnapshot(ManagementObjectCollection objects)
        {
            this.objects = objects;
        }
        
    }

    public class WmiMetricSet
    {
        public void Update()
        {

        }
    }
}
