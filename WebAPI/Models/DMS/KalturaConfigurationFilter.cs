using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public enum KalturaConfigurationOrderBy
    {
        NONE
    }
    /// <summary>
    /// Configuration filter
    /// </summary>
    public class KalturaConfigurationFilter : KalturaFilter<KalturaConfigurationOrderBy>
    {
        [DataMember(Name = "configurationTypeEqual")]
        [JsonProperty("configurationTypeEqual")]
        [XmlElement(ElementName = "configurationTypeEqual")]
        public KalturaConfigurationType ConfigurationTypeEqual { get; set; }

        [DataMember(Name = "configurationGroupIdEqual")]
        [JsonProperty("configurationGroupIdEqual")]
        [XmlElement(ElementName = "configurationGroupIdEqual")]
        public string ConfigurationGroupIdEqual { get; set; }

        public override KalturaConfigurationOrderBy GetDefaultOrderByValue()
        {
            return KalturaConfigurationOrderBy.NONE;
        }
    }
}