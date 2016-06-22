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
    public enum KalturaRecordingContextOrderBy
    {
    }

    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    public class KalturaRecordingContextFilter : KalturaFilter<KalturaRecordingContextOrderBy?>
    {

        /// <summary>
        /// Asset Id's
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlArray(ElementName = "assetIdIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "assetIdIn")]
        public List<KalturaLongValue> AssetIdIn { get; set; }

        public override KalturaRecordingContextOrderBy? GetDefaultOrderByValue()
        {
            return null;
        }
    }
}