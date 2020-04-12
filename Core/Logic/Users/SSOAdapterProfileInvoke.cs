using System.Collections.Generic;

namespace ApiLogic.Users
{
    public class SSOAdapterProfileInvoke
    {
        public ApiObjects.Response.Status Status { get; set; }
        public Dictionary<string, string> AdapterData { get; set; }
    }
}
