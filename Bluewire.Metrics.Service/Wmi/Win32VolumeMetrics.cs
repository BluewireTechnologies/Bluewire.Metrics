using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.Utils;

namespace Bluewire.Metrics.Service.Wmi
{
    public class Win32VolumeMetrics
    {
        private readonly Dictionary<string, Volume> logicalDisks = new Dictionary<string, Volume>();
        private readonly MetricsContext targetContext;
        private Scheduler scheduler;

        public Win32VolumeMetrics(MetricsContext targetContext)
        {
            this.targetContext = targetContext;
            this.scheduler = new ActionScheduler();
            targetContext.Advanced.ContextShuttingDown += (s, e) => Shutdown();
        }

        public void Start(TimeSpan interval)
        {
            scheduler?.Start(interval, Poll);
        }

        public void Shutdown()
        {
            scheduler.Dispose();
            scheduler = null;
        }

        public Task Poll(CancellationToken token)
        {
            var snapshots = new Win32VolumeSource().Query();
            UpdateLogicalDisks(snapshots);
            return Task.CompletedTask;
        }

        private void UpdateLogicalDisks(Win32VolumeSnapshot[] snapshots)
        {
            var removedDisks = new Dictionary<string, Volume>(logicalDisks);
            foreach (var snapshot in snapshots)
            {
                if (String.IsNullOrWhiteSpace(snapshot.MountPoint)) continue;
                removedDisks.Remove(snapshot.MountPoint);
                UpdateLogicalDisk(snapshot);
            }
            foreach (var entry in removedDisks)
            {
                RemoveLogicalDisk(entry.Value);
            }
        }

        private void UpdateLogicalDisk(Win32VolumeSnapshot snapshot)
        {
            Volume knownDisk;
            if (!logicalDisks.TryGetValue(snapshot.MountPoint, out knownDisk))
            {
                // New device.
                AddLogicalDiskFrom(snapshot);
                return;
            }

            if (knownDisk.VolumeSerialNumber == snapshot.SerialNumber)
            {
                // Hot path. Just update existing info about the disk.
                knownDisk.Update(snapshot);
                return;
            }

            // Device identity has changed. Replace the old one.
            RemoveLogicalDisk(knownDisk);
            AddLogicalDiskFrom(snapshot);
        }

        private void AddLogicalDiskFrom(Win32VolumeSnapshot snapshot)
        {
            var newDisk = new Volume(snapshot.MountPoint, snapshot.SerialNumber);
            newDisk.Update(snapshot);
            logicalDisks.Add(snapshot.MountPoint, newDisk);
            var diskContext = targetContext.Context(snapshot.MountPoint);
            newDisk.InitialiseContext(diskContext);
        }

        private void RemoveLogicalDisk(Volume disk)
        {
            targetContext.ShutdownContext(disk.DriveLetter);
            logicalDisks.Remove(disk.DriveLetter);
        }

        class Volume
        {
            private const double BytesPerMegabyte = 1024 * 1024;

            private double capacityMegabytes;
            private double freeSpaceMegabytes;

            public string DriveLetter { get; }
            public string VolumeSerialNumber { get; }

            public Volume(string driveLetter, string volumeSerialNumber)
            {
                DriveLetter = driveLetter;
                VolumeSerialNumber = volumeSerialNumber;
            }

            public void InitialiseContext(MetricsContext context)
            {
                context.Gauge("Capacity", () => capacityMegabytes, Unit.MegaBytes);
                context.Gauge("Free Space", () => freeSpaceMegabytes, Unit.MegaBytes);
                context.Gauge("Free Space %", () => 100 * freeSpaceMegabytes / capacityMegabytes, Unit.Percent);
            }

            public void Update(Win32VolumeSnapshot snapshot)
            {
                capacityMegabytes = snapshot.CapacityBytes / BytesPerMegabyte ?? double.NaN;
                freeSpaceMegabytes = snapshot.FreeSpaceBytes / BytesPerMegabyte ?? double.NaN;
            }
        }
    }
}
