using ApiLogic.Pricing.Handlers;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FeatureFlag;

namespace Core.ConditionalAccess
{
    public class GrantManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal const string ERROR_SUBSCRIPTION_NOT_EXSITS = "Subscription \\{0} not exists";
        internal const string ERROR_SUBSCRIPTION_NOT_RENEWABLE = "Subscription \\{0} not renewable";
        internal const string ERROR_SUBSCRIPTION_ALREADY_PURCHASED = "Subscription \\{0} already purchased";

        public static Status GrantEntitlements(BaseConditionalAccess cas, int groupId, string userId, long householdId, int contentId, int productId,
            eTransactionType transactionType, string ip, string udid, bool history)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("GrantEntitlements request: siteguid {0}, contentId {1}, productId {2}, productType {3}, userIp {4}, deviceName {5}",
                !string.IsNullOrEmpty(userId) ? userId : string.Empty,     // {0}
                contentId,                                                     // {1}
                productId,                                                     // {2}   
                transactionType.ToString(),                                    // {3}                
                !string.IsNullOrEmpty(ip) ? ip : string.Empty,         // {4}
                !string.IsNullOrEmpty(udid) ? udid : string.Empty  // {5}
                );

            log.Debug(logString);

            // validate siteguid
            if (string.IsNullOrEmpty(userId))
            {
                status.Set((int)eResponseStatus.InvalidUser, "Illegal user ID");
                log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                return status;
            }

            try
            {
                // validate user
                ResponseStatus userValidStatus = ResponseStatus.OK;
                userValidStatus = Utils.ValidateUser(groupId, userId, ref householdId, false, true);

                if (userValidStatus != ResponseStatus.OK)
                {
                    status = Utils.SetResponseStatus(userValidStatus);
                    log.ErrorFormat("User validation failed: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        status = GrantPPV(cas, groupId, userId, householdId, contentId, productId, ip, udid, history);
                        break;
                    case eTransactionType.Subscription:
                        status = GrantSubscription(cas, groupId, userId, householdId, productId, ip, udid, history, 1, null, null, GrantContext.Grant);
                        break;
                    case eTransactionType.Collection:
                        status = GrantCollection(cas, groupId, userId, householdId, productId, ip, udid, history);
                        break;
                    case eTransactionType.ProgramAssetGroupOffer:
                        status = GrantProgramAssetGroupOffer(cas, groupId, long.Parse(userId), householdId, productId, ip, udid, history);
                        break;
                    default:
                        status.Set((int)eResponseStatus.InvalidProductType, "Illegal product Type");
                        log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                        break;
                }

                if (status != null && status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, householdId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on GrantEntitlements key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GrantEntitlements Error. data: {0}", logString, ex));
            }

            return status;
        }

        internal static Status GrantPPV(BaseConditionalAccess cas, int groupId, string userId, long householdId, int contentId, int productId, string ip,
                                                            string udid, bool saveHistory, DateTime? startDate = null, DateTime? endDate = null)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, contentId {2}, productId {3}, userIp {4}, deviceName {5}, saveHistory {6}",
                !string.IsNullOrEmpty(userId) ? userId : string.Empty,     // {0}
                householdId,                                                   // {1}
                contentId,                                                     // {2}
                productId,                                                     // {3}   
                !string.IsNullOrEmpty(ip) ? ip : string.Empty,         // {4}
                !string.IsNullOrEmpty(udid) ? udid : string.Empty, // {5}
                saveHistory);                                                  // {6}

