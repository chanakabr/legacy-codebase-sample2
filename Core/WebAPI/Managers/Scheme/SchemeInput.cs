using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WebAPI.Exceptions;

namespace WebAPI.Managers.Scheme
{
    public class SchemeInputAttribute : Attribute
    {
        public const string ASCII_ONLY_PATTERN = @"[\x00-\x7F]";
        public const string NOT_EMPTY_PATTERN = @"^(?!\s*$).+";
        public const string NO_COMMAS_PATTERN = @"^[^,]+$";

        public Type DynamicType { get; set; }
        public int DynamicMinInt { get; set; }
        public int DynamicMaxInt { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxInteger { get; set; }
        public int MinInteger { get; set; }
        public long MaxLong { get; set; }
        public long MinLong { get; set; }
        public float MaxFloat { get; set; }
        public float MinFloat { get; set; }

        /// <summary>
        /// regular expression template for the string value
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// minimum length of an array
        /// </summary>
        public int MinItems { get; set; }

        /// <summary>
        /// maximum length of an array
        /// </summary>
        public int MaxItems { get; set; }

        /// <summary>
        /// the default value represents what would be assumed by the consumer of the input as the value of the schema if one is not provided.
        /// Unlike JSON Schema, the value MUST conform to the defined type for the Schema Object defined at the same level.
        /// For example, if type is string, then default can be "foo" but cannot be 1
        /// </summary>
        public object Default { get; set; }

        public bool UniqueItems { get; set; }

        public SchemeInputAttribute()
        {
            MaxLength = -1;
            MinLength = -1;
            DynamicMinInt = int.MinValue;
            DynamicMaxInt = int.MaxValue;
            MaxInteger = int.MaxValue;
            MinInteger = int.MinValue;
            MaxLong = long.MaxValue;
            MinLong = long.MinValue;
            MaxFloat = float.MaxValue;
            MinFloat = float.MinValue;
            Pattern = null;
            MinItems = -1;
            MaxItems = -1;
        }

        internal void Validate(string name, object value)
        {
            ValidateDynamicType(name, value);
            ValidateDynamicMinInt(name, value);
            ValidateDynamicMaxInt(name, value);
            ValidateMaxLength(name, value);
            ValidateMinLength(name, value);
            ValidateMaxInteger(name, value);
            ValidateMinInteger(name, value);
            ValidateMaxLong(name, value);
            ValidateMinLong(name, value);
            ValidateMaxFloat(name, value);
            ValidateMinFloat(name, value);
            ValidatePattern(Pattern, name, value);
            ValidateMinItems(name, value);
            ValidateMaxItems(name, value);
            ValidateUniqueItems(name, value);
        }

        private void ValidateDynamicType(string name, object value)
        {
            if (DynamicType != null)
            {
                string sValue = (string)Convert.ChangeType(value, typeof(string));
                try
                {
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        var nValue = 0;
                        if (int.TryParse(sValue, out nValue))
                        {
                            // value should be only String
                            throw new BadRequestException(BadRequestException.ARGUMENT_STRING_SHOULD_BE_ENUM, name, DynamicType.Name);
                        }

                        Enum.Parse(DynamicType, sValue, true);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_STRING_SHOULD_BE_ENUM, name, DynamicType.Name);
                }
            }
        }

