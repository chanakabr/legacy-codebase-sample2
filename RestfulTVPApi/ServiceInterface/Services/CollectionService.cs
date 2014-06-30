using RestfulTVPApi.ServiceModel;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceInterface.Services
{
    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class CollectionService : Service
    {
        public ICollectionRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetCollectionDataRequest request)
        {
            return _repository.GetCollectionData(request);
        }

        public object Get(GetCollectionsPricesRequest request)
        {
            return _repository.GetCollectionsPrices(request);
        }

        public object Get(GetCollectionsPricesWithCouponRequest request)
        {
            return _repository.GetCollectionsPricesWithCoupon(request);
        }        

        #endregion
    }
}