using System;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public class WmiMetricProjectionFactory
    {
        public IWmiMetricProjection Create(WmiMetricProjectionDefinition definition)
        {
            switch (definition.Type)
            {
                case WmiMetricType.String:
                    return new WmiStringMetricProjection
                    {
                        Name = InterpretStringFieldOrConstant(definition.Name),

                        Value = InterpretStringFieldOrConstant(definition.Value),
                        Tag = InterpretStringFieldOrConstant(definition.Tag),
                        Context = InterpretStringFieldOrConstant(definition.Context),

                        Filter  = CreateFilter(definition.Filter)
                    };

                case WmiMetricType.Number:
                    return new WmiNumericMetricProjection
                    {
                        Name = InterpretStringFieldOrConstant(definition.Name),
                        Unit = definition.Unit,

                        Value = InterpretNumericFieldOrConstant(definition.Value),
                        Tag = InterpretStringFieldOrConstant(definition.Tag),
                        Context = InterpretStringFieldOrConstant(definition.Context),

                        Filter  = CreateFilter(definition.Filter)
                    };

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public StringExpression InterpretStringFieldOrConstant(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return default(StringExpression);
            if (value.StartsWith(":")) return new StringExpression(value.Substring(1), true);
            return new StringExpression(value, false);
        }

        public NumericExpression InterpretNumericFieldOrConstant(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return default(NumericExpression);
            if (value.StartsWith(":")) return new NumericExpression(value.Substring(1), true);
            return new NumericExpression(value, false);
        }

        public FilterExpression CreateFilter(WmiMetricProjectionDefinition.FilterDefinition definition)
        {
            if (definition == null) return default(FilterExpression);
            return new FilterExpression(InterpretStringFieldOrConstant(definition.Field), definition.Value);
        }
    }
}
