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

    #region Objects

    [Route("/subscriptions/{subscription_id}", "DELETE", Summary = "Cancel Subscription", Notes = "Cancel Subscription")]
    public class CancelSubscription : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "subscription_purchase_id", Description = "Subscription Purchase ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_purchase_id { get; set; }
    }

    [Route("/subscriptions/{subscription_id}/dummycharge", "POST", Summary = "Dummy Charge User For Subscription", Notes = "Dummy Charge User For Subscription")]
    public class DummyChargeUserForSubscription : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "price", Description = "Price", ParameterType = "body", DataType = SwaggerType.Double, IsRequired = true)]
        public double price { get; set; }
        [ApiMember(Name = "currency", Description = "Currency", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string currency { get; set; }
        [ApiMember(Name = "ppv_module_code", Description = "PPV Module Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string ppv_module_code { get; set; }
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
        [ApiMember(Name = "user_ip", Description = "User IP Address", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string user_ip { get; set; }
        [ApiMember(Name = "extra_parameters", Description = "Extra Parameters", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string extra_parameters { get; set; }
        [ApiMember(Name = "udid", Description = "UDID", ParameterType = "body", DataType = SwaggerType.String, IsRequired = true)]
        public string udid { get; set; }
    }

    [Route("/subscriptions/{subscription_id}/medias", "POST", Summary = "Get Medias In Package", Notes = "Get Medias In Package")]
    public class GetMediasInPackage : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    //ofir - weird route
    [Route("/subscriptions/{subscription_ids}/reflectivemedias", "POST", Summary = "Get Subscriptions Medias", Notes = "Get Subscriptions Medias")]
    public class GetSubscriptionMedias : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] subscription_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy))]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy order_by { get; set; }
    }

    [Route("/subscriptions/{subscription_ids}/prices", "GET", Summary = "Get Subscription Data Prices", Notes = "Get Subscription Data Prices")]
    public class GetSubscriptionDataPrices : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] subscription_ids { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class SubscriptionsService : Service
    {
        public ISubscriptionsRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Delete(CancelSubscription request)
        {
            var response = _repository.CancelSubscription(request.InitObj, request.subscription_id, request.subscription_purchase_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(DummyChargeUserForSubscription request)
        {
            var response = _repository.DummyChargeUserForSubscription(request.InitObj, request.price, request.currency, request.subscription_id, request.coupon_code, request.user_ip, request.extra_parameters, request.udid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Get(GetMediasInPackage request)
        {
            var response = _repository.GetMediasInPackage(request.InitObj, request.subscription_id, request.media_type, request.pic_size, request.page_size, request.page_number);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionMedias request)
        {
            var response = _repository.GetSubscriptionMedias(request.InitObj, request.subscription_ids, request.pic_size, request.order_by);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }

        public HttpResult Get(GetSubscriptionDataPrices request)
        {
            var response = _repository.GetSubscriptionDataPrices(request.InitObj, request.subscription_ids);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            var responseDTO = response.Select(x => x.ToDto());

            return new HttpResult(base.RequestContext.ToPartialResponse(responseDTO), HttpStatusCode.OK);
        }
    }
}
