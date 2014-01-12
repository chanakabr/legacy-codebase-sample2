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

    [Route("/subscriptions/{subscription_id}", "DELETE", Notes = "This method cancels a subscription previously purchased by the user. Requires the subscription identifier and the subscription purchase identifier. Returns boolean success/fail")]
    public class CancelSubscriptionRequest : RequestBase, IReturn<bool>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string subscription_id { get; set; }
        [ApiMember(Name = "subscription_purchase_id", Description = "Subscription Purchase ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_purchase_id { get; set; }
    }

    [Route("/subscriptions/{subscription_id}/dummy_charge", "POST", Notes = "This method performs a user, dummy purchase of a subscription. Used to give the user entitlement to a subscription asset without charge")]
    public class DummyChargeUserForSubscriptionRequest : RequestBase, IReturn<string>
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

    [Route("/subscriptions/{subscription_id}/medias", "POST", Notes = "This method returns an array of media assets belonging to a specific package (subscription)")]
    public class GetMediasInPackageRequest : PagingRequest, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_id { get; set; }
        [ApiMember(Name = "media_type", Description = "Media Type", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int media_type { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    //Ofir - weird route
    [Route("/subscriptions/{subscription_ids}/reflective_medias", "POST", Notes = "This method returns all media for a given array of subscriptions")]
    public class GetSubscriptionMediasRequest : RequestBase, IReturn<IEnumerable<MediaDTO>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public string[] subscription_ids { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy))]
        public Tvinci.Data.Loaders.TvinciPlatform.Catalog.OrderBy order_by { get; set; }
    }

    [Route("/subscriptions/{subscription_ids}/prices", "GET", Notes = "This method returns an array of the subscription data prices")]
    public class GetSubscriptionDataPricesRequest : RequestBase, IReturn<IEnumerable<SubscriptionPriceDTO>>
    {
        [ApiMember(Name = "subscription_ids", Description = "Subscriptions Identifiers", ParameterType = "path", DataType = SwaggerType.Array, IsRequired = true)]
        public int[] subscription_ids { get; set; }
    }

    [Route("/subscriptions/{subscription_id}/product_code", "GET", Notes = "This method returns the In_App product code corresponding to a specific Tvinci subscription ID number. This is because the Tvinci code for a particular product is different than the code used by different customer stores")]
    public class GetSubscriptionProductCodeRequest : RequestBase, IReturn<string>
    {
        [ApiMember(Name = "subscription_id", Description = "Subscription Identifier", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int subscription_id { get; set; }
    }

    [Route("/subscriptions/{subscription_ids}", "GET", Notes = "This method returns an array containing the data of subscription IDs (array) posted to the system")]
    public class GetSubscriptionDataRequest : RequestBase, IReturn<IEnumerable<SubscriptionPriceDTO>>
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

        public HttpResult Delete(CancelSubscriptionRequest request)
        {
            var response = _repository.CancelSubscription(request.InitObj, request.subscription_id, request.subscription_purchase_id);

            return new HttpResult(response, HttpStatusCode.OK);
        }

        public HttpResult Post(DummyChargeUserForSubscriptionRequest request)
        {
            var response = _repository.DummyChargeUserForSubscription(request.InitObj, request.price, request.currency, request.subscription_id, request.coupon_code, request.user_ip, request.extra_parameters, request.udid);

            return new HttpResult(response, HttpStatusCode.OK);
        }

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

        public HttpResult Get(GetSubscriptionMediasRequest request)
        {
            var response = _repository.GetSubscriptionMedias(request.InitObj, request.subscription_ids, request.pic_size, request.order_by);

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

    }
}
