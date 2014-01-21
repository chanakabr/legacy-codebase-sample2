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

        public HttpResult Get(GetMediasInPackageRequest request)
        {
            var response = _repository.GetMediasInPackage(request.InitObj, request.subscription_id, request.media_type, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionDataPricesRequest request)
        {
            var response = _repository.GetSubscriptionDataPrices(request.InitObj, request.subscription_ids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionProductCodeRequest request)
        {
            var response = _repository.GetSubscriptionProductCode(request.InitObj, request.subscription_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionDataRequest request)
        {
            var response = _repository.GetSubscriptionData(request.InitObj, request.subscription_ids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionsPricesWithCouponRequest request)
        {
            var response = _repository.GetSubscriptionsPricesWithCoupon(request.InitObj, request.site_guid, request.subscription_ids, request.coupon_code, request.country_code, request.language_code, request.device_name);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
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
