using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaDefaultParentalSettingsPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// defaultTvSeriesParentalRuleId
        /// </summary>
        [DataMember(Name = "defaultMoviesParentalRuleId")]
        [JsonProperty("defaultMoviesParentalRuleId")]
        [XmlElement(ElementName = "defaultMoviesParentalRuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultMoviesParentalRuleId { get; set; }

        /// <summary>
        /// defaultTvSeriesParentalRuleId
        /// </summary>
        [DataMember(Name = "defaultTvSeriesParentalRuleId")]
        [JsonProperty("defaultTvSeriesParentalRuleId")]
        [XmlElement(ElementName = "defaultTvSeriesParentalRuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultTvSeriesParentalRuleId { get; set; }

        /// <summary>
        /// defaultParentalPin
        /// </summary>
        [DataMember(Name = "defaultParentalPin")]
        [JsonProperty("defaultParentalPin")]
        [XmlElement(ElementName = "defaultParentalPin")]
        [SchemeProperty(IsNullable = true, MaxLength = 50)]
        public string DefaultParentalPin{ get; set; }

        /// <summary>
        /// defaultPurchasePin
        /// </summary>
        [DataMember(Name = "defaultPurchasePin")]
        [JsonProperty("defaultPurchasePin")]
        [XmlElement(ElementName = "defaultPurchasePin")]
        [SchemeProperty(IsNullable = true, MaxLength = 50)]
        public string DefaultPurchasePin { get; set; }

        /// <summary>
        /// defaultPurchaseSettings
        /// </summary>
        [DataMember(Name = "defaultPurchaseSettings")]
        [JsonProperty("defaultPurchaseSettings")]
        [XmlElement(ElementName = "defaultPurchaseSettings")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultPurchaseSettings { get; set; }


        internal override bool Update(int groupId)
        {
            Func<DefaultParentalSettingsPartnerConfig, Status> partnerConfigFunc =
                (DefaultParentalSettingsPartnerConfig parentalPartnerConfig) => DefaultParentalSettingsPartnerConfigManager.Instance.UpsertParentalDefaultConfig(groupId, Utils.Utils.GetUserIdFromKs(), parentalPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, this);

            return true;
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.DefaultParentalSettings; } }
    }
}
