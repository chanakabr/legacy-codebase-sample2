using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for User
/// </summary>
/// 

namespace Tvinci.Helpers.Objects
{


    [Serializable]
    public class User
    {

        public string UserName { set; get; }
        public string Password { set; get; }

        public User()
        {

        }

        public User(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
    }


}