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
            OrderBy = GetDefaultOrderByValue();

            if (parameters != null && parameters.ContainsKey("orderBy") && parameters["orderBy"] != null)
            {
                OrderBy = (KalturaT)Enum.Parse(typeof(KalturaT), parameters["orderBy"].ToString(), true);
            }
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