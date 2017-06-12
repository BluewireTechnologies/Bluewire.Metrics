using System.Diagnostics.Contracts;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public struct FilterExpression
    {
        private readonly StringExpression field;
        private readonly string value;

        public FilterExpression(StringExpression field, string value)
        {
            this.field = field;
            this.value = value;
        }

        [Pure]
        public bool Accept(IManagementObjectAccessor accessor)
        {
            var fieldValue = field.GetString(accessor);
            return fieldValue == (value ?? "");
        }
    }
}