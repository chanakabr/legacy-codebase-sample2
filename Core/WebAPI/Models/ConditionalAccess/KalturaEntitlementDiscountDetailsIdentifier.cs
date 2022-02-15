using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    ///  
    /// </summary>
    [Serializable]
    public partial class KalturaEntitlementDiscountDetailsIdentifier : KalturaEntitlementDiscountDetails
    {
        /// <summary>
        ///Identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }
    }
}