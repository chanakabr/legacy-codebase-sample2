using System.Collections.Generic;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Manager;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;

namespace RestfulTVPApi.ServiceInterface
{
    public class SubscriptionsRepository : ISubscriptionsRepository
    {
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

        public List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs)
        {
            List<SubscriptionPrice> res = new List<SubscriptionPrice>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionDataPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                foreach (int subID in subIDs)
                {

                    var priceObj = ServicesManager.PricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false);

                    res.Add(new SubscriptionPrice
                    {
                        subscription_code = priceObj.object_code,
                        price = priceObj.subscription_price_code.prise.price,
                        currency = priceObj.subscription_price_code.prise.currency.currency_sign
                    });
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public string GetSubscriptionProductCode(InitializationObject initObj, int subID)
        {
            string res = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionProductCode", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                res = ServicesManager.PricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false).product_code;
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

        public List<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs)
        {
            List<Subscription> res = new List<Subscription>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiPricingService _service = ServicesManager.PricingService(groupId, initObj.Platform);

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

        public List<SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string sSiteGUID, string[] sSubscriptions, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                return ServicesManager.ConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(sSubscriptions, sSiteGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

    }
}