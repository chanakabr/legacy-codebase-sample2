using System.Collections.Generic;
using TVPApi;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISubscriptionsRepository
    {
        IEnumerable<Media> GetMediasInPackage(InitializationObject initObj, int baseID, int mediaType, string picSize, int pageSize, int pageIndex);

        IEnumerable<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs);

        string GetSubscriptionProductCode(InitializationObject initObj, int subID);

        IEnumerable<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs);

        IEnumerable<SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string sSiteGUID, string[] sSubscriptions, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName);
    }
}