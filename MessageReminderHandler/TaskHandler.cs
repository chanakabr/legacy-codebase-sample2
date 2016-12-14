using KLogMonitor;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;
using MessageReminderHandler.ws_notifications;
using ApiObjects.Response;
using System.Data;
using System.Reflection;
using System.ServiceModel;

namespace MessageReminderHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string result = "failure";

            try
            {
                log.DebugFormat("starting message reminder request. data={0}", data);

                MessageReminderRequest request = JsonConvert.DeserializeObject<MessageReminderRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("ws_notifications");

                if (string.IsNullOrEmpty(url))
                {
                    log.Error("did not find ws_notifications URL");
                    throw new Exception("did not find ws_notifications URL");
                }

                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.NOTIFICATIONS);

                using (NotificationServiceClient notificationsClient = new NotificationServiceClient())
                {
                    notificationsClient.Endpoint.Address = new EndpointAddress(url);
                    bool success = false;

                    success = notificationsClient.SendMessageReminder(username, password, request.StartTime, request.MessageReminderId);

                    if (!success)
                    {
                        throw new Exception(string.Format(
                            "Message reminder did not finish successfully. group: {0} start time: {1} Id: {2}", request.GroupId, request.StartTime, request.MessageReminderId));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }
    }
}
