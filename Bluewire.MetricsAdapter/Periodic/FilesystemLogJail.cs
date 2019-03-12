using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bluewire.MetricsAdapter.Periodic
{
    public class FilesystemLogJail : ILogJail
    {
        private readonly ILogArchiver archiver;
        private readonly RelativePathMapper pathMapper;

        public FilesystemLogJail(string basePath, ILogArchiver archiver)
        {
            this.archiver = archiver;
            pathMapper = new RelativePathMapper(basePath);
        }

        private const string ArchiveFileName = "archived.zip";

        public async Task Archive(string subdirectoryName)
        {
            var subdirFullPath = pathMapper.GetFullPath(subdirectoryName);
            var archivePath = Path.Combine(subdirFullPath, ArchiveFileName);
            if (File.Exists(archivePath)) return;

            var zipMapper = new RelativePathMapper(subdirFullPath);
            var tempFilePath = pathMapper.GetFullPath(Guid.NewGuid().ToString());

            var sourceFiles = Directory.GetFiles(zipMapper.Root, "*", SearchOption.AllDirectories);
            if (!sourceFiles.Any())
            {
                Delete(subdirectoryName);
                return;
            }
            try
            {
                using (var archiveStream = File.Create(tempFilePath))
                {
                    await archiver.Archive(archiveStream, sourceFiles.Select(f => new LogArchivePart(zipMapper.RemoveRoot(f), f)));
                }
                File.Move(tempFilePath, archivePath);
                foreach (var file in sourceFiles) File.Delete(file);
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }

        public ITextLogInstance Create(string subdirectoryName, string fileName)
        {
            var logContainer = pathMapper.GetFullPath(subdirectoryName);
            return new Instance(logContainer, fileName, Path.Combine(subdirectoryName, fileName));
        }

        public void Delete(string subdirectoryName)
        {
            var logContainer = pathMapper.GetFullPath(subdirectoryName);
            Directory.Delete(logContainer, true);
        }

        public IEnumerable<string> GetSubdirectories()
        {
            return Directory.EnumerateDirectories(pathMapper.Root, "*", SearchOption.AllDirectories).Select(pathMapper.RemoveRoot);
        }

        class Instance : ITextLogInstance
        {
            private readonly string logContainer;
            public string Name { get; }
            private readonly string filePath;
            private TextWriter writer;
            private Stream stream;

            public Instance(string logContainer, string fileName, string logName)
            {
                this.logContainer = logContainer;
                this.Name = logName;
                filePath = Path.Combine(logContainer, fileName);
            }

            public bool Exists => File.Exists(filePath);

            private void Acquire()
            {
                if (!Directory.Exists(logContainer)) Directory.CreateDirectory(logContainer);
                if (stream == null) stream = File.Open(filePath, FileMode.CreateNew, FileAccess.Write);
                if (writer == null) writer = new StreamWriter(stream);
            }

            public TextWriter GetWriter()
            {
                Acquire();
                return writer;
            }

            public void Dispose()
            {
                writer?.Dispose();
                writer = null;
                stream?.Dispose();
                stream = null;
            }
        }
    }
}
