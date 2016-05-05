using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    public class KalturaRecordingContextFilter : KalturaFilter
    {

        /// <summary>
        /// Asset Id's
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlArray(ElementName = "assetIdIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "assetIdIn")]
        public List<KalturaLongValue> AssetIdIn { get; set; }

        public override object GetDefaultOrderByValue()
        {
            return null;
        }
    }
}