using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaUsageModuleFilter: KalturaFilter<KalturaUsageModuleFilterOrderBy>
    {
        /// <summary>
        /// usageModule id
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(DynamicMinInt = 1)]
        public int? IdEqual { get; set; }

        public override KalturaUsageModuleFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaUsageModuleFilterOrderBy.NONE;
        }
    }
}