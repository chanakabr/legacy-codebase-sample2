using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class UserSocialActionResponse
    {
        public List<NetworkActionStatus> networks { get; set; }

        public ApiObjects.Response.Status status { get; set; }

        public UserSocialActionResponse()
        {
            networks = new List<NetworkActionStatus>();
            status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
        }
    }

    public class NetworkActionStatus
    {
        public ApiObjects.Response.Status status { get; set; }
        public SocialPlatform? network { get; set; }
    }
}