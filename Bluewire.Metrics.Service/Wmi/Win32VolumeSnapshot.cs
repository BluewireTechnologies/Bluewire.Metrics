namespace Bluewire.Metrics.Service.Wmi
{
    public class Win32VolumeSnapshot
    {
        public string SerialNumber { get; set; }
        public string MountPoint { get; set; }
        public long? CapacityBytes { get; set; }
        public long? FreeSpaceBytes { get; set; }
        public string Label { get; set; }
    }
}
