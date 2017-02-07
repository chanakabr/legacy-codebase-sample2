using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using Billing;
using CachingProvider.LayeredCache;
using KLogMonitor;
using Pricing;
using QueueWrapper;
using WS_Billing;

namespace ConditionalAccess
{
    public class GrantManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static ApiObjects.Response.Status GrantEntitlements(BaseConditionalAccess cas, int groupId, string userId, long householdId, int contentId, int productId, eTransactionType transactionType,
            string ip, string udid, bool history)
        {
            ApiObjects.Response.Status status = null;
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
                status.Message = "Illegal user ID";
                log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                return status;
            }

            // validate productId
            if (productId < 1)
            {
                status.Message = "Illegal product ID";
                log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                return status;
            }

            try
            {
                // validate user
                ResponseStatus userValidStatus = ResponseStatus.OK;
                userValidStatus = Utils.ValidateUser(groupId, userId, ref householdId);

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
                        status = GrantSubscription(cas, groupId, userId, householdId, productId, ip, udid, history, 1);
                        break;
                    case eTransactionType.Collection:
                        status = GrantCollection(cas, groupId, userId, householdId, productId, ip, udid, history);
                        break;
                    default:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Illegal product Type");
                        log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                        break;
                }

                if (status != null && status.Code == (int)eResponseStatus.OK)
                {
                    string invalidationKey = LayeredCacheKeys.GetGrantEntitlementInvalidationKey(householdId);
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

        internal static ApiObjects.Response.Status GrantPPV(BaseConditionalAccess cas, int groupId, string userId, long householdId, int contentId, int productId, string ip, string udid, bool saveHistory,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

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
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, BaseConditionalAccess.ILLEGAL_CONTENT_ID);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // validate content ID related media
                int mediaID = ConditionalAccess.Utils.GetMediaIDFromFileID(contentId, groupId);
                if (mediaID < 1)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, BaseConditionalAccess.CONTENT_ID_WITH_A_RELATED_MEDIA);
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
                PriceReason ePriceReason = PriceReason.UnKnown;
                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;
                Price oPrice = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(contentId, thePPVModule, userId, string.Empty, groupId,
                                                                                              ref ePriceReason, ref relevantSub, ref relevantCol, ref relevantPP,
                                                                                              string.Empty, string.Empty, udid, true);

