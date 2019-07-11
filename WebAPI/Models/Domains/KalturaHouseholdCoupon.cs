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
    // TODO SHIR - CRUD changes
    // TODO ANAT(BEO-6931) - ADD ALL relevant KalturaHouseholdCoupon data members and validation

    /// <summary>
    /// Household Coupon details
    /// </summary>
    public partial class KalturaHouseholdCoupon : KalturaCrudObject<DomainCoupon>
    {
        public ICrudHandler<DomainCoupon> Handler { get { return DomainCouponHandler.Instance; } }
        
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        public long? HouseholdId { get; set; }
        
        public override void ValidateForAdd()
        {
            if (this.HouseholdId < 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "householdId");
            }
        }

        public override void ValidateForUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}