using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
     public enum KalturaConfigurationGroupDeviceOrderBy
    {
        NONE
    }
    public class KalturaConfigurationGroupDeviceFilter: KalturaFilter<KalturaConfigurationGroupDeviceOrderBy>
    {
        [DataMember(Name = "configurationGroupIdEqual")]
        [JsonProperty("configurationGroupIdEqual")]
        [XmlElement(ElementName = "configurationGroupIdEqual")]
        public string ConfigurationGroupIdEqual { get; set; }

        public override KalturaConfigurationGroupDeviceOrderBy GetDefaultOrderByValue()
        {
            return KalturaConfigurationGroupDeviceOrderBy.NONE;
        }    
    }
}