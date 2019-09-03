using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;
using TVinciShared;

namespace ApiLogic.Api.Managers
{
    public class EventNotificationActionManager : ICrudHandler<EventNotificationAction, string, EventNotificationActionFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<EventNotificationActionManager> lazy = new Lazy<EventNotificationActionManager>(() => new EventNotificationActionManager());

        public static EventNotificationActionManager Instance { get { return lazy.Value; } }

        private EventNotificationActionManager() { }

        public GenericResponse<EventNotificationAction> Add(ContextData contextData, EventNotificationAction eventNotificationActionToAdd)
        {
            var response = new GenericResponse<EventNotificationAction>();

            try
            {
                eventNotificationActionToAdd.CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                eventNotificationActionToAdd.UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);

                // Save CouponWalletAtCB                    
                if (!ApiDAL.SaveEventNotificationActionCB(eventNotificationActionToAdd))
                {
                    log.Error($"Error while SaveEventNotificationActionCB. contextData: {contextData.ToString()}");
                    return response;
                }
                response.Object = eventNotificationActionToAdd;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in SaveEventNotificationActionCB. contextData:{contextData.ToString()}. ex: {ex}");
            }

            return response;
        }

        public GenericResponse<EventNotificationAction> Update(ContextData contextData, EventNotificationAction eventNotificationActionToUpdate)
        {
            var response = new GenericResponse<EventNotificationAction>();

            try
            {
                // get current EventNotificationAction 
                EventNotificationAction currentEventNotificationAction = ApiDAL.GetEventNotificationActionCB(eventNotificationActionToUpdate.Id);
                if (currentEventNotificationAction == null)
                {
                    log.Error($"EventNotificationAction wasn't found id {eventNotificationActionToUpdate.Id}");
                    response.SetStatus(eResponseStatus.EventNotificationIdNotFound);
                    return response;
                }

                eventNotificationActionToUpdate.UpdateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                eventNotificationActionToUpdate.Status = eventNotificationActionToUpdate.Status;
                eventNotificationActionToUpdate.Message = eventNotificationActionToUpdate.Message;
               
                if (!ApiDAL.SaveEventNotificationActionCB(eventNotificationActionToUpdate))
                {
                    log.Error($"Error while saving EventNotificationAction id {eventNotificationActionToUpdate.Id}");
                }
                else
                {
                    response.Object = eventNotificationActionToUpdate;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error($"Update BusinessModuleRule failed ex={ex},  eventNotificationActionId={ eventNotificationActionToUpdate.Id}");
            }

            return response;
        }

        public Status Delete(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<EventNotificationAction> Get(ContextData contextData, string id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<EventNotificationAction> List(ContextData contextData, EventNotificationActionFilter filter)
        {
            throw new NotImplementedException();
        }

        //public GenericResponse<CouponWallet> Add(ContextData contextData, CouponWallet couponWalletToAdd)
        //{
        //    var response = new GenericResponse<CouponWallet>();

        //    try
        //    {
        //        if (string.IsNullOrEmpty(couponWalletToAdd.CouponCode))
        //        {
        //            response.SetStatus(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
        //            return response;
        //        }

        //        if (!contextData.DomainId.HasValue || contextData.DomainId.Value <= 0)
        //        {
        //            response.Status.Set(eResponseStatus.HouseholdRequired, "Household required");
        //            return response;
        //        }

        //        // Get Household's Wallet
        //        List<CouponWallet> couponWalletList = PricingDAL.GetHouseholdCouponWalletCB(contextData.DomainId.Value);

        //        // make sure coupon not already been added
        //        if (couponWalletList?.Count > 0 && couponWalletList.Count(x => x.CouponCode == couponWalletToAdd.CouponCode) > 0)
        //        {
        //            response.SetStatus(eResponseStatus.CouponCodeAlreadyLoaded, "Coupon code already loaded");
        //            return response;
        //        }

        //        if (couponWalletList?.Count == MAX_WALLET_COUPON)
        //        {
        //            response.SetStatus(eResponseStatus.ExceededHouseholdCouponLimit, "Exceeded household coupon limit");
        //            return response;
        //        }

        //        // Check that code is valid
        //        CouponDataResponse couponData = Module.GetCouponStatus(contextData.GroupId, couponWalletToAdd.CouponCode, contextData.DomainId.Value);
        //        if (!Utils.IsCouponValid(couponData))
        //        {
        //            response.SetStatus(eResponseStatus.CouponNotValid, "Coupon code not valid");
        //            return response;
        //        }

        //        // Add coupon to household                                
        //        couponWalletToAdd.CouponGroupId = couponData.Coupon.m_oCouponGroup.m_sGroupCode;
        //        //couponWalletToAdd.CouponId = couponData.Coupon.m_nCouponID; //TODO anat

        //        couponWalletToAdd.CreateDate = DateTime.UtcNow;
        //        if (couponWalletList == null)
        //        {
        //            couponWalletList = new List<CouponWallet>();
        //        }

        //        couponWalletList.Add(couponWalletToAdd);

        //        // Save CouponWalletAtCB                    
        //        if (!PricingDAL.SaveHouseholdCouponWalletCB(contextData.DomainId.Value, couponWalletList))
        //        {
        //            log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. contextData: {0}.", contextData.ToString());
        //            return response;
        //        }
        //        response.Object = couponWalletToAdd;
        //        response.Status.Set(eResponseStatus.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("An Exception was occurred in CouponWallet. contextData:{0}, couponCode:{1}. ex: {2}",
        //                        contextData.ToString(), couponWalletToAdd.CouponCode, ex);
        //    }

        //    return response;
        //}

        //public GenericResponse<CouponWallet> Update(ContextData contextData, CouponWallet objectToUpdate)
        //{
        //    throw new NotImplementedException();
        //}

        //public Status Delete(ContextData contextData, string couponCode)
        //{
        //    Status response = new Status();

        //    try
        //    {
        //        if (string.IsNullOrEmpty(couponCode))
        //        {
        //            response.Set(eResponseStatus.CouponCodeIsMissing, "Coupon code is missing");
        //            return response;
        //        }

        //        if (!contextData.DomainId.HasValue || contextData.DomainId.Value <= 0)
        //        {
        //            response.Set(eResponseStatus.HouseholdRequired, "Household required");
        //            return response;
        //        }

        //        // Get Household's Walt
        //        List<CouponWallet> CouponWalletList = PricingDAL.GetHouseholdCouponWalletCB(contextData.DomainId.Value);

        //        // make sure coupon is in household
        //        if (CouponWalletList == null || CouponWalletList.Count == 0 || CouponWalletList.Count(x => x.CouponCode == couponCode) != 1)
        //        {
        //            response.Set(eResponseStatus.CouponCodeNotInHousehold, "Coupon code not in household");
        //            return response;
        //        }

        //        // remove coupon from walt
        //        CouponWalletList.Remove(CouponWalletList.First(x => x.CouponCode == couponCode));

        //        // Save CouponWalletAtCB                    
        //        if (!PricingDAL.SaveHouseholdCouponWalletCB(contextData.DomainId.Value, CouponWalletList))
        //        {
        //            log.ErrorFormat("Error while SaveHouseholdCouponWalletCB. domainId:{0}", contextData.DomainId);
        //            return response;
        //        }

        //        response.Set(eResponseStatus.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("An Exception was occurred in CouponWallet. contextData:{0}, couponCode:{1}. ex: {2}",
        //                        contextData.ToString(), couponCode, ex);
        //    }

        //    return response;
        //}

        //public GenericResponse<CouponWallet> Get(ContextData contextData, string id)
        //{
        //    throw new NotImplementedException();
        //}

        //public GenericListResponse<CouponWallet> List(ContextData contextData, CouponWalletFilter filter)
        //{
        //    var response = new GenericListResponse<CouponWallet>();
        //    long? householdId = null;

        //    try
        //    {
        //        if (!contextData.DomainId.HasValue || contextData.DomainId.Value <= 0)
        //        {
        //            response.Status.Set(eResponseStatus.HouseholdRequired, "Household required");
        //            return response;
        //        }

        //        householdId = contextData.DomainId.Value;
        //        var shouldFilterByCouponGroupIds = false;
        //        var couponGroupIds = new HashSet<string>();

        //        if (filter != null && filter.BusinessModuleId > 0)
        //        {
        //            shouldFilterByCouponGroupIds = true;
        //            switch (filter.BusinessModuleType)
        //            {
        //                case ApiObjects.eTransactionType.PPV:
        //                    // Get PPV couponGroupIds
        //                    PPVModule ppvModule = Module.GetPPVModuleData(contextData.GroupId, filter.BusinessModuleId.ToString(), string.Empty, string.Empty, string.Empty);
        //                    if (ppvModule?.m_oCouponsGroup != null)
        //                    {
        //                        couponGroupIds.Add(ppvModule.m_oCouponsGroup.m_sGroupCode);
        //                    }
        //                    break;

        //                case ApiObjects.eTransactionType.Subscription:
        //                    // Get Subscription couponGroupIds
        //                    Subscription subscription = Module.GetSubscriptionData(contextData.GroupId, filter.BusinessModuleId.ToString(), string.Empty, string.Empty, string.Empty, false);
        //                    if (subscription?.m_oCouponsGroup != null)
        //                    {
        //                        couponGroupIds.Add(subscription.m_oCouponsGroup.m_sGroupCode);
        //                    }

        //                    if (subscription?.CouponsGroups?.Count > 0)
        //                    {
        //                        foreach (var item in subscription.CouponsGroups.Where(x => (!x.endDate.HasValue || x.endDate.Value >= DateTime.UtcNow)
        //                                                                                 && (!x.startDate.HasValue || x.startDate.Value < DateTime.UtcNow)))
        //                        {
        //                            if (!couponGroupIds.Contains(item.m_sGroupCode))
        //                            {
        //                                couponGroupIds.Add(item.m_sGroupCode);
        //                            }
        //                        }
        //                    }

        //                    break;
        //                case ApiObjects.eTransactionType.Collection:
        //                    // Get Collection couponGroupIds
        //                    Collection collection = Module.GetCollectionData(contextData.GroupId, filter.BusinessModuleId.ToString(), string.Empty, string.Empty, string.Empty, true);
        //                    if (collection?.m_oCouponsGroup != null)
        //                    {
        //                        couponGroupIds.Add(collection.m_oCouponsGroup.m_sGroupCode);
        //                    }

        //                    if (collection?.CouponsGroups?.Count > 0)
        //                    {
        //                        foreach (var item in collection.CouponsGroups.Where(x => (!x.endDate.HasValue || x.endDate.Value >= DateTime.UtcNow)
        //                                                                                 && (!x.startDate.HasValue || x.startDate.Value < DateTime.UtcNow)))
        //                        {
        //                            if (!couponGroupIds.Contains(item.m_sGroupCode))
        //                            {
        //                                couponGroupIds.Add(item.m_sGroupCode);
        //                            }
        //                        }
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        var couponWallet = PricingDAL.GetHouseholdCouponWalletCB(householdId.Value);
        //        if (shouldFilterByCouponGroupIds)
        //        {
        //            if (couponGroupIds.Count > 0 && couponWallet?.Count > 0)
        //            {
        //                response.Objects = couponWallet.Where(x => couponGroupIds.Contains(x.CouponGroupId)).ToList();
        //            }
        //        }
        //        else
        //        {
        //            response.Objects = couponWallet;
        //        }

        //        response.Status.Set(eResponseStatus.OK);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("An Exception was occurred in CouponWallet List. domainId:{0} . ex: {1}", householdId, ex);
        //    }

        //    return response;
        //}
    }

}
