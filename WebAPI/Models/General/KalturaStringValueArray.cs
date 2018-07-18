using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    public partial class KalturaStringValueArray : KalturaOTTObject
    {
        /// <summary>
        /// List of string values
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaStringValue> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            Objects = new List<KalturaStringValue>();
        }
    }
}