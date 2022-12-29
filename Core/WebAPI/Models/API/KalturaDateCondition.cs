using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Date condition
    /// </summary>
    public partial class KalturaDateCondition : KalturaNotCondition
    {
        /// <summary>
        /// Start date 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }

        /// <summary>
        /// End date 
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        public long EndDate { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DATE;
        }
    }
}