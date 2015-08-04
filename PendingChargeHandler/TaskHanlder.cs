using Newtonsoft.Json;
using PendingChargeHandler.WS_CAS;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PendingChargeHandler
{
    public class TaskHanlder : ITaskHandler
    {
        #region ITaskHandler Members

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                Logger.Logger.Log("Info", string.Concat("starting pending charge request. data=", data), "PendingChargeHandler");

                PendingChargeRequest request = JsonConvert.DeserializeObject<PendingChargeRequest>(data);

                string url = TCMClient.Settings.Instance.GetValue<string>("WS_CAS");
                string username = string.Empty;
                string password = string.Empty;

                TVinciShared.WS_Utils.GetWSUNPass(request.GroupID, "ActionName", "cas", "1.1.1.1", ref username, ref password);
                module cas = new module();

                if (!string.IsNullOrEmpty(url))
                {
                    cas.Url = url;
                }

                bool success = false;
                //cas.DoSomething();

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
