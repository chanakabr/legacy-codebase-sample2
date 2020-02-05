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
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// partner configuration for commerce
    /// </summary>
    public partial class KalturaCommercePartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// configuration for bookmark event threshold (when to dispatch the event) in seconds.
        /// </summary>
        [DataMember(Name = "bookmarkEventThresholds")]
        [JsonProperty("bookmarkEventThresholds")]
        [XmlElement(ElementName = "bookmarkEventThresholds", IsNullable = true)]
        public List<KalturaBookmarkEventThreshold> BookmarkEventThresholds { get; set; }

        internal override bool Update(int groupId)
        {
            Func<CommercePartnerConfig, Status> commercePartnerConfigFunc =
                (CommercePartnerConfig commercePartnerConfig) =>
                    PartnerConfigurationManager.UpdateCommerceConfig(groupId, commercePartnerConfig);

            ClientUtils.GetResponseStatusFromWS<KalturaCommercePartnerConfig, CommercePartnerConfig>(commercePartnerConfigFunc, this);

            return true;
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Commerce; } }

        internal Dictionary<KalturaTransactionType, int> GetBookmarkEventThresholds()
        {
            Dictionary<KalturaTransactionType, int> bookmarkEventThresholds = null;
            if (this.BookmarkEventThresholds != null)
            {
                bookmarkEventThresholds = new Dictionary<KalturaTransactionType, int>();
                foreach (var bookmarkEventThreshold in this.BookmarkEventThresholds)
                {
                    if (bookmarkEventThresholds.ContainsKey(bookmarkEventThreshold.TransactionType))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, bookmarkEventThreshold.TransactionType);
                    }

                    bookmarkEventThresholds.Add(bookmarkEventThreshold.TransactionType, bookmarkEventThreshold.Threshold);
                }
            }

            return bookmarkEventThresholds;
        }
    }

    public partial class KalturaBookmarkEventThreshold : KalturaOTTObject
    {
        /// <summary>
        /// bookmark transaction type
        /// </summary>
        [DataMember(Name = "transactionType")]
        [JsonProperty("transactionType")]
        [XmlElement(ElementName = "transactionType")]
        public KalturaTransactionType TransactionType { get; set; }

        /// <summary>
        /// event threshold in seconds
        /// </summary>
        [DataMember(Name = "threshold")]
        [JsonProperty("threshold")]
        [XmlElement(ElementName = "threshold")]
        [SchemeProperty(MinInteger = 1)]
        public int Threshold { get; set; }
    }
}