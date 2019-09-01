using ApiObjects.Response;

namespace Core.Users
{
    public class UserResponse1
    {
        public ApiObjects.Response.Status resp { get; set; }
        public UserResponseObject user { get; set; }

        public UserResponse1()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            user = new UserResponseObject();
        }

        public UserResponse1(ApiObjects.Response.Status resp, UserResponseObject user)
        {
            this.resp = resp;
            this.user = user;
        }
    }
}
