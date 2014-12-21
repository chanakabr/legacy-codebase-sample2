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
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace RestfulTVPApi.ServiceInterface
{
    [RequiresInitializationObject]
    public class ChannelsService : Service
    {
        public IChannelsRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetChannelMultiFilterRequest request)
        {
            return _repository.GetChannelMultiFilter(request);
        }

        public object Get(GetChannelsListRequest request)
        {
            return _repository.GetChannelsList(request);
        }

        public object Get(GetCategoryRequest request)
        {
            return _repository.GetCategory(request);
        }

        public object Get(GetFullCategoryRequest request)
        {
            return _repository.GetFullCategory(request);
        }

        public object Get(GetOrderedChannelMultiFilterRequest request)
        {
            return _repository.GetOrderedChannelMultiFilter(request);
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
