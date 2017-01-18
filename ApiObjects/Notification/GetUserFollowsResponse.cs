using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class GetUserFollowsResponse
    {
        public ApiObjects.Response.Status Status;
        public List<FollowDataBase> Follows;
        public int TotalCount;
    }
}
