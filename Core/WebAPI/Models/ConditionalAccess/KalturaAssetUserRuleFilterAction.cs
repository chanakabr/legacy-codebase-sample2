using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaAssetUserRuleFilterAction : KalturaAssetUserRuleAction
    {
        /// <summary>
        /// Indicates whether to apply on channel
        /// </summary>
        [DataMember(Name = "applyOnChannel")]
        [JsonProperty("applyOnChannel")]
        [XmlElement(ElementName = "applyOnChannel")]
        public bool ApplyOnChannel { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FILTER;
        }
    }
}