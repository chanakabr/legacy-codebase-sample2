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
        /// <summary>
        /// Convert comma separated string to collection.
        /// </summary>
        /// <typeparam name="U">Collection of T</typeparam>
        /// <typeparam name="T">Type of items in collection</typeparam>
        /// <param name="itemsIn">Comma separated string</param>
        /// <param name="propertyName">The propery name of comma separated string (for error message)</param>
        /// <returns></returns>
        internal U GetItemsIn<U,T>(string itemsIn, string propertyName) where T : IConvertible where U : ICollection<T>
        {
            U values = Activator.CreateInstance<U>();
            
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
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, propertyName);
                    }

                    if (value != null && !value.Equals(default(T)))
                    {
                        if (!values.Contains(value))
                        {
                            values.Add(value);
                        }
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