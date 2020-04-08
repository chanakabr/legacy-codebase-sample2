using System.Collections.Generic;

namespace ApiLogic.Users
{
    public class SSOAdapterProfileInvokeResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<ApiObjects.KeyValuePair> Response { get; set; }
    }
}
