using System;
using System.Collections.Generic;
using System.Linq;
using Bluewire.Common.Console;
using log4net.Core;

namespace ReshapeMetrics
{
    public class Arguments : IArgumentList, IVerbosityArgument
    {
        public OutputTargetType OutputTargetType { get; private set; } 
        public string OutputDirectory { get; private set; }

        public IList<string> ArgumentList { get; } = new List<string>();
        public bool PrettyPrint { get; set; }
        public bool UnwrapArchives { get; set; }
        public char? SanitiseKeysCharacter { get; private set; }

        private readonly IEnumerable<Level> logLevels = new List<Level> { Level.Warn, Level.Info, Level.Debug };
        private int logLevel = 1;

        public Level Verbosity
        {
            get { return logLevels.ElementAtOrDefault(logLevel) ?? Level.All; }
        }
            
        public void Verbose()
        {
            logLevel++;
        }

        public void SanitiseKeys(char? character = null)
        {
            SanitiseKeysCharacter = character ?? '-';
        }

        public void UseFileSystem(string path)
        {
            if (OutputTargetType != default(OutputTargetType)) throw new InvalidArgumentsException("Cannot specify multiple output destinations.");
            OutputTargetType = OutputTargetType.FileSystem;
            OutputDirectory = path;
        }
    }
}
