using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using ODBCWrapper;
using System.Web;
using ApiObjects.Notification;
using DAL;
using Newtonsoft.Json;
using KLogMonitor;
using System.Reflection;
namespace Core.Notification
{

    /// <summary>
    /// Abstract class for all types of implementors to inherit from,
    /// provides common fuctionality for all types of implementors.
    /// </summary>
    public abstract class BaseImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string BASE_IMPLEMENTOR_LOG_FILE = "BaseImplementor";
        #region Consts
        /// <summary>
        /// Define the key for fetching appname per group id from config file,
        /// Example:key="NOTIFICATION_BANDLE_APP_NAME_GROUP_ID_125" value="TvinciAppTest" 
        /// </summary>
        private const string GROUP_APP_NAME_KEY_PREFIX = "NOTIFICATION_BANDLE_APP_NAME_GROUP_ID_";
        #endregion

        #region abstract Methods
        /// <summary>
        /// Abstract declaration of sending messgaes, each inherit implementor need to implement this method specifically.
        /// </summary>
        /// <param name="messagesList"></param>
        protected abstract void SendMessages(List<NotificationMessage> messagesList, bool bIsInsertBulkOfMessagesToDB);

        //  protected abstract void SendEmailMessage(NotificationMessage emailMessage);

        /// <summary>
        /// Abstract declaration of fetching the devices for each user,
        /// it can be per user or per group.
        /// The result of this method affect directly on the number of messages to send.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        protected abstract DataTable GetUsersDevices(long groupID, long? userID, NotificationMessageType eType);
        #endregion

        #region prtotected Members
        protected NotificationRequest m_NotificationRequest;
        protected DataTable m_MessagesDataTable = null;
        #endregion


        #region Constructor
        public BaseImplementor(NotificationRequest request)
        {
            m_NotificationRequest = request;
            InitMessagesDataTable();
        }
        #endregion

        #region virtual Methods
        protected virtual long SendOneMessage(NotificationMessage message)
        {
            long messageID = 0;

            //try
            //{
            //    MessageBoxServiceWrapper messageBoxWrapper = new MessageBoxServiceWrapper();
            //    messageID = messageBoxWrapper.AddMessage(message);

            //}
            //catch (Exception ex)
            //{
            //    #region Logging
            //    #endregion
            //}
            return messageID;
        }

