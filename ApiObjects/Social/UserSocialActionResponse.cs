using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Social
{
    public class UserSocialActionResponse
    {
       // public List<NetworkActionStatus> Networks { get; set; }

        public ApiObjects.Response.Status Status { get; set; }

        public UserSocialActionRequest UserAction { get; set; }

        public UserSocialActionResponse(UserSocialActionRequest userAction)
        {
            //Networks = new List<NetworkActionStatus>();
            Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
            this.UserAction = userAction;
        }

        public UserSocialActionResponse()
        {
            Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
        }
    }

    //public class NetworkActionStatus
    //{
    //    public ApiObjects.Response.Status Status { get; set; }
    //    public SocialPlatform? Network { get; set; }
    //}
}