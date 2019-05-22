using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class EngagementAdapterSettingsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<EngagementAdapter> EngagementAdapters { get; set; }

        public EngagementAdapterSettingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            EngagementAdapters = new List<EngagementAdapter>();
        }
    }
}
