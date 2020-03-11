using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class UserResponse
    {
        public TVPPro.SiteManager.TvinciPlatform.Users.ResponseStatus ResponseStatus { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
    }
}
