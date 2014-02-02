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

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class ChannelsService : Service
    {
        public IChannelsRepository _repository { get; set; }  //Injected by IOC

        #region GET

        public object Get(GetChannelMultiFilterRequest request)
        {
            return _repository.GetChannelMultiFilter(request.InitObj, request.channel_id, request.pic_size, request.page_size, request.page_number, request.order_by, request.order_dir, request.tags_metas, request.cut_with);
        }

        public object Get(GetChannelsListRequest request)
        {
            return _repository.GetChannelsList(request.InitObj, request.pic_size);
        }

        public object Get(GetCategoryRequest request)
        {
            return _repository.GetCategory(request.InitObj, request.category_id);
        }

        public object Get(GetFullCategoryRequest request)
        {
            return _repository.GetFullCategory(request.InitObj, request.category_id, request.pic_size);
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
