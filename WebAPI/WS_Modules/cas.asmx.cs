using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Net;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;
using ApiObjects.Response;
using KLogMonitor;
using ApiObjects.Billing;
using System.Reflection;
using ApiObjects;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using ApiObjects.ConditionalAccess;
using ApiObjects.TimeShiftedTv;

namespace WS_ConditionalAccess
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://ca.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class module : System.Web.Services.WebService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public PermittedMediaContainer[] GetUserPermittedItems(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                PermittedMediaContainer[] res = Core.ConditionalAccess.Module.GetUserPermittedItems(nGroupID, sSiteGUID);

                return res != null ? res : new PermittedMediaContainer[0];
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public PermittedMediaContainerResponse GetDomainPermittedItems(string sWSUserName, string sWSPassword, int nDomainID)
        {
            PermittedMediaContainerResponse response = new PermittedMediaContainerResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainPermittedItems(nGroupID, nDomainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public PermittedMediaContainer[] GetUserExpiredItems(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserExpiredItems(nGroupID, sSiteGUID, numOfItems);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public UserCAStatus GetUserCAStatus(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserCAStatus(nGroupID, sSiteGUID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return UserCAStatus.Annonymus;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public string GetLicensedLink(string sWSUserName, string sWSPassword, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetLicensedLink(nGroupID, sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public LicensedLinkResponse GetEPGLicensedLink(string sWSUserName, string sWSPassword, string sSiteGUID, int nMediaFileID, int nEPGItemID, DateTime startTime, string sBasicLink, string sUserIP, string sRefferer,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetEPGLicensedLink(nGroupID, sSiteGUID, nMediaFileID, nEPGItemID, startTime, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, nFormatType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new LicensedLinkResponse() { Status = new Status() { Code = (int)eResponseStatus.WrongPasswordOrUserName, Message = string.Empty } };
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedMediaContainer))]
        public string GetLicensedLinkWithCoupon(string sWSUserName, string sWSPassword, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetLicensedLinkWithCoupon(nGroupID, sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName, couponCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool ActivateCampaign(string sWSUserName, string sWSPassword, int campaignID, CampaignActionInfo actionInfo)
        {

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.ActivateCampaign(nGroupID, campaignID, actionInfo);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(CampaignActionInfo))]
        public virtual CampaignActionInfo ActivateCampaignWithInfo(string sWSUserName, string sWSPassword, int campaignID, CampaignActionInfo actionInfo)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.ActivateCampaignWithInfo(nGroupID, campaignID, actionInfo);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedSubscriptionContainer))]
        public PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(string sWSUserName, string sWSPassword, int nDomainID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainPermittedSubscriptions(nGroupID, nDomainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedSubscriptionContainer))]
        public PermittedSubscriptionContainer[] GetUserPermittedSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserPermittedSubscriptions(nGroupID, sSiteGUID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedCollectionContainer))]
        public PermittedCollectionContainer[] GetDomainPermittedCollections(string sWSUserName, string sWSPassword, int nDomainID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainPermittedCollections(nGroupID, nDomainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedCollectionContainer))]
        public PermittedCollectionContainer[] GetUserPermittedCollections(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserPermittedCollections(nGroupID, sSiteGUID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedSubscriptionContainer))]
        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserExpiredSubscriptions(nGroupID, sSiteGUID, numOfItems);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedCollectionContainer))]
        public PermittedCollectionContainer[] GetUserExpiredCollections(string sWSUserName, string sWSPassword, string sSiteGUID, int numOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserExpiredCollections(nGroupID, sSiteGUID, numOfItems);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedSubscriptionContainer))]
        public bool IsPermittedItem(string sWSUserName, string sWSPassword, string sSiteGUID, int mediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.IsPermittedItem(nGroupID, sSiteGUID, mediaID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PermittedSubscriptionContainer))]
        public bool IsPermittedSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, int subID, ref string reason)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.IsPermittedSubscription(nGroupID, sSiteGUID, subID, ref reason);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingTransactionContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBillingTransactionsResponse))]
        public UserBillingTransactionsResponse[] GetUsersBillingHistory(string sWSUserName, string sWSPassword, string[] arrSiteGUIDs, DateTime dStartDate, DateTime dEndDate)
        {
            // add siteguid to logs/monitor
            if (arrSiteGUIDs != null && arrSiteGUIDs.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var siteGuid in arrSiteGUIDs)
                    sb.Append(String.Format("{0} ", siteGuid));

                HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sb.ToString();
            }

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUsersBillingHistory(nGroupID, arrSiteGUIDs, dStartDate, dEndDate);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TransactionHistoryContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainTransactionsHistoryResponse))]
        public DomainTransactionsHistoryResponse GetDomainTransactionsHistory(string sWSUserName, string sWSPassword, int domainID, DateTime dStartDate, DateTime dEndDate, int pageSize, int pageIndex, TransactionHistoryOrderBy orderBy)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainTransactionsHistory(nGroupID, domainID, dStartDate, dEndDate, pageSize, pageIndex, orderBy);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingTransactionContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBillingTransactionsResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainBillingTransactionsResponse))]
        public DomainsBillingTransactionsResponse GetDomainsBillingHistory(string sWSUserName, string sWSPassword,
            int[] domainIDs, DateTime dStartDate, DateTime dEndDate)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainsBillingHistory(nGroupID, domainIDs, dStartDate, dEndDate);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingTransactionContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingTransactionsResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingTransactions))]
        public BillingTransactions GetUserBillingHistory(string sWSUserName, string sWSPassword, string sSiteGUID, Int32 nStartIndex, Int32 nNumberOfItems, TransactionHistoryOrderBy orderBy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserBillingHistory(nGroupID, sSiteGUID, nStartIndex, nNumberOfItems, orderBy);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public bool RenewCancledSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.RenewCancledSubscription(nGroupID, sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public bool CancelSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode, Int32 nSubscriptionPurchaseID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelSubscription(nGroupID, sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
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
        public ApiObjects.Response.Status CancelSubscriptionRenewal(string sWSUserName, string sWSPassword, int nDomainId, string sSubscriptionCode)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelSubscriptionRenewal(nGroupID, nDomainId, sSubscriptionCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                return (new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Invalid request" });
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(TimeSpan))]
        public bool ChangeSubscriptionDates(string sWSUserName, string sWSPassword, string sSiteGUID, string sSubscriptionCode,
            Int32 nSubscriptionPurchaseID, Int32 dAdditionInDays, bool bNewRenewable)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.ChangeSubscriptionDates(nGroupID, sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID, dAdditionInDays, bNewRenewable);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(WSInt32))]
        public MediaFileItemPricesContainer[] GetItemsPricesEx(string sWSUserName, string sWSPassword, WSInt32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPricesEx(nGroupID, nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        //[WebMethod]
        //[System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        //[System.Xml.Serialization.XmlInclude(typeof(Price))]
        //[System.Xml.Serialization.XmlInclude(typeof(Currency))]
        //[System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        //[System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        //[System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        //[System.Xml.Serialization.XmlInclude(typeof(WSInt32))]
        //public MediaFileItemPricesContainer[] GetItemsPricesWithCouponsEx(string sWSUserName, string sWSPassword, WSInt32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest ,
        //    string sCountryCd2, string sLanguageCode3, string sDeviceName)
        //{
        //    
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        Int32 nSize = nMediaFiles.Length;
        //        Int32[] nMediaFileIDs = new Int32[nSize];
        //        for (int j = 0; j < nSize; j++)
        //            nMediaFileIDs[j] = nMediaFiles[j].m_nInt32;
        //        return Core.ConditionalAccess.Module.GetItemsPrices(nGroupID, nMediaFileIDs, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //            HttpContext.Current.Response.StatusCode = 404;
        //        return null;
        //    }
        //}
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(WSInt32))]
        public MediaFileItemPricesContainer[] GetItemsPricesWithCouponsEx(string sWSUserName, string sWSPassword, WSInt32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPricesWithCouponsEx(nGroupID, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(Int32[]))]
        public MediaFileItemPricesContainer[] GetItemsPrices(string sWSUserName, string sWSPassword, Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPrices(nGroupID, nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(Int32[]))]
        public MediaFileItemPricesContainer[] GetItemsPricesByIP(string sWSUserName, string sWSPassword, Int32[] nMediaFiles, string sUserGUID, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPrices(nGroupID, nMediaFiles, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainerResponse))]
        public MediaFileItemPricesContainerResponse GetItemsPricesWithCoupons(string sWSUserName, string sWSPassword, Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            MediaFileItemPricesContainerResponse response = new MediaFileItemPricesContainerResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPricesWithCoupons(nGroupID, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        public MediaFileItemPricesContainer[] GetItemsPricesWithCouponsByIP(string sWSUserName, string sWSPassword, Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPricesWithCouponsByIP(nGroupID, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPrices(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPrices(nGroupID, sSubscriptions, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(CollectionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public CollectionsPricesContainer[] GetCollectionsPrices(string sWSUserName, string sWSPassword, string[] sCollections, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetCollectionsPrices(nGroupID, sCollections, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesByIP(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesByIP(nGroupID, sSubscriptions, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public PrePaidPricesContainer[] GetPrePaidPrices(string sWSUserName, string sWSPassword, string[] sPrePaids, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetPrePaidPrices(nGroupID, sPrePaids, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesResponse))]
        public SubscriptionsPricesResponse GetSubscriptionsPricesWithCoupon(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            SubscriptionsPricesResponse response = new SubscriptionsPricesResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesWithCoupon(nGroupID, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(CollectionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(string sWSUserName, string sWSPassword, string[] sCollections, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetCollectionsPricesWithCoupon(nGroupID, sCollections, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCouponByIP(string sWSUserName, string sWSPassword, string[] sSubscriptions, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesWithCouponByIP(nGroupID, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesST(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesST(nGroupID, sSubscriptionsList, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(CollectionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public CollectionsPricesContainer[] GetCollectionsPricesST(string sWSUserName, string sWSPassword, string sCollectionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetCollectionsPricesST(nGroupID, sCollectionsList, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesSTByIP(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesSTByIP(nGroupID, sSubscriptionsList, sUserGUID, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public PrePaidPricesContainer[] GetPrePaidPricesST(string sWSUserName, string sWSPassword, string sPrePaidList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetPrePaidPricesST(nGroupID, sPrePaidList, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCoupon(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            string[] sSep = { ";" };
            string[] sSubscriptions = sSubscriptionsList.Split(sSep, StringSplitOptions.RemoveEmptyEntries);

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPrices(nGroupID, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(CollectionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public CollectionsPricesContainer[] GetCollectionsPricesSTWithCoupon(string sWSUserName, string sWSPassword, string sCollectionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetCollectionsPricesSTWithCoupon(nGroupID, sCollectionsList, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(SubscriptionsPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesSTWithCouponByIP(string sWSUserName, string sWSPassword, string sSubscriptionsList, string sUserGUID, string sCouponCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sClientIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetSubscriptionsPricesSTWithCouponByIP(nGroupID, sSubscriptionsList, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        public MediaFileItemPricesContainer[] GetItemsPricesST(string sWSUserName, string sWSPassword, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPrices(nGroupID, nMediaFileIDs, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        public MediaFileItemPricesContainer[] GetItemsPricesSTByIP(string sWSUserName, string sWSPassword, string sMediaFilesCommaSeperated, string sUserGUID, bool bOnlyLowest,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemsPrices(nGroupID, nMediaFileIDs, sUserGUID, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, sClientIP);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse Cellular_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
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
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.Cellular_ChargeUserForSubscription(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_DummyChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_DummyChargeUserForSubscription(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_DummyChargeUserForCollection(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_DummyChargeUserForCollection(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sCollectionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_ChargeUserForPrePaid(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_ChargeUserForPrePaid(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sPrePaidCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_DummyChargeUserForPrePaid(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sPrePaidCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_DummyChargeUserForPrePaid(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sPrePaidCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_DummyChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_DummyChargeUserForMediaFile(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingStatusResponse CC_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BillingStatusResponse response = new BillingStatusResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_ChargeUserForMediaFile(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        //[WebMethod]
        //[System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        //[System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        //public BillingResponse CC_ChargeUserForMediaFile_2(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
        //    string sCountryCd2, string sLanguageCode3, string sDeviceName, int nBillingProvider)
        //{                       
        //    if (string.IsNullOrEmpty(sCountryCd2))
        //        sCountryCd2 = "";
        //    if (string.IsNullOrEmpty(sLanguageCode3))
        //        sLanguageCode3 = "";
        //    if (string.IsNullOrEmpty(sDeviceName))
        //        sDeviceName = "";

        //    

        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        Int32 nMediaID = Utils.GetMediaIDFeomFileID(nMediaFileID, nGroupID);
        //        return new BillingResponse(t.ChargeUserForMediaFile_2(sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, false, nBillingProvider));
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //            HttpContext.Current.Response.StatusCode = 404;
        //        return null;
        //    }
        //}

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingStatusResponse CC_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BillingStatusResponse response = new BillingStatusResponse();

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_ChargeUserForSubscription(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse CC_ChargeUserForCollection(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sCollectionCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string sPaymentMethodID, string sEncryptedCVV)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CC_ChargeUserForCollection(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sCollectionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, sPaymentMethodID, sEncryptedCVV);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse PU_GetPPVPopupPaymentMethodURL(string sWSUserName, string sWSPassword,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode,
            string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.PU_GetPPVPopupPaymentMethodURL(nGroupID, sSiteGUID, dPrice, sCurrencyCode3,
                    nMediaFileID, sPPVModuleCode, sCouponCode, sPaymentMethod, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public int AD_GetCustomDataID(string sWSUserName, string sWSPassword,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 assetID, string sPPVModuleCode, string sCampaignCode,
            string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.AD_GetCustomDataID(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, assetID, sPPVModuleCode, sCampaignCode, sCouponCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, assetType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return 0;
            }
        }

        [WebMethod]
        public int GetCustomDataID(string sWSUserName, string sWSPassword,
            string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 assetID, string sPPVModuleCode, string sCampaignCode,
            string sCouponCode, string sPaymentMethod, string sUserIP,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, int assetType, string sOverrideEndDate, string sPreviewModuleID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetCustomDataID(nGroupID, sSiteGUID, dPrice, sCurrencyCode3,
                        assetID, sPPVModuleCode, sCouponCode, sCampaignCode, sPaymentMethod, sUserIP, sCountryCd2, sLanguageCode3, sDeviceName, assetType, sOverrideEndDate, sOverrideEndDate);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return 0;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse PU_GetSubscriptionPopupPaymentMethodURL(string sWSUserName, string sWSPassword,
            string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode,
            string sCouponCode, string sPaymentMethod, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.PU_GetSubscriptionPopupPaymentMethodURL(nGroupID, sSiteGUID, dPrice, sCurrencyCode3,
                    sSubscriptionCode, sCouponCode, sPaymentMethod, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.SMS_ChargeUserForMediaFile(nGroupID, sSiteGUID, sCellPhone, dPrice, sCurrencyCode3, nMediaFileID, sPPVModuleCode, sCouponCode, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.SMS_ChargeUserForSubscription(nGroupID, sSiteGUID, sCellPhone, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_CheckCodeForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, Int32 nMediaFileID,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.SMS_CheckCodeForMediaFile(nGroupID, sSiteGUID, sCellPhone, sSMSCode, nMediaFileID, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.PPVModule))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse SMS_CheckCodeForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, string sCellPhone, string sSMSCode, string sSubscriptionCode,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.SMS_CheckCodeForSubscription(nGroupID, sSiteGUID, sCellPhone, sSMSCode, sSubscriptionCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidResponseStatus))]
        public PrePaidResponse PP_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.PP_ChargeUserForMediaFile(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidResponseStatus))]
        public PrePaidResponse PP_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sSubscriptionCode, string sCouponCode, string sUserIP, string sExtraParameters,
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
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.PP_ChargeUserForSubscription(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sSubscriptionCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserPrePaidContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(UserPrePaidObject))]
        public UserPrePaidContainer GetUserPrePaidStatus(string sWSUserName, string sWSPassword, string sSiteGUID, string sCurrencyCode3)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserPrePaidStatus(nGroupID, sSiteGUID, sCurrencyCode3);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidHistoryContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(PrePaidHistoryResponse))]
        public PrePaidHistoryResponse GetUserPrePaidHistory(string sWSUserName, string sWSPassword, string sSiteGUID, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserPrePaidHistory(nGroupID, sSiteGUID, nNumberOfItems);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public string GetItemLeftViewLifeCycle(string sWSUserName, string sWSPassword, string sMediaFileID, string sSiteGUID, bool bIsCoGuid,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetItemLeftViewLifeCycle(nGroupID, sMediaFileID, sSiteGUID, bIsCoGuid, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return TimeSpan.Zero.ToString();
            }
        }

        [WebMethod]
        public EntitlementResponse GetEntitlement(string sWSUserName, string sWSPassword, string sMediaFileID, string sSiteGUID, bool bIsCoGuid,
            string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, bool isRecording)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            EntitlementResponse objResponse = null;

            BaseConditionalAccess objConditionalAccess = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GetItemLeftViewLifeCycle", ref objConditionalAccess);
            if (nGroupID != 0 && objConditionalAccess != null)
            {
                return Core.ConditionalAccess.Module.GetEntitlement(nGroupID, sMediaFileID, sSiteGUID, bIsCoGuid, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, isRecording);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                objResponse = new EntitlementResponse()
                {
                    FullLifeCycle = TimeSpan.Zero.ToString(),
                    ViewLifeCycle = TimeSpan.Zero.ToString()
                };
            }

            return objResponse;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse InApp_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, string sProductCode, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                int nMediaFileID = 0;
                Int32 nMediaID = Utils.GetMediaIDFromFileID(sProductCode, nGroupID, ref nMediaFileID);
                return Core.ConditionalAccess.Module.InApp_ChargeUserForMediaFile(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, sProductCode, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, ReceiptData);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse InApp_ChargeUserForSubscription(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode, string sProductCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName, string ReceiptData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {

                return Core.ConditionalAccess.Module.InApp_ChargeUserForSubscription(nGroupID, sSiteGUID, dPrice, sCurrencyCode, sProductCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName, ReceiptData);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        public string GetGoogleSignature(string sWSUserName, string sWSPassword, int nCustomDataID)
        {

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Utils.GetGoogleSignature(nGroupID, nCustomDataID);

            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }



        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BillingResponseStatus))]
        public BillingResponse Cellular_ChargeUserForMediaFile(string sWSUserName, string sWSPassword, string sSiteGUID, double dPrice, string sCurrencyCode3, Int32 nMediaFileID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Int32 nMediaID = Utils.GetMediaIDFromFileID(nMediaFileID, nGroupID);
                return Core.ConditionalAccess.Module.Cellular_ChargeUserForMediaFile(nGroupID, sSiteGUID, dPrice, sCurrencyCode3, nMediaFileID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        public ChangeSubscriptionStatus ChangeSubscription(string sWSUserName, string sWSPassword, string sSiteGuid, int nOldSubscription, int nNewSubscription)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.ChangeSubscription(nGroupID, sSiteGuid, nOldSubscription, nNewSubscription);
            }
            return ChangeSubscriptionStatus.Error;
        }

        [WebMethod]
        /**************************************************************************************************************************
         *  CancelServiceNow 
         *  for PPV - assetID = mediaFileID
         *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
         *  if the cancelation window is available - do the cancel or if the force flag is on
         *  Return status object
         ***************************************************************************************************************************/
        public ApiObjects.Response.Status CancelServiceNow(string sWSUserName, string sWSPassword,
            int nDomainId, int nAssetID, eTransactionType transactionType, bool bIsForce = false)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelServiceNow(nGroupID, nDomainId, nAssetID, transactionType, bIsForce);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                return (new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Invalid request" });
            }
        }

        // OBSOLETE
        [WebMethod]
        /**************************************************************************************************************************
         *  CancelTransaction 
         *  for PPV - assetID = mediaFileID
         *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
         *  if the cancelation window is available - do the cancel and return Core.ConditionalAccess.Module.ue if success, false otherwise
         ***************************************************************************************************************************/
        public bool CancelTransaction(string sWSUserName, string sWSPassword, string sSiteGuid, int nAssetID, eTransactionType transactionType, bool bIsForce = false)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelTransaction(nGroupID, sSiteGuid, nAssetID, transactionType, bIsForce);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return (false);
            }
        }
        [WebMethod]
        /**************************************************************************************************************************
          *  WaiverTransaction 
          *  for PPV - assetID = mediaFileID
          *  for Collection/Subscription assetID = CollectionCode/SubscriptionCode
          *  if the cancelation window is available - do the cancel return Core.ConditionalAccess.Module.ue , else return false
          ***************************************************************************************************************************/
        public ApiObjects.Response.Status WaiverTransaction(string sWSUserName, string sWSPassword, string sSiteGuid, int nAssetID, eTransactionType transactionType)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.WaiverTransaction(nGroupID, sSiteGuid, nAssetID, transactionType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error };
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(LicensedLinkResponse))]
        public LicensedLinkResponse GetLicensedLinks(string sWSUserName, string sWSPassword, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer,
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

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetLicensedLinks(nGroupID, sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new LicensedLinkResponse() { Status = new Status() { Code = (int)eResponseStatus.Error } };
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(NPVRResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(RetrieveQuotaNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(CancelDeleteResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(DeleteNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(CancelNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(RecordNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(QuotaResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(CancelSeriesNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(RecordSeriesByProgramIdNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(RecordSeriesByNameNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(DeleteSeriesNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(RecordResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(ProtectNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(LicensedLinkNPVRCommand))]
        [System.Xml.Serialization.XmlInclude(typeof(LicensedLinkNPVRResponse))]
        public NPVRResponse GetNPVRResponse(BaseNPVRCommand command)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = command != null && command.siteGuid != null ? command.siteGuid : "null";

            if (command != null)
            {
                // get action ID
                HttpContext.Current.Items[KLogMonitor.Constants.ACTION] = command.GetType();

                return command.Execute();
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DomainServicesResponse))]
        public DomainServicesResponse GetDomainServices(string sWSUserName, string sWSPassword, int domainID)
        {
            if (domainID == 0)
                return null;
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetDomainServices(nGroupID, domainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new DomainServicesResponse((int)eResponseStatus.WrongPasswordOrUserName, null);
            }
        }

        [WebMethod]
        public Entitlements GetUserSubscriptions(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            Entitlements response = new Entitlements();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserSubscriptions(nGroupID, sSiteGUID); // GetUserPermittedSubscriptions
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
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
        [WebMethod]
        public TransactionResponse Purchase(string sWSUserName, string sWSPassword, string siteguid, long householdId, double price, string currency, Int32 contentId, 
            int productId, eTransactionType transactionType, string coupon, string userIp, string deviceName, int paymentGatewayId, int paymentMethodId, string adapterData)
        {
            TransactionResponse response = new TransactionResponse();

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "Purchase", ref casImpl);

            if (nGroupID != 0 && casImpl != null)
            {
                response = casImpl.Purchase(siteguid, householdId, price, currency, contentId, productId, transactionType, coupon, userIp, deviceName, paymentGatewayId, paymentMethodId, adapterData);
                if (response == null)
                {
                    response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        public TransactionResponse ProcessReceipt(string sWSUserName, string sWSPassword, string siteguid, long household, Int32 contentId,
                                         int productId, eTransactionType transactionType, string userIp, string deviceName, string purchaseToken, string paymentGatewayName)
        {
            TransactionResponse response = new TransactionResponse();

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "Purchase", ref casImpl);

            if (nGroupID != 0 && casImpl != null)
            {
                response = casImpl.ProcessReceipt(siteguid, household, contentId, productId, transactionType, userIp, deviceName, purchaseToken, paymentGatewayName);
                if (response == null)
                    response = new TransactionResponse((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                response.Status = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status GrantEntitlements(string sWSUserName, string sWSPassword, string siteguid, long housholdId, Int32 contentId,
                                        int productId, eTransactionType transactionType, string userIp, string deviceName, bool history)
        {
            ApiObjects.Response.Status status = null;


            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "GrantEntitlements", ref casImpl);

            if (nGroupID != 0 && casImpl != null)
            {
                status = casImpl.GrantEntitlements(siteguid, housholdId, contentId, productId, transactionType, userIp, deviceName, history);
                if (status == null)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                status = new Status((int)eResponseStatus.Error, "Error");
            }
            return status;
        }

        [WebMethod]
        public ApiObjects.Response.Status UpdatePendingTransaction(string sWSUserName, string sWSPassword, string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus,
            string externalMessage, int failReason, string signature)
        {
            ApiObjects.Response.Status response;

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "UpdatePendingTransaction", ref casImpl);

            if (nGroupID != 0 && casImpl != null)
            {
                response = casImpl.UpdatePendingTransaction(paymentGatewayId, adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
                if (response == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response = new Status((int)eResponseStatus.Error, "Error");
            }
            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status CheckPendingTransaction(string wsUserName, string wsPassword,
        long paymentGatewayPendingId, int numberOfRetries, string billingGuid, long paymentGatewayTransactionId,
        string siteGuid, int productId, int productType)
        {
            ApiObjects.Response.Status response = null;

            // get partner implementation and group ID
            BaseConditionalAccess conditionalAccess = null;
            int nGroupID = Utils.GetGroupID(wsUserName, wsPassword, "CheckPendingTransaction", ref conditionalAccess);

            if (nGroupID != 0 && conditionalAccess != null)
            {
                response = conditionalAccess.CheckPendingTransaction(paymentGatewayPendingId, numberOfRetries, billingGuid, paymentGatewayTransactionId,
                    siteGuid, productId, productType);

                if (response == null)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            else if (nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new Status((int)eResponseStatus.Error, "Error");
            }

            return response;
        }

        [WebMethod]
        public Entitlements GetUserEntitlements(string sWSUserName, string sWSPassword, string sSiteGUID, eTransactionType type, bool isExpired, int pageSize, int pageIndex, ApiObjects.EntitlementOrderBy orderBy)
        {
            Entitlements response = new Entitlements();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.GetUserEntitlements(nGroupID, sSiteGUID, type, isExpired, pageSize, pageIndex, orderBy);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Entitlements))]
        public Entitlements GetDomainEntitlements(string sWSUserName, string sWSPassword, int domainId, eTransactionType type, bool isExpired, int pageSize, int pageIndex, ApiObjects.EntitlementOrderBy orderBy)
        {
            Entitlements response = new Entitlements();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.GetDomainEntitlements(nGroupID, domainId, type, isExpired, pageSize, pageIndex, orderBy);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        public bool Renew(string sWSUserName, string sWSPassword, string siteguid, long purchaseId, string billingGuid, long endDate)
        {
            bool response = false;

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteguid != null ? siteguid : "null";

            // get partner implementation and group ID
            BaseConditionalAccess casImpl = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "Renew", ref casImpl);

            if (nGroupID != 0 && casImpl != null)
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
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseConditionalAccess))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Price))]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Pricing.Currency))]
        [System.Xml.Serialization.XmlInclude(typeof(PriceReason))]
        [System.Xml.Serialization.XmlInclude(typeof(ItemPriceContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(MediaFileItemPricesContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(Int32[]))]
        public AssetItemPriceResponse GetAssetPrices(string username, string password,
            string siteGuid,
            string couponCode, string countryCd2, string languageCode3, string deviceName, string clientIP,
            List<ApiObjects.AssetFiles> assetFiles)
        {
            AssetItemPriceResponse response = null;
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            BaseConditionalAccess conditionalAccess = null;

            Int32 nGroupID = Utils.GetGroupID(username, password, "GetItemPricesWithAssets", ref conditionalAccess);
            if (nGroupID != 0 && conditionalAccess != null)
            {
                response = conditionalAccess.GetAssetPrices(assetFiles, siteGuid, couponCode, countryCd2, languageCode3, deviceName, clientIP);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }

            return response;
        }

        [WebMethod]
        public Status ReconcileEntitlements(string sWSUserName, string sWSPassword, string userId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.ReconcileEntitlements(nGroupID, userId);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.UserBundlesResponse GetUserBundles(string sWSUserName, string sWSPassword, int domainID, int[] fileTypeIDs)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserBundles(nGroupID, domainID, fileTypeIDs);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public ApiObjects.UserPurhcasedAssetsResponse GetUserPurchasedAssets(string sWSUserName, string sWSPassword, int domainID, int[] fileTypeIDs)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetUserPurchasedAssets(nGroupID, domainID, fileTypeIDs);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public ApiObjects.PurchaseSessionIdResponse GetPurchaseSessionID(string sWSUserName, string sWSPassword,
            string userId, double price, string currency, int contentId, string productId, string coupon, string userIP, string udid, eTransactionType transactionType, int previewModuleID)
        {
            ApiObjects.PurchaseSessionIdResponse response = new ApiObjects.PurchaseSessionIdResponse()
            {
                Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = userId != null ? userId : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetPurchaseSessionID(nGroupID, userId, price, currency, contentId, productId, coupon, userIP, udid, transactionType, previewModuleID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public Status RecordTransaction(string sWSUserName, string sWSPassword, string userId, long householdId, int state, string paymentGatewayReferenceID,
            string paymentGatewayResponseCode, int customDataId, double price, string currency, int contentId, int productId, eTransactionType transactionType,
             string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());


            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.RecordTransaction(nGroupID, userId, householdId, state, paymentGatewayReferenceID, paymentGatewayResponseCode, customDataId, price, currency, contentId, productId,
                    transactionType, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public Status UpdateRecordedTransaction(string sWSUserName, string sWSPassword,long householdId, string paymentGatewayReferenceID, string paymentDetails, string paymentMethod, int paymentGatewayId, string paymentMethodExternalID)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.UpdateRecordedTransaction(nGroupID, householdId,paymentGatewayReferenceID, paymentDetails, paymentMethod, paymentGatewayId, paymentMethodExternalID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.CDVRAdapterResponseList GetCDVRAdapters(string sWSUserName, string sWSPassword)
        {
            ApiObjects.CDVRAdapterResponseList response = new ApiObjects.CDVRAdapterResponseList();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.GetCDVRAdapters(nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public Status DeleteCDVRAdapter(string sWSUserName, string sWSPassword, int adapterId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.DeleteCDVRAdapter(nGroupID, adapterId);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.CDVRAdapterResponse InsertCDVRAdapter(string sWSUserName, string sWSPassword, ApiObjects.CDVRAdapter adapter)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.InsertCDVRAdapter(nGroupID, adapter);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.CDVRAdapterResponse GenerateCDVRSharedSecret(string sWSUserName, string sWSPassword, int adapterId)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.GenerateCDVRSharedSecret(nGroupID, adapterId);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.CDVRAdapterResponse SetCDVRAdapter(string sWSUserName, string sWSPassword, ApiObjects.CDVRAdapter adapter)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.SetCDVRAdapter(nGroupID, adapter);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public ApiObjects.CDVRAdapterResponse SendCDVRAdapterConfiguration(string sWSUserName, string sWSPassword, int adapterID)
        {
            ApiObjects.CDVRAdapterResponse response = new ApiObjects.CDVRAdapterResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.SendCDVRAdapterConfiguration(nGroupID, adapterID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public Recording Record(string sWSUserName, string sWSPassword, string userID, long epgID, ApiObjects.RecordingType recordingType)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.Record(nGroupID, userID, epgID, recordingType);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public Recording CancelRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelRecord(nGroupID, userId, domainId, recordingId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new Recording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }

        [WebMethod]
        public Recording DeleteRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.DeleteRecord(nGroupID, userId, domainId, recordingId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new Recording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }

        [WebMethod]
        public SeriesRecording CancelSeriesRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId, long epgId, long seasonNumber)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CancelSeriesRecord(nGroupID, userId, domainId, recordingId, epgId, seasonNumber);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new SeriesRecording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }

        [WebMethod]
        public SeriesRecording DeleteSeriesRecord(string sWSUserName, string sWSPassword, string userId, long domainId, long recordingId, long epgId, long seasonNumber)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.DeleteSeriesRecord(nGroupID, userId, domainId, recordingId, epgId, seasonNumber);
            }
            else
            {  
                HttpContext.Current.Response.StatusCode = 404;
                return new SeriesRecording()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
            }
        }

        [WebMethod]
        public Recording RecordRetry(string sWSUserName, string sWSPassword, long recordingId)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                var recording = Core.ConditionalAccess.Module.RecordRetry(nGroupID, recordingId);

                return recording;
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public RecordingResponse QueryRecords(string sWSUserName, string sWSPassword, string userID, long[] epgIDs)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.QueryRecords(nGroupID, userID, epgIDs);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public Recording GetRecordingStatus(string sWSUserName, string sWSPassword, long recordingId)
        {
            Recording recording = null;

            
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            else
            {
                recording = Core.ConditionalAccess.Module.GetRecordingStatus(nGroupID, recordingId);
            }

            return recording;
        }

        [WebMethod]
        public RecordingResponse SearchDomainRecordings(string sWSUserName, string sWSPassword, string userID, long domainID, ApiObjects.TstvRecordingStatus[] recordingStatuses,
                                                                                      string filter, int pageIndex, int pageSize, ApiObjects.SearchObjects.OrderObj orderBy, bool shouldIgnorePaging)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.SearchDomainRecordings(nGroupID, userID, domainID, recordingStatuses, filter, pageIndex, pageSize, orderBy, shouldIgnorePaging);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public Recording GetRecordingByID(string sWSUserName, string sWSPassword, long domainID, long domainRecordingID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetRecordingByID(nGroupID, domainID, domainRecordingID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status IngestRecording(string sWSUserName, string sWSPassword, long[] epgs, ApiObjects.eAction action)
        {
            ApiObjects.Response.Status status = null;

            
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            else
            {
                status = Core.ConditionalAccess.Module.IngestRecording(nGroupID, epgs, action);
            }

            return status;
        }
               
        [WebMethod]
        public ApiObjects.Response.Status RecoverRecordingMessages(string sWSUserName, string sWSPassword)
        {
            ApiObjects.Response.Status status = null;

            
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            else
            {
                status = Core.ConditionalAccess.Module.RecoverRecordingMessages(nGroupID);
            }

            return status;
        }

        [WebMethod]
        public DomainQuotaResponse GetDomainQuota(string sWSUserName, string sWSPassword, string userID, long domainID)
        {
            DomainQuotaResponse response = null;

            
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID == 0)
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            else
            {
                response = Core.ConditionalAccess.Module.GetDomainQuota(nGroupID, userID, domainID);
            }

            return response;
        }

        [WebMethod]
        public Status RemovePaymentMethodHouseholdPaymentGateway(string sWSUserName, string sWSPassword, int paymentGatewayID, string siteGuid, int householdId, int paymentMethodId, bool force)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[KLogMonitor.Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            Status response = new Status();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.RemovePaymentMethodHouseholdPaymentGateway(nGroupID, paymentGatewayID, siteGuid, householdId, paymentMethodId, force);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public Recording ProtectRecord(string sWSUserName, string sWSPassword, string userID, long recordID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {                
                return Core.ConditionalAccess.Module.ProtectRecord(nGroupID, userID, recordID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public bool CleanupRecordings()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return Core.ConditionalAccess.Module.CleanupRecordings();
        }

        [WebMethod]
        public bool HandleRecordingsLifetime()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return Core.ConditionalAccess.Module.HandleRecordingsLifetime();
        }

        [WebMethod]
        public bool HandleRecordingsScheduledTasks()
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(0);
            return Core.ConditionalAccess.Module.HandleRecordingsScheduledTasks();
        }

        [WebMethod]
        public bool HandleDomainQuotaByRecording(HandleDomainQuataByRecordingTask expiredRecording)
        {
            BaseConditionalAccess t = new TvinciConditionalAccess(expiredRecording.GroupId);
            return Core.ConditionalAccess.Module.HandleDomainQuotaByRecording(expiredRecording);
        }

        [WebMethod]
        public bool HandleFirstFollowerRecording(string sWSUserName, string sWSPassword, string userId, long domainId, string channelId, string seriesId, int seasonNumber)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.HandleFirstFollowerRecording(nGroupID, userId, domainId, channelId, seriesId, seasonNumber);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        public SeriesResponse GetFollowSeries(string sWSUserName, string sWSPassword, string userId, long domainId, SeriesRecordingOrderObj orderBy)
        {
            
            SeriesResponse response = new SeriesResponse();
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetFollowSeries(nGroupID, userId, domainId, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new SeriesResponse()
                {
                    Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
                };
                return response;
            }
        }

        [WebMethod]
        public SeriesRecording RecordSeasonOrSeries(string sWSUserName, string sWSPassword, string userID, long epgID, ApiObjects.RecordingType recordingType)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.RecordSeasonOrSeries(nGroupID, userID, epgID, recordingType);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

        [WebMethod]
        public bool DistributeRecording(string sWSUserName, string sWSPassword, long epgId, long Id, DateTime epgStartDate)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.DistributeRecording(nGroupID, epgId, Id, epgStartDate);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        public bool CompleteDomainSeriesRecordings(string sWSUserName, string sWSPassword, long domainId)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CompleteDomainSeriesRecordings(nGroupID, domainId);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        public LicensedLinkResponse GetRecordingLicensedLink(string sWSUserName, string sWSPassword, string userId, int recordingId, string udid, string userIp, string fileType)
        {
            
            LicensedLinkResponse response = new LicensedLinkResponse()
            {
                Status = new Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString())
            };

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetRecordingLicensedLink(nGroupID, userId, recordingId, udid,userIp, fileType);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return response;
            }
        }

        [WebMethod]
        public bool CheckRecordingDuplicateCrids(string sWSUserName, string sWSPassword, long recordingId)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.CheckRecordingDuplicateCrids(nGroupID, recordingId);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        public Status HandleUserTask(string sWSUserName, string sWSPassword, int domainId, string userId, ApiObjects.UserTaskType actionType)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                result = Core.ConditionalAccess.Module.HandleUserTask(nGroupID, domainId, userId, actionType);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            return result;
        }

        [WebMethod]
        public bool DistributeRecordingWithDomainIds(string sWSUserName, string sWSPassword, long epgId, long Id, DateTime epgStartDate, long[] domainSeriesIds)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.DistributeRecordingWithDomainIds(nGroupID, epgId, Id, epgStartDate, domainSeriesIds);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return false;
            }
        }

        [WebMethod]
        public ApiObjects.KeyValuePair GetSeriesIdAndSeasonNumberByEpgId(string sWSUserName, string sWSPassword, long epgId)
        {
            
            ApiObjects.KeyValuePair result = new ApiObjects.KeyValuePair();
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                result = Core.ConditionalAccess.Module.GetSeriesIdAndSeasonNumberByEpgId(nGroupID, epgId);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;                    
                }                
            }

            return result;
        }

        [WebMethod]
        public SearchableRecording[] GetDomainSearchableRecordings(string sWSUserName, string sWSPassword, long domainId)
        {
            
            SearchableRecording[] result = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                result = Core.ConditionalAccess.Module.GetDomainSearchableRecordings(nGroupID, domainId);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }

            return result;
        }

        [WebMethod]
        public string BulkRecoveryForRenewSubscriptions(string sWSUserName, string sWSPassword, DateTime endDateStartRange, DateTime endDateEndRange)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.BulkRecoveryForRenewSubscriptions(nGroupID, endDateStartRange, endDateEndRange);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return "Failed";
            }
        }

        [WebMethod]
        public Recording GetRecordingByDomainRecordingId(string sWSUserName, string sWSPassword, long domainID, long domainRecordingID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.ConditionalAccess.Module.GetRecordingByDomainRecordingId(nGroupID, domainID, domainRecordingID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }

                return null;
            }
        }

        public Status RemoveHouseholdEntitlements(string sWSUserName, string sWSPassword, int householdId)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Core.ConditionalAccess.Module.RemoveHouseholdEntitlements(nGroupID, householdId);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            return result;
        }

        public Entitlements UpdateEntitlement(string sWSUserName, string sWSPassword, int domainID, Entitlement entitlement)

        {
            Entitlements response = new Entitlements();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.ConditionalAccess.Module.UpdateEntitlement(nGroupID, domainID, entitlement);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }
        
    }
}
