using System;

namespace Bluewire.Metrics.Service.Wmi
{
    public static class Util
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
    }
}
