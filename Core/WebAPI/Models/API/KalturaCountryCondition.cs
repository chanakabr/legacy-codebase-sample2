using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Country condition
    /// </summary>
    public partial class KalturaCountryCondition : KalturaNotCondition
    {
        /// <summary>
        /// Comma separated countries IDs list
        /// </summary>
        [DataMember(Name = "countries")]
        [JsonProperty("countries")]
        [XmlElement(ElementName = "countries")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string Countries { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.COUNTRY;
        }
    }
}