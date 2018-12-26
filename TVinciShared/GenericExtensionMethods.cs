using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public static class GenericExtensionMethods
    {
        /// <summary>
        /// Check if struct object is default (null)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }

        public static T? GetUpdatedValue<T>(this T? value, T? otherValue, ref bool needToUpdate) where T : struct
        {
            if (value != null && value.HasValue && !value.Equals(otherValue))
            {
                needToUpdate = true;
            }
            else
            {
                value = otherValue;
            }

            return value;
        }

        public static T? ToNullable<T>(this T value) where T : struct
        {
            if (value.IsDefault())
            {
                return null;
            }

            return (T?)value;
        }
    }
}
