using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class UsersResponse
    {
        public ApiObjects.Response.Status resp { get; set; }
        public List<UserResponseObject> users { get; set; }

        public UsersResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            users = new List<UserResponseObject>();
        }

        public UsersResponse(ApiObjects.Response.Status resp, List<UserResponseObject> users)
        {
            this.resp = resp;
            this.users = users;
        }
    }
}
