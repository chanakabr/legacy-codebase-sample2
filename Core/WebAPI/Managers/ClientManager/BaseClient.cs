using System;

namespace WebAPI.ClientManagers.Client
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