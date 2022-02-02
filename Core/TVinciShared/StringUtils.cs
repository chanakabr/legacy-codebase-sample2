using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrEmptyOrWhiteSpace(this string value)
        {
            return (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value));
        }

        public static string GetUpdatedValue(this string value, string otherValue, ref bool needToUpdate)
        {
            if (!value.IsNullOrEmptyOrWhiteSpace() && !value.Equals(otherValue))
            {
                needToUpdate = true;
            }
            else
            {
                value = otherValue;
            }

            return value;
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

        //
        // Summary:
        //     Returns a copy of this string converted to lowercase or null in case the input is null.
        //
        // Returns:
        //     A string in lowercase.
        public static string ToLowerOrNull(this string value)
        {
            if (value == null)
                return null;

            return value.ToLower();
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

        /// <summary>
        /// Convert comma separated string to collection.
        /// </summary>
        /// <typeparam name="U">Collection of T</typeparam>
        /// <typeparam name="T">Type of items in collection</typeparam>
        /// <param name="itemsIn">Comma separated string</param>
        /// <returns></returns>
        public static List<T> GetItemsIn<T>(this string itemsIn, out bool failed, bool ignoreDefaultValueValidation = false) where T : IConvertible
        {
            var values = new List<T>();
            failed = false;

            if (!string.IsNullOrEmpty(itemsIn))
            {
                string[] stringValues = itemsIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Type t = typeof(T);

                foreach (string stringValue in stringValues)
                {
                    T value;

                    try
                    {
                        value = (T)Convert.ChangeType(stringValue, t);
                    }
                    catch (Exception)
                    {
                        failed = true;
                        continue;
                    }

                    if (ignoreDefaultValueValidation || !EqualityComparer<T>.Default.Equals(value, default))
                    {
                        if (!values.Contains(value))
                        {
                            values.Add(value);
                        }
                    }
                    else
                    {
                        failed = true;
                    }
                }
            }

            return values;
        }

        public static List<T> ThrowIfFailed<T>(this List<T> list, bool failed, Func<Exception> getter) => failed ? throw getter() : list;

        public static bool IsValidRegex(string pattern)
        {
            try
            {
                new Regex(pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public static string ConvertToCommaSeparatedString<T>(this List<T> values, string defaultValue = null)
        {
            if (values == null)
            {
                return defaultValue;
            }

            return string.Join(",", values);
        }
    }
}
