using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class SignInResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public UserResponseObject user { get; set; }

        public SignInResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.InternalError, string.Empty);
            user = new UserResponseObject();
        }

        public SignInResponse(ApiObjects.Response.Status resp, UserResponseObject user)
        {
            this.resp = resp;
            this.user = user;
        }

    }
}
