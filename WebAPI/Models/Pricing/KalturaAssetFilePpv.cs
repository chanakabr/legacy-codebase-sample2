using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Asset file ppv
    /// </summary>
    public partial class KalturaAssetFilePpv : KalturaOTTObject
    {
        /// <summary>
        ///  Asset file identifier
        /// </summary>
        [DataMember(Name = "assetFileId")]
        [JsonProperty(PropertyName = "assetFileId")]
        [XmlElement(ElementName = "assetFileId")]
        public long AssetFileId { get; set; }

        /// <summary>
        /// Ppv module identifier
        /// </summary>
        [DataMember(Name = "ppvModuleId")]
        [JsonProperty(PropertyName = "ppvModuleId")]
        [XmlElement(ElementName = "ppvModuleId")]
        public long PpvModuleId { get; set; }

        /// <summary>
        /// Start date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// End date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        public long? EndDate { get; set; }        
    }
}