using System.Diagnostics;

namespace Metrics.IISApplicationCounters
{
    public readonly struct ProcessRef
    {
        public static ProcessRef GetCurrentProcess()
        {
            var process = Process.GetCurrentProcess();
            return new ProcessRef(process.ProcessName, process.Id);
        }

        public string Name { get; }
        public int Id { get; }

        public ProcessRef(string processName, int processId)
        {
            Name = processName;
            Id = processId;
        }

        public override string ToString() => $"[{Name}, PID: {Id}]";
    }
}
