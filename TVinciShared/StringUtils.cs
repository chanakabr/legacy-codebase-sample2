using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public static class StringUtils
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        {
            return (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));
        }

        /// <summary>
        /// Convert string value to nullable struct
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">String to convert from</param>
        /// <returns>Converted value, null if can't be converted</returns>
        public static T? TryConvertTo<T>(string value) where T : struct, IConvertible
        {
            if (!string.IsNullOrEmpty(value))
            {
                Type t = typeof(T);

                try
                {
                    var convertedValue = Convert.ChangeType(value, t);
                    if (convertedValue != null && convertedValue is T)
                    {
                        return (T)convertedValue;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Convert string value to nullable struct
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="value">String to convert from</param>
        /// <returns>Converted value, null if can't be converted</returns>
        public static T ConvertTo<T>(string value) where T : struct, IConvertible
        {
            if (!string.IsNullOrEmpty(value))
            {
                Type t = typeof(T);

                try
                {
                    var convertedValue = Convert.ChangeType(value, t);
                    if (convertedValue != null && convertedValue is T)
                    {
                        return (T)convertedValue;
                    }
                }
                catch (Exception)
                {
                    return default(T);
                }
            }

            return default(T);
        }
    }
}
