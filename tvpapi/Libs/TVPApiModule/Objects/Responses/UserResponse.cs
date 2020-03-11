using ApiObjects.Response;
using Core.Users;
using Newtonsoft.Json;

namespace TVPApiModule.Objects.Responses
{
    public class UserResponse
    {
        [JsonProperty(PropertyName = "result")]
        public UserResult Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public UserResponse(Core.Users.UserResponse user)
        {
            if (user != null)
            {
                this.Status = new Responses.Status(user.resp.Code, user.resp.Message);
                this.Result = new UserResult(user);
            }
        }

        public UserResponse(GenericResponse<UserResponseObject> user)
        {
            if (user != null)
            {
                this.Status = new Status(user.Status.Code, user.Status.Message);
                this.Result = new UserResult(user.Object);
            }
        }

        public UserResponse()
        {
        }

    }

    public class UserResult
    {
        [JsonProperty(PropertyName = "user")]
        public Core.Users.UserResponseObject user { get; set; }

        public UserResult(Core.Users.UserResponse userResponse)
        {
            this.user = userResponse.user;
        }

        public UserResult(UserResponseObject User)
        {
            this.user = User;
        }
    }
}
