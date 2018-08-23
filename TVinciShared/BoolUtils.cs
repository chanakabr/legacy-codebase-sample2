using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVinciShared
{
    public static class BoolUtils
    {
        public static bool TryConvert(string value, out bool convertedValue)
        {
            convertedValue = false;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Contains("0") || value.Contains("1"))
            {
                int intVal = 0;
                if (int.TryParse(value, out intVal))
                {
                    convertedValue = intVal == 1 ? true : false;
                    return true;
                }

                return false;
            }

            return bool.TryParse(value, out convertedValue);
        }
    }
}
