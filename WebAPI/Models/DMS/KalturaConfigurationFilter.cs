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

        [DataMember(Name = "configurationIdEqual")]
        [JsonProperty("configurationIdEqual")]
        [XmlElement(ElementName = "configurationIdEqual")]
        public string ConfigurationIdEqual { get; set; }

        public override KalturaConfigurationOrderBy GetDefaultOrderByValue()
        {
            return KalturaConfigurationOrderBy.NONE;
        }
    }
}