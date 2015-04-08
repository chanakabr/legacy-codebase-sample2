using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Pricing;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;

namespace RestfulTVPApi.Clients
{
    public class PricingClient : BaseClient
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(PricingClient));

        #endregion

        #region C'tor
        public PricingClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
          
        }

        public PricingClient()
        {
            // TODO: Complete member initialization
        }
        #endregion C'tor

        #region Properties

        protected RestfulTVPApi.Pricing.mdoule Pricing
        {
            get
            {
                return (Module as RestfulTVPApi.Pricing.mdoule);
            }
        }

        #endregion

        #region Public methods

        public RestfulTVPApi.Objects.Responses.PPVModule GetPPVModuleData(int ppvCode, string sCountry, string sLanguage, string sDevice)
        {
            RestfulTVPApi.Objects.Responses.PPVModule response = null;

            response = Execute(() =>
                {
                    var res = Pricing.GetPPVModuleData(WSUserName, WSPassword, ppvCode.ToString(), sCountry, sLanguage, sDevice);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.PPVModule;

            return response;
        }

        public List<MediaFilePPVModule> GetPPVModuleListForMediaFiles(int[] mediaFiles, string sCountry, string sLanguage, string sDevice)
        {
            List<MediaFilePPVModule> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Pricing.GetPPVModuleListForMediaFiles(WSUserName, WSPassword, mediaFiles, sCountry, sLanguage, sDevice);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<MediaFilePPVModule>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.Subscription> GetSubscriptionsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            List<RestfulTVPApi.Objects.Responses.Subscription> subscriptions = null;

            subscriptions = Execute(() =>
                {
                    var response = Pricing.GetSubscriptionsContainingMediaFile(WSUserName, WSPassword, iMediaID, iMediaFileID);

                    if (response != null)
                        subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();

                    return subscriptions;
                }) as List<RestfulTVPApi.Objects.Responses.Subscription>;

            return subscriptions;
        }

        public RestfulTVPApi.Objects.Responses.Subscription GetSubscriptionData(string subCode, bool getAlsoInactive)
        {
            RestfulTVPApi.Objects.Responses.Subscription sub = null;

            sub = Execute(() =>
            {
                var res = Pricing.GetSubscriptionData(WSUserName, WSPassword, subCode, string.Empty, string.Empty, string.Empty, getAlsoInactive);
                if (res != null)
                    sub = res.ToApiObject();

                return sub;
            }) as RestfulTVPApi.Objects.Responses.Subscription;

            return sub;
        }

        public RestfulTVPApi.Objects.Responses.CouponData GetCouponStatus(string sCouponCode)
        {
            RestfulTVPApi.Objects.Responses.CouponData couponData = null;

            couponData = Execute(() =>
                {
                    var res = Pricing.GetCouponStatus(WSUserName, WSPassword, sCouponCode);
                    if (res != null)
                        couponData = res.ToApiObject();

                    return couponData;
                }) as RestfulTVPApi.Objects.Responses.CouponData;

            return couponData;
        }

        public RestfulTVPApi.Objects.Responses.Enums.CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID)
        {
            RestfulTVPApi.Objects.Responses.Enums.CouponsStatus couponStatus = RestfulTVPApi.Objects.Responses.Enums.CouponsStatus.NotExists;

            couponStatus = (RestfulTVPApi.Objects.Responses.Enums.CouponsStatus)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.CouponsStatus), Execute(() =>
                {
                    couponStatus = (RestfulTVPApi.Objects.Responses.Enums.CouponsStatus)Pricing.SetCouponUsed(WSUserName, WSPassword, sCouponCode, sSiteGUID);
                    return couponStatus;
                }).ToString());

            return couponStatus;
        }

        public List<Campaign> GetCampaignsByType(CampaignTrigger trigger, bool isAlsoInactive, string udid)
        {
            List<Campaign> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Pricing.GetCampaignsByType(WSUserName, WSPassword, trigger, string.Empty, string.Empty, udid, isAlsoInactive);
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
                    var response = Pricing.GetSubscriptionIDsContainingMediaFile(WSUserName, WSPassword, iMediaID, iMediaFileID);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<int>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.Subscription> GetSubscriptionsContainingUserTypes(int isActive, int[] userTypesIDs)
        {
            List<RestfulTVPApi.Objects.Responses.Subscription> subscriptions = null;

            subscriptions = Execute(() =>
                {
                    string sUserTypesIDs = string.Empty;

                    var response = Pricing.GetSubscriptionsContainingUserTypes(WSUserName, WSPassword, string.Empty, string.Empty, string.Empty, isActive, userTypesIDs);
                    if (response != null)
                    {
                        subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();
                    }

                    return subscriptions;
                }) as List<RestfulTVPApi.Objects.Responses.Subscription>;

            return subscriptions;
        }

        public RestfulTVPApi.Objects.Responses.Collection GetCollectionData(string collectionCode, string countryCd2, string languageCode3, string deviceName, bool getAlsoInactive)
        {
            RestfulTVPApi.Objects.Responses.Collection collection = null;

            collection = Execute(() =>
            {
                var response = Pricing.GetCollectionData(WSUserName, WSPassword, collectionCode, countryCd2, languageCode3, deviceName, getAlsoInactive);
                if (response != null)
                {
                    collection = response.ToApiObject();
                }
                //string sUserTypesIDs = string.Empty;

                //var response = Pricing.GetSubscriptionsContainingUserTypes(m_wsUserName, m_wsPassword, string.Empty, string.Empty, string.Empty, isActive, userTypesIDs);
                //if (response != null)
                //{
                //    collection = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();
                //}

                //return collection;
                return collection;
            }) as RestfulTVPApi.Objects.Responses.Collection;

            return collection;
        }

        #endregion

    }
}