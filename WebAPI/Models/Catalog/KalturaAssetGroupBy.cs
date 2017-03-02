using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public class KalturaAssetGroupBy : KalturaOTTObject
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }
    }

    [Serializable]
    public class KalturaAssetMetaGroupBy : KalturaAssetGroupBy
    {
    }
}