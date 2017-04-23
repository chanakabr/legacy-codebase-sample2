using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class EngagementResponseList 
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<Engagement> Engagements { get; set; }

        public EngagementResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            Engagements = new List<Engagement>();
        }
    }
}