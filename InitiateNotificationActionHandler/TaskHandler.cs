using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using InitiateNotificationActionHandler;
using System.ServiceModel;

namespace InitiateNotificationActionHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting User Notification request. data={0}", data);

                InitiateNotificationActionRequest request = JsonConvert.DeserializeObject<InitiateNotificationActionRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("ws_notifications");

                if (string.IsNullOrEmpty(url))
                {
                    log.Error("did not find ws_notifications URL");
                    throw new Exception("did not find ws_notifications URL");
                }

                bool success = false;

                eUserMessageAction action = (eUserMessageAction)request.UserAction;
                success = Core.Notification.Module.InitiateNotificationAction(request.GroupId, action, request.UserId, request.Udid, request.pushToken);

                if (!success)
                    throw new Exception(string.Format("Announcement did not finish successfully. data: {0}", data));
                else
                    result = "success";
            }
            catch (Exception ex)
            {
                log.Error("Announcement did not finish successfully. Exception occured. data:" + data, ex);
                throw ex;
            }

            return result;
        }
    }
}
