using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.Configuration.PlatformServices;
using System.Web;
using Phx.Lib.Log;
using System.Reflection;
using Core.Billing;

namespace TVPPro.SiteManager.Services
{
    public class BillingService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private string wsUserName;
        private string wsPassword;
        private int groupId;

        static volatile BillingService instance;
        static object instanceLock = new object();

        [Serializable]
        public struct BillingUserData
        {
            public string Type;
            public string FileID;
            public string SiteGuid;
            public string Rating;
            public string Description;
            public string CatalogPrice;
            public string Price;
            public string FullPrice;
            public string PPVModulePrice;
            public string UserIP;
            public string PPVModuleCode;
            public string PPVDiscount;
            public string Title;
            public string Currency;
            public string CurrencySign;
            public string Availability;
            public string DeviceAvailability;
            public string AvailabilityDate;
            public string ImageLink;
            public string CCDigits;
            public string CCLastFourDigits;
            public string CCVariant;
            public string CheckSum;
            public string Referrer;
            public string TimeStamp;
            public string MediaID;
            public string MediaType;
            public string TotalItems;
            public string TotalPrice;
            public string IsRecurring;
            public string CouponCode;
            public string AutoRedirectURL;
            public string CampaignID;
            public string CommerceID;
            public string IsEntitledToPreviewModule;
            public string PreviewModuleID;
        }

        #region C'tor
        public BillingService()
        {
            wsUserName = PlatformServicesConfiguration.Instance.Data.BillingService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.BillingService.DefaultPassword;
            groupId = Core.Billing.Utils.GetGroupID(wsUserName, wsPassword, string.Empty);
        }

        #endregion

        #region

        public static BillingService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new BillingService();
                        }
                    }
                }

                return instance;
            }
        }
        #endregion

        public string GetUserCCDigits(string sSiteGUID)
        {
            string sCCDigits = string.Empty;
            try
            {                
                sCCDigits = Core.Billing.Module.CC_GetUserCCDigits(groupId, sSiteGUID);
                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user CreditCard digits Protocol CC_GetUserCCDigits, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, SiteGUID: {3} ", ex.Message, wsUserName, wsPassword, sSiteGUID);
            }

            return sCCDigits;
        }

        public string GetClientCCCheckSum(string sUserIP, string sTimeStamp)
        {
            string sClientChecksum = string.Empty;
            try
            {
                sClientChecksum = Core.Billing.Module.CC_GetClientCheckSum(groupId, sUserIP, sTimeStamp);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user CheckSum Protocol CC_GetClientCheckSum, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, sUserIP: {3} ", ex.Message, wsUserName, wsPassword, sUserIP);
            }

            return sClientChecksum;
        }

        public string GetClientMerchantSig(string sData)
        {
            string sRet = string.Empty;
            try
            {
                sRet = Core.Billing.Module.GetClientMerchantSig(groupId, sData);                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user CheckSum Protocol GetClientMerchantSig, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, sUserIP: {3} ", ex.Message, wsUserName, wsPassword, sData);
            }
            return sRet;
        }

        public AdyenBillingDetail GetLastBillingUserInfo(string sSiteGuid, int billingMethod)
        {
            AdyenBillingDetail lastBillingInfo = null;
            try
            {
                lastBillingInfo = Core.Billing.Module.GetLastBillingUserInfo(groupId, sSiteGuid, billingMethod);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user Last billing Protocol GetLastBillingUserInfo, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, sSiteGuid: {3}, billingMethod: {4} ", ex.Message, wsUserName, wsPassword, sSiteGuid, billingMethod);
            }

            return lastBillingInfo;
        }

        public AdyenBillingDetail GetLastBillingTypeUserInfo(string sSiteGuid)
        {
            AdyenBillingDetail lastBillingInfo = null;
            try
            {
                lastBillingInfo = Core.Billing.Module.GetLastBillingTypeUserInfo(groupId, sSiteGuid);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error recive user Last billing Protocol GetLastBillingTypeUserInfo, Error Message: {0} Parameters :ws User name : {1} , ws Password: {2}, sSiteGuid: {3}", ex.Message, wsUserName, wsPassword, sSiteGuid);
            }

            return lastBillingInfo;
        }
    }
}
