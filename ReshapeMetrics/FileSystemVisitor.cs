using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace ReshapeMetrics
{
    /// <summary>
    /// Recursively enumerate possible input files, including those in ZIP archives, and
    /// expose their content as TextReaders. Enumeration is forward-only and only the current
    /// file may be read.
    /// </summary>
    /// <remarks>
    /// Do not change this class's properties while using it to explore directory hierarchies.
    /// </remarks>
    public class FileSystemVisitor
    {
        private readonly Options options;

        public FileSystemVisitor() : this(new Options())
        {
        }

        public FileSystemVisitor(Options options)
        {
            this.options = options;
        }

        public IEnumerator<IInputFile> Enumerate(string[] paths)
        {
            foreach (var path in paths)
            {
                if (IsDirectory(path))
                {
                    var directory = new DirectoryInfo(path);
                    foreach (var child in EnumerateDirectory(directory)) yield return child;
                    continue;
                }

                var file = new FileInfo(path);
                foreach (var child in EnumerateFile(file)) yield return child;
            }
        }

        private IEnumerable<IInputFile> EnumerateFile(FileInfo file, string contextPath = "")
        {
            if (IsProbablyZipFile(file.Extension))
            {
                using (var zipStream = file.OpenRead())
                {
                    foreach (var child in EnumerateZipStream(zipStream, file.Name, contextPath)) yield return child;
                }
            }
            else
            {
                yield return new StandardFile(contextPath, file);
            }
        }

        private IEnumerable<IInputFile> EnumerateDirectory(DirectoryInfo directory, string contextPath = "")
        {
            var thisContextPath = Path.Combine(contextPath, directory.Name);
            foreach (var file in directory.EnumerateFiles())
            {
                foreach (var child in EnumerateFile(file, thisContextPath)) yield return child;
            }
            foreach (var subdirectory in directory.EnumerateDirectories())
            {
                foreach (var child in EnumerateDirectory(subdirectory, thisContextPath)) yield return child;
            }
        }

        public IEnumerable<IInputFile> EnumerateZipStream(Stream zipStream, string zipFileName, string contextPath = "")
        {
            var thisContextPath = options.MergeZipFilesWithFolder ? contextPath : Path.Combine(contextPath, zipFileName);
            // The caller owns the underlying Stream so we must not dispose it:
            using (var zipFile = new ZipInputStream(zipStream) { IsStreamOwner = false })
            {
                var entry = zipFile.GetNextEntry();
                while (entry != null)
                {
                    if (entry.IsFile)
                    {
                        if (IsProbablyZipFile(Path.GetExtension(entry.Name)))
                        {
                            foreach (var child in EnumerateZipStream(zipFile, entry.Name, thisContextPath)) yield return child;
                        }
                        else
                        {
                            yield return new FileInZip(thisContextPath, entry, zipFile);
                        }
                    }
                    entry = zipFile.GetNextEntry();
                }
            }
        }

        private static bool IsDirectory(string fullPath)
        {
            return File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory);
        }

        private bool IsProbablyZipFile(string fileExtension)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(fileExtension, ".zip");
        }

        class StandardFile : IInputFile
        {
            private readonly FileInfo file;
            private TextReader reader;

            public StandardFile(string contextPath, FileInfo file)
            {
                this.file = file;
                RelativePath = Path.Combine(contextPath, file.Name);
            }

            public string RelativePath { get; }
            public TextReader GetReader()
            {
                if (reader == null)
                {
                    reader = file.OpenText();
                }
                return reader;
            }

            public void Dispose()
            {
                reader?.Dispose();
                reader = null;
            }
        }

        class FileInZip : IInputFile
        {
            private TextReader reader;

            public FileInZip(string contextPath, ZipEntry file, Stream stream)
            {
                reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true);
                RelativePath = Path.Combine(contextPath, file.Name);
            }

            public string RelativePath { get; }
            public TextReader GetReader() => reader;

            public void Dispose()
            {
                reader = null;
            }
        }

        public struct Options
        {
            public bool MergeZipFilesWithFolder { get; set; }
        }
    }
}
