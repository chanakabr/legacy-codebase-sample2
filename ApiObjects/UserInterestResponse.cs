using ApiObjects.Response;

namespace ApiObjects
{
    public class UserInterestResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public UserInterest UserInterest { get; set; }

        public UserInterestResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            UserInterest = new UserInterest();
        }
    }
}