using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System.Collections.Generic;


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