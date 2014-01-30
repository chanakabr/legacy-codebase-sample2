using System.Collections.Generic;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;

namespace RestfulTVPApi.ServiceModel
{

    #region GET

    [Route("/site/footers/{footer_id}", "GET", Notes = "This method returns the site footer.")]
    public class GetFooterRequest : RequestBase, IReturn<Menu>
    {
        [ApiMember(Name = "footer_id", Description = "Footer ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long footer_id { get; set; }
    }

    [Route("/site/pages/{page_id}", "GET", Notes = "This method returns a specific page from the site map.")]
    public class GetPageRequest : RequestBase, IReturn<PageContext>
    {
        [ApiMember(Name = "page_id", Description = "Page ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long page_id { get; set; }
        [ApiMember(Name = "with_menu", Description = "With Menu?", ParameterType = "path", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool with_menu { get; set; }
        [ApiMember(Name = "with_footer", Description = "With Footer?", ParameterType = "path", DataType = SwaggerType.Boolean, IsRequired = true)]
        public bool with_footer { get; set; }
    }

    [Route("/site/pages/{page_id}/galleries/{gallery_id}", "GET", Notes = "This method returns a specific gallery from the site map.")]
    public class GetGalleryRequest : RequestBase, IReturn<PageGallery>
    {
        [ApiMember(Name = "page_id", Description = "Page ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long page_id { get; set; }
        [ApiMember(Name = "gallery_id", Description = "Gallery ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long gallery_id { get; set; }
    }

    [Route("/site/pages/{page_id}/galleries/{gallery_id}/items", "GET", Notes = "This method returns all gallery items for a specific gallery.")]
    public class GetGalleryContentRequest : PagingRequest, IReturn<List<GalleryItem>>
    {
        [ApiMember(Name = "page_id", Description = "Page ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long page_id { get; set; }
        [ApiMember(Name = "gallery_id", Description = "Gallery ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long gallery_id { get; set; }
        [ApiMember(Name = "pic_size", Description = "Pic Size", ParameterType = "query", DataType = SwaggerType.String, IsRequired = true)]
        public string pic_size { get; set; }
    }

    [Route("/site/pages/{page_id}/galleries/{gallery_id}/items/{item_id}", "GET", Notes = "This method returns content from specific gallery items.")]
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
        [ApiAllowableValues("order_by", typeof(TVPApi.OrderBy))]
        public TVPApi.OrderBy order_by { get; set; }
    }
    
    [Route("/site/menus/{menu_id}", "GET", Notes = "This method returns the site menu.")]
    public class GetMenuRequest : RequestBase, IReturn<Menu>
    {
        [ApiMember(Name = "menu_id", Description = "Menu ID", ParameterType = "path", DataType = SwaggerType.Long, IsRequired = true)]
        public long menu_id { get; set; }
    }

    #endregion

    #region PUT
    #endregion

    #region POST
    #endregion

    #region DELETE
    #endregion

}