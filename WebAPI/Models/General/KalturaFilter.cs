using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    public interface IKalturaFilter : IKalturaOTTObject
    {
    }

    /// <summary>
    /// Base filter
    /// </summary>
    public abstract partial class KalturaFilter<KalturaT> : KalturaOTTObject, IKalturaFilter where KalturaT : struct, IComparable, IFormattable, IConvertible
    {
        public abstract KalturaT GetDefaultOrderByValue();

        public KalturaFilter(Dictionary<string, object> parameters = null) : base(parameters)
        {
            if (parameters != null && parameters.ContainsKey("orderBy") && parameters["orderBy"] != null)
            {
                OrderBy = (KalturaT)Enum.Parse(typeof(KalturaT), parameters["orderBy"].ToString(), true);
            }
        }

        protected override void Init()
        {
            base.Init();
            OrderBy = GetDefaultOrderByValue();
        }

        // TODO SHIR - USE THIS IN ALL PLACES..
        internal HashSet<T> GetIdsIn<T>(string idsIn, string propertyName) where T : IConvertible
        {
            HashSet<T> values = new HashSet<T>();

            if (!string.IsNullOrEmpty(idsIn))
            {
                string[] stringValues = idsIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, propertyName);
                    }

                    if (value != null && !value.Equals(default(T)))
                    {
                        if (values.Contains(value))
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, propertyName);
                        }

                        values.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, propertyName);
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaT OrderBy { get; set; }
    }
}