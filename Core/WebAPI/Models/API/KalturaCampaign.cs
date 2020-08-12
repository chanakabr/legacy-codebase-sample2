using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using ApiLogic.Users.Managers;
using ApiLogic.Base;
using ApiObjects.Response;
using ApiObjects.Base;
using ApiObjects;

namespace WebAPI.Models.API
{
    // TODO MATAN - ADD MAP BETWEEN KalturaCampaign -> Campaign AND Campaign -> KalturaCampaign 
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaCampaign : KalturaCrudObject<Campaign, long>
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// systemName
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName")]
        public string SystemName { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Create date of the rule
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date of the rule
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// isActive
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsActive { get; set; }

        /// <summary>
        /// The discount module id that is promoted to the user
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DiscountModuleId { get; set; }

        /// <summary>
        /// List of conditions for the campaign
        /// </summary>
        [DataMember(Name = "campaignConditions")]
        [JsonProperty("campaignConditions")]
        [XmlElement(ElementName = "campaignConditions")]
        public List<KalturaCondition> CampaignConditions { get; set; }

        /// <summary>
        /// list of free text messages to the user that gives information about the campaign.
        /// </summary>
        [DataMember(Name = "messages")]
        [JsonProperty("messages")]
        [XmlElement(ElementName = "messages")]
        public SerializableDictionary<string, KalturaStringValue> Messages { get; set; }

        public KalturaCampaign()
        {

        }
        internal override ICrudHandler<Campaign, long> Handler
        {
            get
            {
                return CampaignManager.Instance;
            }
        }

        internal override void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (string.IsNullOrEmpty(this.SystemName) || string.IsNullOrWhiteSpace(this.SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (this.Messages == null || this.Messages.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "messages");
            }

            if (this.CampaignConditions == null || this.CampaignConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "campaignConditions");
            }

            if (!this.CampaignConditions.Any(x => x.Type == KalturaRuleConditionType.DATE))
            {
                throw new BadRequestException(BadRequestException.MISSING_MANDATORY_ARGUMENT_IN_PROPERTY, "campaignConditions", "KalturaDateCondition");
            }

            foreach (var condition in this.CampaignConditions)
            {
                condition.Validate();
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        internal override void ValidateForUpdate()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
        }
    }

    public partial class KalturaCampaignListResponse : KalturaListResponse<KalturaCampaign>
    {
        public KalturaCampaignListResponse() : base() { }
    }
}