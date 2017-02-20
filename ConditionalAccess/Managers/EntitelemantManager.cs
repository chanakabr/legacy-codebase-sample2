using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using ConditionalAccess.Response;
using DAL;
using KLogMonitor;
using Pricing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Users;
using WS_Billing;

namespace ConditionalAccess
{
    public class EntitelemantManager
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

                        if (TVinciShared.WS_Utils.GetTcmBoolValue("ShouldUseLicenseLinkCache") && !isRecording)
                        {
                            CachedEntitlementResults cachedEntitlementResults = Utils.GetCachedEntitlementResults(domainId, mediaFileId);
                            if (cachedEntitlementResults != null)
                            {
                                if (cachedEntitlementResults.IsFree)
                                {
                                    Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle);
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
                        MediaFileItemPricesContainer[] arrPrices = cas.GetItemsPrices(arrMediaFileIDs, userId, string.Empty, true, countryCode, languageCode, deviceName);
                        if (arrPrices != null && arrPrices.Length > 0)
                        {
                            MediaFileItemPricesContainer objPrice = arrPrices[0];

                            // If the item is free
                            if (Utils.IsFreeItem(objPrice))
                            {
                                Utils.GetFreeItemLeftLifeCycle(groupId, ref strViewLifeCycle, ref strFullLifeCycle);
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

                                strViewLifeCycle = tsViewLeftSpan.TotalMilliseconds > tsFullLeftSpan.TotalMilliseconds ? tsFullLeftSpan.ToString() : tsViewLeftSpan.ToString();
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

        internal static Entitlements UpdateEntitlement(BaseConditionalAccess cas, int groupId, long domainID, ConditionalAccess.Response.Entitlement entitlement)
        {
            Entitlements response = new Entitlements(new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()));
            try
            {
                if (entitlement == null)
                {
                    log.ErrorFormat("UpdateEntitlement entitlement is null ", "");
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return response;
                }
                // 1. validate household
                //---------------------------------               
                Domain domain = null;
                ApiObjects.Response.Status status = Utils.ValidateDomain(groupId, (int)domainID, out  domain);
                if (status == null || status.Code != (int)eResponseStatus.OK || domain == null)
                {
                    log.ErrorFormat("UpdateEntitlement ValidateUserAndDomain purchaseID = {0} status.Code = {1},  householdId = {2} ", entitlement.purchaseID, status.Code, domainID);
                    response.status = status;
                    return response;
                }

                // get entitelment with the current payment gateway id + current payment method id 
                DataRow dr = ConditionalAccessDAL.GetPurchaseByID(entitlement.purchaseID);
                if (dr == null)
                {
                    log.ErrorFormat("UpdateEntitlement ValidateUserAndDomain purchaseID = {0} status.Code = {1},  householdId = {2} ", entitlement.purchaseID, status.Code, domainID);
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidPurchase, eResponseStatus.InvalidPurchase.ToString());
                    return response;
                }

                if (ODBCWrapper.Utils.GetIntSafeVal(dr, "domain_id") != domainID)
                {
                    log.ErrorFormat("UpdateEntitlement purchaseID = {0} not belong to householdId = {1} ", entitlement.purchaseID, domainID);
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidPurchase, eResponseStatus.InvalidPurchase.ToString());
                    return response;
                }

                // ask if renewable + subscription related to domain 
                bool isRecurring = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_recurring_status") == 1 ? true : false;
                if (!isRecurring)
                {
                    log.DebugFormat("UpdateEntitlement subscription for purchaseID = {0} is not recurring", entitlement.purchaseID);
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.SubscriptionNotRenewable, eResponseStatus.SubscriptionNotRenewable.ToString());
                    return response;
                }

                // get latest PaymentDetailsTransaction
                string billingGuid = ODBCWrapper.Utils.GetSafeStr(dr, "billing_guid");

                // move here to Billing WS (write all in billing)
                string wsUserName = string.Empty;
                string wsPass = string.Empty;
                module wsBillingService = null;
                cas.InitializeBillingModule(ref wsBillingService, ref wsUserName, ref wsPass);

                ApiObjects.Response.Status changeStatus = wsBillingService.ChangePaymentDetails(wsUserName, wsPass, billingGuid, domainID, entitlement.paymentGatewayId, entitlement.paymentMethodId);

                // comlete entitelment details 
                if (changeStatus.Code == (int)eResponseStatus.OK)
                {
                    ConditionalAccess.Response.Entitlement subscriptionEntitlement = CreateSubscriptionEntitelment(cas, dr, false, null);
                    subscriptionEntitlement.paymentGatewayId = entitlement.paymentGatewayId;
                    subscriptionEntitlement.paymentMethodId = entitlement.paymentMethodId;
                    response.entitelments.Add(subscriptionEntitlement);
                }
                response.status = changeStatus;

            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateEntitlement failed ex={0}, GroupID={1}, domainID={2}, purchaseID={3}, paymentGatewayId={4}, paymentMethodId={5} ", ex, groupId, domainID,
                    entitlement.purchaseID, entitlement.paymentGatewayId, entitlement.paymentMethodId);
                response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }
        
