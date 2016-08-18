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
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} values must be of type {1}.", name, DynamicType.Name));
                    }
                }

                if (MaxLength > 0)
                {
                    string sValue = (string)Convert.ChangeType(value, typeof(string));
                    if (sValue.Length > MaxLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} maximum length is {1}.", name, MaxLength));
                }

                if (MinLength >= 0)
                {
                    string sValue = (string)Convert.ChangeType(value, typeof(string));
                    if (sValue.Length < MinLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} minimum length is {1}.", name, MinLength));
                }

                if (MaxInteger < int.MaxValue)
                {
                    int iValue = (int)Convert.ChangeType(value, typeof(int));
                    if (iValue > MaxLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} maximum value is {1}.", name, MaxInteger));
                }

                if (MinInteger > int.MinValue)
                {
                    long lValue = (long)Convert.ChangeType(value, typeof(long));
                    if (lValue < MinLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} minimum value is {1}.", name, MinInteger));
                }

                if (MaxLong < long.MaxValue)
                {
                    long lValue = (long)Convert.ChangeType(value, typeof(long));
                    if (lValue > MaxLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} maximum value is {1}.", name, MaxLong));
                }

                if (MinLong > long.MinValue)
                {
                    int lValue = (int)Convert.ChangeType(value, typeof(int));
                    if (lValue < MinLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} minimum value is {1}.", name, MinLong));
                }

                if (MaxFloat < float.MaxValue)
                {
                    float fValue = (float)Convert.ChangeType(value, typeof(float));
                    if (fValue > MaxLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} maximum value is {1}.", name, MaxFloat));
                }

                if (MinFloat > float.MinValue)
                {
                    float fValue = (float)Convert.ChangeType(value, typeof(float));
                    if (fValue < MinLength)
                        throw new BadRequestException((int)StatusCode.InvalidActionParameters, string.Format("Object property {0} minimum value is {1}.", name, MinFloat));
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