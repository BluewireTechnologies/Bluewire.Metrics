using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluewire.Metrics.Service.Wmi
{
    internal static class Util
    {
        public static bool TryGetAsDouble(object value, out double number)
        {
            try
            {
                number = 0;
                if (value == null) return false;
                if (value is string)
                {
                    if (String.IsNullOrWhiteSpace((string)value)) return false;
                }
                number = Convert.ToDouble(value);
            }
            catch
            {
                number = double.NaN;
            }
            return true;
        }

        public static bool TryGetAsInt64(object value, out long number)
        {
            try
            {
                number = 0;
                if (value == null) return false;
                if (value is string)
                {
                    if (String.IsNullOrWhiteSpace((string)value)) return false;
                }
                number = Convert.ToInt64(value);
                return true;
            }
            catch
            {
                // Subtle difference from TryGetAsDouble: a failure to convert the value
                // returns false, since ints have no representation of 'not a number'.
                number = 0;
                return false;
            }
        }

        public static long? GetAsInt64OrNull(object value)
        {
            long number;
            if (TryGetAsInt64(value, out number)) return number;
            return null;
        }
    }
}
