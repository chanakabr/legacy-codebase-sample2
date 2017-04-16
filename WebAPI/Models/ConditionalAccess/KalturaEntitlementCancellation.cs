using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Cancellation of a user's entitlement
    /// </summary>
    [Serializable]
    public class KalturaEntitlementCancellation : KalturaOTTObject
    {
        /// <summary>
        ///Purchase identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        ///Entitlement type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        [Obsolete]
        public KalturaTransactionType Type
        {
            get;
            set;
        }

        /// <summary>
        ///Product Id (Media File ID, Subscription ID)
        /// </summary>
        [DataMember(Name = "productId")]
        [JsonProperty("productId")]
        [XmlElement(ElementName = "productId")]
        [SchemeProperty(ReadOnly = true)]
        public string ProductId
        {
            get;
            set;
        }

        /// <summary>
        /// The Identifier of the purchasing user
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        public string UserId
        {
            get;
            set;
        }


        /// <summary>
        /// The Identifier of the purchasing household
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(ReadOnly = true)]
        public long HouseholdId
        {
            get;
            set;
        }
    }
}