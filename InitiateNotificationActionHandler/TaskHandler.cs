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
using InitiateNotificationActionHandler.ws_notifications;

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
                log.InfoFormat("starting User Notification request. data={0}", data);

                InitiateNotificationActionRequest request = JsonConvert.DeserializeObject<InitiateNotificationActionRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("ws_notifications");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.NOTIFICATION);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(url))
                    throw new Exception(string.Format(
                        "Notifications service - invalid Credentials or URL: Group: {0}, username: {1}, password: {2}, URL: {3}", request.GroupId, username,password,url));

                NotificationServiceClient notificationsClient = new NotificationServiceClient(string.Empty, url);

                bool success = false;

                eUserMessageAction action = (eUserMessageAction)request.UserAction;

                success = notificationsClient.InitiateNotificationAction(username, password, action, request.UserId, request.Udid, request.pushToken);

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
