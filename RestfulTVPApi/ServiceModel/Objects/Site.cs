using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using RestfulTVPApi.Objects.Responses;

namespace RestfulTVPApi.ServiceModel
{

    #region GET

    //[Route("/site/pages/{page_id}/galleries/{gallery_id}/items/{item_id}", "GET", Notes = "This method returns content from specific gallery items.")]
    public class GetGalleryItemContentRequest : PagingRequest, IReturn<List<Media>>
    {
        [ApiMember(Name = "page_id", Description = "Page ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long page_id { get; set; }
        [ApiMember(Name = "gallery_id", Description = "Gallery ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long gallery_id { get; set; }
        [ApiMember(Name = "item_id", Description = "Item ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long item_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
        [ApiMember(Name = "order_by", Description = "Order By", ParameterType = "query", DataType = "OrderBy", IsRequired = false)]
        [ApiAllowableValues("order_by", typeof(RestfulTVPApi.Objects.Enums.OrderBy))]
        public RestfulTVPApi.Objects.Enums.OrderBy order_by { get; set; }
    }
    
    #endregion

    #region PUT
    #endregion

    #region POST
    #endregion

    #region DELETE
    #endregion

}