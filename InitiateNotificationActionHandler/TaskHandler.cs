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
using InitiateNotificationAction;
using InitiateNotificationAction.ws_notifications;

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

                UserNotificationRequest request = JsonConvert.DeserializeObject<UserNotificationRequest>(data);

                string url = WS_Utils.GetTcmConfigValue("ws_notifications");
                string username = string.Empty;
                string password = string.Empty;

                TasksCommon.RemoteTasksUtils.GetCredentials(request.GroupId, ref username, ref password, ApiObjects.eWSModules.NOTIFICATION);

                NotificationServiceClient notificationsClient = new NotificationServiceClient(string.Empty, url);

                bool success = false;

                InitiateNotificationAction.ws_notifications.eUserMessageAction action = InitiateNotificationAction.ws_notifications.eUserMessageAction.IdentifyPushRegistration;

                switch (request.UserAction)
                {
                    case (int)ApiObjects.eUserMessageAction.AnonymousPushRegistration:
                        action = InitiateNotificationAction.ws_notifications.eUserMessageAction.AnonymousPushRegistration;
                        break;
                    case (int)ApiObjects.eUserMessageAction.IdentifyPushRegistration:
                        action = InitiateNotificationAction.ws_notifications.eUserMessageAction.IdentifyPushRegistration;
                        break;
                    case (int)ApiObjects.eUserMessageAction.Login:
                        action = InitiateNotificationAction.ws_notifications.eUserMessageAction.Login;
                        break;
                    case (int)ApiObjects.eUserMessageAction.Logout:
                        action = InitiateNotificationAction.ws_notifications.eUserMessageAction.Logout;
                        break;
                }

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
