using System;

namespace Bluewire.Metrics.Service.Wmi
{
    /// <summary>
    /// Convert Windows paths to Unix-like paths, eg. E:\database to /e/database
    /// </summary>
    /// <remarks>
    /// Backslashes are typically used as escape characters and ':' is invalid in Windows
    /// filenames, so this sanitisation should make the mount point information more likely
    /// to survive a trip through different systems and formats.
    /// </remarks>
    public class MountPointSanitiser
    {
        public string Sanitise(string raw)
        {
            if (raw == null) return null;
            var clean = raw.Trim().Replace(":", "").Replace('\\', '/').TrimEnd('/');
            if (clean.StartsWith("//?")) return clean.Substring(3);
            if (clean.StartsWith("/")) return clean;
            return '/' + clean;
        }
    }
}
