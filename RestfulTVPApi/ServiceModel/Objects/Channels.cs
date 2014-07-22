using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Context;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{
    #region GET

    [Route("/channels/{ChannelID}/medias", "GET", Notes = "This method returns an array of the media inside the channel")]
    public class GetChannelMultiFilterRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "channel_id", Description = "Channel ID", ParameterType = "path", DataType = SwaggerType.Int, IsRequired = true)]
        public int channel_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(TVPApiModule.Context.OrderBy))]
        public TVPApiModule.Context.OrderBy order_by { get; set; }
        [ApiMember(Name = "order_dir", Description = "Order Direction", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_dir", typeof(eOrderDirection))]
        public eOrderDirection order_dir { get; set; }
        [ApiMember(Name = "tags_metas", Description = "Tags Metas", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> tags_metas { get; set; }
        [ApiMember(Name = "cut_with", Description = "Cut With", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("cut_with", typeof(CutWith))]
        public CutWith cut_with { get; set; }
    }

    [Route("/channels/{ChannelID}/ordered_medias_multi_filter", "GET", Notes = "Returnes channel's media, filtered and ordered by the requested parameters")]
    public class GetOrderedChannelMultiFilterRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "channel_id", Description = "Channel ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long channel_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(TVPApiModule.Context.OrderBy))]
        public TVPApiModule.Context.OrderBy order_by { get; set; }
        [ApiMember(Name = "order_dir", Description = "Order Direction", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        [ApiAllowableValues("order_dir", typeof(eOrderDirection))]
        public eOrderDirection order_dir { get; set; }
        [ApiMember(Name = "tags_metas", Description = "Tags Metas", ParameterType = "query", DataType = SwaggerType.Array, IsRequired = true)]
        public List<KeyValue> tags_metas { get; set; }
        [ApiMember(Name = "cut_with", Description = "Cut With", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        [ApiAllowableValues("cut_with", typeof(CutWith))]
        public CutWith cut_with { get; set; }
        [ApiMember(Name = "order_value", Description = "Order value", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string order_value { get; set; }
    }

    [Route("/channels/{site_guid}", "GET", Notes = "This method returns an array of all channels that exist for this customer site")]
    public class GetChannelsListRequest : RequestBase, IReturn<List<TVPApiModule.Objects.Responses.Channel>>
    {
        [ApiMember(Name = "site_guid", Description = "User identifier", ParameterType = "path", DataType = SwaggerType.String, IsRequired = true)]
        public string site_guid { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/categories/{category_id}", "GET", Notes = "This method searches for all channels in a category. Category is an ordered hierarchical list of channels that belong to a similar theme or type")]
    public class GetCategoryRequest : RequestBase, IReturn<Category>
    {
        [ApiMember(Name = "category_id", Description = "Category ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int category_id { get; set; }
    }

    //Ofir combine with GetCategory
    [Route("/categories/{category_id}/full", "GET", Notes = "Category is an ordered hierarchical list of channels that belong to a similar theme or type. This method searches for a category and its dependences. When the category contains inner categories, the categories and the inner categories are returned")]
    public class GetFullCategoryRequest : RequestBase, IReturn<Category>
    {
        [ApiMember(Name = "category_id", Description = "Category ID", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = true)]
        public int category_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    #endregion

    #region PUT
    #endregion

    #region POST
    #endregion

    #region DELETE
    #endregion

}