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
    public class CouponWalletHandler : ICrudHandler<CouponWallet>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public GenericResponse<CouponWallet> Add(int groupId, CouponWallet couponWalletToAdd, Dictionary<string, object> funcParams)
        {
            var response = new GenericResponse<CouponWallet>();
            long? householdId = null;

            try
            {
                if (string.IsNullOrEmpty(couponWalletToAdd.CouponCode))
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

                // Get Household's Wallet
                List<CouponWallet> couponWallets = PricingDAL.GetHouseholdCouponWalletCB(householdId.Value);
                
                // make sure coupon not already been added
                if (couponWallets?.Count > 0 && couponWallets.Count(x => x.CouponCode == couponWalletToAdd.CouponCode) > 0)
                {
                    response.SetStatus(eResponseStatus.CouponCodeAlreadyLoaded, "Coupon code already loaded");
                    return response;
                }

                // Check that code is valid
                CouponDataResponse couponData = Module.GetCouponStatus(groupId, couponWalletToAdd.CouponCode, householdId.Value);
                if(!Utils.IsCouponValid(couponData))
                {
                    response.SetStatus(eResponseStatus.CouponNotValid, "Coupon code not valid");
                    return response;
                }

                // Add coupon to household                                
                couponWalletToAdd.CouponGroupId = couponData.Coupon.m_oCouponGroup.m_sGroupCode;
                //couponWalletToAdd.CouponId = couponData.Coupon.m_nCouponID; //TODO anat

                couponWalletToAdd.CreateDate = DateTime.UtcNow;
                couponWallets.Add(couponWalletToAdd);

                // Save CouponWalletAtCB                    
                if (!PricingDAL.SaveHouseholdCouponWalletCB(householdId.Value, couponWallets))
                {
                    log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. groupId: {0}, domainId:{1}", groupId, householdId.Value);
                    return response;
                }

                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet. groupId:{0}, couponCode:{1}. ex: {2}",
                    groupId, couponWalletToAdd.CouponCode, ex);
            }

            return response;
        }

        public GenericResponse<CouponWallet> Update(int groupId, CouponWallet objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(long id, Dictionary<string, object> funcParams)
        {
            Status response = new Status();
            long? householdId = null;
            string couponCode = string.Empty;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("householdId"))
                {
                    householdId = funcParams["householdId"] as long?;
                }

                if (!householdId.HasValue || householdId.Value <= 0)
                {
                    response.Set(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                if (funcParams != null && funcParams.ContainsKey("couponCode"))
                {
                    couponCode = funcParams["couponCode"] as string;
                }

                if (string.IsNullOrEmpty(couponCode))
                {
                    response.Set(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
                    return response;
                }

                // Get Household's Walt
                List<CouponWallet> CouponWallets = PricingDAL.GetHouseholdCouponWalletCB(householdId.Value);

                // make sure coupon is in household
                if (CouponWallets?.Count > 0 && CouponWallets.Count(x => x.CouponCode == couponCode) != 1)
                {
                    response.Set(eResponseStatus.CouponCodeNotInHousehold, "Coupon code not in household");
                    return response;
                }

                // remove coupon from walt
                CouponWallets.Remove(CouponWallets.Where(x => x.CouponCode == couponCode).First());

                // Save CouponWalletAtCB                    
                if (!PricingDAL.SaveHouseholdCouponWalletCB(householdId.Value, CouponWallets))
                {
                    log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. domainId:{0}", householdId.Value);
                    return response;
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet. domainId:{0}, couponCode:{1}. ex: {2}",
                    householdId, couponCode, ex);
            }

            return response;
        }

        public GenericListResponse<CouponWallet> List(int groupId, CouponWalletFilter couponWalletFilter)
        {
            var response = new GenericListResponse<CouponWallet>();

            try
            {
                if (couponWalletFilter == null || couponWalletFilter.HouseholdIdEqual <= 0)
                {
                    response.SetStatus(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                // Get Household's Wallet
                response.Objects = PricingDAL.GetHouseholdCouponWalletCB(couponWalletFilter.HouseholdIdEqual);

                response.TotalItems = response.Objects!= null ? 0 : response.Objects.Count;              
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet List. domainId:{0} . ex: {1}",
                    couponWalletFilter.HouseholdIdEqual, ex);
            }

            return response;
        }

    }
}