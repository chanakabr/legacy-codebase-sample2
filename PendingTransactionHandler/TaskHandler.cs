using Newtonsoft.Json;
using PendingTransactionHandler.WS_CAS;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvinciCache;
using TVinciShared;
using ApiObjects;

namespace PendingTransactionHandler
{
    public class TaskHandler : ITaskHandler
    {
        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting pending charge request. data=", data), "PendingTransactionHandler");

                PendingTransactionRequest request = JsonConvert.DeserializeObject<PendingTransactionRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                bool success = false;
                //cas.DoAction(username, password, "");

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Pending charge request on {0} did not finish successfully.", request.ID));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        #endregion
    }
}
