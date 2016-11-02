using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public enum KalturaConfigurationsOrderBy
    {
        NONE
    }
    /// <summary>
    /// Configuration filter
    /// </summary>
    public class KalturaConfigurationsFilter : KalturaFilter<KalturaConfigurationsOrderBy>
    {
        [DataMember(Name = "configurationGroupIdEqual")]
        [JsonProperty("configurationGroupIdEqual")]
        [XmlElement(ElementName = "configurationGroupIdEqual")]
        public string ConfigurationGroupIdEqual { get; set; }

        public override KalturaConfigurationsOrderBy GetDefaultOrderByValue()
        {
            return KalturaConfigurationsOrderBy.NONE;
        }
    }
}