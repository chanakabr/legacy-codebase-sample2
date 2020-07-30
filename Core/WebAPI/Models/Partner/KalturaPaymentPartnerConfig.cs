using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner billing configuration
    /// </summary>
    public partial class KalturaPaymentPartnerConfig : KalturaPartnerConfiguration
    {
        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Payment; } }

        /// <summary>
        /// configuration for unified billing cycles.
        /// </summary>
        [DataMember(Name = "unifiedBillingCycles")]
        [JsonProperty("unifiedBillingCycles")]
        [XmlElement(ElementName = "unifiedBillingCycles", IsNullable = true)]
        public List<KalturaUnifiedBillingCycle> UnifiedBillingCycles { get; set; }

        internal override bool Update(int groupId)
        {
            Func<PaymentPartnerConfig, Status> partnerConfigFunc =
                (PaymentPartnerConfig paymentPartnerConfig) => PartnerConfigurationManager.UpdatePaymentConfig(groupId, paymentPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, this);

            return true;
        }

        internal override void ValidateForUpdate()
        {
            if (UnifiedBillingCycles != null && UnifiedBillingCycles.Count > 0)
            {
                foreach (var unifiedBillingCycle in this.UnifiedBillingCycles)
                {
                    unifiedBillingCycle.Validate();
                }
            }
        }
    }

    public partial class KalturaUnifiedBillingCycle : KalturaOTTObject
    {
        /// <summary>
        /// UnifiedBillingCycle name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// cycle duration
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty("duration")]
        [XmlElement(ElementName = "duration")]
        public KalturaDuration Duration { get; set; }

        /// <summary>
        /// Payment Gateway Id
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Define if partial billing shall be calculated or not
        /// </summary>
        [DataMember(Name = "ignorePartialBilling")]
        [JsonProperty("ignorePartialBilling")]
        [XmlElement(ElementName = "ignorePartialBilling", IsNullable = true)]
        public bool? IgnorePartialBilling { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUnifiedBillingCycle.name");
            }

            if (this.Duration == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUnifiedBillingCycle.duration");
            }
        }
    }
}