using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class UserInterestResponseList
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<UserInterest> UserInterests { get; set; }

        public UserInterestResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            UserInterests = new List<UserInterest>();
        }
    }
}