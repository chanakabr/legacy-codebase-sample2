using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Compensation request parameters
    /// </summary>
    public class KalturaCompensation : KalturaOTTObject
    {
        /// <summary>
        /// Compensation identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Subscription identifier
        /// </summary>
        [DataMember(Name = "subscriptionId")]
        [JsonProperty("subscriptionId")]
        [XmlElement(ElementName = "subscriptionId")]
        [SchemeProperty(ReadOnly = true)]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// Compensation type
        /// </summary>
        [DataMember(Name = "compensationType")]
        [JsonProperty("compensationType")]
        [XmlElement(ElementName = "compensationType")]
        public KalturaCompensationType CompensationType { get; set; }

        /// <summary>
        /// Compensation amount
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount")]
        [SchemeProperty(MinFloat=0)]
        public double Amount { get; set; }

        /// <summary>
        /// The number of renewals for compensation
        /// </summary>
        [DataMember(Name = "totalRenewalIterations")]
        [JsonProperty("totalRenewalIterations")]
        [XmlElement(ElementName = "totalRenewalIterations")]
        [SchemeProperty(MinFloat = 0)]
        public int TotalRenewalIterations { get; set; }

        /// <summary>
        /// The number of renewals the compensation was already applied on 
        /// </summary>
        [DataMember(Name = "appliedRenewalIterations")]
        [JsonProperty("appliedRenewalIterations")]
        [XmlElement(ElementName = "appliedRenewalIterations")]
        [SchemeProperty(ReadOnly = true)]
        public int AppliedRenewalIterations { get; set; }

        /// <summary>
        /// Purchase identifier
        /// </summary>
        [DataMember(Name = "purchaseId")]
        [JsonProperty("purchaseId")]
        [XmlElement(ElementName = "purchaseId")]
        public int PurchaseId { get; set; }

        public void Validate()
        {
            if (CompensationType == KalturaCompensationType.PERCENTAGE && Amount > 100)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, "KalturaCompensation.amount", 100);
            }
        }
    }

    /// <summary>
    /// Compensation type
    /// </summary>
    public enum KalturaCompensationType
    {
        PERCENTAGE = 0,
        FIXED_AMOUNT = 1
    }
}