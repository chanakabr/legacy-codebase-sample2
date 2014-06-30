using RestfulTVPApi.ServiceModel;
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
        public List<Media> GetMediasInPackage(GetMediasInPackageRequest request)
        {
            List<Media> lstMedia = null;

            lstMedia = new APISubscriptionMediaLoader(request.subscription_id, request.GroupID, request.InitObj.Platform, request.InitObj.UDID, SiteHelper.GetClientIP(), request.InitObj.Locale.LocaleLanguage, request.page_size, request.page_number, request.pic_size)
                {
                    MediaTypes = new List<int>() { request.media_type },
                    UseStartDate = Utils.GetUseStartDateValue(request.GroupID, request.InitObj.Platform)
                }.Execute() as List<Media>;


            return lstMedia;
        }

        public List<SubscriptionPrice> GetSubscriptionDataPrices(GetSubscriptionDataPricesRequest request)
        {
            List<SubscriptionPrice> res = new List<SubscriptionPrice>();

            foreach (int subID in request.subscription_ids)
                {

                    var priceObj = ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetSubscriptionData(subID.ToString(), false);

                    res.Add(new SubscriptionPrice
                    {
                        subscription_code = priceObj.object_code,
                        price = priceObj.subscription_price_code.price.price,
                        currency = priceObj.subscription_price_code.price.currency.currency_sign
                    });
                }
            
            return res;
        }

        public string GetSubscriptionProductCode(GetSubscriptionProductCodeRequest request)
        {
            string res = string.Empty;

            res = ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetSubscriptionData(request.subscription_id.ToString(), false).product_code;
            
            return res;
        }

        public List<Subscription> GetSubscriptionData(GetSubscriptionDataRequest request)
        {
            List<Subscription> res = new List<Subscription>();

            ApiPricingService _service = ServicesManager.PricingService(request.GroupID, request.InitObj.Platform);

                foreach (int subID in request.subscription_ids)
                {
                    res.Add(_service.GetSubscriptionData(subID.ToString(), false));
                }
            
            return res;
        }

        public List<SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(GetSubscriptionsPricesWithCouponRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetSubscriptionsPricesWithCoupon(request.subscription_ids, request.site_guid, request.coupon_code, request.country_code, request.language_code, request.device_name);
        }
    }
}