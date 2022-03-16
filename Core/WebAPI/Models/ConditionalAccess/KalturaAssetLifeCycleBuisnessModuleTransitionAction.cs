using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaAssetLifeCycleBuisnessModuleTransitionAction : KalturaAssetLifeCycleTransitionAction
    {
        /// <summary>
        /// Comma separated list of fileType Ids.
        /// </summary>
        [DataMember(Name = "fileTypeIds")]
        [JsonProperty("fileTypeIds")]
        [XmlElement(ElementName = "fileTypeIds")]
        public string FileTypeIds { get; set; }

        /// <summary>
        /// Comma separated list of ppv Ids.
        /// </summary>
        [DataMember(Name = "ppvIds")]
        [JsonProperty("ppvIds")]
        [XmlElement(ElementName = "ppvIds")]
        public string PpvIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION;
            this.AssetLifeCycleRuleTransitionType = KalturaAssetLifeCycleRuleTransitionType.BUSINESS_MODEL;
        }
    }
}