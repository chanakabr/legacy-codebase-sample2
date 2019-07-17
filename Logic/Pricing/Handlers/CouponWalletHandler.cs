using ApiLogic.Base;
using ApiObjects.Base;
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
    public class CouponWalletHandler : ICrudHandler<CouponWallet, string, CouponWalletFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CouponWalletHandler> lazy = new Lazy<CouponWalletHandler>(() => new CouponWalletHandler());

        public static CouponWalletHandler Instance { get { return lazy.Value; } }

        private CouponWalletHandler() { }

        public GenericResponse<CouponWallet> Add(ContextData contextData, CouponWallet couponWalletToAdd)
        {
            var response = new GenericResponse<CouponWallet>();

            try
            {
                if (string.IsNullOrEmpty(couponWalletToAdd.CouponCode))
                {
                    response.SetStatus(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
                    return response;
                }

                if (!contextData.DomainId.HasValue || contextData.DomainId.Value <= 0)
                {
                    response.Status.Set(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }
                
                // Get Household's Wallet
                List<CouponWallet> couponWalletList = PricingDAL.GetHouseholdCouponWalletCB(contextData.DomainId.Value);
                
                // make sure coupon not already been added
                if (couponWalletList?.Count > 0 && couponWalletList.Count(x => x.CouponCode == couponWalletToAdd.CouponCode) > 0)
                {
                    response.SetStatus(eResponseStatus.CouponCodeAlreadyLoaded, "Coupon code already loaded");
                    return response;
                }

                // Check that code is valid
                CouponDataResponse couponData = Module.GetCouponStatus(contextData.GroupId, couponWalletToAdd.CouponCode, contextData.DomainId.Value);
                if(!Utils.IsCouponValid(couponData))
                {
                    response.SetStatus(eResponseStatus.CouponNotValid, "Coupon code not valid");
                    return response;
                }

                // Add coupon to household                                
                couponWalletToAdd.CouponGroupId = couponData.Coupon.m_oCouponGroup.m_sGroupCode;
                //couponWalletToAdd.CouponId = couponData.Coupon.m_nCouponID; //TODO anat

                couponWalletToAdd.CreateDate = DateTime.UtcNow;
                couponWalletList.Add(couponWalletToAdd);

                // Save CouponWalletAtCB                    
                if (!PricingDAL.SaveHouseholdCouponWalletCB(contextData.DomainId.Value, couponWalletList))
                {
                    log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. contextData: {0}.", contextData.ToString());
                    return response;
                }

                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet. contextData:{0}, couponCode:{1}. ex: {2}",
                                contextData.ToString(), couponWalletToAdd.CouponCode, ex);
            }

            return response;
        }

        public GenericResponse<CouponWallet> Update(ContextData contextData, CouponWallet objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(ContextData contextData, string couponCode)
        {
            Status response = new Status();

            try
            {
                if (string.IsNullOrEmpty(couponCode))
                {
                    response.Set(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
                    return response;
                }

                if (!contextData.DomainId.HasValue || contextData.DomainId.Value <= 0)
                {
                    response.Set(eResponseStatus.HouseholdRequired, "Household required");
                    return response;
                }

                // Get Household's Walt
                List<CouponWallet> CouponWalletList = PricingDAL.GetHouseholdCouponWalletCB(contextData.DomainId.Value);

                // make sure coupon is in household
                if (CouponWalletList?.Count > 0 && CouponWalletList.Count(x => x.CouponCode == couponCode) != 1)
                {
                    response.Set(eResponseStatus.CouponCodeNotInHousehold, "Coupon code not in household");
                    return response;
                }

                // remove coupon from walt
                CouponWalletList.Remove(CouponWalletList.Where(x => x.CouponCode == couponCode).First());

                // Save CouponWalletAtCB                    
                if (!PricingDAL.SaveHouseholdCouponWalletCB(contextData.DomainId.Value, CouponWalletList))
                {
                    log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. domainId:{0}", contextData.DomainId);
                    return response;
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet. contextData:{0}, couponCode:{1}. ex: {2}",
                                contextData.ToString(), couponCode, ex);
            }

            return response;
        }
        
        public GenericResponse<CouponWallet> Get(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }
        
        public GenericListResponse<CouponWallet> List(CouponWalletFilter filter)
        {
            var response = new GenericListResponse<CouponWallet>();
            long? householdId = null;

            try
            {
                // TODO ANAT
                //householdId = filter.
                //response.Status.Set(GetHouseholdFromExtraPrams(extraParams, out householdId));
                if (response.IsOkStatusCode())
                {
                    return response;
                }

                // Get Household's Wallet
                response.Objects = PricingDAL.GetHouseholdCouponWalletCB(householdId.Value);

                response.TotalItems = response.Objects != null ? 0 : response.Objects.Count;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("An Exception was occurred in CouponWallet List. domainId:{0} . ex: {1}", householdId, ex);
            }

            return response;
        }
    }
}