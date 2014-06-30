using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Services;

namespace RestfulTVPApi.ServiceInterface
{
    public class CollectionRepository : ICollectionRepository
    {
        public Collection GetCollectionData(GetCollectionDataRequest request)
        {
            return ServicesManager.PricingService(request.GroupID, request.InitObj.Platform).GetCollectionData(request.collection_id, string.Empty, string.Empty, string.Empty, request.get_also_inactive);
        }

        public List<CollectionPricesContainer> GetCollectionsPrices(GetCollectionsPricesRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetCollectionPrices(request.collection_ids, request.site_guid, request.country_code, request.language_code, request.device_name);
        }

        public List<CollectionPricesContainer> GetCollectionsPricesWithCoupon(GetCollectionsPricesWithCouponRequest request)
        {
            return ServicesManager.ConditionalAccessService(request.GroupID, request.InitObj.Platform).GetCollectionPricesWithCoupon(request.collection_ids, request.site_guid, request.country_code, request.language_code, request.coupon_code, request.device_name);
        }        
    }
}