            try
            {
                // validate content ID
                if (contentId < 1)
                {
                    status.Set((int)eResponseStatus.InvalidContentId, BaseConditionalAccess.ILLEGAL_CONTENT_ID);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // validate content ID related media
                int mediaID = Utils.GetMediaIDFromFileID(contentId, groupId);
                if (mediaID < 1)
                {
                    status.Set((int)eResponseStatus.NoMediaRelatedToFile, BaseConditionalAccess.CONTENT_ID_WITH_NO_RELATED_MEDIA);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // validate PPV 
                PPVModule thePPVModule = null;
                status = Utils.ValidatePPVModuleCode(groupId, productId, contentId, ref thePPVModule);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // validate price
                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;
                var fullPrice = Utils.GetMediaFileFinalPriceForNonGetItemsPrices
                    (contentId, thePPVModule, userId, string.Empty, groupId, ref relevantSub, ref relevantCol, ref relevantPP, string.Empty, string.Empty, udid, true, ip);

                if (fullPrice.PriceReason != PriceReason.ForPurchase && !(fullPrice.PriceReason == PriceReason.SubscriptionPurchased && fullPrice.FinalPrice.m_dPrice > 0))
                {
                    // not for purchase
                    status = Utils.SetResponseStatus(fullPrice.PriceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                if (fullPrice.FinalPrice == null)
                {
                    status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // get country by user IP
                string country = string.IsNullOrEmpty(ip) ? string.Empty : Utils.GetIP2CountryName(groupId, ip);

                // create custom data
                string customData = cas.GetCustomData
                    (relevantSub, thePPVModule, null, userId, fullPrice.FinalPrice.m_dPrice, fullPrice.FinalPrice.m_oCurrency.m_sCurrencyCD3, contentId,
                     mediaID, productId.ToString(), string.Empty, string.Empty, ip, country, string.Empty, udid, householdId, fullPrice.CampaignDetails?.Id ?? 0);

                // purchase
                BillingResponse billingResponse = HandleTransactionPurchase(saveHistory, cas, userId, fullPrice.FinalPrice, ip, customData);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    status.Set((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    cas.WriteToUserLog(userId, "While trying to purchase media file id(CC): " + contentId.ToString() + " error returned: " + billingResponse.m_sStatusDescription);
                    return status;
                }

                long lBillingTransactionID = 0;
                long lPurchaseID = 0;
                string billingGuid = Guid.NewGuid().ToString();

                var result = cas.HandleChargeUserForMediaFileBillingSuccess(userId, Convert.ToInt32(householdId), relevantSub, fullPrice.FinalPrice.m_dPrice, fullPrice.FinalPrice.m_oCurrency.m_sCurrencyCD3,
                    string.Empty, ip, country, string.Empty, udid, billingResponse, customData,
                    thePPVModule, contentId, ref lBillingTransactionID, ref lPurchaseID, true, billingGuid, startDate, endDate);

                if (result)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                if (saveHistory)
                {
                    // Enqueue notification for PS so they know a media file was charged
                    var dicData = new Dictionary<string, object>()
                    {
                        {"MediaFileID", contentId},
                        {"BillingTransactionID", lBillingTransactionID},
                        {"PPVModuleCode", productId},
                        {"SiteGUID", userId},
                        {"CouponCode", string.Empty},
                        {"CustomData", customData},
                        {"PurchaseID", lPurchaseID}
                    };

                    // notify purchase
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData, userId, udid, ip))
                    {
                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }
            }
            catch (Exception ex)
            {
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error("Exception occurred. data: " + logString, ex);
            }

            return status;
        }

        internal static Status GrantSubscription(BaseConditionalAccess cas, int groupId, string userId, long householdId, int productId, string ip, string udid, bool saveHistory,
                                                                    int recurringNumber, DateTime? startDate = null, DateTime? endDate = null, GrantContext context = GrantContext.Grant)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, productId {2}, userIp {3}, deviceName {4}, saveHistory {5}",
                !string.IsNullOrEmpty(userId) ? userId : string.Empty,     // {0}
                householdId,                                                   // {1}                
                productId,                                                     // {2}   
                !string.IsNullOrEmpty(ip) ? ip : string.Empty,         // {3}
                !string.IsNullOrEmpty(udid) ? udid : string.Empty, // {4}
                saveHistory);                                                  // {5}

            try
            {
                // get country by user IP
                string country = string.IsNullOrEmpty(ip) ? string.Empty : Utils.GetIP2CountryName(groupId, ip);

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription subscription = null;
                Price priceResponse = Utils.GetSubscriptionFinalPrice(groupId, productId.ToString(), userId, string.Empty,
                    ref priceReason, ref subscription, country, string.Empty, udid, ip);

                if (priceReason == PriceReason.UnKnown)
                {
                    status.Set((int)eResponseStatus.InvalidOffer, "This subscription is invalid");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                bool entitleToPreview = priceReason == PriceReason.EntitledToPreviewModule;

                if (priceReason != PriceReason.ForPurchase && !entitleToPreview)
                {
                    // item not for purchase
                    status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", !string.IsNullOrEmpty(status.Message) ? status.Message : string.Empty, logString);
                    return status;
                }

                // item is for purchase
                if (priceResponse == null)
                {
                    status.Set((int)eResponseStatus.Error, BaseConditionalAccess.GET_PRICE_ERROR);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // verify that ths addon can be grant - via household have right base subscription
                //validate that user have no DLM or Quota - FOR GRANT ONLY 
                if (context == GrantContext.Grant)
                {
                    if (subscription.Type == SubscriptionType.AddOn && subscription.SubscriptionSetIdsToPriority != null && subscription.SubscriptionSetIdsToPriority.Count > 0)
                    {
                        status = Utils.CanPurchaseAddOn(groupId, householdId, subscription);
                        if (status.Code != (int)eResponseStatus.OK)
                        {
                            return status;
                        }
                    }

                    status = CheckSubscriptionOverlap(cas, groupId, subscription, userId, (int)householdId);
                    if (status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                        return status;
                    }
                }

                // price is validated, create custom data
                string customData = cas.GetCustomDataForSubscription(subscription, null, productId.ToString(), string.Empty, userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3,
                                                                 string.Empty, ip, country, string.Empty, udid, string.Empty,
                                                                 entitleToPreview ? subscription.m_oPreviewModule.m_nID + "" : string.Empty,
                                                                 entitleToPreview, true, recurringNumber, saveHistory, (int)context);

                // purchase
                BillingResponse billingResponse = HandleTransactionPurchase(saveHistory, cas, userId, priceResponse, ip, customData);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    // no status error
                    status.Set((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                long lBillingTransactionID = 0;

                // purchase passed
                long.TryParse(billingResponse.m_sRecieptCode, out lBillingTransactionID);
                long purchaseID = 0;

                TransactionResponse response = null;

                // create new GUID for billing transaction
                string billingGuid = Guid.NewGuid().ToString();

                // grant entitlement
                var result = cas.HandleSubscriptionBillingSuccess(ref response, userId, householdId, subscription, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty,
                    ip, country, udid, lBillingTransactionID, customData, productId, billingGuid.ToString(),
                    entitleToPreview, subscription.m_bIsRecurring, startDate, ref purchaseID, ref endDate, context == GrantContext.Grant ? SubscriptionPurchaseStatus.OK : SubscriptionPurchaseStatus.Switched_To);

                if (!result)
                {
                    status.Set((int)eResponseStatus.PurchasePassedEntitlementFailed, "Failed to insert subscription purchase.");
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                    if (context != GrantContext.Renew && subscription.m_nDomainLimitationModule != 0)
                    {
                        cas.UpdateDLM(householdId, subscription.m_nDomainLimitationModule);
                    }

                    // update Quota
                    if (context != GrantContext.Renew && subscription.m_lServices != null && subscription.m_lServices.Count(x => x.ID == (int)eService.NPVR) > 0)
                    {
                        Utils.HandleNPVRQuota(groupId, subscription, householdId, context == GrantContext.Grant);
                    }

                    if (subscription.m_bIsRecurring)
                    {
                        bool enqueueSuccessful = true;
                        var nextRenewalDate = endDate.Value.AddMinutes(0); // default                                           
                        var endDateUnix = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds((DateTime)endDate);

                        var data = new RenewTransactionData(groupId, userId, purchaseID, billingGuid,
                            TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds((DateTime)endDate), nextRenewalDate);
                        if (PhoenixFeatureFlagInstance.Get().IsRenewUseKronos())
                        {
                            ConditionalAccessDAL.Insert_SubscriptionsPurchasesKronos(purchaseID);
                            
                            log.Debug($"Kronos - Renew purchaseID:{purchaseID}");
                            RenewManager.addEventToKronos(groupId, data);
                        }
                        else
                        {
                            var queue = new RenewTransactionsQueue();
                            enqueueSuccessful &= queue.Enqueue(data,
                                string.Format(BaseConditionalAccess.ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                            if (!enqueueSuccessful)
                            {
                                log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                            }
                            else
                            {
                                log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}", nextRenewalDate, data);
                            }
                        }
                        
                        if (enqueueSuccessful)
                        {
                            PurchaseManager.SendRenewalReminder(groupId, data, householdId);
                        }
                    }
                }

                if (saveHistory)
                {
                    // build notification message
                    var dicData = new Dictionary<string, object>()
                    {
                        {"SubscriptionCode", productId},
                        {"BillingTransactionID", lBillingTransactionID},
                        {"SiteGUID", userId},
                        {"PurchaseID", purchaseID},
                        {"CouponCode", string.Empty},
                        {"CustomData", customData}
                    };

                    // notify purchase
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData, userId, udid, ip))
                    {
                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return status;
        }

        internal static Status GrantCollection(BaseConditionalAccess cas, int groupId, string userId, long householdId, int productId, string ip, string udid, bool saveHistory)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // log request
            string logString = string.Format("Purchase request: siteguid {0}, household {1}, productId {2}, userIp {3}, deviceName {4}, saveHistory {5}",
                !string.IsNullOrEmpty(userId) ? userId : string.Empty,     // {0}
                householdId,                                               // {1}                
                productId,                                                 // {2}   
                !string.IsNullOrEmpty(ip) ? ip : string.Empty,             // {3}
                !string.IsNullOrEmpty(udid) ? udid : string.Empty,         // {4}
                saveHistory);                                              // {5}

            try
            {
                // get country by user IP
                string country = string.IsNullOrEmpty(ip) ? string.Empty : Utils.GetIP2CountryName(groupId, ip);

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = null;
                Collection collection = null;
                priceResponse = Utils.GetCollectionFinalPrice(groupId, productId.ToString(), userId, string.Empty, ref priceReason,
                                                              ref collection, country, string.Empty, udid, string.Empty, ip);

                if (priceReason == PriceReason.UnKnown)
                {
                    status.Set((int)eResponseStatus.InvalidOffer, "This collection is invalid");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                if (priceReason != PriceReason.ForPurchase)
                {
                    // not for purchase
                    status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", !string.IsNullOrEmpty(status.Message) ? status.Message : string.Empty, logString);
                    return status;
                }

                if (priceResponse == null)
                {
                    status.Set((int)eResponseStatus.Error, BaseConditionalAccess.GET_PRICE_ERROR);
                    log.ErrorFormat("Error: {0}, data: {1}", !string.IsNullOrEmpty(status.Message) ? status.Message : string.Empty, logString);

                    return status;
                }

                // price validated, create the Custom Data
                string customData = cas.GetCustomDataForCollection(collection, productId.ToString(), userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty,
                                                               ip, country, string.Empty, udid, string.Empty);

                // purchase
                BillingResponse billingResponse = HandleTransactionPurchase(saveHistory, cas, userId, priceResponse, ip, customData);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    // purchase failed - no status error
                    status.Set((int)eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // purchase passed, update entitlement date
                DateTime entitlementDate = DateTime.UtcNow;
                TransactionResponse response = null;

                // create new GUID for billing_transaction
                string billingGuid = Guid.NewGuid().ToString();

                // grant entitlement
                long lBillingTransactionID = 0;
                long purchaseID = 0;
                var result = cas.HandleCollectionBillingSuccess(ref response, userId, householdId, collection, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty, ip,
                                                                          country, udid, lBillingTransactionID, customData, productId,
                                                                          billingGuid, false, entitlementDate, ref purchaseID);
                if (result)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                if (saveHistory)
                {
                    // entitlement passed - build notification message
                    var dicData = new Dictionary<string, object>()
                    { {"CollectionCode", productId},
                      {"BillingTransactionID", lBillingTransactionID},
                      {"SiteGUID", userId},
                      {"PurchaseID", purchaseID},
                      {"CouponCode", string.Empty},
                      {"CustomData", customData}
                    };

                    // notify purchase
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData, userId, udid, ip))
                    {
                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return status;
        }

        internal static Status SwapSubscription(BaseConditionalAccess cas, int groupId, string userId, int oldSubscriptionCode, int newSubscriptionCode, string ip, string udid, bool history)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            try
            {
                //check if user exists
                DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
                int domainID = 0;

                if (!Utils.IsUserValid(userId, groupId, ref domainID, ref suspendStatus))
                {
                    log.Debug("SwapSubscription - User with siteGuid: " + userId + " does not exist. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, eResponseStatus.UserDoesNotExist.ToString());
                    return response;
                }

                if (suspendStatus == DomainSuspentionStatus.Suspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(groupId, long.Parse(userId)))
                {
                    log.Debug("SwapSubscription - User with siteGuid: " + userId + " Suspended. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UserSuspended, eResponseStatus.UserSuspended.ToString());
                    return response;
                }

                PermittedSubscriptionContainer[] userSubsArray = cas.GetUserPermittedSubscriptions(new List<int>() { int.Parse(userId) }, false, 0, domainID); //get all the valid subscriptions that this user has
                Subscription userSubNew = null;
                PermittedSubscriptionContainer userSubOld = new PermittedSubscriptionContainer();
                //check if old sub exists
                if (userSubsArray != null)
                {
                    userSubOld = userSubsArray.Where(x => x.m_sSubscriptionCode == oldSubscriptionCode.ToString()).FirstOrDefault();
                    if (userSubOld == null)
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Format(ERROR_SUBSCRIPTION_NOT_EXSITS, oldSubscriptionCode));
                        return response;
                    }
                }
                //check if the Subscription has autorenewal  
                if (!userSubOld.m_bRecurringStatus)
                {
                    log.Debug("SwapSubscription - Previous Subscription ID: " + oldSubscriptionCode + " is not renewable. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionNotRenewable, string.Format(ERROR_SUBSCRIPTION_NOT_RENEWABLE, oldSubscriptionCode));
                    return response;
                }

                //check if new subscsription already exists for this user
                PermittedSubscriptionContainer userNewSub = userSubsArray.Where(x => x.m_sSubscriptionCode == newSubscriptionCode.ToString()).FirstOrDefault();
                if (userNewSub != null)
                {
                    log.Debug("SwapSubscription - New Subscription ID: " + newSubscriptionCode + " is already attached to this user. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.UnableToPurchaseSubscriptionPurchased, string.Format(ERROR_SUBSCRIPTION_ALREADY_PURCHASED, newSubscriptionCode));
                    return response;
                }

                Subscription s = null;
                s = Core.Pricing.Module.Instance.GetSubscriptionData(groupId, newSubscriptionCode.ToString(), string.Empty, string.Empty, string.Empty, false, userId);
                if (s == null || string.IsNullOrEmpty(s.m_SubscriptionCode))
                {
                    log.Debug("SwapSubscription - New Subscription ID: " + newSubscriptionCode + " was not found. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Format(ERROR_SUBSCRIPTION_NOT_EXSITS, newSubscriptionCode));
                    return response;
                }
                userSubNew = TVinciShared.ObjectCopier.Clone<Subscription>((Subscription)(s));

                if (!userSubNew.m_bIsRecurring)
                {
                    log.Debug("SwapSubscription - New Subscription ID: " + newSubscriptionCode + " is not renewable. Subscription was not changed");
                    response = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionNotRenewable, string.Format(ERROR_SUBSCRIPTION_NOT_RENEWABLE, newSubscriptionCode));
                    return response;
                }

                // check overlapping DLM or Quota in any of subscriptions (except old one)
                List<string> subCodes = userSubsArray.Where(x => x.m_sSubscriptionCode != oldSubscriptionCode.ToString()).Select(y => y.m_sSubscriptionCode).ToList();

                if (subCodes.Count > 0)
                {
                    response = CheckSubscriptionOverlap(cas, groupId, userSubNew, userId, domainID, subCodes);

                    if (response.Code != (int)eResponseStatus.OK)
                    {
                        log.Debug("SwapSubscription - CheckExistsDlmAndQuota: fail" + response.Message);
                        return response;
                    }
                }
                //set new subscprion
                response = SetSubscriptionSwap(cas, groupId, userId, domainID, userSubNew, userSubOld, ip, udid, history);
            }
            catch (Exception exc)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at SwapSubscription. ");
                sb.Append(String.Concat(" Ex Msg: ", exc.Message));
                sb.Append(String.Concat(" Site Guid: ", userId));
                sb.Append(String.Concat(" New Sub: ", newSubscriptionCode));
                sb.Append(String.Concat(" Old Sub: ", oldSubscriptionCode));
                sb.Append(String.Concat(" Ex Type: ", exc.GetType().Name));
                sb.Append(String.Concat(" ST: ", exc.StackTrace));
                log.Error("Exception - " + sb.ToString(), exc);
                #endregion
            }
            return response;
        }

        //the new subscription is 
        //the previous  subscription is cancled and its end date is set to 'now' with new status 
        private static Status SetSubscriptionSwap(BaseConditionalAccess cas, int groupId, string userId, int houseHoldID, Subscription newSubscription, PermittedSubscriptionContainer oldSubscription, string userIp, string deviceName, bool history)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                // update old subscription with end_date = now                
                bool result = DAL.ConditionalAccessDAL.CancelSubscriptionPurchaseTransaction(userId, int.Parse(oldSubscription.m_sSubscriptionCode), houseHoldID, (int)SubscriptionPurchaseStatus.Switched);
                if (!result)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "CancelSubscriptionPurchaseTransaction fail");
                    return response;
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, houseHoldID));
                ApiObjects.Response.Status status = GrantSubscription(cas, groupId, userId, (long)houseHoldID, int.Parse(newSubscription.m_SubscriptionCode), userIp, deviceName, history, 1, null, oldSubscription.m_dEndDate, GrantContext.Swap);

                if (status.Code != (int)eResponseStatus.OK)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "GrantEntitlements fail");
                    return response;
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDomainEntitlementInvalidationKey(groupId, houseHoldID));
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception in SetSubscriptionSwap. ");
                sb.Append(String.Concat("Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Site Guid: ", userId));
                sb.Append(String.Concat(" Stack Trace: ", ex.StackTrace));
                log.Error("SetSubscriptionSwap - " + sb.ToString(), ex);
                #endregion
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Format("fail to swap subscription {0} to {1}", oldSubscription.m_sSubscriptionCode, newSubscription.m_SubscriptionCode)); //status = ChangeSubscriptionStatus.Error;
            }

            response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }

        private static Status CheckSubscriptionOverlap(BaseConditionalAccess cas, int groupId, Subscription subscription, string userId, int domainId, List<string> SubCodes = null)
        {
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                List<Subscription> permittedSubscriptions = null;

                bool npvrCheck = false;
                bool dlmCheck = subscription.m_nDomainLimitationModule > 0 ? true : false;

                // if subscription for grant contain DLM \ Quota ==> than check overlapping  DLM or Quota in any of subscriptions  in permitted subscription 
                if (subscription.m_lServices != null && subscription.m_lServices.Any(x => x.ID == (long)eService.NPVR))
                {
                    npvrCheck = true;
                }

                if (!npvrCheck && !dlmCheck)
                {
                    return status;
                }

                // need to check other subscriptions 
                //get all permitted subscription (if didn't get) 
                if (SubCodes == null || SubCodes.Count == 0)
                {
                    PermittedSubscriptionContainer[] userSubsArray = cas.GetUserPermittedSubscriptions(new List<int>() { int.Parse(userId) }, false, 0, domainId);
                    if (userSubsArray == null || userSubsArray.Count() == 0)
                    {
                        return status;
                    }
                    SubCodes = userSubsArray.Select(x => x.m_sSubscriptionCode).ToList();

                    if (SubCodes == null && SubCodes.Count == 0)
                    {
                        return status;
                    }
                }

                HashSet<long> subscriptionIds = new HashSet<long>();
                long subId = 0;
                foreach (var item in SubCodes)
                {
                    if (long.TryParse(item, out subId) && !subscriptionIds.Contains(subId))
                    {
                        subscriptionIds.Add(subId);
                    }
                }

                SubscriptionsResponse subscriptionsResponse = Pricing.Module.Instance.GetSubscriptions(groupId, subscriptionIds, string.Empty, string.Empty, string.Empty,
                    new AssetSearchDefinition() { UserId = long.Parse(userId), IsAllowedToViewInactiveAssets = true });

                if (subscriptionsResponse != null &&
                    subscriptionsResponse.Status.Code == (int)eResponseStatus.OK &&
                    subscriptionsResponse.Subscriptions != null &&
                    subscriptionsResponse.Subscriptions.Count() > 0)
                {
                    permittedSubscriptions = subscriptionsResponse.Subscriptions.ToList();
                }

                if (npvrCheck && permittedSubscriptions != null)
                {
                    if (permittedSubscriptions.Count(x => x.m_lServices != null && x.m_lServices.Count(y => y.ID == (long)eService.NPVR) > 0) > 0)
                    {
                        status = new Status((int)eResponseStatus.ServiceAlreadyExists, eResponseStatus.ServiceAlreadyExists.ToString());
                        return status;
                    }
                }

                if (ApplicationConfiguration.Current.ShouldSubscriptionOverlapConsiderDLM.Value && dlmCheck && permittedSubscriptions != null)
                {
                    if (permittedSubscriptions.Select(x => x.m_nDomainLimitationModule > 0).Count() > 0)
                    {
                        status = new Status((int)eResponseStatus.DlmExist, eResponseStatus.DlmExist.ToString());
                        return status;
                    }
                }
            }
            catch
            {
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return status;
        }

        private static BillingResponse HandleTransactionPurchase(bool saveHistory, BaseConditionalAccess cas, string userId, Price price, string ip, string customData)
        {
            BillingResponse billingResponse = new BillingResponse() { m_oStatus = BillingResponseStatus.UnKnown };

            if (saveHistory)
            {
                billingResponse = cas.HandleCCChargeUser(userId, price.m_dPrice, price.m_oCurrency.m_sCurrencyCD3, ip, customData,
                    1, 1, string.Empty, string.Empty, string.Empty, true, false);
            }
            else
            {
                billingResponse.m_oStatus = BillingResponseStatus.Success;
                billingResponse.m_sRecieptCode = string.Empty;
            }

            return billingResponse;
        }

        private static Status GrantProgramAssetGroupOffer(BaseConditionalAccess cas, int groupId, long userId, long householdId, int pagoId, string ip, string udid, bool saveHistory)
        {
            Status status = new Status();

            // log request
            string logString = $"Purchase request: userID {userId}, household {householdId}, productId {pagoId}, userIp {ip}, deviceName {udid}, saveHistory {saveHistory}";

            try
            {
                // get country by user IP
                string country = string.IsNullOrEmpty(ip) ? string.Empty : Utils.GetIP2CountryName(groupId, ip);

                // get pago
                ProgramAssetGroupOffer pago = null;
                pago = PagoManager.Instance.GetProgramAssetGroupOffer(groupId, pagoId);
                if (pago == null)
                {
                    status.Set(eResponseStatus.InvalidOffer, "This programAssetGroupOffer is invalid");
                    log.Warn($"Warning: {status.Message}, data: {logString}");
                    return status;
                }

                if(!PagoManager.Instance.IsPagoAllowed(pago))
                {
                    status.Set(eResponseStatus.NotForPurchase, "Not valid for purchase");
                    log.Warn($"Warning: This programAssetGroupOffer is NotForPurchase , data: {logString}");
                    return status;
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = null;
                priceResponse = PriceManager.GetPagoFinalPrice(groupId, userId, ref priceReason, pago, country, ip);

                if (priceReason == PriceReason.UnKnown)
                {
                    status.Set(eResponseStatus.InvalidOffer, "This programAssetGroupOffer is invalid");
                    log.Warn($"Warning: {status.Message}, data: {logString}");
                    return status;
                }

                if (priceReason != PriceReason.ForPurchase)
                {
                    // not for purchase
                    status = Utils.SetResponseStatus(priceReason);
                    log.Warn($"Warning: {status.Message}, data: {logString}");
                    return status;
                }

                if (priceResponse == null)
                {
                    status.Set(eResponseStatus.Error, BaseConditionalAccess.GET_PRICE_ERROR);
                    log.Warn($"Warning: {status.Message}, data: {logString}");
                    return status;
                }

                // price validated, create the Custom Data
                string customData = cas.GetCustomDataForPago(pago, pagoId, userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, ip, country, string.Empty);

                // purchase
                BillingResponse billingResponse = HandleTransactionPurchase(saveHistory, cas, userId.ToString(), priceResponse, ip, customData);

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    // purchase failed - no status error
                    status.Set(eResponseStatus.PurchaseFailed, BaseConditionalAccess.PURCHASE_FAILED);
                    log.Warn($"Warning: {status.Message}, data: {logString}");
                    return status;
                }

                // purchase passed, update entitlement date
                DateTime entitlementDate = DateTime.UtcNow;
                TransactionResponse response = null;

                // create new GUID for billing_transaction
                string billingGuid = Guid.NewGuid().ToString();

                // grant entitlement
                long lBillingTransactionID = 0;
                long purchaseID = 0;
                var result = cas.HandlePagoBillingSuccess(ref response, userId, householdId, pago, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, ip,
                                                            country, udid, lBillingTransactionID, customData, pagoId, billingGuid, false, entitlementDate, ref purchaseID);
                if (result)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
            }

            return status;
        }
    }
}