        internal static Entitlements GetUsersEntitlementSubscriptionsItems(BaseConditionalAccess cas, int groupId, List<int> userIds, bool isExpired, int domainID, bool shouldCheckByDomain, int pageSize, int pageIndex, EntitlementOrderBy orderBy)
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

                DataTable allSubscriptionsPurchases = ConditionalAccessDAL.Get_UsersPermittedSubscriptions(userIds, isExpired, domainID, (int)orderBy);

                if (allSubscriptionsPurchases == null || allSubscriptionsPurchases.Rows == null || allSubscriptionsPurchases.Rows.Count == 0)
                {
                    entitlementsResponse.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no permitted items");
                    return entitlementsResponse;
                }

                //Get Iteration Rows according page size and index
                IEnumerable<DataRow> iterationRows = GetIterationRows(pageSize, pageIndex, allSubscriptionsPurchases);

                // get all builingGuid from subscription 
                List<string> billingGuids = (from row in allSubscriptionsPurchases.AsEnumerable()
                                             where row.Field<int>("IS_RECURRING_STATUS") == 1
                                             select row.Field<string>("BILLING_GUID")).ToList(); // only renewable subscriptions 
                List<PaymentDetails> renewPaymentDetails = null;
                if (billingGuids != null && billingGuids.Count > 0)
                {
                    // call billing service to get all transaction payment details
                    string sWSUserName = "";
                    string sWSPass = "";
                    module bm = new module();
                    Utils.GetWSCredentials(groupId, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                    renewPaymentDetails = bm.GetPaymentDetails(sWSUserName, sWSPass, billingGuids);
                }

                ConditionalAccess.Response.Entitlement entitlement = null;
                foreach (DataRow dr in iterationRows)
                {
                    entitlement = CreateSubscriptionEntitelment(cas, dr, isExpired, renewPaymentDetails);
                    if (entitlement != null)
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

                //Get Iteration Rows according page size and index
                IEnumerable<DataRow> iterationRows = GetIterationRows(pageSize, pageIndex, allCollectionsPurchases);

                ConditionalAccess.Response.Entitlement entitlement = null;
                foreach (DataRow dr in iterationRows)
                {
                    entitlement = CreateCollectionEntitelment(cas, dr, isExpired);
                    if (entitlement != null)
                    {
                        if (entitlement != null)
                        {
                            entitlementsResponse.entitelments.Add(entitlement);
                        }
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


        private static ConditionalAccess.Response.Entitlement CreateSubscriptionEntitelment(BaseConditionalAccess cas, DataRow dataRow, bool isExpired, List<PaymentDetails> renewPaymentDetails)
        {
            UsageModule oUsageModule = null;
            ConditionalAccess.Response.Entitlement entitlement = new ConditionalAccess.Response.Entitlement();
            PaymentDetails paymentDetails = null;

            entitlement.type = eTransactionType.Subscription;
            entitlement.entitlementId = ODBCWrapper.Utils.GetSafeStr(dataRow, "SUBSCRIPTION_CODE");
            entitlement.currentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "NUM_OF_USES");
            entitlement.maxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_NUM_OF_USES");

            DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "END_DATE");
            entitlement.purchaseDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "START_DATE");
            entitlement.lastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "LAST_VIEW_DATE");

            // check whether subscription is in its grace period
            int gracePeriodMinutes = ODBCWrapper.Utils.GetIntSafeVal(dataRow["GRACE_PERIOD_MINUTES"]);
            entitlement.IsInGracePeriod = false;
            if (!isExpired && endDate < DateTime.UtcNow)
            {
                endDate = endDate.AddMinutes(gracePeriodMinutes);
                entitlement.IsInGracePeriod = true;
            }

            string billingGuid = ODBCWrapper.Utils.GetSafeStr(dataRow, "BILLING_GUID");

            entitlement.recurringStatus = false;
            entitlement.nextRenewalDate = DateTime.MaxValue;
            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "IS_RECURRING_STATUS") == 1)
            {
                entitlement.recurringStatus = true;
                entitlement.nextRenewalDate = endDate;

                // get renew payment details 
                if (renewPaymentDetails != null)
                {
                    paymentDetails = renewPaymentDetails.Where(x => x.BillingGuid == billingGuid).FirstOrDefault();
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

            entitlement.isRenewable = false;
            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow["IS_RECURRING"]) == 1)
            {
                entitlement.isRenewable = true;
            }

            entitlement.endDate = endDate;
            entitlement.currentDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "cDate");

            if (ODBCWrapper.Utils.GetIntSafeVal(dataRow, "WAIVER") == 0 &&
               entitlement.lastViewDate < entitlement.purchaseDate)// user didn't waiver yet and didn't use the PPV yet
            {
                bool cancellationWindow = false;
                cas.IsCancellationWindow(ref oUsageModule, entitlement.entitlementId, entitlement.purchaseDate, ref cancellationWindow, eTransactionType.Subscription);
                entitlement.cancelWindow = cancellationWindow;
            }

            entitlement.purchaseID = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "ID");
            entitlement.paymentMethod = Utils.GetBillingTransMethod(ODBCWrapper.Utils.GetIntSafeVal(dataRow, "billing_transaction_id"), billingGuid);
            entitlement.deviceUDID = ODBCWrapper.Utils.GetSafeStr(dataRow, "device_name");
            entitlement.deviceName = string.Empty;
            if (!string.IsNullOrEmpty(entitlement.deviceUDID))
            {
                entitlement.deviceName = Utils.GetDeviceName(entitlement.deviceUDID);
            }

            entitlement.mediaFileID = 0;
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
                int takeTop = pageIndex * pageSize;
                Int64 maxTransactionID = (from row in usersPermittedItems.AsEnumerable().Take(takeTop)
                                          select row.Field<Int64>("ID")).ToList().Min();
                filteredRows = (from row in usersPermittedItems.AsEnumerable()
                                where (Int64)row["ID"] < maxTransactionID
                                select row).Take(pageSize).ToList();
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

            return entitlement;

        }

        private static ConditionalAccess.Response.Entitlement CreateCollectionEntitelment(BaseConditionalAccess cas, DataRow dataRow, bool isExpired)
        {
            UsageModule oUsageModule = null;
            ConditionalAccess.Response.Entitlement entitlement = new ConditionalAccess.Response.Entitlement();

            entitlement.type = eTransactionType.Collection;
            entitlement.entitlementId = ODBCWrapper.Utils.GetSafeStr(dataRow, "COLLECTION_CODE");

            entitlement.endDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "END_DATE");
            int maxUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "MAX_NUM_OF_USES");
            int currentUses = ODBCWrapper.Utils.GetIntSafeVal(dataRow, "NUM_OF_USES");
            entitlement.lastViewDate = ODBCWrapper.Utils.GetDateSafeVal(dataRow, "LAST_VIEW_DATE");

            if (isExpired && maxUses != 0 && currentUses >= maxUses)
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
                cas.IsCancellationWindow(ref oUsageModule, entitlement.entitlementId, entitlement.purchaseDate, ref cancellationWindow, eTransactionType.Subscription);
                entitlement.cancelWindow = cancellationWindow;
            }
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
                Compensation currentCompensation = ConditionalAccessDAL.GetSubscriptionCompensation(compensation.PurchaseId);
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
                int subscriptionId = ODBCWrapper.Utils.GetIntSafeVal(subscriptionPurchaseRow, "SUBSCRIPTION_CODE");
                Subscription subscription = Utils.GetSubscription(groupId, subscriptionId);

                // validate subscription
                if (subscription == null)
                {
                    // subscription wasn't found
                    log.Error(string.Format("subscription wasn't found. subscriptionId = {0}", subscriptionId));
                    return response;
                }

                string billingGuid = ODBCWrapper.Utils.GetSafeStr(subscriptionPurchaseRow, "billing_guid");

                // check if payment gateway supports this
                string billingUserName = string.Empty;
                string billingPassword = string.Empty;
                module wsBillingService = null;
                Utils.InitializeBillingModule(ref wsBillingService, groupId, ref billingUserName, ref billingPassword);
                ApiObjects.Response.Status verificationStatus = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                try
                {
                    verificationStatus = wsBillingService.GetPaymentGatewayVerificationStatus(billingUserName, billingPassword, billingGuid);
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
                response.Compensation = ConditionalAccessDAL.InsertSubscriptionCompernsation(groupId, domainId, compensation.PurchaseId, compensation.CompensationType, compensation.Amount, 
                    compensation.TotalRenewals, subscriptionId);
                {
                    log.DebugFormat("Failed to insert compensation data for userId = {0}, purchaseId = {1}", userId, compensation.PurchaseId);
                    return response;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in AddSubscriptionCompensation", ex);
            }

            return response;
        }
    }
}
