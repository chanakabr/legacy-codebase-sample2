using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Header condition
    /// </summary>
    public partial class KalturaHeaderCondition : KalturaNotCondition
    {
        /// <summary>
        /// Header key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Header value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.HEADER;
        }
    }
}