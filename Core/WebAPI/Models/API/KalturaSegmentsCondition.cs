using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Segments condition
    /// </summary>
    public partial class KalturaSegmentsCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated segments IDs list 
        /// </summary>
        [DataMember(Name = "segmentsIds")]
        [JsonProperty("segmentsIds")]
        [XmlElement(ElementName = "segmentsIds")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string SegmentsIds { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.SEGMENTS;
        }
    }
}