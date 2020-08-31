using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiLogic.Users.Managers;
using ApiLogic.Base;
using ApiObjects;

namespace WebAPI.Models.API
{
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
        /// Start date of the rule
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        public long StartDate { get; set; }

        /// <summary>
        /// End date of the rule
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        public long EndDate { get; set; }

        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public int Status { get; set; }

        /// <summary>
        /// state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaObjectState State { get; set; }

        /// <summary>
        /// The discount module id that is promoted to the user
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        [SchemeProperty(IsNullable = true)]
        public long? DiscountModuleId { get; set; }

        /// <summary>
        /// These conditions define the discount that apply one the campaign
        /// </summary>
        [DataMember(Name = "discountConditions")]
        [JsonProperty("discountConditions")]
        [XmlElement(ElementName = "discountConditions")]
        public List<KalturaCondition> DiscountConditions { get; set; }

        /// <summary>
        /// list of free strings to the user that gives information about the campaign.
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData")]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Free text message to the user that gives information about the campaign.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        [SchemeProperty(MaxLength = 1200)]
        public string Message { get; set; }

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
            //TODO - Shir or Matan
            // validate start & end dates

            if (string.IsNullOrEmpty(this.Name) || string.IsNullOrWhiteSpace(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (string.IsNullOrEmpty(this.SystemName) || string.IsNullOrWhiteSpace(this.SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (string.IsNullOrEmpty(this.Message))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "message");
            }

            if (this.DiscountModuleId.HasValue && (this.DiscountConditions == null || this.DiscountConditions.Count == 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "discountConditions");
            }

            if (this.DiscountConditions != null && this.DiscountConditions.Count > 0)
            {
                if (!this.DiscountModuleId.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "discountModuleId");
                }

                foreach (var condition in this.DiscountConditions)
                {
                    condition.Validate();
                }
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        internal override void ValidateForUpdate()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            //get list count of all, if has active 500 and activating the 501 return error
        }
    }

    public partial class KalturaCampaignListResponse : KalturaListResponse<KalturaCampaign>
    {
        public KalturaCampaignListResponse() : base() { }
    }

    public enum KalturaObjectState
    {
        INACTIVE = 0,
        ACTIVE = 1,
        ARCHIVE = 2
    }
}