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
    public enum KalturaPersonalAssetWith
    {
        bookmark,
        pricing,
        following
    }

    /// <summary>
    /// Holder object for personal asset with enum
    /// </summary>    
    public class KalturaPersonalAssetWithHolder : KalturaOTTObject
    {
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaPersonalAssetWith type
        {
            get;
            set;
        }
    }
}