using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public enum KalturaConfigurationGroupTagOrderBy
    {
        NONE
    }
    public class KalturaConfigurationGroupTagFilter: KalturaFilter<KalturaConfigurationGroupTagOrderBy>
    {
        [DataMember(Name = "configurationGroupIdEqual")]
        [JsonProperty("configurationGroupIdEqual")]
        [XmlElement(ElementName = "configurationGroupIdEqual")]
        public string ConfigurationGroupIdEqual { get; set; }

        public override KalturaConfigurationGroupTagOrderBy GetDefaultOrderByValue()
        {
            return KalturaConfigurationGroupTagOrderBy.NONE;
        }    
    }
}