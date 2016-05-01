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
        [DataMember(Name = "assetIds")]
        [JsonProperty(PropertyName = "assetIds")]
        [XmlArray(ElementName = "assetIds", IsNullable = true)]
        [XmlArrayItem(ElementName = "assetIds")]
        public List<long> AssetIds { get; set; }


    }
}