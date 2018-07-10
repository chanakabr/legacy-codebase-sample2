
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

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
    }
}