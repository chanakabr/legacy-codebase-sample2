using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Extentions;
using TVPApiModule.Context;

namespace TVPApiModule.Services
{
    public class ApiPricingService : BaseService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiPricingService));

        //private TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiPricingService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiPricingService()
        {
            // TODO: Complete member initialization
        }
        #endregion C'tor

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule Pricing
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule);
            }
        }

        #endregion

        #region Public methods

        public TVPApiModule.Objects.Responses.PPVModule GetPPVModuleData(int ppvCode, string sCountry, string sLanguage, string sDevice)
        {
            TVPApiModule.Objects.Responses.PPVModule response = null;

            response = Execute(() =>
                {
                    var res = Pricing.GetPPVModuleData(m_wsUserName, m_wsPassword, ppvCode.ToString(), sCountry, sLanguage, sDevice);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.PPVModule;

            return response;
        }

        public List<MediaFilePPVModule> GetPPVModuleListForMediaFiles(int[] mediaFiles, string sCountry, string sLanguage, string sDevice)
        {
            List<MediaFilePPVModule> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Pricing.GetPPVModuleListForMediaFiles(m_wsUserName, m_wsPassword, mediaFiles, sCountry, sLanguage, sDevice);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<MediaFilePPVModule>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            List<TVPApiModule.Objects.Responses.Subscription> subscriptions = null;

            subscriptions = Execute(() =>
                {
                    var response = Pricing.GetSubscriptionsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);

                    if (response != null)
                        subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();

                    return subscriptions;
                }) as List<TVPApiModule.Objects.Responses.Subscription>;

            return subscriptions;
        }

        public TVPApiModule.Objects.Responses.Subscription GetSubscriptionData(string subCode, bool getAlsoInactive)
        {
            TVPApiModule.Objects.Responses.Subscription sub = null;

            sub = Execute(() =>
            {
                var res = Pricing.GetSubscriptionData(m_wsUserName, m_wsPassword, subCode, string.Empty, string.Empty, string.Empty, getAlsoInactive);
                if (res != null)
                    sub = res.ToApiObject();

                return sub;
            }) as TVPApiModule.Objects.Responses.Subscription;

            return sub;
        }

        public TVPApiModule.Objects.Responses.CouponData GetCouponStatus(string sCouponCode)
        {
            TVPApiModule.Objects.Responses.CouponData couponData = null;

            couponData = Execute(() =>
                {
                    var res = Pricing.GetCouponStatus(m_wsUserName, m_wsPassword, sCouponCode);
                    if (res != null)
                        couponData = res.ToApiObject();

                    return couponData;
                }) as TVPApiModule.Objects.Responses.CouponData;

            return couponData;
        }

        public TVPApiModule.Objects.Responses.CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID)
        {
            TVPApiModule.Objects.Responses.CouponsStatus couponStatus = TVPApiModule.Objects.Responses.CouponsStatus.NotExists;

            couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)Enum.Parse(typeof(TVPApiModule.Objects.Responses.CouponsStatus), Execute(() =>
                {
                    couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)Pricing.SetCouponUsed(m_wsUserName, m_wsPassword, sCouponCode, sSiteGUID);
                    return couponStatus;
                }).ToString());

            return couponStatus;
        }

        public List<Campaign> GetCampaignsByType(CampaignTrigger trigger, bool isAlsoInactive, string udid)
        {
            List<Campaign> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Pricing.GetCampaignsByType(m_wsUserName, m_wsPassword, trigger, string.Empty, string.Empty, udid, isAlsoInactive);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<Campaign>;

            return retVal;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            List<int> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Pricing.GetSubscriptionIDsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<int>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionsContainingUserTypes(int isActive, int[] userTypesIDs)
        {
            List<TVPApiModule.Objects.Responses.Subscription> subscriptions = null;

            subscriptions = Execute(() =>
                {
                    string sUserTypesIDs = string.Empty;

                    var response = Pricing.GetSubscriptionsContainingUserTypes(m_wsUserName, m_wsPassword, string.Empty, string.Empty, string.Empty, isActive, userTypesIDs);
                    if (response != null)
                    {
                        subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();
                    }

                    return subscriptions;
                }) as List<TVPApiModule.Objects.Responses.Subscription>;

            return subscriptions;
        }

        #endregion
    }
}
