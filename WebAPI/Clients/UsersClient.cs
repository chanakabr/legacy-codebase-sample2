using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients.Utils;

namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        //private readonly ILog logger = LogManager.GetLogger(typeof(UsersClient));

        public UsersClient()
        {
            // TODO: Complete member initialization
        }
       
        protected WebAPI.Users.UsersService Users
        {
            get
            {
                return (Module as WebAPI.Users.UsersService);
            }
        }
    }
}