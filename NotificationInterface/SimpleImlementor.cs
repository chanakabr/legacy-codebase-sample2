using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using DAL;
using ODBCWrapper;
using NotificationObj;



namespace NotificationInterface
{

    /// <summary>
    /// Responsible for handling notifications with relatively small number
    /// of messgaes that sent synchronically.
    /// </summary>
    public class SimpleImlementor : BaseImplementor
    {

       #region Constructor
       public SimpleImlementor(NotificationRequest request) : base(request)
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
                        Logger.Logger.Log("SendMessages", "Notifictaion messages count==" + messagesList.Count.ToString(), SIMPLE_IMPLEMENTOR_LOG_FILE);
                        ProcessOneMessage(message);
                }
                catch(Exception ex)
                {
                    #region Logging
                    Logger.Logger.Log("SendMessages", string.Format("Exception. Excpetion Message: {0} , Notification Message Guid: {1} , Notification ID: {2} , Notification Message Request ID: {3} ,  Notification Message ID: {4} ,  User ID: {5} , Notification Message Type: {6}", ex.Message, message.ID != null ? message.ID.ToString() : "null", message.NotificationID, message.NotificationRequestID, message.NotificationMessageID, message.UserID, message.Type), SIMPLE_IMPLEMENTOR_LOG_FILE);
                    #endregion
                }                
            }
           Logger.Logger.Log("SendMessages-Before bulk insert", "Notifictaion messages count==" + messagesList.Count.ToString(), SIMPLE_IMPLEMENTOR_LOG_FILE);

           if (bIsInsertBulkOfMessagesToDB && messagesList != null && messagesList.Count > 0)
           {
               try
               {
                   Task.Factory.StartNew(() => { InsertMessagesBulkCopy(messagesList); });
               }
               catch (Exception ex)
               {
                   #region Logging
                   Logger.Logger.Log("SendMessages-bulk insert failed", "Exception=" + ex.ToString(), SIMPLE_IMPLEMENTOR_LOG_FILE);
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
