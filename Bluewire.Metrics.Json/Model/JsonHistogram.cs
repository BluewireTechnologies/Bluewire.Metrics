namespace Bluewire.Metrics.Json.Model
{
    public class JsonHistogram
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public double LastValue { get; set; }
        public double Min { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public double Median { get; set; }
        public double Percentile75 { get; set; }
        public double Percentile95 { get; set; }
        public double Percentile98 { get; set; }
        public double Percentile99 { get; set; }
        public double Percentile999 { get; set; }
        public int SampleSize { get; set; }

        public string LastUserValue { get; set; }
        public string MinUserValue { get; set; }
        public string MaxUserValue { get; set; }

        public string Unit { get; set; }
        public string[] Tags { get; set; }
    }
}
