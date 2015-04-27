using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients.Utils;

namespace WebAPI.Clients
{
    public class SocialClient : BaseClient
    {
        public SocialClient()
        {
            // TODO: Complete member initialization
        }

        protected WebAPI.Social.module Social
        {
            get
            {
                return (Module as WebAPI.Social.module);
            }
        }
    }
}