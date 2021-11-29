using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaApplyPlaybackAdapterAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// Playback Adapter Identifier 
        /// </summary>
        [DataMember(Name = "adapterId")]
        [JsonProperty("adapterId")]
        [XmlElement(ElementName = "adapterId")]
        public int AdapterId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER;
        }
    }
}