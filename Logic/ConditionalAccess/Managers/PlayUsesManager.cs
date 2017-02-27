using ApiObjects;
using ApiObjects.Catalog;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess
{
    public class PlayUsesManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string GET_DOMAIN_LAST_USE_WITH_CREDIT_LAYERED_CACHE_CONFIG_NAME = "GetDomainLastUseWithCredit";

        /// <summary>
        /// Handle Play Uses
        /// </summary>
        public static void HandlePlayUses(BaseConditionalAccess cas, MediaFileItemPricesContainer price, string userId, Int32 mediaFileId, string ip,
                                    string countryCode, string languageCode, string udid, string couponCode, long domainId, int groupId)
        {
            if (price == null || price.m_oItemPrices == null || price.m_oItemPrices.Length == 0)
            {
                return;
            }
            ItemPriceContainer itemPriceContainer = price.m_oItemPrices[0];

            int mediaId = GetMediaIdByFildId(groupId, mediaFileId);
            if (mediaId == 0)
            {
                return;
            }

            countryCode = string.IsNullOrEmpty(countryCode) ? Utils.GetIP2CountryName(groupId, ip) : countryCode;

            int releventCollectionID = ExtractRelevantCollectionID(itemPriceContainer);
            cas.HandleCouponUses(itemPriceContainer.m_relevantSub, itemPriceContainer.m_sPPVModuleCode, userId, itemPriceContainer.m_oPrice.m_dPrice,
                                itemPriceContainer.m_oPrice.m_oCurrency.m_sCurrencyCD3, mediaFileId, couponCode, ip, countryCode, languageCode, udid, false, 0, releventCollectionID);
            Int32 nRelPP = ExtractRelevantPrePaidID(itemPriceContainer);
            List<Task> tasks = new List<Task>();
            ContextData contextData = new ContextData();
            if (IsPurchasedAsPurePPV(itemPriceContainer))
            {
                string sPPVMCd = itemPriceContainer.m_sPPVModuleCode;

                bool isCreditDownloaded = PPV_DoesCreditNeedToDownloaded(sPPVMCd, null, null, countryCode, languageCode, udid, Utils.GetRelatedMediaFiles(itemPriceContainer, mediaFileId),
                                                                            domainId, mediaFileId, groupId, mediaId, Utils.GetStartDate(itemPriceContainer), Utils.GetEndDate(itemPriceContainer));
                if (isCreditDownloaded)
                {
                    if (ConditionalAccessDAL.Insert_NewPPVUse(groupId, mediaFileId, itemPriceContainer.m_sPPVModuleCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID))
                    {
                        //last use updated - update "LastUseWithCredit" validation key 
                        string invalidationKey = LayeredCacheKeys.GetLastUseWithCreditForDomainInvalidationKey(groupId, domainId, mediaId);
                        if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                        {
                            log.DebugFormat("Failed to set invalidationKey, key: {0}", invalidationKey);
                        }

                        DomainEntitlements domainEntitlements = null;
                        if (Utils.TryGetDomainEntitlementsFromCache(groupId, (int)domainId, null, ref domainEntitlements))
                        {
                            if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                            {
                                string key = string.Format("{0}_{1}", itemPriceContainer.m_lPurchasedMediaFileID, itemPriceContainer.m_sPPVModuleCode);
                                if (domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary.ContainsKey(key))
                                {
                                    int purchaseId = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].ID;
                                    int numOfUses = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].numOfUses;
                                    DateTime? endDate = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].endDate;
                                    if (UpdatePPVPurchases(purchaseId, itemPriceContainer.m_sPPVModuleCode, countryCode, languageCode, udid, groupId, numOfUses, endDate.Value))
                                    {
                                        //PPV Purchases updated - update purchase validation key
                                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetPurchaseInvalidationKey(domainId)))
                                        {
                                            log.DebugFormat("Failed to set invalidationKey, key: {0}", invalidationKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // failed to insert ppv use.
                        throw new Exception(GetPPVUseInsertionFailureExMsg(mediaFileId, itemPriceContainer.m_sPPVModuleCode, userId, nRelPP, releventCollectionID));
                    }
                }
                else
                {
                    tasks.Add(Task.Factory.StartNew(() => Utils.InsertOfflinePpvUse(groupId, mediaFileId, itemPriceContainer.m_sPPVModuleCode, userId, countryCode,
                                                                                    languageCode, udid, nRelPP, releventCollectionID, contextData)));
                }
            }
            else
            {
                string purchasingUserId = GetPurchasingSiteGuid(itemPriceContainer);
                purchasingUserId = string.IsNullOrEmpty(purchasingUserId) ? userId : purchasingUserId;

                if (IsPurchasedAsPartOfSub(itemPriceContainer))
                {
                    int numOfUses = 0;
                    bool setPurchaseInvalidationKey = false;
                    DomainEntitlements domainEntitlements = null;
                    if (Utils.TryGetDomainEntitlementsFromCache(groupId, (int)domainId, null, ref domainEntitlements))
                    {
                        if (domainEntitlements != null && domainEntitlements.DomainBundleEntitlements != null && domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions != null)
                        {
                            if (domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions.ContainsKey(itemPriceContainer.m_relevantSub.m_sObjectCode))
                            {
                                numOfUses = domainEntitlements.DomainBundleEntitlements.EntitledSubscriptions[itemPriceContainer.m_relevantSub.m_sObjectCode].nNumOfUses;
                            }
                        }
                    }

                    // PPV purchased as part of Subscription
                    bool isCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(itemPriceContainer.m_relevantSub.m_sObjectCode, userId,
                                                                                        Utils.GetRelatedMediaFiles(itemPriceContainer, mediaFileId), groupId,
                                                                                        eBundleType.SUBSCRIPTION, numOfUses);
                    if (isCreditDownloaded)
                    {
                        if (!ConditionalAccessDAL.Insert_NewSubscriptionUse(groupId, itemPriceContainer.m_relevantSub.m_sObjectCode, mediaFileId, userId, countryCode, languageCode, udid, nRelPP))
                        {
                            // failed to insert subscription use
                            throw new Exception(GetSubUseInsertionFailureExMsg(itemPriceContainer.m_relevantSub.m_sObjectCode, mediaFileId, userId,
                                isCreditDownloaded, nRelPP));
                        }

                        if (ConditionalAccessDAL.Update_SubPurchaseNumOfUses(groupId, purchasingUserId, itemPriceContainer.m_relevantSub.m_sObjectCode))
                        {
                            //Subscription Purchases updated - update purchase validation key
                            setPurchaseInvalidationKey = true;
                        }
                        else
                        {
                            // failed to update num of uses in subscriptions_purchases.
                            #region Logging
                            StringBuilder sb = new StringBuilder("Failed to update num of uses in subscriptions_purchases table. ");
                            sb.Append(String.Concat("Sub Cd: ", itemPriceContainer.m_relevantSub.m_sObjectCode));
                            sb.Append(String.Concat(" Site Guid: ", purchasingUserId));
                            sb.Append(String.Concat(" Group ID: ", groupId));
                            sb.Append(String.Concat(" MF ID: ", mediaFileId));

                            log.Debug("CriticalError - " + sb.ToString());
                            #endregion
                        }
                    }
                    else
                    {
                        tasks.Add(Task.Factory.StartNew(() => Utils.InsertOfflineSubscriptionUse(groupId, mediaFileId, itemPriceContainer.m_relevantSub.m_sObjectCode, userId,
                                                                                                countryCode, languageCode, udid, nRelPP, contextData)));
                    }

                    string modifiedPPVModuleCode = GetPPVModuleCodeForPPVUses(itemPriceContainer.m_relevantSub.m_sObjectCode, eTransactionType.Subscription);

                    bool isPpvCreditDownloaded = PPV_DoesCreditNeedToDownloaded(modifiedPPVModuleCode, itemPriceContainer.m_relevantSub, null, countryCode, languageCode, udid,
                                                                                Utils.GetRelatedMediaFiles(itemPriceContainer, mediaFileId), domainId, mediaFileId, groupId, mediaId,
                                                                                Utils.GetStartDate(itemPriceContainer), Utils.GetEndDate(itemPriceContainer));
                    if (isPpvCreditDownloaded)
                    {
                        if (ConditionalAccessDAL.Insert_NewPPVUse(groupId, mediaFileId, modifiedPPVModuleCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID))
                        {
                            //last use updated - update "LastUseWithCredit" validation key 
                            string invalidationKey = LayeredCacheKeys.GetLastUseWithCreditForDomainInvalidationKey(groupId, domainId, mediaId);
                            if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                            {
                                log.DebugFormat("Failed to set invalidationKey, key: {0}", invalidationKey);
                            }

                            if (domainEntitlements != null && domainEntitlements.DomainPpvEntitlements != null && domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary != null)
                            {
                                string key = string.Format("{0}_{1}", itemPriceContainer.m_lPurchasedMediaFileID, itemPriceContainer.m_sPPVModuleCode);
                                if (domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary.ContainsKey(key))
                                {
                                    int purchaseId = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].ID;
                                    int PPVnumOfUses = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].numOfUses;
                                    DateTime? endDate = domainEntitlements.DomainPpvEntitlements.EntitlementsDictionary[key].endDate;
                                    if (UpdatePPVPurchases(purchaseId, itemPriceContainer.m_sPPVModuleCode, countryCode, languageCode, udid, groupId, PPVnumOfUses, endDate.Value))
                                    {
                                        //PPV Purchases updated - update purchase validation key
                                        setPurchaseInvalidationKey = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // failed to insert ppv use
                            throw new Exception(GetPPVUseInsertionFailureExMsg(mediaFileId, modifiedPPVModuleCode, userId, nRelPP, releventCollectionID));
                        }
                    }
                    else
                    {
                        tasks.Add(Task.Factory.StartNew(() => Utils.InsertOfflinePpvUse(groupId, mediaFileId, modifiedPPVModuleCode, userId, countryCode, languageCode, udid, nRelPP, releventCollectionID, contextData)));
                    }

                    if (setPurchaseInvalidationKey && !LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetPurchaseInvalidationKey(domainId)))
                    {
                        log.DebugFormat("Failed SetInvalidationKey for setPurchaseInvalidationKey, domainId: {0}", domainId);
                    }
                }
                else
                {
                    //TODO: **************IRA HAS TO LOOK*******************
                    //HandleCollectionPlayUse(cas, userId, mediaFileId, ip, countryCode, languageCode, udid, couponCode, domainId, groupId, itemPriceContainer, releventCollectionID, nRelPP, purchasingUserId);
                }

                if (tasks != null && tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }
        }

        //private static void HandleCollectionPlayUse(BaseConditionalAccess cas, string userId, Int32 mediaFileId, string ip, string countryCode, string languageCode, string udid, string couponCode, long domainId, int groupId, ItemPriceContainer itemPriceContainer, int releventCollectionID, Int32 nRelPP, string purchasingUserId)
        //{
        //    // PPV purchased as part of Collection

        //    Int32 nIsCreditDownloaded = Utils.Bundle_DoesCreditNeedToDownloaded(itemPriceContainer.m_relevantCol.m_sObjectCode, userId, Utils.GetRelatedMediaFiles(itemPriceContainer, mediaFileId), groupId, eBundleType.COLLECTION) ? 1 : 0;

        //    //collections_uses

        //    if (ConditionalAccessDAL.Insert_NewCollectionUse(groupId, itemPriceContainer.m_relevantCol.m_sObjectCode, mediaFileId,
        //        userId, nIsCreditDownloaded > 0, countryCode, languageCode, udid) < 1)
        //    {
        //        // failed to insert values in collections_uses
        //        // throw here an exception
        //        throw new Exception(GetColUseInsertionFailureMsg(itemPriceContainer.m_relevantCol.m_sObjectCode, mediaFileId,
        //            userId, nIsCreditDownloaded > 0, nRelPP));

        //    }
        //    if (nIsCreditDownloaded == 1)
        //    {
        //        if (!ConditionalAccessDAL.Update_ColPurchaseNumOfUses(itemPriceContainer.m_relevantCol.m_sObjectCode, purchasingUserId, groupId))
        //        {
        //            // failed to update num of uses in collections_purchases. logging
        //            #region Logging
        //            StringBuilder sb = new StringBuilder("Failed to increment num of uses in collections_purchases. ");
        //            sb.Append(String.Concat(" Col Code: ", itemPriceContainer.m_relevantCol.m_sObjectCode));
        //            sb.Append(String.Concat(" Group ID: ", groupId));
        //            sb.Append(String.Concat(" Site Guid: ", purchasingUserId));

        //            log.Error("CriticalError - " + sb.ToString());
        //            #endregion
        //        }
        //    }

        //    string modifiedPPVModuleCode = GetPPVModuleCodeForPPVUses(itemPriceContainer.m_relevantCol.m_sObjectCode, eTransactionType.Collection);

        //    Int32 nIsCreditDownloaded1 = PPV_DoesCreditNeedToDownloaded(modifiedPPVModuleCode, null, itemPriceContainer.m_relevantCol, countryCode, languageCode, udid, lUsersIds,
        //                                                                Utils.GetRelatedMediaFiles(itemPriceContainer, mediaFileId), domainId, mediaFileId, groupId);

        //    if (ConditionalAccessDAL.Insert_NewPPVUse(groupId, mediaFileId, modifiedPPVModuleCode, userId, nIsCreditDownloaded1 > 0,
        //        countryCode, languageCode, udid, nRelPP, releventCollectionID) < 1)
        //    {
        //        // failed to insert ppv use
        //        throw new Exception(GetPPVUseInsertionFailureExMsg(mediaFileId, modifiedPPVModuleCode, userId, nRelPP, releventCollectionID));
        //    }

        //    long nPPVID = 0;
        //    if (nIsCreditDownloaded1 == 1)
        //    {
        //        string sRelCol = string.Empty;
        //        nPPVID = GetActivePPVPurchaseID(itemPriceContainer.m_lPurchasedMediaFileID > 0 ? new List<int>(1) { itemPriceContainer.m_lPurchasedMediaFileID } : new List<int>(1) { mediaFileId },
        //                                        ref sRelCol, lUsersIds, domainID, groupId);

        //        if (nPPVID == 0 && !string.IsNullOrEmpty(couponCode))
        //        {
        //            nPPVID = InsertPPVPurchases(userId, mediaFileId, itemPriceContainer.m_oPrice.m_dPrice,
        //                itemPriceContainer.m_oPrice.m_oCurrency.m_sCurrencyCD3, sRelCol, 0, countryCode, languageCode, udid,
        //                itemPriceContainer.m_sPPVModuleCode, couponCode, ip, domainID, groupId, cas);
        //        }

        //        UpdatePPVPurchases(nPPVID, itemPriceContainer.m_sPPVModuleCode, countryCode, languageCode, udid, groupId);
        //    }
        //}

        private static int ExtractRelevantCollectionID(ItemPriceContainer price)
        {
            int res = 0;
            if (price != null && price.m_relevantCol != null)
            {
                Int32.TryParse(price.m_relevantCol.m_sObjectCode, out res);
            }

            return res;
        }

        private static int ExtractRelevantPrePaidID(ItemPriceContainer price)
        {
            if (price.m_relevantSub == null && price.m_relevantCol == null && price.m_relevantPP != null)
            {
                return price.m_relevantPP.m_ObjectCode;
            }

            return 0;
        }

        private static bool IsPurchasedAsPartOfPrePaid(ItemPriceContainer price)
        {
            return price.m_relevantPP != null;
        }

        private static bool IsPurchasedAsPurePPV(ItemPriceContainer price)
        {
            return price.m_relevantSub == null && price.m_relevantCol == null;
        }

        private static bool IsPurchasedAsPartOfSub(ItemPriceContainer price)
        {
            return price.m_relevantCol == null;
        }

        private static bool PPV_DoesCreditNeedToDownloaded(string productCode, Subscription subscription, Collection collection, string countryCode, string languageCode, string udid,
                                                            List<int> mediaFileIDs, long domainId, int mediaFileId, int groupId, int mediaId, DateTime? startDate, DateTime? endDate)
        {
            bool isCreditDownloaded = false;
            Int32 nViewLifeCycle = 0;
            int fullLifeCycle = 0;
            bool isOfflinePlayback = false;
            eTransactionType transactionType = eTransactionType.PPV;
            int OfflineStatus = 0;

            if (OfflineStatus == 1)
            {
                UsageModule OfflineUsageModule = Core.Pricing.Module.GetOfflineUsageModule(groupId, countryCode, languageCode, udid);
                nViewLifeCycle = OfflineUsageModule.m_tsViewLifeCycle;
                fullLifeCycle = OfflineUsageModule.m_tsMaxUsageModuleLifeCycle;
            }
            // in case of PPV
            else if (subscription == null && collection == null)
            {
                PPVModule ppvModule = GetPPVModuleDataForDoesCreditNeedToDownload(productCode, groupId);
                if (ppvModule == null)
                {
                    throw new Exception(String.Concat("PPV_DoesCreditNeedToDownloaded. PPV Module was returned null by WS_Pricing. PPV Code: ", productCode));
                }

                nViewLifeCycle = ppvModule.m_oUsageModule.m_tsViewLifeCycle;
                fullLifeCycle = ppvModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle;
                isOfflinePlayback = ppvModule.m_oUsageModule.m_bIsOfflinePlayBack;
                transactionType = eTransactionType.PPV;
            }
            // in case of Subscription
            else if (subscription != null && subscription.m_oSubscriptionUsageModule != null)
            {
                nViewLifeCycle = subscription.m_oSubscriptionUsageModule.m_tsViewLifeCycle;
                fullLifeCycle = subscription.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle;
                isOfflinePlayback = subscription.m_oSubscriptionUsageModule.m_bIsOfflinePlayBack;
                transactionType = eTransactionType.Subscription;
            }
            // in case of Collection
            else if (collection != null && collection.m_oCollectionUsageModule != null)
            {
                nViewLifeCycle = collection.m_oCollectionUsageModule.m_tsViewLifeCycle;
                fullLifeCycle = collection.m_oCollectionUsageModule.m_tsMaxUsageModuleLifeCycle;
                isOfflinePlayback = collection.m_oCollectionUsageModule.m_bIsOfflinePlayBack;
                transactionType = eTransactionType.Collection;
            }

            DateTime? lastUseWithCredit = GetDomainLastUseWithCredit(groupId, domainId, mediaFileId, mediaFileIDs, productCode, mediaId);

            if (lastUseWithCredit.HasValue)
            {
                DateTime dNow = DateTime.UtcNow;
                // in case DB time isn't in sync with server time
                if (dNow < lastUseWithCredit.Value)
                {
                    dNow = lastUseWithCredit.Value.AddSeconds(1);
                }

                DateTime dEndDate = Utils.GetEndDateTime(lastUseWithCredit.Value, nViewLifeCycle);

                if (dNow >= dEndDate)
                {
                    isCreditDownloaded = true;
                }
            }
            else
            {
                isCreditDownloaded = true;
            }

            if (TVinciShared.WS_Utils.GetTcmBoolValue("ShouldUseLicenseLinkCache"))
            {
                // update lastUseWithCredit value according to nIsCreditDownloaded
                lastUseWithCredit = isCreditDownloaded ? DateTime.UtcNow : lastUseWithCredit;
                CachedEntitlementResults cachedEntitlementResults = new CachedEntitlementResults(nViewLifeCycle, fullLifeCycle, lastUseWithCredit.Value, false, isOfflinePlayback, transactionType, startDate, endDate);
                if (!Utils.InsertOrSetCachedEntitlementResults(domainId, mediaFileId, cachedEntitlementResults))
                {
                    log.DebugFormat("Failed to insert CachedEntitlementResults: {0}, domainId: {1}, mediaFileId: {2}", cachedEntitlementResults.ToString(), domainId, mediaFileId);
                }
            }

            return isCreditDownloaded;
        }

        private static DateTime? GetDomainLastUseWithCredit(int groupId, long domainId, int mediaFileId, List<int> mediaFileIDs, string productCode, int mediaId)
        {
            DateTime? lastUseWithCredit = null;
            try
            {
                if (mediaId > 0)
                {
                    string key = LayeredCacheKeys.GetLastUseWithCreditForDomainKey(groupId, domainId, mediaId);
                    List<FileCreditUsedDetails> filesCreditUsedDetails = null;
                    // try to get from cache            
                    bool cacheResult = LayeredCache.Instance.Get<List<FileCreditUsedDetails>>(key, ref filesCreditUsedDetails, GetFilesCreditUsedDetails, new Dictionary<string, object>() { { "groupId", groupId },
                                            { "domainId", (int)domainId }, { "mediaId", mediaId } }, groupId, GET_DOMAIN_LAST_USE_WITH_CREDIT_LAYERED_CACHE_CONFIG_NAME,
                                            GetDomainLastUseWithCreditInvalidationKeys(groupId, domainId, mediaId));
                    if (cacheResult && filesCreditUsedDetails != null)
                    {
                        // get the file with the latest date used
                        List<FileCreditUsedDetails> filteredFilesCreditUsedDetails = filesCreditUsedDetails.Where(x => x.ProductCode == productCode && mediaFileIDs.Contains(x.Id)).ToList();
                        if (filteredFilesCreditUsedDetails != null && filteredFilesCreditUsedDetails.Count > 0)
                        {
                            FileCreditUsedDetails fileCreditUsedDetails = filteredFilesCreditUsedDetails.OrderByDescending(x => x.DateUsed).First();
                            if (fileCreditUsedDetails != null)
                            {
                                lastUseWithCredit = TVinciShared.DateUtils.UnixTimeStampToDateTime(fileCreditUsedDetails.DateUsed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDomainLastUseWithCredit for groupId: {0}", groupId), ex);
            }

            return lastUseWithCredit;
        }

        private static int GetMediaIdByFildId(int groupId, int mediaFileId)
        {
            int mediaId = 0;
            try
            {
                MeidaMaper[] mapper = Utils.GetMediaMapper(groupId, new int[1] { mediaFileId });
                if (mapper != null && mapper.Length == 1)
                {
                    mediaId = mapper[0].m_nMediaID;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetMediaIdByFildId for groupId: {0}, mediaFileId: {1}", groupId, mediaFileId), ex);
            }

            return mediaId;
        }

        private static List<string> GetDomainLastUseWithCreditInvalidationKeys(int groupId, long domainId, int mediaId)
        {
            return new List<string>()
            {
                LayeredCacheKeys.GetLastUseWithCreditForDomainInvalidationKey(groupId, domainId, mediaId)
            };
        }

        private static Tuple<List<FileCreditUsedDetails>, bool> GetFilesCreditUsedDetails(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<FileCreditUsedDetails> filesCreditUsedDetails = null;

            try
            {
                if (funcParams != null && funcParams.Count == 3 && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId") && funcParams.ContainsKey("domainId"))
                {
                    int? groupId, mediaId, domainId;
                    groupId = funcParams["groupId"] as int?;
                    domainId = funcParams["domainId"] as int?;
                    mediaId = funcParams["mediaId"] as int?;
                    if (groupId.HasValue && mediaId.HasValue && domainId.HasValue)
                    {
                        DataTable dt = DAL.ConditionalAccessDAL.GetFilesCreditUsedDetails(groupId.Value, domainId.Value, mediaId.Value);
                        if (dt != null && dt.Rows != null)
                        {
                            filesCreditUsedDetails = new List<FileCreditUsedDetails>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                int id = ODBCWrapper.Utils.GetIntSafeVal(dr, "MEDIA_FILE_ID", 0);
                                string productCode = ODBCWrapper.Utils.GetSafeStr(dr, "PPVMODULE_CODE");
                                DateTime? lastDateUsed = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");

                                if (id > 0 && !string.IsNullOrEmpty(productCode) && lastDateUsed.HasValue)
                                {
                                    filesCreditUsedDetails.Add(new FileCreditUsedDetails() { Id = id, ProductCode = productCode, DateUsed = TVinciShared.DateUtils.DateTimeToUnixTimestamp(lastDateUsed.Value) });
                                }
                            }

                            res = filesCreditUsedDetails != null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("PlayUsesManager.GetFilesCreditUsedDetails failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<FileCreditUsedDetails>, bool>(filesCreditUsedDetails, res);
        }

        private static PPVModule GetPPVModuleDataForDoesCreditNeedToDownload(string ppvModuleCode, int groupId)
        {
            string actualPPVModuleCode = string.Empty;
            if (ppvModuleCode.Contains("s: "))
                actualPPVModuleCode = ppvModuleCode.Replace("s: ", string.Empty);
            else
            {
                if (ppvModuleCode.Contains("b: "))
                    actualPPVModuleCode = ppvModuleCode.Replace("b: ", string.Empty);
                else
                    actualPPVModuleCode = ppvModuleCode;
            }

            string wsUsername = string.Empty, wsPassword = string.Empty;
            Utils.GetWSCredentials(groupId, eWSModules.PRICING, ref wsUsername, ref wsPassword);

            return Utils.GetPPVModuleDataWithCaching(actualPPVModuleCode, wsUsername, wsPassword, groupId, string.Empty, string.Empty, string.Empty);
        }

        private static string GetPPVUseInsertionFailureExMsg(long mediaFileID, string ppvModuleCode, string siteGuid, int nRelPP, int nRelevantCol)
        {
            StringBuilder sb = new StringBuilder("Failed to insert new ppv use. ");
            sb.Append(String.Concat("MF ID: ", mediaFileID));
            sb.Append(String.Concat(" PPV MC: ", ppvModuleCode));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", true));
            sb.Append(String.Concat(" Rel PP: ", nRelPP));
            sb.Append(String.Concat(" Rel Col: ", nRelevantCol));

            return sb.ToString();
        }

        private static Int32 GetActivePPVPurchaseID(List<int> relatedMediaFileIDs, ref string sRelSub, List<int> lUsersIds, int domainID, int groupId)
        {
            Int32 nRet = 0;
            DataTable dt = ConditionalAccessDAL.Get_AllPPVPurchasesByUserIDsAndMediaFileIDs(groupId, relatedMediaFileIDs, lUsersIds, domainID);

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {

                nRet = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);
                sRelSub = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["SUBSCRIPTION_CODE"]);
            }
            return nRet;
        }

        /// <summary>
        /// Insert PPV Purchases
        /// </summary>
        private static long InsertPPVPurchases(string sSiteGUID, Int32 nMediaFileID, double dPrice, string sCurrency, string sSubCode, Int32 nRecieptCode, string sCountryCd, string sLANGUAGE_CODE,
                                                string sDEVICE_NAME, string sPPVModuleCode, string sCouponCode, string sUserIP, int domainId, int groupId, BaseConditionalAccess cas)
        {
            long purchaseId = 0;

            PPVModule thePPVModule = Core.Pricing.Module.GetPPVModuleData(groupId, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            Subscription relevantSub = Core.Pricing.Module.GetSubscriptionData(groupId, sSubCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, false);

            Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupId);

            string sCustomData = cas.GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode,
                sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            DateTime endDate = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
            purchaseId = ConditionalAccessDAL.Insert_NewPPVPurchase(groupId, nMediaFileID, sSiteGUID, dPrice, sCurrency,
                                                                            thePPVModule.m_oUsageModule != null ? thePPVModule.m_oUsageModule.m_nMaxNumberOfViews : 0, sCustomData,
                                                                            relevantSub != null ? relevantSub.m_sObjectCode : string.Empty, nRecieptCode, DateTime.UtcNow, endDate,
                                                                            DateTime.UtcNow, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, domainId);

            return purchaseId;
        }

        /// <summary>
        /// Update PPV Purchases
        /// </summary>
        private static bool UpdatePPVPurchases(long nPPVPurchaseID, string sPPVModuleCode, string sCOUNTRY_CODE, string sLANGUAGE_CODE,
            string sDEVICE_NAME, int groupId, int numOfUses, DateTime endDate)
        {
            bool res = false;

            PPVModule thePPVModule = GetPPVModule(sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, groupId);
            DateTime d = endDate;

            if (thePPVModule != null && thePPVModule.m_oUsageModule != null && thePPVModule.m_oUsageModule.m_nMaxNumberOfViews > 0)
            {
                numOfUses += 1;
                if (numOfUses >= thePPVModule.m_oUsageModule.m_nMaxNumberOfViews)
                {
                    d = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsViewLifeCycle);
                }

                res = ConditionalAccessDAL.Update_PPVNumOfUses(nPPVPurchaseID, d < endDate ? (DateTime?)d : null);
                if (!res)
                {
                    #region Logging
                    StringBuilder sb = new StringBuilder("Error at UpdatePPVPurchases. Probably failed to update num of uses value. ");
                    sb.Append(String.Concat(" PPV Purchase ID: ", nPPVPurchaseID));
                    sb.Append(String.Concat(" PPV M CD: ", sPPVModuleCode));
                    log.Error("Error - " + sb.ToString());
                    #endregion
                }
            }

            return res;
        }

        private static bool IsLastView(long nPPVPurchaseID, ref DateTime endDateTime)
        {
            int nMaxNumOfUses = 0;
            int nNumOfUses = 0;
            ConditionalAccessDAL.Get_IsLastViewData(nPPVPurchaseID, ref nNumOfUses, ref nMaxNumOfUses, ref endDateTime);

            return nNumOfUses + 1 >= nMaxNumOfUses;
        }

        private static PPVModule GetPPVModule(string sPPVModuleCode, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, int groupId)
        {
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;

            PPVModule thePPVModule = Core.Pricing.Module.GetPPVModuleData(groupId, sPPVModuleCode, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);

            return thePPVModule;
        }

        private static string GetPurchasingSiteGuid(ItemPriceContainer price)
        {
            if (!string.IsNullOrEmpty(price.m_sPurchasedBySiteGuid))
            {
                return price.m_sPurchasedBySiteGuid;
            }

            return string.Empty;
        }

        private static string GetColUseInsertionFailureMsg(string colCode, long mediaFileID, string siteGuid, bool isCreditDownloaded, int relPP)
        {
            StringBuilder sb = new StringBuilder("Failed to insert value into collection_uses table. ");
            sb.Append(String.Concat("Col ID: ", colCode));
            sb.Append(String.Concat(" MF ID: ", mediaFileID));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", isCreditDownloaded));
            sb.Append(String.Concat(" Rel PP: ", relPP));

            return sb.ToString();

        }

        private static string GetSubUseInsertionFailureExMsg(string subCode, long mediaFileID, string siteGuid, bool isCreditDownloaded, int relPP)
        {
            StringBuilder sb = new StringBuilder("Failed to insert sub use. ");
            sb.Append(String.Concat("Sub Code: ", subCode));
            sb.Append(String.Concat(" MF ID: ", mediaFileID));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Is CD: ", isCreditDownloaded));
            sb.Append(String.Concat(" Rel PP: ", relPP));

            return sb.ToString();
        }

        private static string GetPPVModuleCodeForPPVUses(string ppvModuleCode, eTransactionType purchasedAs)
        {
            string res = string.Empty;
            switch (purchasedAs)
            {
                case eTransactionType.Subscription:
                    res = String.Concat("s: ", ppvModuleCode);
                    break;
                case eTransactionType.Collection:
                    res = String.Concat("b: ", ppvModuleCode);
                    break;
                default:
                    // ppv
                    res = ppvModuleCode;
                    break;

            }

            return res;
        }


        #region Delete canidates

        ///// <summary>
        ///// Update Susbscription Purchase
        ///// </summary>
        //protected static void UpdateCollectionPurchases(string sColCd, string sSiteGUID)
        //{
        //    ODBCWrapper.DirectQuery directQuery = null;
        //    try
        //    {
        //        directQuery = new ODBCWrapper.DirectQuery();
        //        directQuery += "update collections_purchases set NUM_OF_USES=NUM_OF_USES+1,LAST_VIEW_DATE=getdate() where ";
        //        directQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", GetActiveCollectionPurchaseID(sColCd, sSiteGUID));
        //        directQuery.Execute();
        //    }
        //    finally
        //    {
        //        if (directQuery != null)
        //        {
        //            directQuery.Finish();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get Active Subscription Purchase ID
        ///// </summary>
        //protected static Int32 GetActiveSubscriptionPurchaseID(string sSubCd, string sSiteGUID)
        //{
        //    Int32 nRet = 0;
        //    ODBCWrapper.DataSetSelectQuery selectQuery = null;
        //    try
        //    {
        //        selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //        selectQuery += "select ID from subscriptions_purchases with (nolock) where is_active=1 and status=1 and ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SITE_USER_GUID", "=", sSiteGUID);
        //        selectQuery += " and (MAX_NUM_OF_USES>=NUM_OF_USES OR MAX_NUM_OF_USES=0) and START_DATE<getdate() and (end_date is null or end_date>getdate()) and ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "=", sSubCd);
        //        selectQuery += " and ";
        //        selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
        //        if (selectQuery.Execute("query", true) != null)
        //        {
        //            Int32 nCount = selectQuery.Table("query").DefaultView.Count;
        //            if (nCount > 0)
        //                nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString());
        //        }
        //    }
        //    finally
        //    {
        //        if (selectQuery != null)
        //        {
        //            selectQuery.Finish();
        //        }
        //    }
        //    return nRet;
        //}

        #endregion

    }
}
