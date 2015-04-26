using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Clients
{
    public class ConditionalAccessClient : BaseClient
    {
        public ConditionalAccessClient()
        {

        }

        #region Properties

        protected WebAPI.ConditionalAccess.module ConditionalAccess
        {
            get
            {
                return (Module as WebAPI.ConditionalAccess.module);
            }
        }

        #endregion
    }
}