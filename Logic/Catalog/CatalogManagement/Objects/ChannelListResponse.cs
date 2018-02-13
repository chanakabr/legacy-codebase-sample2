using ApiObjects.Response;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class ChannelListResponse
    {

        public Status Status { get; set; }

        public List<Channel> Channels { get; set; }

        public ChannelListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Channels = new List<Channel>();
        }

    }
}
