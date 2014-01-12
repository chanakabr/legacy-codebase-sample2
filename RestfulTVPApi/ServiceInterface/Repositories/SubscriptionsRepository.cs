using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Pricing;


namespace RestfulTVPApi.ServiceInterface
{
    public class SubscriptionsRepository : ISubscriptionsRepository
    {
        //Ofir - Moved from Player.
        //Ofir - Should SiteGuid be a param?
        public bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID)
        {
            bool response = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                response = new ApiConditionalAccessService(groupId, initObj.Platform).CancelSubscription(initObj.SiteGuid, sSubscriptionID, sSubscriptionPurchaseID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Moved from CRM.
        //Ofir - Should SiteGuid be a param?
        //Ofir - Why UDID is a param and siteguid not?
        public string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, initObj.SiteGuid, sExtraParameters, sUDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        //Ofir - Moved from Media.
        public List<Media> GetMediasInPackage(InitializationObject initObj, int baseID, int mediaType, string picSize, int pageSize, int pageIndex)
        {
            List<Media> lstMedia = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetMediasInPackage", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                lstMedia = new APISubscriptionMediaLoader(baseID, groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, pageSize, pageIndex, picSize)
                {
                    MediaTypes = new List<int>() { mediaType },
                    UseStartDate = bool.Parse(ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate)
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        //Ofir - Moved from Media.
        public List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] subIDs, string picSize, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy)
        {
            List<Media> lstMedia = new List<Media>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionMedias", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                List<KeyValue> orList = new List<KeyValue>();

                foreach (var id in subIDs)
                    orList.Add(new KeyValue() { m_sKey = "Base ID", m_sValue = id });

                lstMedia = new APISearchMediaLoader(groupID, initObj.Platform, initObj.UDID, SiteHelper.GetClientIP(), initObj.Locale.LocaleLanguage, 0, 0, picSize, true, orList, null, null)
                {
                    UseStartDate = true,
                    OrderBy = orderBy,
                }.Execute() as List<Media>;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return lstMedia;
        }

        //Ofir - Moved from Media.
        public List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs)
        {
            List<SubscriptionPrice> res = new List<SubscriptionPrice>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionDataPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                foreach (int subID in subIDs)
                {
                    var priceObj = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false);

                    res.Add(new SubscriptionPrice
                    {
                        SubscriptionCode = priceObj.m_sObjectCode,
                        Price = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_dPrice,
                        Currency = priceObj.m_oSubscriptionPriceCode.m_oPrise.m_oCurrency.m_sCurrencySign
                    });
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - Moved from Media.
        public string GetSubscriptionProductCode(InitializationObject initObj, int subID)
        {
            string res = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionProductCode", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false).m_ProductCode;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        //Ofir - Moved from Pricing.
        public List<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs)
        {
            List<Subscription> res = new List<Subscription>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiPricingService _service = new ApiPricingService(groupId, initObj.Platform);

                foreach (int subID in subIDs)
                {
                    res.Add(_service.GetSubscriptionData(subID.ToString(), false));
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }
    }
}