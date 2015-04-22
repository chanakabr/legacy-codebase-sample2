using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebAPI.Clients
{
    public class ApiClient : BaseClient
    {
        public ApiClient()
        {
            // TODO: Complete member initialization
        }

        protected WebAPI.Api.API Api
        {
            get
            {
                return (Module as WebAPI.Api.API);
            }
        }
    }
}