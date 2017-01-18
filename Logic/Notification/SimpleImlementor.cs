using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using DAL;
using ODBCWrapper;
using ApiObjects.Notification;
using KLogMonitor;
using System.Reflection;



namespace Core.Notification
{

    /// <summary>
    /// Responsible for handling notifications with relatively small number
    /// of messgaes that sent synchronically.
    /// </summary>
    public class SimpleImlementor : BaseImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constructor
        public SimpleImlementor(NotificationRequest request)
            : base(request)
        {

        }
        #endregion

        private const string SIMPLE_IMPLEMENTOR_LOG_FILE = "SimpleImplementor";
        #region protected methods
        /// <summary>
        /// Send the messages in the messagesList param, 
        /// the messages are sent synchronically and afterwards saved to db in a bulk operation.
        /// </summary>
        /// <param name="messagesList"></param>

        protected override void SendMessages(List<NotificationMessage> messagesList, bool bIsInsertBulkOfMessagesToDB)
        {
            foreach (NotificationMessage message in messagesList)
            {
                try
                {
                    log.Debug("SendMessages - Notification messages count==" + messagesList.Count.ToString());
                    ProcessOneMessage(message);
                }
                catch (Exception ex)
                {
                    #region Logging
                    log.Error("SendMessages - " + string.Format("Exception. Exception Message: {0} , Notification Message Guid: {1} , Notification ID: {2} , Notification Message Request ID: {3} ,  Notification Message ID: {4} ,  User ID: {5} , Notification Message Type: {6}", ex.Message, message.ID != null ? message.ID.ToString() : "null", message.NotificationID, message.NotificationRequestID, message.NotificationMessageID, message.UserID, message.Type), ex);
                    #endregion
                }
            }
            log.Debug("SendMessages-Before bulk insert - notification messages count==" + messagesList.Count.ToString());

            if (bIsInsertBulkOfMessagesToDB && messagesList != null && messagesList.Count > 0)
            {
                try
                {
                    Task.Factory.StartNew(() => { InsertMessagesBulkCopy(messagesList); });
                }
                catch (Exception ex)
                {
                    #region Logging
                    log.Error("SendMessages-bulk insert failed - Exception=" + ex.ToString(), ex);
                    #endregion
                }
            }
        }

        /// <summary>
        /// Concrete implementation of the GetUsersDevices() method,
        /// here the users devices is fetched by user id.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        protected override DataTable GetUsersDevices(long groupID, long? userID, NotificationMessageType eType)
        {
            DataTable res = null;
            switch (eType)
            {
                case NotificationMessageType.Push:
                    {
                        // unlike other notifications, push notifications should be sent only to devices the user is currently using,
                        // hence we use a different SP for that.
                        res = UsersDal.GetDevicesToUsers(groupID, userID, true);
                        break;
                    }
                default:
                    {
                        res = UsersDal.GetDevicesToUsers(groupID, userID);
                        break;
                    }
            }

            return res;
        }
        #endregion
    }
}
