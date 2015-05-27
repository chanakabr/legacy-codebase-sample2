using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class LoginResponse
    {
        [JsonProperty(PropertyName = "result")]
        public UserLogIn Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public LoginResponse(TVPPro.SiteManager.TvinciPlatform.Users.LoginResponse logIn)
        {
            if (logIn != null)
            {
                this.Status = new Responses.Status(logIn.resp.Code, logIn.resp.Message);
                this.Result = new UserLogIn(logIn);
            }
        }

        public LoginResponse()
        {
        }

    }

    public class UserLogIn
    {
        [JsonProperty(PropertyName = "user")]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject user { get; set; }

        public UserLogIn(TVPPro.SiteManager.TvinciPlatform.Users.LoginResponse logIn)
        {
            this.user = logIn.user;
        }
    }
}
