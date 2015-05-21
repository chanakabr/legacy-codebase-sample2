using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SignInResponse
    {
        [JsonProperty(PropertyName = "result")]
        public UserSignIn Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public SignInResponse(TVPPro.SiteManager.TvinciPlatform.Users.SignInResponse signIn)
        {
            if (signIn != null)
            {
                this.Status = new Responses.Status(signIn.resp.Code, signIn.resp.Message);
                this.Result = new UserSignIn(signIn);
            }
        }

        public SignInResponse()
        {
        }

    }

    public class UserSignIn
    {
        [JsonProperty(PropertyName = "user")]
        public TVPPro.SiteManager.TvinciPlatform.Users.UserResponseObject user { get; set; }

        public UserSignIn(TVPPro.SiteManager.TvinciPlatform.Users.SignInResponse signIn)
        {
            this.user = signIn.user;
        }
    }
}
