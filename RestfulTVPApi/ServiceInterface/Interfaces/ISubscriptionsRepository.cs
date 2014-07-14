using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISubscriptionsRepository
    {
        List<Media> GetMediasInPackage(GetMediasInPackageRequest request);

        List<SubscriptionPrice> GetSubscriptionDataPrices(GetSubscriptionDataPricesRequest request);

        string GetSubscriptionProductCode(GetSubscriptionProductCodeRequest request);

        List<Subscription> GetSubscriptionData(GetSubscriptionDataRequest request);

        List<SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(GetSubscriptionsPricesWithCouponRequest request);
    }
}