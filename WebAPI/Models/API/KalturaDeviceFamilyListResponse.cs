using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.Domains;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaDeviceFamilyListResponse : KalturaListResponse
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaDeviceFamily> Objects { get; set; }
    }
}
