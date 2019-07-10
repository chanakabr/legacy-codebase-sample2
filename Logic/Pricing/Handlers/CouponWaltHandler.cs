using ApiLogic.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Pricing.Handlers
{
    public class CouponWaltHandler : ICrudHandler<CouponWalt>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public GenericResponse<CouponWalt> Add(int groupId, CouponWalt couponWaltToAdd, Dictionary<string, object> funcParams)
        {
            var response = new GenericResponse<CouponWalt>();
            long? householdId = null;

            try
            {
                if (string.IsNullOrEmpty(couponWaltToAdd.CouponCode))
                {
                    response.SetStatus(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
                    return response;
                }

                if (funcParams != null && funcParams.ContainsKey("householdId"))
                {
                    householdId = funcParams["householdId"] as long?;
                }
                
                if(!householdId.HasValue || householdId.Value <= 0)
                {
                    response.SetStatus(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                couponWaltToAdd.DomainId = householdId.Value;

                // Get Household's Walt
                List<CouponWalt> couponWalts = PricingDAL.GetHouseholdCouponWaltCB(couponWaltToAdd.DomainId);
                
                // make sure coupon not already been added
                if (couponWalts?.Count > 0 && couponWalts.Count(x => x.CouponCode == couponWaltToAdd.CouponCode) > 0)
                {
                    response.SetStatus(eResponseStatus.CouponCodeAlreadyLoaded, "Coupon code already loaded");
                    return response;
                }

                // Check that code is valid
                CouponDataResponse couponData = Module.GetCouponStatus(groupId, couponWaltToAdd.CouponCode, couponWaltToAdd.DomainId);
                if(!Utils.IsCouponValid(couponData))
                {
                    response.SetStatus(eResponseStatus.CouponNotValid, "Coupon code not valid");
                    return response;
                }

                // Add coupon to household                                
                couponWaltToAdd.CouponGroupId = couponData.Coupon.m_oCouponGroup.m_sGroupCode;

                couponWaltToAdd.CreateDate = DateTime.UtcNow;
                couponWalts.Add(couponWaltToAdd);

                // Save CouponWaltAtCB                    
                if (!PricingDAL.SaveHouseholdCouponWaltCB(householdId.Value, couponWalts))
                {
                    log.ErrorFormat("Error while SaveHouseholdCouponWaltCB. groupId: {0}, domainId:{1}", groupId, couponWaltToAdd.DomainId);
                    return response;
                }

                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWalt. groupId:{0}, couponCode:{1}. ex: {2}",
                    groupId, couponWaltToAdd.CouponCode, ex);
            }

            return response;
        }

        public GenericResponse<CouponWalt> Update(int groupId, CouponWalt objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(long id)
        {
            throw new NotImplementedException();
        }
    }
}