using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class TopicListResponse
    {

        public Status Status { get; set; }

        public List<Topic> Topics { get; set; }

        public TopicListResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Topics = new List<Topic>();
        }

    }
}
