using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public class KalturaStringValueArray : KalturaOTTObject
    {
        /// <summary>
        /// List of string values
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> Objects { get; set; }

        public KalturaStringValueArray()
        {
            Objects = new List<KalturaStringValue>();
        }
    }
}