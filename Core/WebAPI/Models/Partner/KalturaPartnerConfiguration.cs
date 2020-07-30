using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner  base configuration
    /// </summary>
    public abstract partial class KalturaPartnerConfiguration : KalturaOTTObject
    {
        protected abstract KalturaPartnerConfigurationType ConfigurationType { get; }
        internal abstract bool Update(int groupId);
        internal virtual void ValidateForUpdate() { }
    }

    public partial class KalturaPartnerConfigurationListResponse : KalturaListResponse
    {
        /// <summary>
        /// Partner Configurations
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaPartnerConfiguration> Objects { get; set; }
    }
}