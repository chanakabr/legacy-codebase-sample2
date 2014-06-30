using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ICollectionRepository
    {
        Collection GetCollectionData(GetCollectionDataRequest request);

        List<CollectionPricesContainer> GetCollectionsPrices(GetCollectionsPricesRequest request);

        List<CollectionPricesContainer> GetCollectionsPricesWithCoupon(GetCollectionsPricesWithCouponRequest request);       
    }
}