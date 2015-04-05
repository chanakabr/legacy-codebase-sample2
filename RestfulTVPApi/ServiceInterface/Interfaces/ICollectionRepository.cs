using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RestfulTVPApi.ServiceInterface
{
    public interface ICollectionRepository
    {
        Collection GetCollectionData(GetCollectionDataRequest request);

        List<CollectionPricesContainer> GetCollectionsPrices(GetCollectionsPricesRequest request);

        List<CollectionPricesContainer> GetCollectionsPricesWithCoupon(GetCollectionsPricesWithCouponRequest request);       
    }
}