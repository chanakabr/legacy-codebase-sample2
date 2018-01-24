using ApiObjects.Response;
using GroupsCacheManager;
using System.Collections.Generic;

namespace Core.Catalog.CatalogManagement
{
    public class ChannelsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<Channel> Channels { get; set; }

        public int TotalItems { get; set; }

        public ChannelsResponse()
        {
            Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Channels = new List<Channel>();
            TotalItems = 0;
        }
    }
}
