using System;
using System.Collections;
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

        public static Type GetRealType(this Type type)
        {
            var realType = type;
            if (type.IsNullableType())
            {
                realType = type.GetGenericArguments()[0];
            }

            if (realType.IsArray)
            {
                realType = realType.GetElementType();
            }
            else if (realType.IsGenericType)
            {
                //if List
                if (realType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    realType = realType.GetGenericArguments()[0];
                }
                else if (realType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    realType = realType.GetGenericArguments()[1];
                }
            }
            return realType;
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsNullOrEmpty<T>(this T value) where T : ICollection
        {
            return value?.Count > 0;
        }
    }
}
