using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Catalog;
using ApiObjects.ConditionalAccess;
using ApiObjects.Response;
using Core.Billing;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace Core.ConditionalAccess
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        public static PermittedMediaContainer[] GetUserPermittedItems(int groupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                PermittedMediaContainer[] res = t.GetUserPermittedItems(sSiteGUID);
                return res != null ? res : new PermittedMediaContainer[0];
            }
            else
            {
                return null;
            }
        }


        public static PermittedMediaContainerResponse GetDomainPermittedItems(int groupID, int nDomainID)
        {
            PermittedMediaContainerResponse response = new PermittedMediaContainerResponse();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response.PermittedMediaContainer = t.GetDomainPermittedItems(nDomainID);
                if (response.PermittedMediaContainer != null)
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                else
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }


        public static PermittedMediaContainer[] GetUserExpiredItems(int groupID, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPermittedItems(new List<int> { int.Parse(sSiteGUID) }, true, numOfItems);
            }
            else
            {
                return null;
            }
        }


        public static UserCAStatus GetUserCAStatus(int groupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserCAStatus(sSiteGUID);
            }
            else
            {
                return UserCAStatus.Annonymus;
            }
        }


        public static string GetLicensedLink(int groupID, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName))
                sDeviceName = "";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetLicensedLink(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, string.Empty);
            }
            else
            {
                return null;
            }
        }


        public static LicensedLinkResponse GetEPGLicensedLink(int groupID, string sSiteGUID, int nMediaFileID, int nEPGItemID, DateTime startTime, string sBasicLink, string sUserIP, string sRefferer,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, int nFormatType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = string.Empty;
            if (String.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = string.Empty;
            if (String.IsNullOrEmpty(sDeviceName))
                sDeviceName = string.Empty;

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetEPGLink(nEPGItemID.ToString(), startTime, nFormatType, sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, string.Empty);
            }
            else
            {
                return new LicensedLinkResponse() { Status = new Status() { Code = (int)eResponseStatus.WrongPasswordOrUserName, Message = string.Empty } };
            }
        }


        public static string GetLicensedLinkWithCoupon(int groupID, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string couponCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetLicensedLink(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, couponCode);
            }
            else
            {
                return null;
            }
        }


        public static bool ActivateCampaign(int groupID, int campaignID, CampaignActionInfo actionInfo)
        {

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.ActivateCampaign(campaignID, actionInfo);
            }
            else
            {
                return false;
            }
        }


        public static CampaignActionInfo ActivateCampaignWithInfo(int groupID, int campaignID, CampaignActionInfo actionInfo)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.ActivateCampaignWithInfo(campaignID, actionInfo);
            }
            else
            {
                return null;
            }
        }


        public static PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(int groupID, int nDomainID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetDomainPermittedSubscriptions(nDomainID);
            }
            else
            {
                return null;
            }
        }


        public static PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(int groupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPermittedSubscriptions(sSiteGUID);
            }
            else
            {
                return null;
            }
        }


        public static PermittedCollectionContainer[] GetDomainPermittedCollections(int groupID, int nDomainID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetDomainPermittedCollections(nDomainID);
            }
            else
            {
                return null;
            }
        }


        public static PermittedCollectionContainer[] GetUserPermittedCollections(int groupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPermittedCollections(sSiteGUID);
            }
            else
            {
                return null;
            }
        }


        public static PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(int groupID, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPermittedSubscriptions(new List<int>() { int.Parse(sSiteGUID) }, true, numOfItems);
            }
            else
            {
                return null;
            }
        }



        public static PermittedCollectionContainer[] GetUserExpiredCollections(int groupID, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPermittedCollections(new List<int>() { int.Parse(sSiteGUID) }, true, numOfItems);
            }
            else
            {
                return null;
            }
        }



        public static bool IsPermittedItem(int groupID, string sSiteGUID, int mediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.IsItemPermitted(sSiteGUID, mediaID);
            }
            else
            {
                return false;
            }
        }


        public static bool IsPermittedSubscription(int groupID, string sSiteGUID, int subID, ref string reason)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.IsSubscriptionPurchased(sSiteGUID, subID.ToString(), ref reason);
            }
            else
            {
                return false;
            }
        }


        public static UserBillingTransactionsResponse[] GetUsersBillingHistory(int groupID, string[] arrSiteGUIDs, DateTime dStartDate, DateTime dEndDate)
        {
            // add siteguid to logs/monitor
            if (arrSiteGUIDs != null && arrSiteGUIDs.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var siteGuid in arrSiteGUIDs)
                    sb.Append(String.Format("{0} ", siteGuid));

                HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sb.ToString();
            }

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUsersBillingHistory(arrSiteGUIDs, dStartDate, dEndDate);
            }
            else
            {
                return null;
            }
        }


        public static DomainTransactionsHistoryResponse GetDomainTransactionsHistory(int groupID, int domainID, DateTime dStartDate, DateTime dEndDate, int pageSize, int pageIndex, TransactionHistoryOrderBy orderBy)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetDomainTransactionsHistory(domainID, dStartDate, dEndDate, pageSize, pageIndex, orderBy);
            }
            else
            {
                return null;
            }
        }


        public static DomainsBillingTransactionsResponse GetDomainsBillingHistory(int groupID,
            int[] domainIDs, DateTime dStartDate, DateTime dEndDate)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetDomainsBillingHistory(domainIDs, dStartDate, dEndDate);
            }
            else
            {
                return null;
            }
        }


        public static BillingTransactions GetUserBillingHistory(int groupID, string sSiteGUID, Int32 nStartIndex, Int32 nNumberOfItems, TransactionHistoryOrderBy orderBy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserBillingHistory(sSiteGUID, nStartIndex, nNumberOfItems, orderBy);
            }
            else
            {
                return null;
            }
        }


        public static bool RenewCancledSubscription(int groupID, string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.RenewCacledSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
            }
            else
            {
                return false;
            }
        }


        public static bool CancelSubscription(int groupID, string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelSubscription(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.
        /// Returns Status Object
        /// </summary>
        /// <param name="sWSUserName"></param>
        /// <param name="sWSPassword"></param>
        /// <param name="nDomainId"></param>
        /// <param name="sSubscriptionCode"></param>
        /// <param name="nSubscriptionPurchaseID"></param>
        /// <returns></returns>
        public static ApiObjects.Response.Status CancelSubscriptionRenewal(int groupID, int nDomainId, string sSubscriptionCode)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelSubscriptionRenewal(nDomainId, sSubscriptionCode);
            }
            else
            {
                return (new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Invalid request" });
            }
        }


        public static bool ChangeSubscriptionDates(int groupID, string sSiteGUID, string sSubscriptionCode,
            Int32 nSubscriptionPurchaseID, Int32 dAdditionInDays, bool bNewRenewable)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.UpdateSubscriptionDate(sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID, dAdditionInDays, bNewRenewable);
            }
            else
            {
                return false;
            }
        }


        public static MediaFileItemPricesContainer[] GetItemsPricesEx(int groupID, WSInt32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nSize = nMediaFiles.Length;
                Int32[] nMediaFileIDs = new Int32[nSize];
                for (int j = 0; j < nSize; j++)
                    nMediaFileIDs[j] = nMediaFiles[j].m_nInt32;
                return t.GetItemsPrices(nMediaFileIDs, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static MediaFileItemPricesContainer[] GetItemsPricesWithCouponsEx(int groupID, WSInt32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nSize = nMediaFiles.Length;
                Int32[] nMediaFileIDs = new Int32[nSize];
                for (int j = 0; j < nSize; j++)
                    nMediaFileIDs[j] = nMediaFiles[j].m_nInt32;
                return t.GetItemsPrices(nMediaFileIDs, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static MediaFileItemPricesContainer[] GetItemsPrices(int groupID, Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemsPrices(nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static MediaFileItemPricesContainer[] GetItemsPricesByIP(int groupID, Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemsPrices(nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static MediaFileItemPricesContainerResponse GetItemsPricesWithCoupons(int groupID, Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            MediaFileItemPricesContainerResponse response = new MediaFileItemPricesContainerResponse();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response.ItemsPrices = t.GetItemsPrices(nMediaFiles, sUserGUID, sCouponCode != null ? sCouponCode : string.Empty, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
                if (response.ItemsPrices != null)
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                else
                    response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        public static MediaFileItemPricesContainer[] GetItemsPricesWithCouponsByIP(int groupID, Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemsPrices(nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionsPricesContainer[] GetSubscriptionsPrices(int groupID, string[] sSubscriptions, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static CollectionsPricesContainer[] GetCollectionsPrices(int groupID, string[] sCollections, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetCollectionsPrices(sCollections, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesByIP(int groupID, string[] sSubscriptions, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }


        public static PrePaidPricesContainer[] GetPrePaidPrices(int groupID, string[] sPrePaids, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetPrePaidPrices(sPrePaids, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static SubscriptionsPricesResponse GetSubscriptionsPricesWithCoupon(int groupID, string[] sSubscriptions, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            SubscriptionsPricesResponse response = new SubscriptionsPricesResponse();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                try
                {
                    response.SubscriptionsPrices = t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, sCouponCode != null ? sCouponCode : string.Empty, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
                }
                catch (Exception)
                {
                    response.Status = new Status((int)eResponseStatus.Error, "Error");
                }
                if (response.SubscriptionsPrices != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, "OK");
                }
                else
                    response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }


        public static CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(int groupID, string[] sCollections, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetCollectionsPrices(sCollections, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCouponByIP(int groupID, string[] sSubscriptions, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }


        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesST(int groupID, string sSubscriptionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sSubscriptions = sSubscriptionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static CollectionsPricesContainer[] GetCollectionsPricesST(int groupID, string sCollectionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sCollections = sCollectionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetCollectionsPrices(sCollections, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesSTByIP(int groupID, string sSubscriptionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sSubscriptions = sSubscriptionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, string.Empty, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static PrePaidPricesContainer[] GetPrePaidPricesST(int groupID, string sPrePaidList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sPrePaids = sPrePaidList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetPrePaidPrices(sPrePaids, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCoupon(int groupID, string sSubscriptionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sSubscriptions = sSubscriptionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static CollectionsPricesContainer[] GetCollectionsPricesSTWithCoupon(int groupID, string sCollectionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sCollections = sCollectionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetCollectionsPrices(sCollections, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }

        public static SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCouponByIP(int groupID, string sSubscriptionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sSubscriptions = sSubscriptionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetSubscriptionsPrices(sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }


        public static MediaFileItemPricesContainer[] GetItemsPricesST(int groupID, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            Int32[] nMediaFileIDs = null;
            string[] sMediaIDs = sMediaFilesCommaSeperated.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            if (sMediaIDs.Length > 0)
                nMediaFileIDs = new int[sMediaIDs.Length];
            for (int j = 0; j < sMediaIDs.Length; j++)
                nMediaFileIDs[j] = int.Parse(sMediaIDs[j]);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemsPrices(nMediaFileIDs, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }

        public static MediaFileItemPricesContainer[] GetItemsPricesSTByIP(int groupID, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            Int32[] nMediaFileIDs = null;
            string[] sMediaIDs = sMediaFilesCommaSeperated.Split(sSep, StringSplitOptions.RemoveEmptyEntries);
            if (sMediaIDs.Length > 0)
                nMediaFileIDs = new int[sMediaIDs.Length];
            for (int j = 0; j < sMediaIDs.Length; j++)
                nMediaFileIDs[j] = int.Parse(sMediaIDs[j]);

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemsPrices(nMediaFileIDs, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse Cellular_ChargeUserForSubscription(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.Cellular_ChargeUserForSubscription(sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse CC_DummyChargeUserForSubscription(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CC_ChargeUserForBundle(sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, true, string.Empty, string.Empty, eBundleType.SUBSCRIPTION);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse CC_DummyChargeUserForCollection(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CC_ChargeUserForBundle(sSiteGUID, dPrice, sCurrencyCode3, sCollectionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, true, string.Empty, string.Empty, eBundleType.COLLECTION);
            }
            else
            {
                return null;
            }
        }



        public static BillingResponse CC_ChargeUserForPrePaid(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CC_ChargeUserForPrePaid(sSiteGUID, dPrice, sCurrencyCode3, sPrePaidCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false);
            }
            else
            {
                return null;
            }
        }



        public static BillingResponse CC_DummyChargeUserForPrePaid(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CC_ChargeUserForPrePaid(sSiteGUID, dPrice, sCurrencyCode3, sPrePaidCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, true);
            }
            else
            {
                return null;
            }
        }



        public static BillingResponse CC_DummyChargeUserForMediaFile(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                return t.CC_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, true, string.Empty, string.Empty);
            }
            else
            {
                return null;
            }
        }



        public static BillingStatusResponse CC_ChargeUserForMediaFile(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BillingStatusResponse response = new BillingStatusResponse();

            if (string.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = "";
            if (string.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = "";
            if (string.IsNullOrEmpty(sDeviceName))
                sDeviceName = "";

            BaseConditionalAccess t = null;

            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                response.BillingResponse = t.CC_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false, sPaymentMethodID, sEncryptedCVV);

                if (response.BillingResponse == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }
                else
                {
                    string statusDesc = !string.IsNullOrEmpty(response.BillingResponse.m_sStatusDescription) ? response.BillingResponse.m_sStatusDescription : "Error";
                    if (response.BillingResponse.m_oStatus == BillingResponseStatus.Success)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                    }
                    else if (response.BillingResponse.m_oStatus == BillingResponseStatus.UnKnownUser)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, statusDesc);
                    }
                    else
                    {
                        eResponseStatus status;
                        if (Enum.TryParse(response.BillingResponse.m_oStatus.ToString(), out status))
                        {
                            response.Status = new ApiObjects.Response.Status((int)status, statusDesc);
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, statusDesc);
                        }
                    }
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        public static BillingStatusResponse CC_ChargeUserForSubscription(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BillingStatusResponse response = new BillingStatusResponse();

            if (String.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = string.Empty;
            if (String.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = string.Empty;
            if (String.IsNullOrEmpty(sDeviceName))
                sDeviceName = string.Empty;
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response.BillingResponse = t.CC_ChargeUserForBundle(sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false, sPaymentMethodID, sEncryptedCVV, eBundleType.SUBSCRIPTION);

                if (response.BillingResponse == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }
                else
                {
                    string statusDesc = !string.IsNullOrEmpty(response.BillingResponse.m_sStatusDescription) ? response.BillingResponse.m_sStatusDescription : "Error";
                    if (response.BillingResponse.m_oStatus == BillingResponseStatus.Success)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                    }
                    else if (response.BillingResponse.m_oStatus == BillingResponseStatus.UnKnownUser)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, statusDesc);
                    }
                    else
                    {
                        eResponseStatus status;
                        if (Enum.TryParse(response.BillingResponse.m_oStatus.ToString(), out status))
                        {
                            response.Status = new ApiObjects.Response.Status((int)status, statusDesc);
                        }
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, statusDesc);
                        }
                    }
                }
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }


        public static BillingResponse CC_ChargeUserForCollection(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CC_ChargeUserForBundle(sSiteGUID, dPrice, sCurrencyCode3, sCollectionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false, sPaymentMethodID, sEncryptedCVV, eBundleType.COLLECTION);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse PU_GetPPVPopupPaymentMethodURL(int groupID,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode,
            string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                return t.PU_GetPPVPopupPaymentMethodURL(sSiteGUID, dPrice, sCurrencyCode3,
                    nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sPaymentMethod, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static int AD_GetCustomDataID(int groupID,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 assetID, string sPPVModuleCode, string sCampaignCode,
            string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {

                if (assetType == 1)
                {
                    Int32 nMediaID = Utils.GetMediaIDFromFileID(assetID, groupID);
                    return t.GetPPVCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID, nMediaID, sPPVModuleCode, sCouponCode, sCampaignCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                else if (assetType == 2)
                {
                    return t.GetSubscriptionCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                else if (assetType == 3)
                {
                    return t.GetPrePaidCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName);

                }
                else if (assetType == 4)
                {
                    return t.GetCollectionCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName);

                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


        public static int GetCustomDataID(int groupID,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 assetID, string sPPVModuleCode, string sCampaignCode,
            string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType, string sOverrideEndDate, string sPreviewModuleID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {

                if (assetType == 1)
                {
                    Int32 nMediaID = Utils.GetMediaIDFromFileID(assetID, groupID);
                    return t.GetPPVCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID, nMediaID, sPPVModuleCode, sCouponCode, sCampaignCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, sOverrideEndDate);
                }
                else if (assetType == 2)
                {
                    return t.GetBundleCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, sOverrideEndDate, sPreviewModuleID, eBundleType.SUBSCRIPTION);
                }
                else if (assetType == 3)
                {
                    return t.GetPrePaidCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, sOverrideEndDate);

                }
                else if (assetType == 4)
                {
                    return t.GetBundleCustomDataID(sSiteGUID, dPrice, sCurrencyCode3,
                        assetID.ToString(), sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, sOverrideEndDate, sPreviewModuleID, eBundleType.COLLECTION);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


        public static BillingResponse PU_GetSubscriptionPopupPaymentMethodURL(int groupID,
            string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode,
            string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.PU_GetSubscriptionPopupPaymentMethodURL(sSiteGUID, dPrice, sCurrencyCode3,
                    sSubscriptionCode, sCouponCode, sPaymentMethod, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse SMS_ChargeUserForMediaFile(int groupID, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                return t.SMS_ChargeUserForMediaFile(sSiteGUID, sCellPhone, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse SMS_ChargeUserForSubscription(int groupID, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.SMS_ChargeUserForSubscription(sSiteGUID, sCellPhone, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse SMS_CheckCodeForMediaFile(int groupID, string sSiteGUID, string sCellPhone, string sSMSCode, Int32 nMediaFileID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.SMS_CheckCodeForMediaFile(sSiteGUID, sCellPhone, sSMSCode, nMediaFileID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static BillingResponse SMS_CheckCodeForSubscription(int groupID, string sSiteGUID, string sCellPhone, string sSMSCode, string sSubscriptionCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.SMS_CheckCodeForSubscription(sSiteGUID, sCellPhone, sSMSCode, sSubscriptionCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static PrePaidResponse PP_ChargeUserForMediaFile(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                return t.PP_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static PrePaidResponse PP_ChargeUserForSubscription(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.PP_ChargeUserForSubscription(sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                return null;
            }
        }


        public static UserPrePaidContainer GetUserPrePaidStatus(int groupID, string sSiteGUID, string sCurrencyCode3)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPrePaidStatus(sSiteGUID, sCurrencyCode3);
            }
            else
            {
                return null;
            }
        }


        public static PrePaidHistoryResponse GetUserPrePaidHistory(int groupID, string sSiteGUID, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPrePaidSHistory(sSiteGUID, nNumberOfItems);
            }
            else
            {
                return null;
            }
        }



        public static string GetItemLeftViewLifeCycle(int groupID, string sMediaFileID, string sSiteGUID, bool bIsCoGuid,
            string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCOUNTRY_CODE) == true)
                sCOUNTRY_CODE = "";
            if (String.IsNullOrEmpty(sLANGUAGE_CODE) == true)
                sLANGUAGE_CODE = "";
            if (String.IsNullOrEmpty(sDEVICE_NAME) == true)
                sDEVICE_NAME = "";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetItemLeftViewLifeCycle(sMediaFileID, sSiteGUID, bIsCoGuid, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
            }
            else
            {
                return TimeSpan.Zero.ToString();
            }
        }


        public static EntitlementResponse GetEntitlement(int groupID, string sMediaFileID, string sSiteGUID, bool bIsCoGuid,
            string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, bool isRecording)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            EntitlementResponse objResponse = null;

            if (string.IsNullOrEmpty(sCOUNTRY_CODE))
            {
                sCOUNTRY_CODE = string.Empty;
            }

            if (string.IsNullOrEmpty(sLANGUAGE_CODE))
            {
                sLANGUAGE_CODE = string.Empty;
            }

            if (string.IsNullOrEmpty(sDEVICE_NAME))
            {
                sDEVICE_NAME = string.Empty;
            }

            BaseConditionalAccess objConditionalAccess = null;
            Utils.GetBaseConditionalAccessImpl(ref objConditionalAccess, groupID);
            if (objConditionalAccess != null)
            {
                return objConditionalAccess.GetEntitlement(sMediaFileID, sSiteGUID, bIsCoGuid, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, isRecording);
            }
            else
            {
                objResponse = new EntitlementResponse()
                {
                    FullLifeCycle = TimeSpan.Zero.ToString(),
                    ViewLifeCycle = TimeSpan.Zero.ToString()
                };
            }

            return objResponse;
        }


        public static BillingResponse InApp_ChargeUserForMediaFile(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, string sProductCode, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                int nMediaFileID = 0;
                Int32 nMediaID = Utils.GetMediaIDFromFileID(sProductCode, groupID, ref nMediaFileID);
                return t.CC_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, ReceiptData);
            }
            else
            {
                return null;
            }
        }

        public static BillingResponse InApp_ChargeUserForSubscription(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode, string sProductCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2) == true)
                sCountryCd2 = "";
            if (String.IsNullOrEmpty(sLanguageCode3) == true)
                sLanguageCode3 = "";
            if (String.IsNullOrEmpty(sDeviceName) == true)
                sDeviceName = "";
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {

                return t.InApp_ChargeUserForSubscription(sSiteGUID, dPrice, sCurrencyCode, sProductCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, ReceiptData);
            }
            else
            {
                return null;
            }
        }

        public static string GetGoogleSignature(int groupID, int nCustomDataID)
        {

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return Utils.GetGoogleSignature(groupID, nCustomDataID);

            }
            else
            {
                return null;
            }
        }




        public static BillingResponse Cellular_ChargeUserForMediaFile(int groupID, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (string.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = "";
            if (string.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = "";
            if (string.IsNullOrEmpty(sDeviceName))
                sDeviceName = "";

            BaseConditionalAccess t = null;

            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, groupID);
                return t.Cellular_ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false);
            }
            else
            {
                return null;
            }
        }



        public static ChangeSubscriptionStatus ChangeSubscription(int groupID, string sSiteGuid, int nOldSubscription, int nNewSubscription)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.ChangeSubscription(sSiteGuid, nOldSubscription, nNewSubscription);
            }
            return ChangeSubscriptionStatus.Error;
        }


        /**************************************************************************************************************************
         *  CancelServiceNow 
         *  for PPV - assetID = mediaFileID
         *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
         *  if the cancelation window is available - do the cancel or if the force flag is on
         *  Return status object
         ***************************************************************************************************************************/
        public static ApiObjects.Response.Status CancelServiceNow(int groupID,
            int nDomainId, int nAssetID, ApiObjects.eTransactionType transactionType, bool bIsForce = false)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelServiceNow(nDomainId, nAssetID, transactionType, bIsForce);
            }
            else
            {
                return (new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Invalid request" });
            }
        }

        // OBSOLETE

        /**************************************************************************************************************************
         *  CancelTransaction 
         *  for PPV - assetID = mediaFileID
         *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
         *  if the cancelation window is available - do the cancel and return true if success, false otherwise
         ***************************************************************************************************************************/
        public static bool CancelTransaction(int groupID, string sSiteGuid, int nAssetID, eTransactionType transactionType, bool bIsForce = false)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelTransaction(sSiteGuid, nAssetID, transactionType, bIsForce);
            }
            else
            {
                return (false);
            }
        }

        /**************************************************************************************************************************
          *  WaiverTransaction 
          *  for PPV - assetID = mediaFileID
          *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
          *  if the cancelation window is available - do the cancel return true , else return false
          ***************************************************************************************************************************/
        public static ApiObjects.Response.Status WaiverTransaction(int groupID, string sSiteGuid, int nAssetID, eTransactionType transactionType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.WaiverTransaction(sSiteGuid, nAssetID, transactionType);
            }
            else
            {
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error };
            }
        }


        public static LicensedLinkResponse GetLicensedLinks(int groupID, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            if (String.IsNullOrEmpty(sCountryCd2))
                sCountryCd2 = string.Empty;
            if (String.IsNullOrEmpty(sLanguageCode3))
                sLanguageCode3 = string.Empty;
            if (String.IsNullOrEmpty(sDeviceName))
                sDeviceName = string.Empty;

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetLicensedLinks(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, string.Empty);
            }
            else
            {
                return new LicensedLinkResponse() { Status = new Status() { Code = (int)eResponseStatus.Error } };
            }
        }


        public static NPVRResponse GetNPVRResponse(BaseNPVRCommand command)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = command != null && command.siteGuid != null ? command.siteGuid : "null";

            if (command != null)
            {
                // get action ID
                HttpContext.Current.Items[KLogMonitor.Constants.ACTION] = command.GetType();

                return command.Execute();
            }

            return null;
        }


        public static DomainServicesResponse GetDomainServices(int groupID, int domainID)
        {
            if (domainID == 0)
                return null;
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetDomainServices(groupID, domainID);
            }
            else
            {
                return new DomainServicesResponse((int)eResponseStatus.WrongPasswordOrUserName, null);
            }
        }


        public static Entitlements GetUserSubscriptions(int groupID, string sSiteGUID)
        {
            Entitlements response = new Entitlements();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserSubscriptions(sSiteGUID); // GetUserPermittedSubscriptions
            }
            else
            {
                response = new Entitlements();
                response.status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        ///Charge a household for specific content utilizing the household’s pre-assigned payment gateway. 
        ///Online, one-time charge only of various content types. 
        ///Upon successful charge, watch entitlements are granted.
        ///siteGUID : User’s household to charge 
        ///price : Net sum to charge
        ///currency	: Identifier for paying currency, according to ISO 4217
        ///contentID : Identifier for the content to purchase. Relevant only if Product type = PPV
        ///productID : Identifier for the package from which this content is offered  
        ///transactionType : Package type. Possible values: PPV, Subscription, Collection (eTransactionType not include prepaid)
        ///coupon :	A valid coupon to apply for this purchase

        public static TransactionResponse Purchase(int groupID, string siteguid, long householdId, double price, string currency, Int32 contentId,
            int productId, eTransactionType transactionType, string coupon, string userIp, string deviceName, int paymentGatewayId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse();

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Utils.GetBaseConditionalAccessImpl(ref casImpl, groupID);

            if (casImpl != null)
            {
                response = casImpl.Purchase(siteguid, householdId, price, currency, contentId, productId, transactionType, coupon, userIp, deviceName, paymentGatewayId, paymentMethodId, adapterData);
                if (response == null)
                {
                    response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            return response;
        }


        public static TransactionResponse ProcessReceipt(int groupID, string siteguid, long household, Int32 contentId,
                                         int productId, eTransactionType transactionType, string userIp, string deviceName, string purchaseToken, string paymentGatewayName)
        {
            TransactionResponse response = new TransactionResponse();

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Utils.GetBaseConditionalAccessImpl(ref casImpl, groupID);

            if (casImpl != null)
            {
                response = casImpl.ProcessReceipt(siteguid, household, contentId, productId, transactionType, userIp, deviceName, purchaseToken, paymentGatewayName);
                if (response == null)
                    response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }


        public static ApiObjects.Response.Status GrantEntitlements(int groupID, string siteguid, long housholdId, Int32 contentId,
                                        int productId, eTransactionType transactionType, string userIp, string deviceName, bool history)
        {
            ApiObjects.Response.Status status = null;


            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Utils.GetBaseConditionalAccessImpl(ref casImpl, groupID);

            if (casImpl != null)
            {
                status = casImpl.GrantEntitlements(siteguid, housholdId, contentId, productId, transactionType, userIp, deviceName, history);
                if (status == null)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            return status;
        }


        public static ApiObjects.Response.Status UpdatePendingTransaction(int groupID, string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus,
            string externalMessage, int failReason, string signature)
        {
            ApiObjects.Response.Status response = null;

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Utils.GetBaseConditionalAccessImpl(ref casImpl, groupID);

            if (casImpl != null)
            {
                response = casImpl.UpdatePendingTransaction(paymentGatewayId, adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
                if (response == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            return response;
        }


        public static ApiObjects.Response.Status CheckPendingTransaction(int groupId,
        long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId,
        string siteGuid, int productId, int productType)
        {
            ApiObjects.Response.Status response = null;

            // get partner implementation and group ID
            BaseConditionalAccess conditionalAccess = null;
            Utils.GetBaseConditionalAccessImpl(ref conditionalAccess, groupId);
            if (conditionalAccess != null)
            {
                response = conditionalAccess.CheckPendingTransaction(paymentGatewayPendingId, numberOfRetries, billingGuid, paymentGatewayTransactionId,
                    siteGuid, productId, productType);

                if (response == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }

            return response;
        }


        public static Entitlements GetUserEntitlements(int groupID, string sSiteGUID, eTransactionType type, bool isExpired, int pageSize, int pageIndex, ApiObjects.EntitlementOrderBy orderBy)
        {
            Entitlements response = new Entitlements();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response = t.GetUserEntitlements(sSiteGUID, type, isExpired, pageSize, pageIndex, orderBy);
            }
            else
            {
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }


        public static Entitlements GetDomainEntitlements(int groupID, int domainId, eTransactionType type, bool isExpired, int pageSize, int pageIndex, ApiObjects.EntitlementOrderBy orderBy)
        {
            Entitlements response = new Entitlements();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response = t.GetDomainEntitlements(domainId, type, isExpired, pageSize, pageIndex);
            }
            else
            {
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }


        public static bool Renew(int groupID, string siteguid, long purchaseId, string billingGuid, long endDate)
        {
            bool response = false;

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Utils.GetBaseConditionalAccessImpl(ref casImpl, groupID);

            if (casImpl != null)
            {
                bool shouldUpdateTaskStatus = true;
                try
                {
                    response = casImpl.Renew(siteguid, purchaseId, billingGuid, endDate, ref shouldUpdateTaskStatus);
                }
                catch (Exception ex)
                {
                    log.Error("Error while trying to renew", ex);
                }

                // Update subscription renewing status to "not active"

                if (shouldUpdateTaskStatus && !casImpl.UpdateSubscriptionRenewingStatus(purchaseId, billingGuid, false))
                {
                    log.ErrorFormat("Error while trying to update subscription renewing status to 0. purchaseId: {0}, billingGuid: {1}",
                        purchaseId,                                                       // {0}
                        !string.IsNullOrEmpty(billingGuid) ? billingGuid : string.Empty); // {1}
                }
            }
            return response;
        }


        public static AssetItemPriceResponse GetAssetPrices(int groupID,
            string siteGuid,
            string couponCode, string countryCd2, string languageCode3, string deviceName, string clientIP,
            List<ApiObjects.AssetFiles> assetFiles)
        {
            AssetItemPriceResponse response = null;
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            BaseConditionalAccess conditionalAccess = null;
            Utils.GetBaseConditionalAccessImpl(ref conditionalAccess, groupID);
            if (conditionalAccess != null)
            {
                response = conditionalAccess.GetAssetPrices(assetFiles, siteGuid, couponCode, countryCd2, languageCode3, deviceName, clientIP);
            }

            return response;
        }


        public static Status ReconcileEntitlements(int groupID, string userId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response = t.ReconcileEntitlements(userId);
            }

            return response;
        }


        public static ApiObjects.UserBundlesResponse GetUserBundles(int groupID, int domainID, int[] fileTypeIDs)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserBundles(domainID, fileTypeIDs);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.UserPurhcasedAssetsResponse GetUserPurchasedAssets(int groupID, int domainID, int[] fileTypeIDs)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetUserPurchasedAssets(domainID, fileTypeIDs);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.PurchaseSessionIdResponse GetPurchaseSessionID(int groupID,
            string userId, double price, string currency, int contentId, string productId, string coupon, string userIP, string udid, eTransactionType transactionType, int previewModuleID)
        {
            ApiObjects.PurchaseSessionIdResponse response = new ApiObjects.PurchaseSessionIdResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = userId != null ? userId : "null";

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                coupon = coupon != null ? coupon : string.Empty;

                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        int mediaId = Utils.GetMediaIDFromFileID(contentId, groupID);
                        response.PurchaseCustomDataId = t.GetPPVCustomDataID(userId, price, currency, contentId, mediaId, productId, coupon, string.Empty, string.Empty, userIP, string.Empty,
                            string.Empty, udid, string.Empty);
                        break;
                    case eTransactionType.Subscription:
                        response.PurchaseCustomDataId = t.GetBundleCustomDataID(userId, price, currency,
                        productId, string.Empty, coupon, string.Empty, userIP, string.Empty, string.Empty, udid, string.Empty, previewModuleID.ToString(), eBundleType.SUBSCRIPTION);
                        break;
                    case eTransactionType.Collection:
                        response.PurchaseCustomDataId = t.GetBundleCustomDataID(userId, price, currency,
                        productId, string.Empty, coupon, string.Empty, userIP, string.Empty, string.Empty, udid, string.Empty, previewModuleID.ToString(), eBundleType.COLLECTION);
                        break;
                    default:
                        break;
                }

                if (response.PurchaseCustomDataId > 0)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }


        public static Status RecordTransaction(int groupID, string userId, long householdId, int state, string paymentGatewayReferenceID,
            string paymentGatewayResponseCode, int customDataId, double price, string currency, int contentId, int productId, eTransactionType transactionType,
             string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.RecordTransaction(userId, householdId, state, paymentGatewayReferenceID, paymentGatewayResponseCode, customDataId, price, currency, contentId, productId,
                    transactionType, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }

            return response;
        }


        public static Status UpdateRecordedTransaction(int groupID, long householdId, string paymentGatewayReferenceID, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.UpdateRecordedTransaction(householdId, paymentGatewayReferenceID, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }

            return response;
        }


        public static ApiObjects.CDVRAdapterResponseList GetCDVRAdapters(int groupID)
        {
            ApiObjects.CDVRAdapterResponseList response = new ApiObjects.CDVRAdapterResponseList();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.GetCDVRAdapters();
            }

            return response;
        }


        public static Status DeleteCDVRAdapter(int groupID, int adapterId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.DeleteCDVRAdapter(adapterId);
            }

            return response;
        }


        public static ApiObjects.CDVRAdapterResponse InsertCDVRAdapter(int groupID, ApiObjects.CDVRAdapter adapter)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.InsertCDVRAdapter(adapter);
            }

            return response;
        }


        public static ApiObjects.CDVRAdapterResponse GenerateCDVRSharedSecret(int groupID, int adapterId)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.GenerateCDVRSharedSecret(adapterId);
            }

            return response;
        }


        public static ApiObjects.CDVRAdapterResponse SetCDVRAdapter(int groupID, ApiObjects.CDVRAdapter adapter)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.SetCDVRAdapter(adapter);
            }

            return response;
        }


        public static ApiObjects.CDVRAdapterResponse SendCDVRAdapterConfiguration(int groupID, int adapterID)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            if (t != null)
            {
                response = t.SendConfigurationToAdapter(adapterID);
            }

            return response;
        }


        public static ApiObjects.TimeShiftedTv.Recording Record(int groupID, string userID, long epgID, ApiObjects.RecordingType recordingType)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.Record(userID, epgID, recordingType);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording CancelRecord(int groupID, string userId, long domainId, long recordingId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelOrDeleteRecord(userId, domainId, recordingId, ApiObjects.TstvRecordingStatus.Canceled);
            }
            else
            {
                return new ApiObjects.TimeShiftedTv.Recording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording DeleteRecord(int groupID, string userId, long domainId, long recordingId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelOrDeleteRecord(userId, domainId, recordingId, ApiObjects.TstvRecordingStatus.Deleted);
            }
            else
            {
                return new ApiObjects.TimeShiftedTv.Recording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }


        public static ApiObjects.TimeShiftedTv.SeriesRecording CancelSeriesRecord(int groupID, string userId, long domainId, long recordingId, long epgId, long seasonNumber)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelOrDeleteSeriesRecord(userId, domainId, recordingId, epgId, seasonNumber, ApiObjects.TstvRecordingStatus.Canceled);
            }
            else
            {
                return new ApiObjects.TimeShiftedTv.SeriesRecording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }


        public static ApiObjects.TimeShiftedTv.SeriesRecording DeleteSeriesRecord(int groupID, string userId, long domainId, long recordingId, long epgId, long seasonNumber)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CancelOrDeleteSeriesRecord(userId, domainId, recordingId, epgId, seasonNumber, ApiObjects.TstvRecordingStatus.Deleted);
            }
            else
            {
                return new ApiObjects.TimeShiftedTv.SeriesRecording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording RecordRetry(int groupID, long recordingId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                var recording = Core.Recordings.RecordingsManager.Instance.RecordRetry(groupID, recordingId);

                return recording;
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.TimeShiftedTv.RecordingResponse QueryRecords(int groupID, string userID, long[] epgIDs)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                long domainID = 0;
                return t.QueryRecords(userID, epgIDs.ToList(), ref domainID, ApiObjects.RecordingType.Single, true);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording GetRecordingStatus(int groupID, long recordingId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            ApiObjects.TimeShiftedTv.Recording recording = t.GetRecordingStatus(groupID, recordingId);

            return recording;
        }


        public static ApiObjects.TimeShiftedTv.RecordingResponse SearchDomainRecordings(int groupID, string userID, long domainID, ApiObjects.TstvRecordingStatus[] recordingStatuses,
                                                                                      string filter, int pageIndex, int pageSize, ApiObjects.SearchObjects.OrderObj orderBy, bool shouldIgnorePaging)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.SerachDomainRecordings(userID, domainID, recordingStatuses.ToList(), filter, pageIndex, pageSize, orderBy, shouldIgnorePaging);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording GetRecordingByID(int groupID, long domainID, long domainRecordingID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetRecordingByID(domainID, domainRecordingID);
            }
            else
            {
                return null;
            }
        }


        public static ApiObjects.Response.Status IngestRecording(int groupID, long[] epgs, ApiObjects.eAction action)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            ApiObjects.Response.Status status = t.IngestRecording(epgs.ToList(), action);

            return status;
        }


        public static ApiObjects.Response.Status RecoverRecordingMessages(int groupID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);

            ApiObjects.Response.Status status = t.RecoverRecordingMessages(groupID);

            return status;
        }


        public static ApiObjects.TimeShiftedTv.DomainQuotaResponse GetDomainQuota(int groupID, string userID, long domainID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            ApiObjects.TimeShiftedTv.DomainQuotaResponse response = t.GetDomainQuota(userID, domainID);

            return response;
        }


        public static Status RemovePaymentMethodHouseholdPaymentGateway(int groupID, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status response = new Status();

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.RemovePaymentMethodHouseholdPaymentGateway(groupID, paymentGatewayID, siteGuid, householdId, paymentMethodId, force);
            }
            else
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording ProtectRecord(int groupID, string userID, long recordID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.ProtectRecord(userID, recordID);
            }
            else
            {
                return null;
            }
        }


        public static bool CleanupRecordings()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return t.CleanupRecordings();
        }


        public static bool HandleRecordingsLifetime()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return t.HandleRecordingsLifetime();
        }


        public static bool HandleRecordingsScheduledTasks()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return t.HandleRecordingsScheduledTasks();
        }


        public static bool HandleDomainQuotaByRecording(ApiObjects.TimeShiftedTv.HandleDomainQuataByRecordingTask expiredRecording)
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(expiredRecording.GroupId);
            return t.HandleDomainQuotaByRecording(expiredRecording);
        }


        public static bool HandleFirstFollowerRecording(int groupID, string userId, long domainId, string channelId, string seriesId, int seasonNumber)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.HandleFirstFollowerRecording(userId, domainId, channelId, seriesId, seasonNumber);
            }
            else
            {
                return false;
            }
        }


        public static ApiObjects.TimeShiftedTv.SeriesResponse GetFollowSeries(int groupID, string userId, long domainId, ApiObjects.TimeShiftedTv.SeriesRecordingOrderObj orderBy)
        {
            BaseConditionalAccess t = null;
            ApiObjects.TimeShiftedTv.SeriesResponse response = new ApiObjects.TimeShiftedTv.SeriesResponse();
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetFollowSeries(userId, domainId, orderBy);
            }
            else
            {
                response = new ApiObjects.TimeShiftedTv.SeriesResponse()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
                return response;
            }
        }


        public static ApiObjects.TimeShiftedTv.SeriesRecording RecordSeasonOrSeries(int groupID, string userID, long epgID, ApiObjects.RecordingType recordingType)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.RecordSeasonOrSeries(userID, epgID, recordingType);
            }
            else
            {
                return null;
            }
        }


        public static bool DistributeRecording(int groupID, long epgId, long Id, DateTime epgStartDate)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.DistributeRecording(epgId, Id, epgStartDate);
            }
            else
            {
                return false;
            }
        }


        public static bool CompleteDomainSeriesRecordings(int groupID, long domainId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CompleteDomainSeriesRecordings(domainId);
            }
            else
            {
                return false;
            }
        }


        public static LicensedLinkResponse GetRecordingLicensedLink(int groupID, string userId, int recordingId, string udid, string userIp, string fileType)
        {
            BaseConditionalAccess t = null;
            LicensedLinkResponse response = new LicensedLinkResponse()
            {
                Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetRecordingLicensedLink(userId, recordingId, udid, userIp, fileType);
            }
            else
            {
                return response;
            }
        }


        public static bool CheckRecordingDuplicateCrids(int groupID, long recordingId)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.CheckRecordingDuplicateCrids(groupID, recordingId);
            }
            else
            {
                return false;
            }
        }


        public static Status HandleUserTask(int groupID, int domainId, string userId, ApiObjects.UserTaskType actionType)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                result = t.HandleUserTask(domainId, userId, actionType);
            }
            return result;
        }


        public static bool DistributeRecordingWithDomainIds(int groupID, long epgId, long Id, DateTime epgStartDate, long[] domainSeriesIds)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.DistributeRecording(epgId, Id, epgStartDate, domainSeriesIds.ToList());
            }
            else
            {
                return false;
            }
        }


        public static ApiObjects.KeyValuePair GetSeriesIdAndSeasonNumberByEpgId(int groupID, long epgId)
        {
            BaseConditionalAccess t = null;
            ApiObjects.KeyValuePair result = new ApiObjects.KeyValuePair();
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                result = t.GetSeriesIdAndSeasonNumberByEpgId(epgId);
            }

            return result;
        }


        public static ApiObjects.TimeShiftedTv.SearchableRecording[] GetDomainSearchableRecordings(int groupID, long domainId)
        {
            BaseConditionalAccess t = null;
            ApiObjects.TimeShiftedTv.SearchableRecording[] result = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                result = t.GetDomainSearchableRecordings(groupID, domainId);
            }

            return result;
        }


        public static string BulkRecoveryForRenewSubscriptions(int groupID, DateTime endDateStartRange, DateTime endDateEndRange)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.BulkRecoveryForRenewSubscriptions(endDateStartRange, endDateEndRange);
            }
            else
            {
                return "Failed";
            }
        }


        public static ApiObjects.TimeShiftedTv.Recording GetRecordingByDomainRecordingId(int groupID, long domainID, long domainRecordingID)
        {
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                return t.GetRecordingByID(domainID, domainRecordingID, false);
            }
            else
            {
                return null;
            }
        }

        public static Status RemoveHouseholdEntitlements(int groupID, int householdId)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                t.RemoveHouseholdEntitlements(householdId);
            }

            return result;
        }

        public static Entitlements UpdateEntitlement(int groupID, int domainID, Entitlement entitlement)
        {
            Entitlements response = new Entitlements();
            BaseConditionalAccess t = null;
            Utils.GetBaseConditionalAccessImpl(ref t, groupID);
            if (t != null)
            {
                response = t.UpdateEntitlement(domainID, entitlement);
            }
            else
            {
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

    }
}
