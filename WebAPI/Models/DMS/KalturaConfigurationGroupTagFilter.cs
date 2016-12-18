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
    /// <summary>
    /// Configuration group tag filter
    /// </summary>
    public class KalturaConfigurationGroupTagFilter: KalturaFilter<KalturaConfigurationGroupTagOrderBy>
    {
        /// <summary>
        /// the ID of the configuration group for which to return related configurations group tags
        /// </summary>
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