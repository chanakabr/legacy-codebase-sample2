using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPWebApi.Models
{
    public class User
    {
        public UserData user_data { get; set; }
        public string password { get; set; }
        public string affiliate_code { get; set; }
    }
}