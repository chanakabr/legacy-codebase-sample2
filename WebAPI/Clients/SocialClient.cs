using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers.Client;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class SocialClient : BaseClient
    {
        public SocialClient()
        {
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