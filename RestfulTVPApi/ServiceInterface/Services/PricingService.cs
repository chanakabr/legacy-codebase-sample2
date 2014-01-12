using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using RestfulTVPApi.ServiceModel;
using TVPPro.SiteManager.TvinciPlatform.Pricing;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/coupons/{coupon_code}", "GET", Notes = "This method returns the status of a specific coupon")]
    public class GetCouponStatusRequest : RequestBase, IReturn<CouponData>
    {
        [ApiMember(Name = "coupon_code", Description = "Coupon Code", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string coupon_code { get; set; }
    }

    [Route("/ppv_modules/{ppv_code}", "GET", Notes = "This method retrieves all information regarding a specific PPV module")]
    public class GetPPVModuleDataRequest : RequestBase, IReturn<PPVModule>
    {
        [ApiMember(Name = "ppv_code", Description = "PPV Code", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int ppv_code { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class PricingService : Service
    {
        public IPricingRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetCouponStatusRequest request)
        {
            var response = _repository.GetCouponStatus(request.InitObj, request.coupon_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetPPVModuleDataRequest request)
        {
            var response = _repository.GetPPVModuleData(request.InitObj, request.ppv_code);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
