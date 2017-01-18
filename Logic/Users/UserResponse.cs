using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class UserResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public UserResponseObject user { get; set; }

        public UserResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            user = new UserResponseObject();
        }

        public UserResponse(ApiObjects.Response.Status resp, UserResponseObject user)
        {
            this.resp = resp;
            this.user = user;
        }
    }
}
