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

        #region ITaskHandler Members

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
                {
                    throw new Exception(string.Format(
                        "Announcement did not finish successfully. group: {0} user id: {1} Udid: {2}, push token: {3}", request.GroupId, request.UserId, request.Udid, request.pushToken));
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
