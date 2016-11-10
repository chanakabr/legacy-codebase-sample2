using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class UserSocialActionResponse
    {
        public List<NetworkActionStatus> Networks { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public UserSocialActionResponse()
        {
            Networks = new List<NetworkActionStatus>();
            Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
        }
    }

    public class NetworkActionStatus
    {
        public ApiObjects.Response.Status Status { get; set; }
        public SocialPlatform? Network { get; set; }
    }
}