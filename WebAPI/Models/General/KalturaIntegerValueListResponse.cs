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
    /// <summary>
    /// Integer list wrapper
    /// </summary>
      [DataContract(Name = "KalturaIntegerValueListResponse", Namespace = "")]
    [XmlRoot("KalturaIntegerValueListResponse")]
    public class KalturaIntegerValueListResponse : KalturaListResponse
    {
        /// <summary>
        /// Interger value items
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]        
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]       
        public List<KalturaIntegerValue> Values { get; set; }

    }
}