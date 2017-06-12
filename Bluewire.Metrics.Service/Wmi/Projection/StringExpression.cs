using System;
using System.Diagnostics.Contracts;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public struct StringExpression
    {
        private readonly Func<IManagementObjectAccessor, string> fieldAccessor;

        public StringExpression(string field, bool isFieldReference)
        {
            fieldAccessor = GetStringAccessor(field, isFieldReference);
        }

        [Pure]
        public string GetString(IManagementObjectAccessor accessor)
        {
            return fieldAccessor?.Invoke(accessor);
        }

        [Pure]
        private static Func<IManagementObjectAccessor, string> GetStringAccessor(string field, bool isFieldReference)
        {
            if (isFieldReference) return a => a.Get(field)?.ToString() ?? "";
            return a => field;
        }
    }
}
