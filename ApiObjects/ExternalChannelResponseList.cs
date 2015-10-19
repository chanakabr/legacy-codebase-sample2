using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class ExternalChannelResponseList
    {
         public ApiObjects.Response.Status Status { get; set; }
         public List<ExternalChannel> ExternalChannels { get; set; }

        public ExternalChannelResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            ExternalChannels = new List<ExternalChannel>();
        }

        public ExternalChannelResponseList(ApiObjects.Response.Status status, List<ExternalChannel> externalChannels)
        {
            this.Status = status;
            this.ExternalChannels = externalChannels;
        }

    }
}
