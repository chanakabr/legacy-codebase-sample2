using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using DAL;
using KLogMonitor;
using ApiObjects.Notification;

namespace Core.Notification
{
    public class PullNotification : NotificationBase, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PullNotification()
            : base()
        {
        }

        public void Send(NotificationRequest request)
        {
            //Get all messages for pull , per user per device         
            List<NotificationMessage> messagesList = GetMessages(request);                  
            InsertPullMessages(messagesList);
        }

        private void InsertPullMessages(List<NotificationMessage> messagesList)
        {
            List<long> deviceIds = messagesList.Select(m => m.DeviceID).ToList<long>();
            DAL.NotificationDal.InsertMessage(messagesList[0].NotificationID,messagesList[0].NotificationRequestID,messagesList[0].UserID, deviceIds,messagesList[0].AppName,messagesList[0].PublishDate, 
                (int)NotificationMessageType.Pull ,messagesList[0].MessageText);
        }

        private List<NotificationMessage> GetMessages(NotificationRequest request)
        {
            List<NotificationMessage> messagesList = new List<NotificationMessage>();

            NotificationMessageType messageType = request.MessageType;
            string messageText = request.PullMessageText;
            NotificationRequestAction[] actions = request.Actions;
            string appName = GetAppNameFromConfig(request.GroupID);
            DataTable dtUsersDevices = UsersDal.GetDevicesToUsers(request.GroupID, (long?)request.UserID);

            NotificationMessage notificationMessage = null;
            foreach (DataRowView dr in dtUsersDevices.DefaultView)
            {
                long userID = ODBCWrapper.Utils.GetLongSafeVal(dr["user_id"]);
                long deviceID = ODBCWrapper.Utils.GetLongSafeVal(dr["device_id"]);
                string udID = ODBCWrapper.Utils.GetSafeStr(dr["device_udid"]);
                notificationMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, userID, NotificationMessageStatus.NotStarted, messageText, request.Title, appName, udID, deviceID, request.Actions, null, request.GroupID);
                messagesList.Add(notificationMessage);
            }
            // insert PC also 
            notificationMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, (long)request.UserID, NotificationMessageStatus.NotStarted, messageText, request.Title, appName, string.Empty, 0, request.Actions, null,
                request.GroupID);
            messagesList.Add(notificationMessage);

            return messagesList;
        }
    }
}
