using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Clients
{
    public class DomainsClient : BaseClient
    {
        public DomainsClient()
        { }

        #region Properties

        protected WebAPI.Domains.module Domains
        {
            get
            {
                return (Module as WebAPI.Domains.module);
            }
        }

        #endregion

    }
}