        protected virtual void ProcessOneMessage(NotificationMessage message)
        {

            string sResult = string.Empty;
            string sErrorMsg = string.Empty;
            string sBaseDMSAddress = Utils.GetWSURL("DMSBaseAddress");

            try
            {
                log.Debug("ProcessOneMessage - " + string.Format("Entering ProcessOneMessage try block. Message: {0} , DMS Base Address: {1}", message.ToString(), sBaseDMSAddress));

                DMSPushRequest dpr = ConvertNotificationMessageToDMSPushRequest(message);
                if (sBaseDMSAddress.Length == 0 || !TVinciShared.WS_Utils.TrySendHttpPostRequest(GetDMSAddressWithCredentials(sBaseDMSAddress), JsonConvert.SerializeObject(dpr), "application/json", Encoding.UTF8, ref sResult, ref sErrorMsg))
                {
                    throw new Exception(string.Format("DMS Address is empty or failed to send DMS the push request. Error msg: {0} Result string: {1}, DMS Address: {2}", sErrorMsg, sResult, sBaseDMSAddress));
                }
                else
                {
                    if (!string.IsNullOrEmpty(sResult))
                    {
                        DMSPushResponse resp = JsonConvert.DeserializeObject<DMSPushResponse>(sResult);
                        if (resp.Status.Trim().ToLower() == "success")
                            message.Status = NotificationMessageStatus.Successful;
                        else
                        {
                            message.Status = NotificationMessageStatus.Failed;
                            log.Debug("ProcessOneMessage - " + string.Format("Fail from DMS. Response string: {0} , Error msg: {1} , Status returned from DMS: {2} , Message returned from DMS: {3} , NotificationMessage obj: {4}", sResult, sErrorMsg, resp.Status, resp.Message, message.ToString()));
                        }
                    }
                    else
                    {
                        message.Status = NotificationMessageStatus.Failed;
                        log.Debug("ProcessOneMessage - " + string.Format("Response from DMS is null or empty. Error Msg: {0} , NotificationMessage obj: {1}", sErrorMsg, message.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ProcessOneMessage - " + string.Format("Fail. Exception: {0} , NotificationMessage obj: {1}", ex.ToString(), message.ToString()), ex);
                message.Status = NotificationMessageStatus.Failed;
            }

        }

        private string GetDMSAddressWithCredentials(string sBaseDMSAddress)
        {
            return String.Concat(sBaseDMSAddress, "?username=", Utils.GetWSURL("DMSUsername"), "&password=", Utils.GetWSURL("DMSPassword"));
        }

        private DMSPushRequest ConvertNotificationMessageToDMSPushRequest(NotificationMessage message)
        {
            DMSPushRequest res = new DMSPushRequest();
            res.message_box_id = 17; // as long as it is not 0 it's ok. messagebox module is deprecated.
            res.udid = message.UdID;
            res.app_name = message.AppName;
            res.badge = message.nBadge;
            res.messageBody = message.MessageText;
            return res;
        }


        public virtual NotificationMessage GetEmailMessage(NotificationRequest request)
        {
            NotificationMessageType messageType = request.MessageType;
            NotificationRequestAction[] actions = request.Actions;
            string appName = GetAppNameFromConfig(request.GroupID);
            NotificationMessage emailMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, request.UserID, NotificationMessageStatus.NotStarted, request.MessageText, request.Title, appName, string.Empty, 0, request.Actions, request.oExtraParams, request.GroupID);
            return emailMessage;
        }


        /// <summary>
        /// Insert all messages to the db at once using bulk operation.
        /// </summary>
        /// <param name="messagesList"></param>
        protected virtual void InsertMessagesBulkCopy(List<NotificationMessage> messagesList)
        {
            FillMessagesDataTable(messagesList);
            if (m_MessagesDataTable != null)
            {
                ODBCWrapper.InsertQuery insertMessagesBulk = new ODBCWrapper.InsertQuery();
                insertMessagesBulk.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                try
                {
                    insertMessagesBulk.InsertBulk("notifications_messages", m_MessagesDataTable);
                }
                catch (Exception ex)
                {
                    #region Logging
                    bool b = messagesList != null && messagesList.Count > 0 && messagesList[0].ID != null;
                    log.Error("InsertMessagesBulkCopy - " + string.Format("Exception. Exception msg: {0} , First message guid: {1} First message user id: {2}", ex.Message, b ? messagesList[0].ID.ToString() : "null", messagesList[0].UserID.ToString()), ex);
                    #endregion
                }
                finally
                {
                    if (insertMessagesBulk != null)
                    {
                        insertMessagesBulk.Finish();
                    }
                    insertMessagesBulk = null;
                }
                m_MessagesDataTable.Clear();
            }
        }

        public virtual void Send(List<NotificationMessage> messagesList, bool bIsInsertBulkOfMessagesToDB)
        {
            SendMessages(messagesList, bIsInsertBulkOfMessagesToDB);
        }

        public virtual void Send(List<NotificationMessage> messagesList)
        {
            Send(messagesList, true);
        }

        /// <summary>
        ///  Return list of NotificationMessage objects according to NotificationRequest.
        ///  the number of messages affected by the number of records returned from GetUsersDevices() method
        ///  which has concrete implementation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual List<NotificationMessage> GetMessages(NotificationRequest request)
        {
            List<NotificationMessage> messagesList = new List<NotificationMessage>();

            NotificationMessageType messageType = request.MessageType;
            string messageText = request.MessageText;
            NotificationRequestAction[] actions = request.Actions;
            string appName = GetAppNameFromConfig(request.GroupID);

            DataTable dtUsersDevices = GetUsersDevices(request.GroupID, (long?)request.UserID, NotificationMessageType.Push); //Concrete implementation for each implementor

            int nBadge = 0;
            if (request.MessageType == NotificationMessageType.Push)
            {
                /* if it's push notification we need to calculate badge number.
                 * badge == the number of notifications which appears on the user's application icon in his device.
                 * it is not a bug that we pass NotificationMessageType.Pull to the SP below. We need to count Pull messages
                 * because the number of push messages depends whether the user is logged in on any device or not. 
                 */
                nBadge = NotificationDal.Get_CountOfUniqueNotifications(request.UserID, (byte)NotificationMessageType.Pull, (byte)NotificationMessageViewStatus.Unread);
            }

            foreach (DataRowView dr in dtUsersDevices.DefaultView)
            {
                long userID = ODBCWrapper.Utils.GetLongSafeVal(dr["user_id"]);
                long deviceID = ODBCWrapper.Utils.GetLongSafeVal(dr["device_id"]);
                string udID = ODBCWrapper.Utils.GetSafeStr(dr["device_udid"]);
                NotificationMessage notificationMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, userID, NotificationMessageStatus.NotStarted, messageText, request.Title, appName, udID, deviceID, request.Actions, null, request.GroupID, nBadge);
                messagesList.Add(notificationMessage);
            }

            return messagesList;
        }

        public virtual string GetAppNameFromConfig(long groupID)
        {
            string groupAppNameKey = GROUP_APP_NAME_KEY_PREFIX + groupID.ToString();
            return TVinciShared.WS_Utils.GetTcmConfigValue(groupAppNameKey);
        }
        #endregion

        #region private Methods

        /// <summary>
        /// Initialize the structure of a data table that used to insert 
        /// the messages to db in a bulk way.
        /// </summary>
        private void InitMessagesDataTable()
        {
            m_MessagesDataTable = new DataTable();
            m_MessagesDataTable.Columns.Add("notification_id", typeof(long));
            m_MessagesDataTable.Columns.Add("notification_request_id", typeof(long));
            m_MessagesDataTable.Columns.Add("user_id", typeof(long));
            m_MessagesDataTable.Columns.Add("device_id", typeof(long));
            m_MessagesDataTable.Columns.Add("app_name", typeof(string));
            m_MessagesDataTable.Columns.Add("publish_date", typeof(DateTime));
            m_MessagesDataTable.Columns.Add("status", typeof(Byte));
            m_MessagesDataTable.Columns.Add("messageVia", typeof(int));
            m_MessagesDataTable.Columns.Add("message", typeof(string));

        }

        private void FillMessagesDataTable(List<NotificationMessage> messagesList)
        {
            if (messagesList != null && messagesList.Count > 0)
            {
                foreach (NotificationMessage message in messagesList)
                {
                    DataRow row = m_MessagesDataTable.NewRow();
                    row["notification_id"] = message.NotificationID;
                    row["notification_request_id"] = message.NotificationRequestID;
                    row["user_id"] = message.UserID;
                    row["device_id"] = message.DeviceID;
                    row["app_name"] = message.AppName;
                    row["publish_date"] = message.PublishDate;
                    row["status"] = message.Status;
                    row["messageVia"] = (int)NotificationMessageType.Push;
                    row["message"] = message.MessageText;

                    m_MessagesDataTable.Rows.Add(row);
                }
            }
        }
        #endregion


    }
}
