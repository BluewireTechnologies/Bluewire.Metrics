using System;
using System.IO;

namespace ReshapeMetrics
{
    public class OutputToDirectoryHierarchy : IOutputDescriptor
    {
        private readonly string root;

        public OutputToDirectoryHierarchy(string root)
        {
            if (!Path.IsPathRooted(root)) throw new ArgumentException($"Not an absolute path: {root}");
            this.root = root;
        }

        public IOutput GetOutputFor(string relativePath, EnvironmentLookup environment)
        {
            return new Impl(Path.Combine(root, relativePath));
        }

        class Impl : IOutput
        {
            private readonly string targetPath;
            private readonly string targetDirectory;

            private Stream stream;
            private TextWriter writer;

            public Impl(string targetPath)
            {
                this.targetPath = targetPath;
                targetDirectory = Path.GetDirectoryName(targetPath);
                if (targetDirectory == null) throw new ArgumentException($"Unable to get directory information for the target path: {targetPath}");
            }

            public TextWriter GetWriter()
            {
                if (writer == null)
                {
                    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                    stream = File.Open(targetPath, FileMode.Create);
                    writer = new StreamWriter(stream);
                }
                return writer;
            }

            public void Dispose()
            {
                writer?.Flush();
                writer?.Dispose();
                writer = null;
                stream?.Dispose();
                stream = null;
            }
        }

        public void Dispose()
        {
        }
    }
}
