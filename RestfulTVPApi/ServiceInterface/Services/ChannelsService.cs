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

    #region Objects

    [Route("/channels/{ChannelID}/medias", "GET", Notes = "This method returns an array of the media inside the channel")]
    public class GetChannelMultiFilterRequest : PagingRequest, IReturn<IEnumerable<Media>>
    {
        [ApiMember(Name = "channel_id", Description = "Channel ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int channel_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(TVPApi.OrderBy))]
        public TVPApi.OrderBy order_by { get; set; }
        [ApiMember(Name = "order_dir", Description = "Order Direction", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_dir", typeof(eOrderDirection))]
        public eOrderDirection order_dir { get; set; }
        [ApiMember(Name = "tags_metas", Description = "Tags Metas", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> tags_metas { get; set; }
        [ApiMember(Name = "cut_with", Description = "Cut With", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("cut_with", typeof(CutWith))]
        public CutWith cut_with { get; set; }
    }

    [Route("/channels/{ChannelID}", "GET", Notes = "This method returns an array of all channels that exist for this customer site")]
    public class GetChannelsListRequest : RequestBase, IReturn<IEnumerable<Channel>>
    {
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/categories/{category_id}", "GET", Notes = "This method searches for all channels in a category. Category is an ordered hierarchical list of channels that belong to a similar theme or type")]
    public class GetCategoryRequest : RequestBase, IReturn<Category>
    {
        [ApiMember(Name = "category_id", Description = "Category ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int category_id { get; set; }
    }

    [Route("/categories/{category_id}/full", "GET", Notes = "Category is an ordered hierarchical list of channels that belong to a similar theme or type. This method searches for a category and its dependences. When the category contains inner categories, the categories and the inner categories are returned")]
    public class GetFullCategoryRequest : RequestBase, IReturn<Category>
    {
        [ApiMember(Name = "category_id", Description = "Category ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int category_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    [RequiresInitializationObject]
    public class ChannelsService : Service
    {
        public IChannelsRepository _repository { get; set; }  //Injected by IOC

        public HttpResult Get(GetChannelMultiFilterRequest request)
        {
            var response = _repository.GetChannelMultiFilter(request.InitObj, request.channel_id, request.pic_size, request.page_size, request.page_number, request.order_by, request.order_dir, request.tags_metas, request.cut_with);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetChannelsListRequest request)
        {
            var response = _repository.GetChannelsList(request.InitObj, request.pic_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetCategoryRequest request)
        {
            var response = _repository.GetCategory(request.InitObj, request.category_id);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }

        public HttpResult Get(GetFullCategoryRequest request)
        {
            var response = _repository.GetFullCategory(request.InitObj, request.category_id, request.pic_size);

            if (response == null)
            {
                return new HttpResult(HttpStatusCode.InternalServerError);
            }

            return new HttpResult(base.RequestContext.ToPartialResponse(response), HttpStatusCode.OK);
        }
    }
}
