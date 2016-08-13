using System;
using System.IO;

// Copied from Bluewire.Common.FileSystem, v9.3.0.

namespace Bluewire.MetricsAdapter.Periodic
{
    public class RelativePathMapper
    {
        public string Root { get; }

        public RelativePathMapper(string root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (!Path.IsPathRooted(root)) throw new ArgumentException("Jail root path must be absolute", nameof(root));
            this.Root = root + Path.DirectorySeparatorChar;
        }

        public string RemoveRoot(string path)
        {
            if (path.StartsWith(this.Root, StringComparison.InvariantCultureIgnoreCase))
            {
                // remove jail root from path.
                path = path.Substring(this.Root.Length);
            }
            // remove drive root if present.
            if (Path.IsPathRooted(path)) path = path.Substring(Path.GetPathRoot(path).Length);
            return path;
        }

        public string GetFullPath(string partialPath)
        {
            var path = Path.GetFullPath(Path.Combine(this.Root, RemoveRoot(partialPath)));

            if (!path.StartsWith(Path.GetFullPath(this.Root), StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Path does not lie within the jail", nameof(partialPath));
            }

            return path;
        }
    }
}
