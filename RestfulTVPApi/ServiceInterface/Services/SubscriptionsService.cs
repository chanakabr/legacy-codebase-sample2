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
            return _repository.GetMediasInPackage(request);
        }

        public object Get(GetSubscriptionDataPricesRequest request)
        {
            return _repository.GetSubscriptionDataPrices(request);
        }

        public object Get(GetSubscriptionProductCodeRequest request)
        {
            return _repository.GetSubscriptionProductCode(request);
        }

        public object Get(GetSubscriptionDataRequest request)
        {
            return _repository.GetSubscriptionData(request);
        }

        public object Get(GetSubscriptionsPricesWithCouponRequest request)
        {
            return _repository.GetSubscriptionsPricesWithCoupon(request);
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
