using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;

namespace WebAPI.Managers.Scheme
{
    public class SchemeInputAttribute : Attribute
    {
        public Type DynamicType { get; set; }
        public int MaxLength { get; set; }
        public int MinLength { get; set; }
        public int MaxInteger { get; set; }
        public int MinInteger { get; set; }
        public long MaxLong { get; set; }
        public long MinLong { get; set; }
        public float MaxFloat { get; set; }
        public float MinFloat { get; set; }

        public SchemeInputAttribute()
        {
            MaxLength  = -1;
            MinLength  = -1;
            MaxInteger  = int.MaxValue;
            MinInteger  = int.MinValue;
            MaxLong = long.MaxValue;
            MinLong = long.MinValue;
            MaxFloat = float.MaxValue;
            MinFloat = float.MinValue;
        }

        internal void Validate(string name, object value)
        {
            RequestType requiresPermission = RequestType.READ;
            if(HttpContext.Current.Items[RequestParser.REQUEST_TYPE] != null)
                requiresPermission = (RequestType)HttpContext.Current.Items[RequestParser.REQUEST_TYPE];

            if (isA(requiresPermission, RequestType.WRITE))
            {
                if (DynamicType != null)
                {
                    string sValue = (string)Convert.ChangeType(value, typeof(string));
                    try
                    {
                        Enum.Parse(DynamicType, sValue, true);
                    }
                    catch (ArgumentException e)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_STRING_SHOULD_BE_ENUM, name, DynamicType.Name);
                    }
                }

                if (MaxLength > 0)
                {
                    string sValue = (string)Convert.ChangeType(value, typeof(string));
                    if (sValue.Length > MaxLength)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, name, MaxLength.ToString());
                }

                if (MinLength >= 0)
                {
                    string sValue = (string)Convert.ChangeType(value, typeof(string));
                    if (sValue.Length < MinLength)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MIN_LENGTH_CROSSED, name, MinLength.ToString());
                }

                if (MaxInteger < int.MaxValue)
                {
                    int iValue = (int)Convert.ChangeType(value, typeof(int));
                    if (iValue > MaxInteger)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxInteger.ToString());
                }

                if (MinInteger > int.MinValue)
                {
                    long lValue = (long)Convert.ChangeType(value, typeof(long));
                    if (lValue < MinInteger)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinInteger.ToString());
                }

                if (MaxLong < long.MaxValue)
                {
                    long lValue = (long)Convert.ChangeType(value, typeof(long));
                    if (lValue > MaxLong)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxLong.ToString());
                }

                if (MinLong > long.MinValue)
                {
                    int lValue = (int)Convert.ChangeType(value, typeof(int));
                    if (lValue < MinLong)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinLong.ToString());
                }

                if (MaxFloat < float.MaxValue)
                {
                    float fValue = (float)Convert.ChangeType(value, typeof(float));
                    if (fValue > MaxFloat)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, name, MaxFloat.ToString());
                }

                if (MinFloat > float.MinValue)
                {
                    float fValue = (float)Convert.ChangeType(value, typeof(float));
                    if (fValue < MinFloat)
                        throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, name, MinFloat.ToString());
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