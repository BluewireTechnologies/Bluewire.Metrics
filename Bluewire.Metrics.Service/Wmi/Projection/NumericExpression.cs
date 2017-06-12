using System;
using System.Diagnostics.Contracts;

namespace Bluewire.Metrics.Service.Wmi.Projection
{
    public struct NumericExpression
    {
        private readonly Func<IManagementObjectAccessor, double> fieldAccessor;

        public NumericExpression(string field, bool isFieldReference)
        {
            fieldAccessor = GetNumericAccessor(field, isFieldReference);
        }

        [Pure]
        public double GetNumber(IManagementObjectAccessor accessor)
        {
            return fieldAccessor?.Invoke(accessor) ?? double.NaN;
        }

        [Pure]
        private static Func<IManagementObjectAccessor, double> GetNumericAccessor(string field, bool isFieldReference)
        {
            if (isFieldReference)
            {
                return a => GetNumber(a.Get(field));
            }
            var number = GetNumber(field);
            return a => number;
        }

        private static double GetNumber(object value)
        {
            double number;
            if (Util.TryGetAsDouble(value, out number)) return number;
            return double.NaN;
        }
    }
}