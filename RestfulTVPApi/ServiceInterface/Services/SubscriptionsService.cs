using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;

namespace RestfulTVPApi.ServiceInterface
{

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class SubscriptionsService : Service
    {
        public ISubscriptionsRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetMediasInPackageRequest request)
        {
            return _repository.GetMediasInPackage(request.InitObj, request.subscription_id, request.media_type, request.pic_size, request.page_size, request.page_number);
        }

        public object Get(GetSubscriptionDataPricesRequest request)
        {
            return _repository.GetSubscriptionDataPrices(request.InitObj, request.subscription_ids);
        }

        public object Get(GetSubscriptionProductCodeRequest request)
        {
            return _repository.GetSubscriptionProductCode(request.InitObj, request.subscription_id);
        }

        public object Get(GetSubscriptionDataRequest request)
        {
            return _repository.GetSubscriptionData(request.InitObj, request.subscription_ids);
        }

        public object Get(GetSubscriptionsPricesWithCouponRequest request)
        {
            return _repository.GetSubscriptionsPricesWithCoupon(request.InitObj, request.site_guid, request.subscription_ids, request.coupon_code, request.country_code, request.language_code, request.device_name);
        }

        #endregion

        #region PUT
        #endregion

        #region POST
        #endregion

        #region DELETE
        #endregion

    }
}