                if (ePriceReason != PriceReason.ForPurchase && !(ePriceReason == PriceReason.SubscriptionPurchased && oPrice.m_dPrice > 0))
                {
                    // not for purchase
                    status = Utils.SetResponseStatus(ePriceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                if (oPrice == null)
                {

                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                string country = string.Empty;
                if (!string.IsNullOrEmpty(ip))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, ip);
                }

                // create custom data
                string customData = cas.GetCustomData(relevantSub, thePPVModule, null, userId, oPrice.m_dPrice, oPrice.m_oCurrency.m_sCurrencyCD3, contentId,
                    mediaID, productId.ToString(), string.Empty, string.Empty, ip, country, string.Empty, udid);

                string billingGuid = Guid.NewGuid().ToString();

                // purchase
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                module wsBillingService = null;
                BillingResponse oResponse = new BillingResponse();
                oResponse.m_oStatus = BillingResponseStatus.UnKnown;

                if (saveHistory)
                {

                    cas.InitializeBillingModule(ref wsBillingService, ref sWSUserName, ref sWSPass);

                    oResponse = cas.HandleCCChargeUser(sWSUserName, sWSPass, userId, oPrice.m_dPrice, oPrice.m_oCurrency.m_sCurrencyCD3, ip, customData,
                        1, 1, string.Empty, string.Empty, string.Empty, true, false, ref wsBillingService);
                }
                else
                {
                    oResponse.m_oStatus = BillingResponseStatus.Success;
                    oResponse.m_sRecieptCode = string.Empty;
                }

                if (oResponse == null || oResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    cas.WriteToUserLog(userId, "While trying to purchase media file id(CC): " + contentId.ToString() + " error returned: " + oResponse.m_sStatusDescription);
                    return status;
                }

                long lBillingTransactionID = 0;
                long lPurchaseID = 0;

                var result = cas.HandleChargeUserForMediaFileBillingSuccess(sWSUserName, sWSPass, userId, Convert.ToInt32(householdId), relevantSub, oPrice.m_dPrice, oPrice.m_oCurrency.m_sCurrencyCD3,
                    string.Empty, ip, country, string.Empty, udid, oResponse, customData,
                    thePPVModule, contentId, ref lBillingTransactionID, ref lPurchaseID, true, ref wsBillingService, billingGuid, startDate, endDate);

                if (result)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedMediaFile, dicData))
                    {
                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }

            }
            catch (Exception ex)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
                log.Error("Exception occurred. data: " + logString, ex);
            }
            return status;
        }

        internal static ApiObjects.Response.Status GrantSubscription(BaseConditionalAccess cas, int groupId, string userId, long householdId, int productId, string ip, string udid, bool saveHistory,
                                                                                int recurringNumber, DateTime? startDate = null, DateTime? endDate = null, GrantContext context = GrantContext.Grant)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

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
                string country = string.Empty;
                if (!string.IsNullOrEmpty(ip))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, ip);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Subscription subscription = null;
                Price priceResponse = Utils.GetSubscriptionFinalPrice(groupId, productId.ToString(), userId, string.Empty,
                    ref priceReason, ref subscription, country, string.Empty, udid);

                if (priceReason == PriceReason.UnKnown)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "The subscription is unknown");
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
                    // incorrect price
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                //validate that user have no DLM or Quota - FOR GRANT ONLY 
                if (context == GrantContext.Grant)
                {
                    status = cas.CheckSubscriptionOverlap(subscription, userId);
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
                                                                 entitleToPreview, true, recurringNumber, saveHistory);

                // create new GUID for billing transaction
                string billingGuid = Guid.NewGuid().ToString();

                // purchase
                module wsBillingService = null;
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                BillingResponse billingResponse = new BillingResponse();
                billingResponse.m_oStatus = BillingResponseStatus.UnKnown;
                long lBillingTransactionID = 0;
                if (saveHistory)
                {
                    cas.InitializeBillingModule(ref wsBillingService, ref sWSUserName, ref sWSPass);

                    billingResponse = cas.HandleCCChargeUser(sWSUserName, sWSPass, userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, ip, customData,
                       1, 1, string.Empty, string.Empty, string.Empty, true, false, ref wsBillingService);

                }
                else
                {
                    billingResponse.m_oStatus = BillingResponseStatus.Success;
                    billingResponse.m_sRecieptCode = string.Empty;
                }

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    // no status error
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // purchase passed
                long.TryParse(billingResponse.m_sRecieptCode, out lBillingTransactionID);
                long purchaseID = 0;

                TransactionResponse response = null;

                // grant entitlement
                var result = cas.HandleSubscriptionBillingSuccess(ref response, userId, householdId, subscription, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty,
                    ip, country, udid, lBillingTransactionID, customData, productId, billingGuid.ToString(),
                    entitleToPreview, subscription.m_bIsRecurring, startDate, ref purchaseID, ref endDate, context == GrantContext.Grant ? SubscriptionPurchaseStatus.OK : SubscriptionPurchaseStatus.Switched_To);

                if (result)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    // entitlement passed, update domain DLM with new DLM from subscription or if no DLM in new subscription, with last domain DLM
                    if (context != GrantContext.Renew && subscription.m_nDomainLimitationModule != 0)
                    {
                        cas.UpdateDLM(householdId, subscription.m_nDomainLimitationModule);
                    }

                    // update Quota
                    if (context != GrantContext.Renew && subscription.m_lServices != null && subscription.m_lServices.Where(x => x.ID == (int)eService.NPVR).Count() > 0)
                    {
                        Utils.HandleNPVRQuota(groupId, subscription, householdId, context == GrantContext.Grant);
                    }

                    if (subscription.m_bIsRecurring)
                    {

                        DateTime nextRenewalDate = endDate.Value.AddMinutes(0); // default                                           

                        // enqueue renew transaction
                        RenewTransactionsQueue queue = new RenewTransactionsQueue();
                        RenewTransactionData data = new RenewTransactionData(groupId, userId, purchaseID, billingGuid,
                            TVinciShared.DateUtils.DateTimeToUnixTimestamp((DateTime)endDate), nextRenewalDate);
                        bool enqueueSuccessful = queue.Enqueue(data, string.Format(BaseConditionalAccess.ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                        if (!enqueueSuccessful)
                        {
                            log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                        }
                        else
                            log.DebugFormat("New task created (upon subscription purchase success). next renewal date: {0}, data: {1}", nextRenewalDate, data);
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
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedSubscription, dicData))
                    {
                        log.ErrorFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return status;
        }

        internal static ApiObjects.Response.Status GrantCollection(BaseConditionalAccess cas, int groupId, string userId, long householdId, int productId, string ip, string udid, bool saveHistory)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

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
                string country = string.Empty;
                if (!string.IsNullOrEmpty(ip))
                {
                    // get country by user IP
                    country = Utils.GetIP2CountryName(groupId, ip);
                }

                // validate price
                PriceReason priceReason = PriceReason.UnKnown;
                Price priceResponse = null;
                Collection collection = null;
                priceResponse = Utils.GetCollectionFinalPrice(groupId, productId.ToString(), userId, string.Empty, ref priceReason,
                                                              ref collection, country, string.Empty, udid, string.Empty);

                if (priceReason == PriceReason.UnKnown)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "The collection is unknown");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                bool isEntitledToPreviewModule = priceReason == PriceReason.EntitledToPreviewModule;

                if (priceReason != PriceReason.ForPurchase && !isEntitledToPreviewModule)
                {
                    // not for purchase
                    status = Utils.SetResponseStatus(priceReason);
                    log.ErrorFormat("Error: {0}, data: {1}", !string.IsNullOrEmpty(status.Message) ? status.Message : string.Empty, logString);
                    return status;
                }

                if (priceResponse == null)
                {
                    // incorrect price
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                    log.ErrorFormat("Error: {0}, data: {1}", !string.IsNullOrEmpty(status.Message) ? status.Message : string.Empty, logString);

                    return status;
                }

                // price validated, create the Custom Data
                string customData = cas.GetCustomDataForCollection(collection, productId.ToString(), userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty,
                                                               ip, country, string.Empty, udid, string.Empty);

                // create new GUID for billing_transaction
                string billingGuid = Guid.NewGuid().ToString();

                // purchase
                module wsBillingService = null;
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                BillingResponse billingResponse = new BillingResponse();
                billingResponse.m_oStatus = BillingResponseStatus.UnKnown;

                if (saveHistory)
                {
                    cas.InitializeBillingModule(ref wsBillingService, ref sWSUserName, ref sWSPass);
                    billingResponse = cas.HandleCCChargeUser(sWSUserName, sWSPass, userId, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, ip, customData,
                      1, 1, string.Empty, string.Empty, string.Empty, true, false, ref wsBillingService);
                }
                else
                {
                    billingResponse.m_oStatus = BillingResponseStatus.Success;
                    billingResponse.m_sRecieptCode = string.Empty;
                }

                if (billingResponse == null || billingResponse.m_oStatus != BillingResponseStatus.Success)
                {
                    // purchase failed - no status error
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "purchase failed");
                    log.ErrorFormat("Error: {0}, data: {1}", status.Message, logString);
                    return status;
                }

                // purchase passed, update entitlement date
                DateTime entitlementDate = DateTime.UtcNow;
                TransactionResponse response = null;

                // grant entitlement
                long lBillingTransactionID = 0;
                long purchaseID = 0;
                var result = cas.HandleCollectionBillingSuccess(ref response, userId, householdId, collection, priceResponse.m_dPrice, priceResponse.m_oCurrency.m_sCurrencyCD3, string.Empty, ip,
                                                                          country, udid, lBillingTransactionID, customData, productId,
                                                                          billingGuid, isEntitledToPreviewModule, entitlementDate, ref purchaseID);
                if (result)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
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
                    if (!cas.EnqueueEventRecord(NotifiedAction.ChargedCollection, dicData))
                    {
                        log.DebugFormat("Error while enqueue purchase record: {0}, data: {1}", status.Message, logString);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception occurred. data: " + logString, ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error);
            }
            return status;
        }

    }

}
