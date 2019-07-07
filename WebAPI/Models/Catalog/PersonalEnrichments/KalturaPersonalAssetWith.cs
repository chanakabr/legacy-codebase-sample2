using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Obsolete]
    public enum KalturaPersonalAssetWith
    {
        bookmark,
        pricing,
        following
    }

    /// <summary>
    /// Holder object for personal asset with enum
    /// </summary>  
    [Obsolete]  
    public partial class KalturaPersonalAssetWithHolder : KalturaOTTObject
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