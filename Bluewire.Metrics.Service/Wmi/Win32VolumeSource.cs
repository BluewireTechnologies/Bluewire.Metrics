using System.Collections.Generic;
using System.Management;

namespace Bluewire.Metrics.Service.Wmi
{
    public class Win32VolumeSource
    {
        public Win32VolumeSnapshot[] Query()
        {
            var scope = new ManagementScope(@"root\cimv2", new ConnectionOptions { Impersonation = ImpersonationLevel.Impersonate });
            var query = new ObjectQuery("select * from Win32_Volume");
            var searcher = new ManagementObjectSearcher(scope, query);
            using (var results = searcher.Get())
            {
                var snapshots = new List<Win32VolumeSnapshot>();
                foreach (var row in results)
                {
                    snapshots.Add(Map(row));
                }
                return snapshots.ToArray();
            }
        }

        private Win32VolumeSnapshot Map(ManagementBaseObject row)
        {
            return new Win32VolumeSnapshot {
                MountPoint = new MountPointSanitiser().Sanitise(row["Name"]?.ToString()),
                SerialNumber = row["SerialNumber"]?.ToString(),
                CapacityBytes = Util.GetAsInt64OrNull(row["Capacity"]),
                FreeSpaceBytes = Util.GetAsInt64OrNull(row["FreeSpace"]),
                Label = row["Label"]?.ToString()
            };
        }
    }
}
