using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription Dependency Set
    /// </summary>
    public partial class KalturaSubscriptionDependencySet : KalturaSubscriptionSet
    {

        /// <summary>
        /// Base Subscription identifier
        /// </summary>
        [DataMember(Name = "baseSubscriptionId")]
        [JsonProperty("baseSubscriptionId")]
        [XmlElement(ElementName = "baseSubscriptionId")]       
        public long? BaseSubscriptionId { get; set; }

    }
}