using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using System.Collections.Generic;
using ApiObjects;

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

        public List<MediaFileTypeQuality> GetQualities()
        {
            var types = Utils.Utils.ParseCommaSeparatedValues<List<KalturaMediaFileTypeQuality>, KalturaMediaFileTypeQuality>(QualityIn, "qualityIn", true, true);
            return AutoMapper.Mapper.Map<List<MediaFileTypeQuality>>(types);
        }
    }
    
    [Serializable]
    public partial class KalturaFilterFileByQualityInDiscoveryAction : KalturaFilterFileByQualityAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByQualityInDiscovery;
        }
    }

    [Serializable]
    public partial class KalturaFilterFileByQualityInPlaybackAction : KalturaFilterFileByQualityAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByQualityInPlayback;
        }
    }
}