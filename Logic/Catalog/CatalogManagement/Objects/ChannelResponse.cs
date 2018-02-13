using ApiObjects.Response;
using GroupsCacheManager;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class ChannelResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public Channel Channel { get; set; }        

        public ChannelResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Channel = null;
        }
    }
}
