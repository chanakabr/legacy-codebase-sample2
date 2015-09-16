using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using SubscriptionRenewHandler.WS_CAS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;

namespace SubscriptionRenewHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.InfoFormat("Renew subscription request. data={0}", data);

                SubscriptionRenewRequest request = JsonConvert.DeserializeObject<SubscriptionRenewRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupID, ref username, ref password, ApiObjects.eWSModules.CONDITIONALACCESS);

                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                bool success = cas.Rewnew(username, password, request.SiteGuid, request.PurchaseId);

                if (!success)
                {
                    throw new Exception(string.Format(
                        "Renew subscription request on purchase id = {0} and site guid = {1} did not finish successfully.", request.PurchaseId, request.SiteGuid));
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
