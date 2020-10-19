using ApiLogic.ConditionalAccess.Modules;
using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using ApiObjects.Pricing;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.ConditionalAccess.Modules;
using Core.ConditionalAccess.Response;
using Core.Pricing;
using Core.Users;
using DAL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;

namespace Core.ConditionalAccess
{
    public class EntitlementManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static EntitlementResponse GetEntitlement(BaseConditionalAccess cas, int groupId, string p_mediaFileId, string userId, bool isCoGuid, string countryCode, string languageCode, string deviceName, bool isRecording)
        {
            EntitlementResponse response = new EntitlementResponse();

            int mediaFileId = 0;
            string strViewLifeCycle = TimeSpan.Zero.ToString();
            string strFullLifeCycle = TimeSpan.Zero.ToString();
            bool isOfflinePlayback = false;
            bool shouldCheckEntitlement = false;
            int domainId = 0;

            try
            {
                if (isCoGuid)
                {
                    if (!Utils.GetMediaFileIDByCoGuid(p_mediaFileId, groupId, userId, ref mediaFileId))
                    {
                        throw new Exception("Failed to retrieve Media File ID from WS Catalog.");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(p_mediaFileId) || !Int32.TryParse(p_mediaFileId, out mediaFileId))
                    {
                        throw new ArgumentException(String.Concat("MediaFileID is in incorrect format: ", p_mediaFileId));
                    }
                }

                if (mediaFileId > 0)
                {
                    if (isRecording)
                    {
                        long householdId = 0;
                        ResponseStatus validateStatus = Utils.ValidateUser(groupId, userId, ref householdId);
                        domainId = (int)householdId;
                        if (validateStatus == ResponseStatus.OK)
                        {
                            int enableCdvr = 0, enableNonEntitled = 0, enableNonExisting = 0;
                            TimeShiftedTvPartnerSettings accountSettings = Utils.GetTimeShiftedTvPartnerSettings(groupId);
                            if (accountSettings != null)
                            // Assuming accountSettings is not null (no errors), all account settings must have values so no need to check .HasValue
                            {
                                enableCdvr = accountSettings.IsCdvrEnabled.Value ? 1 : 0;
                                enableNonEntitled = accountSettings.IsRecordingPlaybackNonEntitledChannelEnabled.Value ? 1 : 0;
                                enableNonExisting = accountSettings.IsRecordingPlaybackNonExistingChannelEnabled.Value ? 1 : 0;
                            }

                            DataTable dt = ConditionalAccessDAL.GetChannelByMediaFileId(groupId, mediaFileId);
                            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                            {
                                DataRow dr = dt.Rows[0];
                                int enableChannelCdvr = ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_CDVR", 0);
                                int enableChannelNonEntitled = ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_RECORDING_PLAYBACK_NON_ENTITLED", 0);

                                if (enableCdvr == 1 && enableChannelCdvr == 2)
                                {
                                    enableCdvr = enableChannelCdvr;
                                }

                                if (enableNonEntitled == 1 && enableChannelNonEntitled == 2)
                                {
                                    enableNonEntitled = enableChannelNonEntitled;
                                }

                                if (enableCdvr == 1 && cas.IsServiceAllowed(domainId, eService.NPVR))
                                {
                                    if (enableNonEntitled == 1)
                                    {
                                        //shouldCheckEntitlement is already false
                                        //bIsOfflinePlayback is already false so no need to assign 
                                        Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle);
                                    }
                                    // if the user has the NPVR service then check entitlements, otherwise he isn't entitled
                                    else
                                    {
                                        shouldCheckEntitlement = true;
                                    }
                                }
                            }
                            else if (enableNonExisting == 1 && cas.IsServiceAllowed(domainId, eService.NPVR))
                            {
                                //shouldCheckEntitlement is already false
                                //bIsOfflinePlayback is already false so no need to assign 
                                Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle);
                            }
                        }
                    }
                    else
                    {
                        shouldCheckEntitlement = true;
                    }

                    if (shouldCheckEntitlement)
                    {
                        List<int> lstUsersIds = Utils.GetAllUsersDomainBySiteGUID(userId, groupId, ref domainId);

                        if (ApplicationConfiguration.Current.LicensedLinksCacheConfiguration.ShouldUseCache.Value && !isRecording)
                        {
                            CachedEntitlementResults cachedEntitlementResults = Utils.GetCachedEntitlementResults(domainId, mediaFileId);
                            if (cachedEntitlementResults != null)
                            {
                                if (cachedEntitlementResults.IsFree)
                                {
                                    Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle, cachedEntitlementResults.EntitlementEndDate);
                                    response.ViewLifeCycle = strViewLifeCycle;
                                    response.FullLifeCycle = strFullLifeCycle;
                                }
                                else
                                {
                                    DateTime now = DateTime.UtcNow;
                                    response.IsOfflinePlayBack = cachedEntitlementResults.IsOfflinePlayback;
                                    DateTime viewEndDate = Utils.GetEndDateTime(cachedEntitlementResults.CreditDownloadedDate, cachedEntitlementResults.ViewLifeCycle);
                                    TimeSpan viewLifeCycleLeft = viewEndDate.Subtract(now);
                                    if (viewLifeCycleLeft.TotalSeconds < 0)
                                    {
                                        viewLifeCycleLeft = new TimeSpan();
                                    }

                                    TimeSpan fullLifeCycleLeft = new TimeSpan();
                                    if (cachedEntitlementResults.TransactionType != eTransactionType.PPV && cachedEntitlementResults.EntitlementEndDate.HasValue)
                                    {
                                        fullLifeCycleLeft = cachedEntitlementResults.EntitlementEndDate.Value.Subtract(now);
                                    }
                                    else if (cachedEntitlementResults.TransactionType == eTransactionType.PPV && cachedEntitlementResults.FullLifeCycle > 0 && cachedEntitlementResults.EntitlementStartDate.HasValue)
                                    {
                                        DateTime endDate = Utils.GetEndDateTime(cachedEntitlementResults.EntitlementStartDate.Value, cachedEntitlementResults.FullLifeCycle);
                                        fullLifeCycleLeft = endDate.Subtract(now);
                                    }

                                    if (fullLifeCycleLeft.TotalMilliseconds < 0)
                                    {
                                        fullLifeCycleLeft = new TimeSpan();
                                    }

                                    response.ViewLifeCycle = viewLifeCycleLeft.TotalMilliseconds > fullLifeCycleLeft.TotalMilliseconds ? fullLifeCycleLeft.ToString() : viewLifeCycleLeft.ToString();
                                    response.FullLifeCycle = fullLifeCycleLeft.ToString();
                                }

                                return response;
                            }
                        }

                        int[] arrMediaFileIDs = { mediaFileId };

                        // check permissions      
                        BlockEntitlementType blockEntitlement = BlockEntitlementType.NO_BLOCK;
                        bool permittedPpv = APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_PPV);
                        bool permittedSubscription = APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(groupId, userId, RolePermissions.PLAYBACK_SUBSCRIPTION);

                        if (!permittedPpv && !permittedSubscription)
                        {
                            blockEntitlement = BlockEntitlementType.BLOCK_ALL;
                        }
                        else if (!permittedPpv)
                        {
                            blockEntitlement = BlockEntitlementType.BLOCK_PPV;
                        }
                        else if (!permittedSubscription)
                        {
                            blockEntitlement = BlockEntitlementType.BLOCK_SUBSCRIPTION;
                        }

                        MediaFileItemPricesContainer[] arrPrices = cas.GetItemsPrices(arrMediaFileIDs, userId, string.Empty, true, languageCode, deviceName, string.Empty, null, blockEntitlement);

                        if (arrPrices != null && arrPrices.Length > 0)
                        {
                            MediaFileItemPricesContainer objPrice = arrPrices[0];

                            // If the item is free
                            if (Utils.IsFreeItem(objPrice))
                            {
                                Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle, objPrice.m_oItemPrices[0].m_dtEndDate);
                            }
                            else if (Utils.IsItemPurchased(objPrice))
                            // Item is not free and also not user is not suspended
                            {
                                bool bIsOfflineStatus = false;
                                string sPPVMCode = string.Empty;
                                int nViewLifeCycle = 0;
                                int nFullLifeCycle = 0;
                                DateTime dtViewDate = new DateTime();
                                DateTime dtNow = DateTime.UtcNow;
                                List<int> lstRelatedMediaFiles = Utils.GetRelatedMediaFiles(objPrice.m_oItemPrices[0], mediaFileId);
                                DateTime? dtEntitlementStartDate = Utils.GetStartDate(objPrice.m_oItemPrices[0]);
                                DateTime? dtEntitlementEndDate = Utils.GetEndDate(objPrice.m_oItemPrices[0]);

                                string sPricingUsername = string.Empty;
                                string sPricingPassword = string.Empty;

                                Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref sPricingUsername, ref sPricingPassword);

