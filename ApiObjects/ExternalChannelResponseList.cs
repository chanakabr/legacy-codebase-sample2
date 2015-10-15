using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class ExternalChannelResponseList
    {
         public ApiObjects.Response.Status Status { get; set; }
         public List<ExternalChannelBase> ExternalChannels { get; set; }

        public ExternalChannelResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            ExternalChannels = new List<ExternalChannelBase>();
        }

        public ExternalChannelResponseList(ApiObjects.Response.Status status, List<ExternalChannelBase> externalChannels)
        {
            this.Status = status;
            this.ExternalChannels = externalChannels;
        }

    }
}
