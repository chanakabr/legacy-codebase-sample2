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
            var response = _repository.GetChannelMultiFilter(request.InitObj, request.channel_id, request.pic_size, request.page_size, request.page_number, request.order_by, request.order_dir, request.tags_metas, request.cut_with);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetChannelsListRequest request)
        {
            var response = _repository.GetChannelsList(request.InitObj, request.pic_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetCategoryRequest request)
        {
            var response = _repository.GetCategory(request.InitObj, request.category_id);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public object Get(GetFullCategoryRequest request)
        {
            var response = _repository.GetFullCategory(request.InitObj, request.category_id, request.pic_size);

            if (response == null)
            {
                return new HttpResult(string.Empty, HttpStatusCode.InternalServerError);
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
