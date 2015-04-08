using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceInterface
{
    public class CollectionRepository : ICollectionRepository
    {
        public Collection GetCollectionData(GetCollectionDataRequest request)
        {
            return ClientsManager.PricingClient().GetCollectionData(request.collection_id, string.Empty, string.Empty, string.Empty, request.get_also_inactive);
        }

        public List<CollectionPricesContainer> GetCollectionsPrices(GetCollectionsPricesRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetCollectionPrices(request.collection_ids, request.site_guid, request.country_code, request.language_code, request.device_name);
        }

        public List<CollectionPricesContainer> GetCollectionsPricesWithCoupon(GetCollectionsPricesWithCouponRequest request)
        {
            return ClientsManager.ConditionalAccessClient().GetCollectionPricesWithCoupon(request.collection_ids, request.site_guid, request.country_code, request.language_code, request.coupon_code, request.device_name);
        }        
    }
}