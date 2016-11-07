using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

using System.Linq;
using Ingest.Clients.ClientManager;
using WS_API;

namespace Ingest.Clients
{
    public class ApiClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ApiClient()
        {
        }

        protected API Api
        {
            get
            {
                return (Module as API);
            }
        }

        internal int GetGroupIdByUsernamePassword(string username, string password)
        {
            int groupId = 0;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    groupId = Api.GetGroupIdByUsernamePassword(username, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
            }

            return groupId;
        }

        internal bool UpdateFreeFileTypeOfModule(int groupID, int moduleID)
        {
            bool result = false;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    result = Api.UpdateFreeFileTypeOfModule(groupID, moduleID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling api service. exception: {1}", ex);
            }

            return result;
        }

    }
}