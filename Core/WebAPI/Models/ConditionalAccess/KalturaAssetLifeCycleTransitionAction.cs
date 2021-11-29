using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public abstract partial class KalturaAssetLifeCycleTransitionAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// Asset LifeCycle Rule Action Type
        /// </summary>
        [DataMember(Name = "assetLifeCycleRuleActionType")]
        [JsonProperty("assetLifeCycleRuleActionType")]
        [XmlElement(ElementName = "assetLifeCycleRuleActionType")]
        public KalturaAssetLifeCycleRuleActionType AssetLifeCycleRuleActionType { get; set; }

        /// <summary>
        /// Asset LifeCycle Rule Transition Type
        /// </summary>
        [DataMember(Name = "assetLifeCycleRuleTransitionType")]
        [JsonProperty("assetLifeCycleRuleTransitionType")]
        [XmlElement(ElementName = "assetLifeCycleRuleTransitionType")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaAssetLifeCycleRuleTransitionType AssetLifeCycleRuleTransitionType { get; protected set; }
    }

}