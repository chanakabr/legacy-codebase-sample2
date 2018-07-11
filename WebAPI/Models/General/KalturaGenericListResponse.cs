using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.General
{
    // TODO SHIR - AFTER APPROVE CHANGE ALL KalturaListResponse TO THIS ONE
    /// <summary>
    /// Generic response list
    /// </summary>
    [Serializable]
    public partial class KalturaGenericListResponse<KalturaT> : KalturaListResponse
        where KalturaT : KalturaOTTObject
    {
        /// <summary>
        /// A list of objects
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaT> Objects { get; set; }

        public KalturaGenericListResponse()
        {
            TotalCount = 0;
        }

        /// <summary>
        /// real objectType
        /// </summary>
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public override string objectType { get { return typeof(KalturaT).Name + "ListResponse"; } }
    }
}