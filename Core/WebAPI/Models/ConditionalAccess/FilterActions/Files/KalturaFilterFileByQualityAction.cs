using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// Filter Files By their Quality
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new [] {"qualityIn"})]
    public abstract partial class KalturaFilterFileByQualityAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated qualities
        /// </summary>
        [DataMember(Name = "qualityIn")]
        [JsonProperty("qualityIn")]
        [XmlElement(ElementName = "qualityIn")]
        [SchemeProperty(DynamicType = typeof(KalturaMediaFileTypeQuality), MinLength = 1)]
        public string QualityIn { get; set; }
    }
}