using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingest.Clients.ClientManager
{
    public class BaseClient
    {
        #region Private

        private Object lockObject = new Object();

        #endregion

        #region Properties

        public object Module { get; set; }

        public string WSUserName { get; set; }

        public string WSPassword { get; set; }

        public ClientType ClientType { get; set; }

        #endregion

    }
}