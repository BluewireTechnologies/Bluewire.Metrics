namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public class WmiStringMetricProjection : IWmiMetricProjection
    {
        public StringExpression Name { get; set; }
        
        public StringExpression Value { get; set; }
        public StringExpression Tag { get; set; }
        public StringExpression Context { get; set; }

        public FilterExpression Filter { get; set; }

        public void Project(IManagementObjectAccessor obj, IWmiMetricsReceiver receiver)
        {
            if (!Filter.Accept(obj)) return;
            receiver.AddString(Context.GetString(obj), Name.GetString(obj), Value.GetString(obj));
        }
    }
}
