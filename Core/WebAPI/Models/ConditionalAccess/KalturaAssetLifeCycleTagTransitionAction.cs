using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaAssetLifeCycleTagTransitionAction : KalturaAssetLifeCycleTransitionAction
    {
        /// <summary>
        /// Comma separated list of tag Ids.
        /// </summary>
        [DataMember(Name = "tagIds")]
        [JsonProperty("tagIds")]
        [XmlElement(ElementName = "tagIds")]
        public string TagIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.ASSET_LIFE_CYCLE_TRANSITION;
            this.AssetLifeCycleRuleTransitionType = KalturaAssetLifeCycleRuleTransitionType.TAG;
        }
    }
}