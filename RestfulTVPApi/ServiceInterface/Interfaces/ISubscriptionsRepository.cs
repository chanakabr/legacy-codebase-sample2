using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Pricing;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISubscriptionsRepository
    {
        bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID);

        string DummyChargeUserForSubscription(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID);

        List<Media> GetMediasInPackage(InitializationObject initObj, int baseID, int mediaType, string picSize, int pageSize, int pageIndex);

        List<Media> GetSubscriptionMedias(InitializationObject initObj, string[] subIDs, string picSize, Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy orderBy);

        List<SubscriptionPrice> GetSubscriptionDataPrices(InitializationObject initObj, int[] subIDs);

        string GetSubscriptionProductCode(InitializationObject initObj, int subID);

        List<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs);
    }
}