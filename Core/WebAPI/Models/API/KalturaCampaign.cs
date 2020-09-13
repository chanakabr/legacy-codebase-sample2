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
        [SchemeProperty(MinLong = 1)]
        public long StartDate { get; set; }

        /// <summary>
        /// End date of the rule
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(MinLong = 2)]
        public long EndDate { get; set; }

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
        /// state
        /// </summary>
        [DataMember(Name = "state")]
        [JsonProperty("state")]
        [XmlElement(ElementName = "state")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaObjectState State { get; set; }

        /// <summary>
        /// The Promotion that is promoted to the user
        /// </summary>
        [DataMember(Name = "promotion")]
        [JsonProperty("promotion")]
        [XmlElement(ElementName = "promotion")]
        [SchemeProperty(IsNullable = true)]
        public KalturaPromotion Promotion { get; set; }

        /// <summary>
        /// Free text message to the user that gives information about the campaign.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        [SchemeProperty(MaxLength = 1200)]
        public string Message { get; set; }

        // TODO SHIR \ MATAN - if we put null in update it map it with string empty

        /// <summary>
        /// Comma separated collection IDs list
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        public string CollectionIdIn { get; set; }

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
            ValidateDates(false);

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

            if (Promotion != null)
            {
                Promotion.Validate();
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        internal override void ValidateForUpdate()
        {
            ValidateDates(true);

            if (Promotion != null)
            {
                Promotion.Validate();
            }
        }

        private void ValidateDates(bool isUpdate)
        {
            if (StartDate == 0 && EndDate != 0)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "StartDate", "EndDate");
            }

            if (StartDate != 0 && EndDate == 0)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "EndDate", "StartDate");
            }

            var now = TVinciShared.DateUtils.GetUtcUnixTimestampNow();
            if (EndDate <= now && (!isUpdate || this.EndDate != 0))
            {
                throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "EndDate");
            }

            if (StartDate < now && (!isUpdate || this.StartDate != 0))
            {
                throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "StartDate");
            }

            if (EndDate <= StartDate && StartDate != 0 && EndDate != 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "StartDate", "EndDate");
            }
        }

        public List<long> GetCollectionIds()
        {
            if (this.CollectionIdIn == null) { return null; }
            return this.GetItemsIn<List<long>, long>(this.CollectionIdIn, "collectionIdIn");
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