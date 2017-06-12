using Metrics;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public class WmiNumericMetricProjection : IWmiMetricProjection
    {
        public StringExpression Name { get; set; }
        public Unit Unit { get; set; }

        public NumericExpression Value { get; set; }
        public StringExpression Tag { get; set; }
        public StringExpression Context { get; set; }

        public FilterExpression Filter { get; set; }

        public void Project(IManagementObjectAccessor obj, IWmiMetricsReceiver receiver)
        {
            if (!Filter.Accept(obj)) return;

            var number = Value.GetNumber(obj);
            receiver.AddNumber(Context.GetString(obj), Name.GetString(obj), number, Unit, Tag.GetString(obj));
        }
    }
}