                                // Get latest use (watch/download) of the media file. If there was one, continue.
                                if (ConditionalAccessDAL.Get_LatestMediaFilesUse(lstUsersIds, lstRelatedMediaFiles, ref sPPVMCode, ref bIsOfflineStatus, ref dtNow,
                                    ref dtViewDate))
                                {
                                    if (bIsOfflineStatus)
                                    {
                                        string sGroupUsageModuleCode = string.Empty;

                                        if (PricingDAL.Get_GroupUsageModuleCode(groupId, "PRICING_CONNECTION", ref sGroupUsageModuleCode))
                                        {
                                            UsageModule objUsageModule = Utils.GetUsageModuleDataWithCaching(sGroupUsageModuleCode, sPricingUsername, sPricingPassword,
                                                countryCode, languageCode, deviceName, groupId, "GetOfflineUsageModuleData");

                                            if (objUsageModule != null)
                                            {
                                                nViewLifeCycle = objUsageModule.m_tsViewLifeCycle;
                                                nFullLifeCycle = objUsageModule.m_tsMaxUsageModuleLifeCycle;
                                                isOfflinePlayback = objUsageModule.m_bIsOfflinePlayBack;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool bIsSuccess = cas.GetLifeCycleByPPVMCode(countryCode, languageCode, deviceName, ref isOfflinePlayback, sPPVMCode,
                                            ref nViewLifeCycle, ref nFullLifeCycle, sPricingUsername, sPricingPassword);

                                        // If getting didn't succeed for any reason, write to log
                                        if (!bIsSuccess)
                                        {
                                            log.Error(string.Format("Error-{0}",
                                                GetPricingErrLogMsg(sPPVMCode, userId, p_mediaFileId, isCoGuid, countryCode, languageCode, deviceName, eTransactionType.PPV)));
                                        }
                                    }
                                }

                                TimeSpan tsViewLeftSpan = new TimeSpan();
                                // If we found the view cycle (and there was a view), calculate what's left of it
                                // Base date is the view date
                                if (nViewLifeCycle > 0)
                                {
                                    DateTime dtViewEndDate = Utils.GetEndDateTime(dtViewDate, nViewLifeCycle);
                                    tsViewLeftSpan = dtViewEndDate.Subtract(dtNow);
                                    if (tsViewLeftSpan.TotalMilliseconds < 0)
                                        tsViewLeftSpan = new TimeSpan();
                                }

                                eTransactionType eBusinessModuleType = Utils.GetBusinessModuleType(sPPVMCode);
                                TimeSpan tsFullLeftSpan = new TimeSpan();
                                // If it is a subscription, use the end date that is saved in the DB and that was gotten in GetItemPrice
                                if (eBusinessModuleType == eTransactionType.Subscription || eBusinessModuleType == eTransactionType.Collection)
                                {
                                    if (dtEntitlementEndDate.HasValue)
                                    {
                                        tsFullLeftSpan = dtEntitlementEndDate.Value.Subtract(dtNow);
                                        if (tsFullLeftSpan.TotalMilliseconds < 0)
                                            tsFullLeftSpan = new TimeSpan();
                                        strFullLifeCycle = tsFullLeftSpan.ToString();
                                    }
                                }
                                else if (eBusinessModuleType == eTransactionType.PPV)
                                {
                                    // If we found the full cycle, meaning the user purchased the media file, calculate what's left of it
                                    // Base date is purchase date
                                    if (nFullLifeCycle > 0 && dtEntitlementStartDate.HasValue)
                                    {
                                        DateTime dtSubscriptionEndDate = Utils.GetEndDateTime(dtEntitlementStartDate.Value, nFullLifeCycle);
                                        tsFullLeftSpan = dtSubscriptionEndDate.Subtract(dtNow);
                                        if (tsFullLeftSpan.TotalMilliseconds < 0)
                                            tsFullLeftSpan = new TimeSpan();
                                        strFullLifeCycle = tsFullLeftSpan.ToString();
                                    }
                                }

                                if (tsFullLeftSpan.TotalMilliseconds > 0 && tsViewLeftSpan.TotalMilliseconds > tsFullLeftSpan.TotalMilliseconds)
                                {
                                    strViewLifeCycle = tsFullLeftSpan.ToString();
                                }
                                else
                                {
                                    strViewLifeCycle = tsViewLeftSpan.ToString();
                                }
                            }
                        }
                    }

                } // end if nMediaFileID > 0
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetItemLeftLifeCycle. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" MF ID or CG: ", p_mediaFileId));
                sb.Append(String.Concat(" Is CG: ", isCoGuid.ToString().ToLower()));
                sb.Append(String.Concat(" Site Guid: ", userId));
                sb.Append(String.Concat(" Country Cd: ", countryCode));
                sb.Append(String.Concat(" Lng Cd: ", languageCode));
                sb.Append(String.Concat(" Device Name: ", deviceName));
                sb.Append(String.Concat(" this is: ", "EntitelemantManager"));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            response.ViewLifeCycle = strViewLifeCycle;
            response.FullLifeCycle = strFullLifeCycle;
            response.IsOfflinePlayBack = isOfflinePlayback;

            return (response);
        }

        internal static Entitlements UpdateEntitlement(BaseConditionalAccess cas, long domainId, Entitlement entitlement)
        {
            Entitlements response = new Entitlements(new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()));
            int groupId = cas.m_nGroupID;

            try
            {
                if (entitlement == null)
                {
                    log.ErrorFormat("UpdateEntitlement entitlement is null ", "");
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }


                // validate household
                ApiObjects.Response.Status status = Utils.ValidateDomain(groupId, (int)domainId, out Domain domain);
                if (status == null || status.Code != (int)eResponseStatus.OK || domain == null)
                {
                    log.ErrorFormat("UpdateEntitlement ValidateUserAndDomain purchaseID = {0} status.Code = {1},  householdId = {2} ", entitlement.purchaseID, status.Code, domainId);
                    response.status = status;
                    return response;
                }

                switch (entitlement.type)
                {
                    case eTransactionType.Subscription:
                        {
                            // get latest PaymentDetailsTransaction
                            // get entitelment with the current payment gateway id + current payment method id 
                            string billingGuid = string.Empty;
                            DateTime endDateFromDB = DateTime.MaxValue;
                            string userId = null;

                            var subscriptionEntitlementResponse = GetEntitlementById(cas, entitlement.purchaseID, domainId, ref billingGuid, ref endDateFromDB, ref userId, entitlement.paymentGatewayId == 0);

                            if (!subscriptionEntitlementResponse.HasObject())
                            {
                                response.status.Set(subscriptionEntitlementResponse.Status);
                                return response;
                            }

                            var subscriptionEntitlement = subscriptionEntitlementResponse.Object;

                            if (entitlement.endDate > DateTime.MinValue && subscriptionEntitlement.UnifiedPaymentId > 0)
                            {
                                response.status.Set(eResponseStatus.Error, "Cant update end date for unified entitlement");
                                return response;
                            }

                            if (entitlement.paymentGatewayId > 0)
                            {
                                // move here to Billing WS (write all in billing)
                                var changeStatus = Billing.Module.ChangePaymentDetails(groupId, billingGuid, domainId, entitlement.paymentGatewayId, entitlement.paymentMethodId);

                                if (!changeStatus.IsOkStatusCode())
                                {
                                    response.status = changeStatus;
                                    return response;
                                }

                                // complete entitlement details
                                subscriptionEntitlement.paymentGatewayId = entitlement.paymentGatewayId;
                                subscriptionEntitlement.paymentMethodId = entitlement.paymentMethodId;

                                //unified billing cycle updates
                                Utils.HandleUnifiedBillingCycle(groupId, domainId, entitlement.paymentGatewayId, endDateFromDB, entitlement.purchaseID, subscriptionEntitlement.UnifiedPaymentId, 0);

                            }

                            if (entitlement.endDate > DateTime.MinValue)
                            {
                                if (!ConditionalAccessDAL.Update_EntitlementEndDate(groupId, (int)domainId, entitlement.purchaseID, entitlement.endDate, "Update_SubscriptionEndDate"))
                                {
                                    response.status.Set(eResponseStatus.Error, "Failed to Update Entitlement EndDate");
                                    return response;
                                }

                                subscriptionEntitlement.endDate = entitlement.endDate;
                                subscriptionEntitlement.nextRenewalDate = entitlement.endDate;

                                if (entitlement.recurringStatus)
                                {
                                    // enqueue renew transaction
                                    RenewTransactionsQueue queue = new RenewTransactionsQueue();
                                    DateTime nextRenewalDate = entitlement.endDate.AddMinutes(-5);

                                    if (entitlement.paymentGatewayId > 0)
                                    {
                                        var paymentGatewayResponse = Core.Billing.Module.GetPaymentGatewayById(groupId, subscriptionEntitlement.paymentGatewayId);
                                        if (!paymentGatewayResponse.HasObject())
                                        {
                                            nextRenewalDate = entitlement.endDate.AddMinutes(paymentGatewayResponse.Object.RenewalStartMinutes);
                                        }
                                    }

                                    var data = new RenewTransactionData(groupId, userId, subscriptionEntitlement.purchaseID, billingGuid, DateUtils.DateTimeToUtcUnixTimestampSeconds(entitlement.endDate), nextRenewalDate);
                                    bool enqueueSuccessful = queue.Enqueue(data, string.Format(BaseConditionalAccess.ROUTING_KEY_PROCESS_RENEW_SUBSCRIPTION, groupId));
                                    if (!enqueueSuccessful)
                                    {
                                        log.ErrorFormat("Failed enqueue of renew transaction {0}", data);
                                    }
                                    else
                                    {
                                        PurchaseManager.SendRenewalReminder(data, domainId);
                                        log.DebugFormat("New task created (upon UpdateEntitlementEndDate). Next renewal date: {0}, data: {1}", nextRenewalDate, data);
                                    }
                                }

                            }

                            response.status.Set(eResponseStatus.OK);
                            response.entitelments.Add(subscriptionEntitlement);

                            break;
                        }
                    case eTransactionType.PPV:
                        {
                            var entitlements = GetUsersEntitlementPPVItems(cas, cas.m_nGroupID, new List<int>(), false, (int)domainId, true, -1, 0, EntitlementOrderBy.PurchaseDateAsc);
                            if (!entitlements.status.IsOkStatusCode())
                            {
                                response.status.Set(entitlements.status);
                                return response;
                            }

                            if (entitlements.totalItems > 0)
                            {
                                var ppv = entitlements.entitelments.FirstOrDefault(x => x.purchaseID == entitlement.purchaseID);
                                if (ppv != null)
                                {
                                    if (!ConditionalAccessDAL.Update_EntitlementEndDate(groupId, (int)domainId, entitlement.purchaseID, entitlement.endDate, "Update_PpvEndDate"))
                                    {
                                        response.status.Set(eResponseStatus.Error, "Failed to Update Entitlement EndDate");
                                        return response;
                                    }

                                    ppv.endDate = entitlement.endDate;

                                    response.status.Set(eResponseStatus.OK);
                                    response.entitelments.Add(ppv);
                                }
                            }

                            break;
                        }
                    case eTransactionType.Collection:
                        {
                            var entitlements = GetUsersEntitlementCollectionsItems(cas, cas.m_nGroupID, new List<int>(), false, (int)domainId, true, -1, 0, EntitlementOrderBy.PurchaseDateAsc);
                            if (!entitlements.status.IsOkStatusCode())
                            {
                                response.status.Set(entitlements.status);
                                return response;
                            }

                            if (entitlements.totalItems > 0)
                            {
                                var collection = entitlements.entitelments.FirstOrDefault(x => x.purchaseID == entitlement.purchaseID);
                                if (collection != null)
                                {
                                    if (!ConditionalAccessDAL.Update_EntitlementEndDate(groupId, (int)domainId, entitlement.purchaseID, entitlement.endDate, "Update_CollectionEndDate"))
                                    {
                                        response.status.Set(eResponseStatus.Error, "Failed to Update Entitlement EndDate");
                                        return response;
                                    }

                                    collection.endDate = entitlement.endDate;

                                    response.status.Set(eResponseStatus.OK);
                                    response.entitelments.Add(collection);
                                }
                            }

                            break;
                        }
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateEntitlement failed ex={0}, GroupID={1}, domainID={2}, purchaseID={3}, paymentGatewayId={4}, paymentMethodId={5} ", ex, groupId, domainId,
                    entitlement.purchaseID, entitlement.paymentGatewayId, entitlement.paymentMethodId);
                response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            if (response.status.IsOkStatusCode())
            {
                string invalidationKey = LayeredCacheKeys.GetPurchaseInvalidationKey(domainId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on Purchase key = {0}", invalidationKey);
                }
            }

            return response;
        }

        private static GenericResponse<Entitlement> GetEntitlementById(BaseConditionalAccess cas, long purchaseId, long domainId, ref string billingGuid,
            ref DateTime endDateFromDB, ref string userId, bool includeNonRecurring = false)
        {
            var response = new GenericResponse<Entitlement>();
            DataRow dr = ConditionalAccessDAL.GetPurchaseByID((int)purchaseId);
            if (dr == null)
            {
                log.ErrorFormat("GetEntitlementById - InvalidPurchase. purchaseId:{0}.", purchaseId);
                response.SetStatus(eResponseStatus.InvalidPurchase);
                return response;
            }

            // ask if renewable + subscription related to domain 
            if (ODBCWrapper.Utils.GetIntSafeVal(dr, "domain_id") != domainId)
            {
                log.ErrorFormat("GetEntitlementById - InvalidPurchase: purchaseID {0} not belong to householdId {1}.", purchaseId, domainId);
                response.SetStatus(eResponseStatus.InvalidPurchase);
                return response;
            }

            var subscriptionEntitlement = CreateSubscriptionEntitelment(cas, dr, false, null);

            if (!subscriptionEntitlement.recurringStatus)
            {
                if (includeNonRecurring)
                {
                    response.Object = subscriptionEntitlement;
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    log.DebugFormat("GetEntitlementById - subscription for purchaseID {0} is not recurring.", purchaseId);
                    response.SetStatus(eResponseStatus.SubscriptionNotRenewable);
                }
            }
            else
            {
                billingGuid = ODBCWrapper.Utils.GetSafeStr(dr, "BILLING_GUID");
                endDateFromDB = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");
                userId = ODBCWrapper.Utils.GetSafeStr(dr, "SITE_USER_GUID");
                response.Object = subscriptionEntitlement;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        internal static Entitlements GetUsersEntitlementSubscriptionsItems(BaseConditionalAccess cas, int groupId, List<int> userIds, bool isExpired, int domainId, bool shouldCheckByDomain, int pageSize, int pageIndex, EntitlementOrderBy orderBy)
        {
            Entitlements entitlementsResponse = new Entitlements();

            try
            {
                // Get domainID from one of the users
                if (shouldCheckByDomain && domainId == 0 && userIds.Count > 0)
                {
                    UserResponseObject user = Utils.GetExistUser(userIds.First().ToString(), groupId);
                    if (user != null && user.m_RespStatus == ResponseStatus.OK && user.m_user != null)
                    {
                        domainId = user.m_user.m_domianID;
                    }
                }

                DataTable allSubscriptionsPurchases = ConditionalAccessDAL.Get_UsersPermittedSubscriptions(userIds, isExpired, domainId, (int)orderBy);

                if (allSubscriptionsPurchases == null || allSubscriptionsPurchases.Rows == null || allSubscriptionsPurchases.Rows.Count == 0)
                {
                    entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no permitted items");
                    return entitlementsResponse;
                }

                HashSet<long> subIds = new HashSet<long>();
                long subId = 0;

                foreach (DataRow dr in allSubscriptionsPurchases.Rows)
                {
                    subId = ODBCWrapper.Utils.GetLongSafeVal(dr, "SUBSCRIPTION_CODE");
                    if (!subIds.Contains(subId))
                    {
                        subIds.Add(subId);
                    }
                }

                var result = Api.api.GetObjectVirtualAssetObjectIds(groupId, new AssetSearchDefinition(), ObjectVirtualAssetInfoType.Subscription, subIds);
                if (result.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    entitlementsResponse.status = result.Status;
                    return entitlementsResponse;
                }

                if (result.ResultStatus == ObjectVirtualAssetFilterStatus.None || result.ObjectIds == null || result.ObjectIds.Count == 0)
                {
                    entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no permitted items");
                    return entitlementsResponse;
                }

                List<DataRow> rows = new List<DataRow>();
                foreach (DataRow dr in allSubscriptionsPurchases.Rows)
                {
                    subId = ODBCWrapper.Utils.GetLongSafeVal(dr, "SUBSCRIPTION_CODE");
                    if (result.ObjectIds.Contains(subId))
                    {
                        rows.Add(dr);
                    }
                }

                entitlementsResponse.totalItems = rows.Count;

                //Get Iteration Rows according page size and index
                IEnumerable<DataRow> iterationRows = GetIterationRows(pageSize, pageIndex, rows);

                // get all builingGuid from subscription 
                List<string> billingGuids = (from row in rows
                                             where row.Field<int>("IS_RECURRING_STATUS") == 1
                                             select row.Field<string>("BILLING_GUID")).ToList(); // only renewable subscriptions 
                List<PaymentDetails> renewPaymentDetails = null;
                if (billingGuids != null && billingGuids.Count > 0)
                {
                    // call billing service to get all transaction payment details
                    renewPaymentDetails = Core.Billing.Module.GetPaymentDetails(groupId, billingGuids);
                }

                List<long> purchaseIds = (from row in iterationRows
                                          select row.Field<long>("ID")).ToList(); // 
                Dictionary<long, long> purchaseIdToScheduledSubscriptionId = Utils.GetPurchaseIdToScheduledSubscriptionIdMap(groupId, domainId, purchaseIds, SubscriptionSetModifyType.Downgrade);
                ConditionalAccess.Response.Entitlement entitlement = null;
                foreach (DataRow dr in iterationRows)
                {
                    entitlement = CreateSubscriptionEntitelment(cas, dr, isExpired, renewPaymentDetails, purchaseIdToScheduledSubscriptionId);
                    if (entitlement != null && long.TryParse(entitlement.entitlementId, out subId))
                    {
                        entitlementsResponse.entitelments.Add(entitlement);
                    }
                }

                entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Exception at GetUserPermittedSubscriptions. {0} - ", ex);
                entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return entitlementsResponse;
        }

        internal static Entitlements GetUsersEntitlementPPVItems(BaseConditionalAccess cas, int groupId, List<int> lUsersIDs, bool isExpired, int domainID, bool shouldCheckByDomain, int pageSize, int pageIndex, EntitlementOrderBy orderBy)
        {
            Entitlements entitlementsResponse = new Entitlements();
            List<int> mediaFileIds = new List<int>();
            try
            {
                // Get domainID from one of the users
                if (shouldCheckByDomain && domainID == 0 && lUsersIDs.Count > 0)
                {
                    UserResponseObject user = Utils.GetExistUser(lUsersIDs.First().ToString(), groupId);
                    if (user != null && user.m_RespStatus == ResponseStatus.OK && user.m_user != null)
                    {
                        domainID = user.m_user.m_domianID;
                    }
                }

                DataTable allPPVModules = ConditionalAccessDAL.Get_All_Users_PPV_modules(lUsersIDs, isExpired, domainID, (int)orderBy);
                if (allPPVModules == null || allPPVModules.Rows == null || allPPVModules.Rows.Count == 0)
                {
                    entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no permitted items");
                    return entitlementsResponse;
                }

                entitlementsResponse.totalItems = allPPVModules.Rows.Count;

                //Get Iteration Rows according page size and index
                IEnumerable<DataRow> iterationRows = GetIterationRows(pageSize, pageIndex, allPPVModules);

                ConditionalAccess.Response.Entitlement entitlement = null;
                foreach (DataRow dr in iterationRows)
                {
                    entitlement = CreatePPVEntitelment(cas, dr);
                    if (entitlement != null)
                    {
                        if (!mediaFileIds.Contains(entitlement.mediaFileID))
                        {
                            mediaFileIds.Add(entitlement.mediaFileID);
                        }
                        entitlementsResponse.entitelments.Add(entitlement);
                    }
                }

                MeidaMaper[] mapper = Utils.GetMediaMapper(groupId, mediaFileIds.ToArray());


                foreach (ConditionalAccess.Response.Entitlement entitlementRes in entitlementsResponse.entitelments)
                {
                    int mediaID = mapper.Where(x => x.m_nMediaFileID == entitlementRes.mediaFileID).FirstOrDefault().m_nMediaID;
                    entitlementRes.mediaID = mediaID;
                }

                entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Exception at GetUserPermittedItems. {0} - ", ex);
                entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return entitlementsResponse;
        }

        internal static Entitlements GetUsersEntitlementCollectionsItems(BaseConditionalAccess cas, int groupId, List<int> userIds, bool isExpired, int domainID, bool shouldCheckByDomain, int pageSize, int pageIndex, EntitlementOrderBy orderBy)
        {
            Entitlements entitlementsResponse = new Entitlements();
            try
            {
                // Get domainID from one of the users
                if (shouldCheckByDomain && domainID == 0 && userIds.Count > 0)
                {
                    UserResponseObject user = Utils.GetExistUser(userIds.First().ToString(), groupId);
                    if (user != null && user.m_RespStatus == ResponseStatus.OK && user.m_user != null)
                    {
                        domainID = user.m_user.m_domianID;
                    }
                }

                DataTable allCollectionsPurchases = ConditionalAccessDAL.Get_UsersPermittedCollections(userIds, isExpired, domainID, (int)orderBy);
                if (allCollectionsPurchases == null || allCollectionsPurchases.Rows == null || allCollectionsPurchases.Rows.Count == 0)
                {
                    entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no permitted items");
                    return entitlementsResponse;
                }

                entitlementsResponse.totalItems = allCollectionsPurchases.Rows.Count;

                //Get Iteration Rows according page size and index
                IEnumerable<DataRow> iterationRows = GetIterationRows(pageSize, pageIndex, allCollectionsPurchases);

                ConditionalAccess.Response.Entitlement entitlement = null;
                foreach (DataRow dr in iterationRows)
                {
                    entitlement = CreateCollectionEntitelment(cas, dr, isExpired);
                    if (entitlement != null)
                    {
                        entitlementsResponse.entitelments.Add(entitlement);
                    }
                }
                entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Exception at GetUserPermittedCollections {0} - ", ex);
                entitlementsResponse.status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            return entitlementsResponse;
        }

        private static Entitlement CreateSubscriptionEntitelment(BaseConditionalAccess cas, DataRow dataRow, bool isExpired, List<PaymentDetails> renewPaymentDetails, Dictionary<long, long> purchaseIdToScheduledSubscriptionId = null)
        {
            var entitlement = new Entitlement()
            {
                type = eTransactionType.Subscription,
                entitlementId = ODBCWrapper.Utils.GetSafeStr(dataRow, "SUBSCRIPTION_CODE"),
                currentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "NUM_OF_USES"),
                maxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_NUM_OF_USES"),
                purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "START_DATE"),
                lastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "LAST_VIEW_DATE"),
                recurringStatus = false,
                nextRenewalDate = DateTime.MaxValue,
                IsInGracePeriod = false,
                isRenewable = false,
                currentDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "cDate"),
                purchaseID = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ID"),
                deviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow, "device_name"),
                deviceName = string.Empty,
                mediaFileID = 0,
                IsSuspended = ODBCWrapper.Utils.GetIntSafeVal(dataRow["subscription_status"]) == (int)SubscriptionPurchaseStatus.Suspended,
                UnifiedPaymentId = ODBCWrapper.Utils.GetLongSafeVal(dataRow, "unified_process_id"),
                IsPending = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_PENDING") == 1
            };

            DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "END_DATE");

            // check whether subscription is in its grace period
            int gracePeriodMinutes = ODBCWrapper.Utils.GetIntSafeVal(dataRow["GRACE_PERIOD_MINUTES"]);
            if (!isExpired && endDate < DateTime.UtcNow)
            {
                endDate = endDate.AddMinutes(gracePeriodMinutes);
                entitlement.IsInGracePeriod = true;
            }

            string billingGuid = ODBCWrapper.Utils.GetSafeStr(dataRow, "BILLING_GUID");

            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_RECURRING_STATUS") == 1)
            {
                entitlement.recurringStatus = true;
                entitlement.nextRenewalDate = endDate;

                // get renew payment details 
                if (renewPaymentDetails != null)
                {
                    var paymentDetails = renewPaymentDetails.FirstOrDefault(x => x.BillingGuid == billingGuid);
                    if (paymentDetails != null)
                    {
                        entitlement.paymentGatewayId = paymentDetails.PaymentGatewayId;
                        entitlement.paymentMethodId = paymentDetails.PaymentMethodId;
                    }
                }
            }

            if (isExpired && entitlement.maxUses != 0 && entitlement.currentUses >= entitlement.maxUses)
            {
                endDate = entitlement.lastViewDate;
            }
            entitlement.endDate = endDate;

            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow["IS_RECURRING"]) == 1)
            {
                entitlement.isRenewable = true;
            }

            // user didn't waiver yet and didn't use the PPV yet
            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER") == 0 && entitlement.lastViewDate < entitlement.purchaseDate)
            {
                bool cancellationWindow = false;
                UsageModule oUsageModule = null;
                cas.IsCancellationWindow(ref oUsageModule, entitlement.entitlementId, entitlement.purchaseDate, ref cancellationWindow, eTransactionType.Subscription);
                entitlement.cancelWindow = cancellationWindow;
            }

            entitlement.paymentMethod = Utils.GetBillingTransMethod(ODBCWrapper.Utils.GetIntSafeVal(dataRow, "billing_transaction_id"), billingGuid);

            if (!string.IsNullOrEmpty(entitlement.deviceUDID))
            {
                entitlement.deviceName = Utils.GetDeviceName(entitlement.deviceUDID);
            }

            if (purchaseIdToScheduledSubscriptionId != null && purchaseIdToScheduledSubscriptionId.ContainsKey(entitlement.purchaseID))
            {
                entitlement.ScheduledSubscriptionId = purchaseIdToScheduledSubscriptionId[entitlement.purchaseID];
            }

            return entitlement;
        }

        private static string GetPricingErrLogMsg(string businessModuleCode, string siteGuid, string mediaFileIDStr, bool isCoGuid, string countryCd, string langCode,
            string deviceName, eTransactionType businessModuleType)
        {
            StringBuilder sb = new StringBuilder("Failed to retrieve business module code from WS Pricing at GetItemLeftViewLifeCycle. ");
            sb.Append(String.Concat(" BM Cd: ", businessModuleCode));
            sb.Append(String.Concat(" BM Type: ", businessModuleType.ToString().ToLower()));
            sb.Append(String.Concat(" SG: ", siteGuid));
            sb.Append(String.Concat(" MF: ", mediaFileIDStr));
            sb.Append(String.Concat(" Is CG: ", isCoGuid.ToString().ToLower()));
            sb.Append(String.Concat(" Country Cd: ", countryCd));
            sb.Append(String.Concat(" Lng Cd: ", langCode));
            sb.Append(String.Concat(" Device Name: ", deviceName));

            return sb.ToString();
        }

        private static IEnumerable<DataRow> GetIterationRows(int pageSize, int pageIndex, DataTable usersPermittedItems)
        {
            IEnumerable<DataRow> iterationRows = null;
            List<DataRow> filteredRows = null;

            if (pageIndex > 0 && pageSize > 0)
            {
                filteredRows = usersPermittedItems.AsEnumerable().Skip(pageSize * pageIndex).Take(pageSize).ToList();
                //int takeTop = pageIndex * pageSize;
                //Int64 maxTransactionID = (from row in usersPermittedItems.AsEnumerable().Take(takeTop)
                //                          select row.Field<Int64>("ID")).ToList().Min();
                //filteredRows = (from row in usersPermittedItems.AsEnumerable()
                //                where (Int64)row["ID"] < maxTransactionID
                //                select row).Take(pageSize).ToList();
            }

            if (filteredRows != null)
            {
                iterationRows = filteredRows;
            }
            else if (pageSize > -1)
            {
                iterationRows = usersPermittedItems.AsEnumerable().Take(pageSize);
            }
            else
            {
                iterationRows = usersPermittedItems.AsEnumerable();
            }

            return iterationRows;
        }

        private static IEnumerable<DataRow> GetIterationRows(int pageSize, int pageIndex, List<DataRow> usersPermittedItems)
        {
            IEnumerable<DataRow> iterationRows = null;
            List<DataRow> filteredRows = null;

            if (pageIndex > 0 && pageSize > 0)
            {
                filteredRows = usersPermittedItems.AsEnumerable().Skip(pageSize * pageIndex).Take(pageSize).ToList();
                //int takeTop = pageIndex * pageSize;
                //Int64 maxTransactionID = (from row in usersPermittedItems.AsEnumerable().Take(takeTop)
                //                          select row.Field<Int64>("ID")).ToList().Min();
                //filteredRows = (from row in usersPermittedItems.AsEnumerable()
                //                where (Int64)row["ID"] < maxTransactionID
                //                select row).Take(pageSize).ToList();
            }

            if (filteredRows != null)
            {
                iterationRows = filteredRows;
            }
            else if (pageSize > -1)
            {
                iterationRows = usersPermittedItems.AsEnumerable().Take(pageSize);
            }
            else
            {
                iterationRows = usersPermittedItems.AsEnumerable();
            }

            return iterationRows;
        }

        private static ConditionalAccess.Response.Entitlement CreatePPVEntitelment(BaseConditionalAccess cas, DataRow dataRow)
        {
            UsageModule oUsageModule = null;
            ConditionalAccess.Response.Entitlement entitlement = new ConditionalAccess.Response.Entitlement();

            entitlement.type = eTransactionType.PPV;
            entitlement.entitlementId = ODBCWrapper.Utils.GetSafeStr(dataRow, "ppv");
            entitlement.currentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "NUM_OF_USES");

            entitlement.endDate = new DateTime(2099, 1, 1);
            if (dataRow["END_DATE"] != null && dataRow["END_DATE"] != DBNull.Value)
                entitlement.endDate = (DateTime)(dataRow["END_DATE"]);

            entitlement.currentDate = DateTime.UtcNow;
            if (dataRow["cDate"] != null && dataRow["cDate"] != DBNull.Value)
                entitlement.currentDate = (DateTime)(dataRow["cDate"]);

            entitlement.lastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow["LAST_VIEW_DATE"]);

            entitlement.purchaseDate = DateTime.UtcNow;
            if (dataRow["START_DATE"] != null && dataRow["START_DATE"] != DBNull.Value)
                entitlement.purchaseDate = (DateTime)(dataRow["START_DATE"]);

            entitlement.purchaseID = (int)ODBCWrapper.Utils.GetLongSafeVal(dataRow, "ID");
            entitlement.paymentMethod = Utils.GetBillingTransMethod(ODBCWrapper.Utils.GetIntSafeVal(dataRow, "billing_transaction_id"), ODBCWrapper.Utils.GetSafeStr(dataRow, "BILLING_GUID"));
            entitlement.deviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow, "device_name");
            if (!string.IsNullOrEmpty(entitlement.deviceUDID))
            {
                entitlement.deviceName = Utils.GetDeviceName(entitlement.deviceUDID);
            }

            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER") == 0 &&
                entitlement.lastViewDate < entitlement.purchaseDate)// user didn't waiver yet and didn't use the PPV yet
            {
                bool cancellationWindow = false;
                cas.IsCancellationWindow(ref oUsageModule, entitlement.entitlementId, entitlement.purchaseDate, ref cancellationWindow, eTransactionType.PPV);
                entitlement.cancelWindow = cancellationWindow;
            }

            entitlement.maxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_NUM_OF_USES");
            entitlement.mediaFileID = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MEDIA_FILE_ID");
            entitlement.mediaID = 0;
            entitlement.IsPending = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_PENDING") == 1;

            return entitlement;

        }

        private static ConditionalAccess.Response.Entitlement CreateCollectionEntitelment(BaseConditionalAccess cas, DataRow dataRow, bool isExpired)
        {
            UsageModule oUsageModule = null;
            ConditionalAccess.Response.Entitlement entitlement = new ConditionalAccess.Response.Entitlement();

            entitlement.type = eTransactionType.Collection;
            entitlement.entitlementId = ODBCWrapper.Utils.GetSafeStr(dataRow, "COLLECTION_CODE");

            entitlement.endDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "END_DATE");
            entitlement.maxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_NUM_OF_USES");
            entitlement.currentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "NUM_OF_USES");
            entitlement.lastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "LAST_VIEW_DATE");

            if (isExpired && entitlement.maxUses != 0 && entitlement.currentUses >= entitlement.maxUses)
            {
                entitlement.endDate = entitlement.lastViewDate;
            }

            entitlement.currentDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "cDate");
            entitlement.purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "CREATE_DATE");
            entitlement.purchaseID = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ID");
            entitlement.paymentMethod = Utils.GetBillingTransMethod(ODBCWrapper.Utils.GetIntSafeVal(dataRow, "billing_transaction_id"), ODBCWrapper.Utils.GetSafeStr(dataRow, "BILLING_GUID"));
            entitlement.deviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow, "device_name");
            entitlement.deviceName = string.Empty;
            if (!string.IsNullOrEmpty(entitlement.deviceUDID))
            {
                entitlement.deviceName = Utils.GetDeviceName(entitlement.deviceUDID);
            }

            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER") == 0 &&
              entitlement.lastViewDate < entitlement.purchaseDate)// user didn't waiver yet and didn't use the PPV yet
            {
                bool cancellationWindow = false;
                cas.IsCancellationWindow(ref oUsageModule, entitlement.entitlementId, entitlement.purchaseDate, ref cancellationWindow, eTransactionType.Collection);
                entitlement.cancelWindow = cancellationWindow;
            }

            entitlement.IsPending = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_PENDING") == 1;

            return entitlement;
        }

        public static CompensationResponse AddCompensation(BaseConditionalAccess cas, int groupId, string userId, Compensation compensation)
        {
            CompensationResponse response = new CompensationResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                // validate user
                int domainId = 0;
                DomainSuspentionStatus suspendStatus = DomainSuspentionStatus.OK;
                if (!Utils.IsUserValid(userId, groupId, ref domainId, ref suspendStatus))
                {
                    log.ErrorFormat("Invalid user, userId = {0}", userId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidUser, "Invalid user");
                    return response;
                }
                if (suspendStatus == DomainSuspentionStatus.Suspended)
                {
                    log.ErrorFormat("Domain suspended, userId = {0}", userId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DomainSuspended, "Domain suspended");
                    return response;
                }

                // make sure compensation doesn't already exists for purchaseId
                Compensation currentCompensation = ConditionalAccessDAL.GetSubscriptionCompensationByPurchaseId(compensation.PurchaseId);
                if (currentCompensation != null)
                {
                    log.ErrorFormat("Compensation already exists for purchaseId = {0}", compensation.PurchaseId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CompensationAlreadyExists, "Compensation already exists");
                    return response;
                }

                // validate purchase
                DataRow subscriptionPurchaseRow = ConditionalAccessDAL.GetPurchaseByID(compensation.PurchaseId);

                if (subscriptionPurchaseRow == null)
                {
                    log.ErrorFormat("Subscription purchase not found purchaseId = {0}", compensation.PurchaseId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidPurchase, "Invalid purchase");
                    return response;
                }

                if (ODBCWrapper.Utils.GetIntSafeVal(subscriptionPurchaseRow, "domain_id") != domainId)
                {
                    log.ErrorFormat("Purchase does not belong to householdId = {0}, purchaseId = {1} ", domainId, compensation.PurchaseId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidPurchase, "Invalid purchase");
                    return response;
                }

                bool isRecurring = ODBCWrapper.Utils.GetIntSafeVal(subscriptionPurchaseRow, "is_recurring_status") == 1;
                if (!isRecurring)
                {
                    log.ErrorFormat("Subscription is not renewable for purchaseId = {0}", compensation.PurchaseId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionNotRenewable, "Subscription is not renewable");
                    return response;
                }

                DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(subscriptionPurchaseRow, "END_DATE");
                if (endDate < DateTime.UtcNow)
                {
                    log.ErrorFormat("Household is not entitled for purchaseId = {0}, householdId = {1}", compensation.PurchaseId, domainId);
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NotEntitled, "Not entitled");
                    return response;
                }

                // get subscription
                compensation.SubscriptionId = ODBCWrapper.Utils.GetIntSafeVal(subscriptionPurchaseRow, "SUBSCRIPTION_CODE");
                Subscription subscription = Utils.GetSubscription(groupId, (int)compensation.SubscriptionId, userId);

                // validate subscription
                if (subscription == null)
                {
                    // subscription wasn't found
                    log.Error(string.Format("subscription wasn't found. subscriptionId = {0}", compensation.SubscriptionId));
                    return response;
                }

                string billingGuid = ODBCWrapper.Utils.GetSafeStr(subscriptionPurchaseRow, "billing_guid");

                // check if payment gateway supports this
                ApiObjects.Response.Status verificationStatus = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                try
                {
                    PaymentDetails paymentDetails = null;
                    verificationStatus = Core.Billing.Module.GetPaymentGatewayVerificationStatus(groupId, billingGuid, ref paymentDetails);
                }
                catch (Exception ex)
                {
                    log.Error("Error while calling the billing GetPaymentGatewayVerificationStatus", ex);
                    return response;
                }

                if (verificationStatus.Code != (int)eResponseStatus.OK)
                {
                    response.Status = verificationStatus;
                    log.ErrorFormat("Verification payment gateway does not support compensation. billingGuid = {0}, purchaseId = {1}", billingGuid, compensation.PurchaseId);
                    return response;
                }

                // insert the compensation data
                compensation.Id = ConditionalAccessDAL.InsertSubscriptionCompernsation(groupId, domainId, compensation.PurchaseId, compensation.CompensationType, compensation.Amount,
                    compensation.TotalRenewals, compensation.SubscriptionId);
                if (compensation.Id == 0)
                {
                    log.DebugFormat("Failed to insert compensation data for userId = {0}, purchaseId = {1}", userId, compensation.PurchaseId);
                    return response;
                }

                //Add to Recurring Renew Details - BEO-8601
                var recurringData = ConditionalAccessDAL.GetRecurringRenewDetails(compensation.PurchaseId);
                if (recurringData != null && (recurringData.Compensation == null || recurringData.Compensation.Id != compensation.Id))
                {
                    recurringData.Compensation = compensation;
                    ConditionalAccessDAL.SaveRecurringRenewDetails(recurringData, compensation.PurchaseId);
                }

                response.Compensation = compensation;
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error("Error in AddSubscriptionCompensation", ex);
            }

            return response;
        }

        internal static ApiObjects.Response.Status DeleteCompensation(BaseConditionalAccess cas, int groupId, long compensationId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                Compensation compensation = ConditionalAccessDAL.GetSubscriptionCompensation(compensationId);
                if (compensation == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.CompensationNotFound, "Compensation not found");
                    return response;
                }
                if (ConditionalAccessDAL.DeleteSubscriptionCompernsation(compensationId))
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while deleting subscription compensation. ID = {0}", compensationId), ex);
            }

            return response;
        }

        internal static CompensationResponse GetCompensation(BaseConditionalAccess cas, int groupId, long compensationId)
        {
            CompensationResponse response = new CompensationResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                response.Compensation = ConditionalAccessDAL.GetSubscriptionCompensation(compensationId);
                if (response.Compensation == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.CompensationNotFound, "Compensation not found");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while getting subscription compensation. ID = {0}", compensationId), ex);
            }

            return response;
        }

        internal static ApiObjects.Response.Status CancelScheduledSubscription(int groupId, long domainId, long scheduledSubscriptionId)
        {
            ApiObjects.Response.Status res = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                long subscriptionSetModifyDetailsId = 0, purchaseId = 0;
                // check if cancellation is allowed
                Subscription subscriptionToCancel = Pricing.Module.GetSubscriptionData(groupId, scheduledSubscriptionId.ToString(), string.Empty, string.Empty, string.Empty, false);
                if (subscriptionToCancel != null && subscriptionToCancel.BlockCancellation)
                {
                    res = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionCancellationIsBlocked, "Cancellation is blocked for this subscription");
                    return res;
                }

                if (!Utils.GetSubscriptionSetModifyDetailsByDomainAndSubscriptionId(groupId, domainId, scheduledSubscriptionId, ref subscriptionSetModifyDetailsId, ref purchaseId, SubscriptionSetModifyType.Downgrade))
                {
                    res = new ApiObjects.Response.Status((int)eResponseStatus.ScheduledSubscriptionNotFound, eResponseStatus.ScheduledSubscriptionNotFound.ToString());
                    return res;
                }

                // set original purchase recurring_status to 1
                if (!ConditionalAccessDAL.UpdateSubscriptionPurchaseRenewalStatus(purchaseId, 1))
                {
                    log.ErrorFormat("Failed to Update Subscription purchase renewal status, groupId: {0}, domainId: {1}, scheduledSubscriptionId: {2}, purchaseId: {3}",
                                    groupId, domainId, scheduledSubscriptionId, purchaseId);
                    return res;
                }

                if (!ConditionalAccessDAL.UpdateSubscriptionSetModifyDetails(subscriptionSetModifyDetailsId, 2, null))
                {
                    log.ErrorFormat("Failed to Update SubscriptionSetModifyDetails to canceled, groupId: {0}, domainId: {1}, scheduledSubscriptionId: {2}", groupId, domainId, scheduledSubscriptionId);
                    return res;
                }

                if (!Utils.DeleteSubscriptionSetDowngradeDetails(groupId, subscriptionSetModifyDetailsId))
                {
                    log.WarnFormat("Failed to delete SubscriptionSetModifyDetails, groupId: {0}, domainId: {1}, scheduledSubscriptionId: {2}, subscriptionSetModifyDetailsId: {3}",
                                    groupId, domainId, scheduledSubscriptionId, subscriptionSetModifyDetailsId);
                }

                string invalidationKey = LayeredCacheKeys.GetCancelSubscriptionRenewalInvalidationKey(domainId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key on CancelScheduledSubscription key = {0}", invalidationKey);
                }

                res = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CancelScheduledSubscription, , groupId: {0}, domainId: {1}, scheduledSubscriptionId: {2}", groupId, domainId, scheduledSubscriptionId), ex);
            }

            return res;
        }

        internal static ApiObjects.Response.Status ApplyCoupon(BaseConditionalAccess cas, int groupId, long domainId, string userId, long purchaseId, string couponCode)
        {
            var status = ApiObjects.Response.Status.Ok;

            try
            {
                // validate Entitlement
                string billingGuid = string.Empty;
                DateTime endDateFromDB = DateTime.MaxValue;
                string siteGuid = null;

                var entitlement = GetEntitlementById(cas, purchaseId, domainId, ref billingGuid, ref endDateFromDB, ref siteGuid);
                if (!entitlement.HasObject())
                {
                    status.Set(entitlement.Status);
                    return status;
                }

                var subscriptionId = int.Parse(entitlement.Object.entitlementId);
                var subscription = Utils.GetSubscription(groupId, subscriptionId);
                if (subscription == null)
                {
                    status.Set(eResponseStatus.SubscriptionDoesNotExist, "ProductId doesn't exist");
                    return status;
                }

                // validate coupon
                var couponData = Utils.GetCouponData(groupId, couponCode, domainId);
                if (couponData == null)
                {
                    status.Set(eResponseStatus.CouponNotValid);
                    return status;
                }

                var renewData = ConditionalAccessDAL.GetRecurringRenewDetails(purchaseId);

                if (renewData != null && renewData.LeftCouponRecurring > 0)
                {
                    status.Set(eResponseStatus.OtherCouponIsAlreadyAppliedForSubscription);
                    return status;
                }

                if (renewData != null && renewData.CampaignDetails != null && renewData.CampaignDetails.Id > 0)
                {
                    status.Set(eResponseStatus.CampaignIsAlreadyAppliedForSubscription);
                    return status;
                }


                bool validCoupon = false;
                // look if this coupon group id exsits in coupon list 
                if (
                        (
                            subscription.m_oCouponsGroup != null
                            &&
                            subscription.m_oCouponsGroup.m_sGroupCode.Equals(couponData.m_oCouponGroup.m_sGroupCode)
                        )
                        ||
                        (
                            subscription.CouponsGroups != null
                            &&
                            subscription.CouponsGroups.Count(x => x.m_sGroupCode.Equals(couponData.m_oCouponGroup.m_sGroupCode)
                                                             && (!x.endDate.HasValue || x.endDate.Value >= DateTime.UtcNow)) > 0
                        )
                   )
                {
                    validCoupon = true;
                }

                if (!validCoupon)
                {
                    status.Set(eResponseStatus.CouponNotValid);
                    return status;
                }

                if (renewData == null)
                {
                    if (!subscription.m_bIsRecurring)
                    {
                        status.Set(eResponseStatus.SubscriptionNotRenewable);
                        return status;
                    }

                    // Init renewData (for backwards compatibility)
                    var renewDetailsRow = ConditionalAccessDAL.Get_RenewDetails(groupId, purchaseId, billingGuid);
                    renewData = InitializeRecurringRenewDetails(groupId, renewDetailsRow, purchaseId, subscription);
                }

                renewData.CouponCode = couponCode;
                renewData.CouponRemainder = 0;
                renewData.IsCouponHasEndlessRecurring = couponData.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon == 0;
                renewData.LeftCouponRecurring = couponData.m_oCouponGroup.m_nMaxRecurringUsesCountForCoupon;
                renewData.IsCouponGiftCard = couponData.m_oCouponGroup.couponGroupType == CouponGroupType.GiftCard;

                ConditionalAccessDAL.SaveRecurringRenewDetails(renewData, purchaseId);
                cas.HandleCouponUses(subscriptionId, userId, 0, couponCode, true, 0, domainId);
                status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in ApplyCoupon. householdId:{0}.", domainId), ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        public static RecurringRenewDetails InitializeRecurringRenewDetails(int groupId, DataRow renewDetailsRow, long purchaseId, Subscription subscription, string billingGuid = null, string couponCode = null)
        {
            int totalRenews = 0;

            if (renewDetailsRow != null)
            {
                totalRenews = ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "total_number_of_payments");
            }

            int leftCouponRecurring = 0;
            bool isCouponGiftCard = false, isCouponEndlessRecurring = false;
            var couponGroupId = Utils.GetCouponGroupIdForFirstCoupon(groupId, subscription, ref couponCode, purchaseId);
            if (couponGroupId > 0)
            {
                // look if this coupon group id is a gift card in the subscription list 
                CouponsGroupResponse cg = Pricing.Module.GetCouponsGroup(groupId, couponGroupId);
                if (cg.Status.IsOkStatusCode())
                {
                    isCouponGiftCard = cg.CouponsGroup.couponGroupType == CouponGroupType.GiftCard;
                    leftCouponRecurring = cg.CouponsGroup.m_nMaxRecurringUsesCountForCoupon - totalRenews;
                    if (leftCouponRecurring < 0) { leftCouponRecurring = 0; }
                    isCouponEndlessRecurring = cg.CouponsGroup.m_nMaxRecurringUsesCountForCoupon == 0;
                }
            }

            var renewData = new RecurringRenewDetails()
            {
                CouponCode = couponCode,
                CouponRemainder = ConditionalAccessDAL.GetCouponRemainder(purchaseId),
                IsPurchasedWithPreviewModule = renewDetailsRow.Table.Columns.Contains("Preview_Module_ID") ?
                    ODBCWrapper.Utils.GetIntSafeVal(renewDetailsRow, "Preview_Module_ID") > 0 :
                    ApiDAL.Get_IsPurchasedWithPreviewModuleByBillingGuid(groupId, billingGuid, (int)purchaseId),
                LeftCouponRecurring = leftCouponRecurring,
                TotalNumOfRenews = totalRenews,
                Compensation = ConditionalAccessDAL.GetSubscriptionCompensationByPurchaseId(purchaseId),
                IsCouponGiftCard = isCouponGiftCard,
                IsCouponHasEndlessRecurring = isCouponEndlessRecurring
            };

            if (ConditionalAccessDAL.SaveRecurringRenewDetails(renewData, purchaseId))
            {
                ConditionalAccessDAL.DeleteCouponRemainder(purchaseId);
            }

            return renewData;
        }

        public static ApiObjects.Response.Status UpdatePendingEntitlement(int groupId, eTransactionType productType, string billingGuid, PaymentGateway paymentGateway)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                switch (productType)
                {
                    case eTransactionType.PPV:
                        {
                            return UpdatePendingPPVEntitlement(groupId, billingGuid);

                        }
                    case eTransactionType.Subscription:
                        {
                            return UpdatePendingSubscriptionEntitlement(groupId, billingGuid, paymentGateway);

                        }
                    case eTransactionType.Collection:
                        {
                            return UpdatePendingCollectionEntitlement(groupId, billingGuid);
                        }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdatePendingEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        private static ApiObjects.Response.Status UpdatePendingSubscriptionEntitlement(int groupId, string billingGuid, PaymentGateway paymentGateway)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetSubscriptionsPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;

                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        int purchaseId = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int houseHoldId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");

                        //Get subscription
                        string productId = ODBCWrapper.Utils.ExtractString(row, "SUBSCRIPTION_CODE");
                        bool isRecurring = ODBCWrapper.Utils.ExtractInteger(row, "IS_RECURRING_STATUS") == 1;

                        Subscription subscription = Utils.GetSubscription(groupId, int.Parse(productId));
                        DateTime endDate = Utils.CalcSubscriptionEndDate(subscription, false, now);

                        SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(groupId)
                        {
                            purchaseId = purchaseId,
                            productId = productId,
                            siteGuid = siteGuid,
                            isRecurring = isRecurring,
                            startDate = now,
                            endDate = endDate,
                            entitlementDate = now,
                            houseHoldId = houseHoldId,
                            billingGuid = billingGuid,
                            IsPending = false,
                            UpdateFromPending = true
                        };

                        subscriptionPurchase.Update();

                        if (isRecurring)
                        {
                            //create renew msg
                            DateTime nextRenewalDate = endDate.AddMinutes(-5);

                            if (paymentGateway == null)
                            {
                                paymentGateway = Core.Billing.Module.GetPaymentGatewayByBillingGuid(groupId, houseHoldId, billingGuid);
                            }

                            if (paymentGateway != null)
                            {
                                nextRenewalDate = endDate.AddMinutes(paymentGateway.RenewalStartMinutes);
                            }

                            long endDateUnix = DateUtils.DateTimeToUtcUnixTimestampSeconds(endDate);

                            PurchaseManager.RenewTransactionMessageInQueue(groupId, siteGuid, billingGuid, purchaseId, endDateUnix, nextRenewalDate, houseHoldId);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdatePendingSubscriptionEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
        }

        private static ApiObjects.Response.Status UpdatePendingPPVEntitlement(int groupId, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetPPVPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;

                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        int purchaseId = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int houseHoldId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");

                        string customdata = ODBCWrapper.Utils.ExtractString(row, "CUSTOMDATA");
                        int contentId = ODBCWrapper.Utils.ExtractInteger(row, "MEDIA_FILE_ID");

                        int ppvmTagLength = 6;
                        int ppvTagStart = customdata.IndexOf("<ppvm>") + ppvmTagLength;
                        int ppvTagEnd = customdata.IndexOf("</ppvm>");

                        int productId = 0;
                        if (int.TryParse(customdata.Substring(ppvTagStart, ppvTagEnd - ppvTagStart), out productId))
                        {
                            PPVModule ppvModule = null;
                            status = Utils.ValidatePPVModuleCode(groupId, productId, contentId, ref ppvModule);
                            if (status.Code != (int)eResponseStatus.OK)
                            {
                                return status;
                            }

                            DateTime endDate = Utils.GetEndDateTime(now, ppvModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle, true, true);

                            PpvPurchase ppvPurchase = new PpvPurchase(groupId)
                            {
                                purchaseId = purchaseId,
                                contentId = contentId,
                                siteGuid = siteGuid,
                                startDate = now,
                                endDate = endDate,
                                entitlementDate = now,
                                houseHoldId = houseHoldId,
                                billingGuid = billingGuid,
                                IsPending = false
                            };

                            bool res = ppvPurchase.Update();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdatePendingPPVEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
        }

        private static ApiObjects.Response.Status UpdatePendingCollectionEntitlement(int groupId, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetCollectionsPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;

                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        int purchaseId = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int houseHoldId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");

                        string productId = ODBCWrapper.Utils.ExtractString(row, "COLLECTION_CODE");

                        Collection collection = Pricing.Module.GetCollectionData(groupId, productId, string.Empty, string.Empty, string.Empty, false);
                        DateTime endDate = Utils.CalcCollectionEndDate(collection, now);

                        CollectionPurchase collectionPurchase = new CollectionPurchase(groupId)
                        {
                            purchaseId = purchaseId,
                            productId = productId,
                            siteGuid = siteGuid,
                            startDate = now,
                            endDate = endDate,
                            createAndUpdateDate = now,
                            houseHoldId = houseHoldId,
                            billingGuid = billingGuid,
                            IsPending = false
                        };

                        collectionPurchase.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdatePendingCollectionEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK };
        }

        public static ApiObjects.Response.Status DeletePendingEntitlement(int groupId, eTransactionType productType, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                switch (productType)
                {
                    case eTransactionType.PPV:
                        {
                            status = DeletePendingPPVEntitlement(groupId, billingGuid);
                            break;
                        }
                    case eTransactionType.Subscription:
                        {
                            status = DeletePendingSubscriptionEntitlement(groupId, billingGuid);
                            break;
                        }
                    case eTransactionType.Collection:
                        {
                            status = DeletePendingCollectionEntitlement(groupId, billingGuid);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeletePendingEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        private static ApiObjects.Response.Status DeletePendingSubscriptionEntitlement(int groupId, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetSubscriptionsPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int householdId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");
                        string productId = ODBCWrapper.Utils.ExtractString(row, "SUBSCRIPTION_CODE");

                        SubscriptionPurchase subscriptionPurchase = new SubscriptionPurchase(groupId)
                        {
                            productId = productId,
                            siteGuid = siteGuid,
                            houseHoldId = householdId
                        };

                        var dalResult = subscriptionPurchase.Delete();

                        if (dalResult)
                        {
                            status.Set(eResponseStatus.OK);

                            string coupon = ODBCWrapper.Utils.ExtractString(row, "coupon_code");
                            
                            if (!string.IsNullOrEmpty(coupon))
                            {
                                // reduce coupon count
                                Pricing.Module.SetCouponUses(groupId, coupon, siteGuid, 0, 0, 0, 0, householdId, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeletePendingSubscriptionEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        private static ApiObjects.Response.Status DeletePendingPPVEntitlement(int groupId, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetPPVPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;

                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        int purchaseId = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int householdId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");
                        int contentId = ODBCWrapper.Utils.ExtractInteger(row, "MEDIA_FILE_ID");

                        PpvPurchase ppvPurchase = new PpvPurchase(groupId)
                        {
                            contentId = contentId,
                            siteGuid = siteGuid,
                            houseHoldId = householdId
                        };

                        if (ppvPurchase.Delete())
                        {
                            status.Set(eResponseStatus.OK);

                            string customdata = ODBCWrapper.Utils.ExtractString(row, "CUSTOMDATA");
                            int couponTagLength = 4;
                            int couponTagStart = customdata.IndexOf("<cc>") + couponTagLength;
                            int couponTagEnd = customdata.IndexOf("</cc>");

                            string coupon = customdata.Substring(couponTagStart, couponTagEnd - couponTagStart);
                            if (!string.IsNullOrEmpty(coupon))
                            {
                                // reduce coupon count
                                Pricing.Module.SetCouponUses(groupId, coupon, siteGuid, 0, 0, 0, 0, householdId, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeletePendingPPVEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }

        private static ApiObjects.Response.Status DeletePendingCollectionEntitlement(int groupId, string billingGuid)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                // Get subscriptions_purchase
                DataTable dt = ConditionalAccessDAL.GetCollectionsPurchasesByBillingGuid(groupId, billingGuid);
                if (dt?.Rows.Count > 0)
                {
                    DateTime now = DateTime.UtcNow;

                    var row = dt.Rows[0];

                    if (row != null)
                    {
                        int purchaseId = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                        string siteGuid = ODBCWrapper.Utils.ExtractString(row, "SITE_USER_GUID");
                        int householdId = ODBCWrapper.Utils.ExtractInteger(row, "DOMAIN_ID");
                        string productId = ODBCWrapper.Utils.ExtractString(row, "COLLECTION_CODE");

                        CollectionPurchase collectionPurchase = new CollectionPurchase(groupId)
                        {
                            productId = productId,
                            siteGuid = siteGuid,
                            houseHoldId = householdId
                        };

                        if (collectionPurchase.Delete())
                        {
                            status.Set(eResponseStatus.OK);

                            string customdata = ODBCWrapper.Utils.ExtractString(row, "CUSTOMDATA");
                            int couponTagLength = 4;
                            int couponTagStart = customdata.IndexOf("<cc>") + couponTagLength;
                            int couponTagEnd = customdata.IndexOf("</cc>");

                            string coupon = customdata.Substring(couponTagStart, couponTagEnd - couponTagStart);
                            if (!string.IsNullOrEmpty(coupon))
                            {
                                // reduce coupon count
                                Pricing.Module.SetCouponUses(groupId, coupon, siteGuid, 0, 0, 0, 0, householdId, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeletePendingCollectionEntitlement. groupId: {groupId} billingGuid:{billingGuid}.", ex);
                status.Set(eResponseStatus.Error);
            }

            return status;
        }
    }
}