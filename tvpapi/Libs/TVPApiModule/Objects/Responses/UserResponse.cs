using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserResponse
    {
        [JsonProperty(PropertyName = "result")]
        public UserResult Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public UserResponse(TVPPro.SiteManager.TvinciPlatform.Users.UserResponse user)
        {
            if (user != null)
            {
                this.Status = new Responses.Status(user.resp.Code, user.resp.Message);
                this.Result = new UserResult(user);
            }
        }

        public UserResponse()
        {
        }

    }

    public class UserResult
    {
        [JsonProperty(PropertyName = "user")]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject user { get; set; }

        public UserResult(TVPPro.SiteManager.TvinciPlatform.Users.UserResponse userResponse)
        {
            this.user = userResponse.user;
        }
    }
}
