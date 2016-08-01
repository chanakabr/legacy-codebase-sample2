using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    public interface IKalturaFilter
    {
    }

    /// <summary>
    /// Base filter
    /// </summary>
    public abstract class KalturaFilter<T> : KalturaOTTObject, IKalturaFilter where T : struct, IComparable, IFormattable, IConvertible
    {
        public abstract T GetDefaultOrderByValue();

        public KalturaFilter()
        {
            OrderBy = GetDefaultOrderByValue();
        }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public T OrderBy { get; set; }
    }
}