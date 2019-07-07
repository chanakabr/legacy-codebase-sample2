using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Pricing;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    // TODO ANAT(BEO-6931) - ADD ALL relevant KalturaHouseholdCoupon data members and validation

    /// <summary>
    /// Household Coupon details
    /// </summary>
    public partial class KalturaHouseholdCoupon : KalturaOTTObject, IKalturaCrudHandeledObject<DomainCoupon>
    {
        private static readonly DomainCouponHandler domainCouponHandler = new DomainCouponHandler();

        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        public long? HouseholdId { get; set; }

        public ICrudHandler<DomainCoupon> GetHandler()
        {
            return domainCouponHandler;
        }

        public void ValidateForAdd()
        {
            if (this.HouseholdId < 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "householdId");
            }
        }

        public void ValidateForUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}