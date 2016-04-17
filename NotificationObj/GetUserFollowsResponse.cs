using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationObj
{
    public class GetUserFollowsResponse
    {
        public ApiObjects.Response.Status Status;
        public List<FollowData> Follows;
        public int TotalCount;
    }
}