        private void ValidateDynamicMinInt(string name, object value)
        {
            if (DynamicMinInt > int.MinValue)
            {
                string sValue = (string)Convert.ChangeType(value, typeof(string));
                if (!string.IsNullOrEmpty(sValue))
                {
                    string[] splitNumbers = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string number in splitNumbers)
                    {
                        long parsedNumber;
                        if (!long.TryParse(number, out parsedNumber))
                        {
                            throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, name);
                        }

                        if (parsedNumber < DynamicMinInt)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_STRING_CONTAINED_MIN_VALUE_CROSSED, name, DynamicMinInt);
                        }
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, name);
                }
            }
        }

        private void ValidateDynamicMaxInt(string name, object value)
        {
            if (DynamicMaxInt < int.MaxValue)
            {
                string sValue = (string)Convert.ChangeType(value, typeof(string));
                if (!string.IsNullOrEmpty(sValue))
                {
                    string[] splitNumbers = sValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string number in splitNumbers)
                    {
                        long parsedNumber;
                        if (!long.TryParse(number, out parsedNumber))
                        {
                            throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, name);
                        }

                        if (parsedNumber > DynamicMaxInt)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_STRING_CONTAINED_MAX_VALUE_CROSSED, name, DynamicMaxInt);
                        }
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, name);
                }
            }
        }

        private void ValidateMaxLength(string name, object value)
        {
            if (MaxLength > 0)
            {
                string sValue = (string)Convert.ChangeType(value, typeof(string));
                if (sValue.Length > MaxLength)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, name, MaxLength.ToString());
            }
        }

        private void ValidateMinLength(string name, object value)
        {
            if (MinLength >= 0)
            {
                string sValue = (string)Convert.ChangeType(value, typeof(string));
                if (sValue.Length < MinLength)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_LENGTH_CROSSED, name, MinLength.ToString());
            }
        }

        private void ValidateMaxInteger(string name, object value)
        {
            if (MaxInteger < int.MaxValue)
            {
                int iValue = (int)Convert.ChangeType(value, typeof(int));
                if (iValue > MaxInteger)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxInteger.ToString());
            }
        }

        private void ValidateMinInteger(string name, object value)
        {
            if (MinInteger > int.MinValue)
            {
                int lValue = (int)Convert.ChangeType(value, typeof(int));
                if (lValue < MinInteger)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinInteger.ToString());
            }
        }

        private void ValidateMaxLong(string name, object value)
        {
            if (MaxLong < long.MaxValue)
            {
                long lValue = (long)Convert.ChangeType(value, typeof(long));
                if (lValue > MaxLong)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxLong.ToString());
            }
        }

        private void ValidateMinLong(string name, object value)
        {
            if (MinLong > long.MinValue)
            {
                long lValue = (long)Convert.ChangeType(value, typeof(long));
                if (lValue < MinLong)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinLong.ToString());
            }
        }

        private void ValidateMaxFloat(string name, object value)
        {
            if (MaxFloat < float.MaxValue)
            {
                float fValue = (float)Convert.ChangeType(value, typeof(float));
                if (fValue > MaxFloat)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxFloat.ToString());
            }
        }

        private void ValidateMinFloat(string name, object value)
        {
            if (MinFloat > float.MinValue)
            {
                float fValue = (float)Convert.ChangeType(value, typeof(float));
                if (fValue < MinFloat)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinFloat.ToString());
            }
        }

        public static void ValidatePattern(string pattern, string name, object value)
        {
            if (pattern != null)
            {
                try
                {
                    if (!Regex.IsMatch(value.ToString(), pattern))
                        throw new BadRequestException(BadRequestException.ARGUMENT_MATCH_PATTERN_CROSSED, name, pattern);
                }
                catch (Exception)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_MATCH_PATTERN_CROSSED, name, pattern);
                }
            }
        }

        private void ValidateMinItems(string name, object value)
        {
            if (MinItems >= 0)
            {
                var items = GetEnumerableCount(GetEnumerable(value));
                if (items < MinItems)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_ITEMS_CROSSED, name, MinItems);
            }
        }

        private void ValidateMaxItems(string name, object value)
        {
            if (MaxItems >= 0)
            {
                var items = GetEnumerableCount(GetEnumerable(value));
                if (items > MaxItems)
                    throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, name, MaxItems);
            }
        }

        private IEnumerable<object> GetEnumerable(object value)
        {
            if (value.GetType().IsArray)
            {
                return (value as IEnumerable).Cast<object>();
            }

            return (value as Newtonsoft.Json.Linq.JArray);
        }

        private int GetEnumerableCount(IEnumerable<object> array)
        {
            if (array != null)
            {
                return array.Count();
            }
            return 0;
        }

        private void ValidateUniqueItems(string name, object value)
        {
            if (UniqueItems)
            {
                var array = GetEnumerable(value);
                if (GetEnumerableCount(array) != GetEnumerableCount(array.Distinct()))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, name);
                }
            }
        }

        protected bool isA(RequestType a, RequestType b)
        {
            return isA(a, (int)b);
        }

        protected bool isA(RequestType a, int b)
        {
            return ((int)a & b) > 0;
        }
    }
}