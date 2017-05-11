using ApiObjects;
using ApiObjects.Notification;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;

namespace DAL
{
    /// <summary>
    /// Handle db operations against MessageBox db.
    /// </summary>
    public class NotificationDal : BaseDal
    {
        private const string SP_INSERT_NOTIFICATION_REQUEST = "InsertNotifictaionRequest";
        private const string SP_GET_NOTIFICATION_REQUESTS = "GetNotifictaionRequests";
        private const string SP_UPDATE_NOTIFICATION_REQUEST_STATUS = "UpdateNotificationRequestStatus";
        private const string SP_GET_NOTIFICATION_BY_GROUP_AND_TRIGGER_TYPE = "GetNotifictaionByGroupAndTriggerType";
        private const string SP_IS_NOTIFICATION_EXIST = "IsNotifictaionExist";
        private const string SP_GET_DEVICE_NOTIFICATION = "GetDeviceNotification";
        private const string SP_UPDATE_NOTIFICATION_MESSAGE_VIEW_STATUS = "UpdateNotificationMessageViewStatus";
        private const int NUM_OF_INSERT_TRIES = 10;
        private const int SLEEP_BETWEEN_RETRIES_MILLI = 1000;
        private const string CB_DESIGN_DOC_NOTIFICATION = "notification";
        private const string CB_DESIGN_DOC_INBOX = "inbox";
        private const string TVINCI_CONNECTION = "MAIN_CONNECTION_STRING";
        private const string MESSAGE_BOX_CONNECTION = "MESSAGE_BOX_CONNECTION_STRING";

        private const int MAX_NUMBER_OF_PUSH_MESSAGES_PER_USER_IN_AN_HOUR = 4;
        private const int TTL_USER_PUSH_COUNTER_DOCUMENT_SECONDS = 3600;


        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.NOTIFICATION);

        private static string GetDeviceDataKey(int groupId, string udid)
        {
            return string.Format("device_data_{0}_{1}", groupId, udid);
        }

        private static string GetUserNotificationKey(int groupId, int userId)
        {
            return string.Format("user_notification_{0}_{1}", groupId, userId);
        }

        private static string GetUserFollowsNotificationKey(int groupId, long userId, long notificationId)
        {
            return string.Format("user_notification_item:{0}:{1}:{2}", groupId, userId, notificationId);
        }

        private static string GetInboxMessageKey(int groupId, long userId, string messageId)
        {
            return string.Format("inbox_message:{0}:{1}:{2}", groupId, userId, messageId);
        }

        private static string GetUserPushKey(int groupId, long userId)
        {
            return string.Format("user_push:{0}:{1}", groupId, userId);
        }

        private static string GetInboxSystemAnnouncementKey(int groupId, string messageId)
        {
            return string.Format("system_inbox:{0}:{1}", groupId, messageId);
        }

        private static string GetNotificationCleanupKey()
        {
            return "notification_cleanup";
        }

        /// <summary>
        /// Insert one notification request to notifications_requests table
        /// by calling InsertNotifictaionRequest stored procedure.
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="notificationID"></param>
        /// <param name="status"></param>
        public static void InsertNotificationRequest(int requestType, long groupID, long userID, long notificationID, byte status, int triggerType)
        {
            ODBCWrapper.StoredProcedure spInsertNotificationRequest = new ODBCWrapper.StoredProcedure(SP_INSERT_NOTIFICATION_REQUEST);
            spInsertNotificationRequest.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");

            spInsertNotificationRequest.AddParameter("@notification_request_type", requestType);
            spInsertNotificationRequest.AddParameter("@group_id", groupID);
            spInsertNotificationRequest.AddParameter("@user_id", userID);
            spInsertNotificationRequest.AddParameter("@notification_id", notificationID);
            spInsertNotificationRequest.AddParameter("@triggerType", triggerType);
            spInsertNotificationRequest.AddParameter("@status", status);

            DataTable dt = spInsertNotificationRequest.Execute();
        }

        /// <summary>
        /// Returns dataset of notification requests and their actions 
        /// by calling GetNotifictaionRequests stored procedure.
        /// this stored procedure returns 2 resultsets(requests ans actions)
        /// represened by 2 datatable objects in the returned dataset.
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="notificationID"></param>
        /// <param name="status"></param>
        public static DataSet GetNotificationRequests(int? topRowsNumber, long groupID, long? notificationIRequestID, byte? status)
        {
            ODBCWrapper.StoredProcedure spGetNotificationRequests = new ODBCWrapper.StoredProcedure(SP_GET_NOTIFICATION_REQUESTS);
            spGetNotificationRequests.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");

            spGetNotificationRequests.AddNullableParameter<int?>("@topRowsNumber", topRowsNumber);
            spGetNotificationRequests.AddParameter("@groupID", groupID);
            spGetNotificationRequests.AddNullableParameter<long?>("@notificationRequestID", notificationIRequestID);
            spGetNotificationRequests.AddNullableParameter<byte?>("@status", status);

            DataSet ds = spGetNotificationRequests.ExecuteDataSet();
            return ds;
        }

        /// <summary>
        /// Update status of several notification requests at notifications_requests table
        /// by calling UpdateNotificationRequestStatus stored procedure. 
        /// </summary>
        /// <param name="requestsIDsList"></param>
        /// <param name="status"></param>
        public static void UpdateNotificationRequestStatus(List<long> requestsIDsList, byte status)
        {
            ODBCWrapper.StoredProcedure spUpdateNotificationRequestStatus = new ODBCWrapper.StoredProcedure(SP_UPDATE_NOTIFICATION_REQUEST_STATUS);
            spUpdateNotificationRequestStatus.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spUpdateNotificationRequestStatus.AddIDListParameter<long>("@requestsIDs", requestsIDsList, "id");
            spUpdateNotificationRequestStatus.AddParameter("@status", status);
            DataTable dt = spUpdateNotificationRequestStatus.Execute();
        }

        /// <summary>
        /// Calls IsNotifictaionExist stored procedure to check if a notification
        /// exists in notifications table according to group id and trigger type.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        public static bool IsNotificationExist(long groupID, int triggerType)
        {
            ODBCWrapper.StoredProcedure spIsNotificationExist = new ODBCWrapper.StoredProcedure(SP_IS_NOTIFICATION_EXIST);
            spIsNotificationExist.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spIsNotificationExist.AddParameter("@groupID", groupID);
            spIsNotificationExist.AddParameter("@triggerType", triggerType);
            bool result = spIsNotificationExist.ExecuteReturnValue<bool>();
            return result;
        }

        /// <summary>
        /// Returns datatable of notifications from notifications table
        /// by calling GetNotifictaionByGroupAndTriggerType stored procedure. 
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        public static DataTable GetNotifictaionByGroupAndTriggerType(long groupID, int triggerType)
        {
            ODBCWrapper.StoredProcedure spGetNotification = new ODBCWrapper.StoredProcedure(SP_GET_NOTIFICATION_BY_GROUP_AND_TRIGGER_TYPE);
            spGetNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetNotification.AddParameter("@groupID", groupID);
            spGetNotification.AddParameter("@triggerType", triggerType);
            DataSet ds = spGetNotification.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetDeviceNotification(long groupID, long userID, long deviceID, byte? view_status, int? topRowNumber, int? notificationType)
        {
            ODBCWrapper.StoredProcedure spGetDeviceNotification = new ODBCWrapper.StoredProcedure(SP_GET_DEVICE_NOTIFICATION);
            spGetDeviceNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetDeviceNotification.AddParameter("@groupID", groupID);
            spGetDeviceNotification.AddParameter("@userID", userID);
            spGetDeviceNotification.AddParameter("@deviceID", deviceID);
            spGetDeviceNotification.AddNullableParameter<byte?>("@viewStatus", view_status);
            spGetDeviceNotification.AddNullableParameter<int?>("@topRowNumber", topRowNumber);
            spGetDeviceNotification.AddParameter("@notificationType", notificationType);

            DataSet ds = spGetDeviceNotification.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;

        }

        public static bool UpdateNotificationMessageViewStatus(long userID, long? notificationIRequestID, long? notificationMessageID, byte? view_status)
        {
            ODBCWrapper.StoredProcedure spUpdateNotificationMessageStatus = new ODBCWrapper.StoredProcedure(SP_UPDATE_NOTIFICATION_MESSAGE_VIEW_STATUS);
            spUpdateNotificationMessageStatus.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spUpdateNotificationMessageStatus.AddParameter("@userID", userID);
            spUpdateNotificationMessageStatus.AddNullableParameter<long?>("@notificationRequestID", notificationIRequestID);
            spUpdateNotificationMessageStatus.AddNullableParameter<long?>("@notificationMessageID", notificationMessageID);
            spUpdateNotificationMessageStatus.AddParameter("@view_status", view_status);
            bool result = spUpdateNotificationMessageStatus.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool InsertUserNotification(long userID, Dictionary<int, List<int>> tagIDs, int groupID)
        {
            ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("InsertUserNotification");
            spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertUserNotification.AddParameter("@user_id", userID);
            spInsertUserNotification.AddKeyValueListParameter<int, int>("@TagValuesIDs", tagIDs, "key", "value");
            spInsertUserNotification.AddParameter("@group_id", groupID);


            bool result = spInsertUserNotification.ExecuteReturnValue<bool>();

            return result;
        }

        public static bool UpdateUserNotification(long userID, Dictionary<int, List<int>> tagIDs, int groupID, int status)
        {
            ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("UpdateUserNotification");
            spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertUserNotification.AddParameter("@user_id", userID);
            spInsertUserNotification.AddKeyValueListParameter<int, int>("@TagValuesIDs", tagIDs, "key", "value");
            spInsertUserNotification.AddParameter("@group_id", groupID);
            spInsertUserNotification.AddParameter("@status", status);

            bool result = spInsertUserNotification.ExecuteReturnValue<bool>();

            return result;
        }

        //Insert notification tag request with List of users    
        public static void InsertNotificationTagRequest(int requestType, long groupID, long notificationID, int status, List<long> usersID, string sExtraParams, int triggerType, int mediaID)
        {
            ODBCWrapper.StoredProcedure spTagNotification = new ODBCWrapper.StoredProcedure("InsertNotificationTagRequest");
            spTagNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spTagNotification.AddParameter("@notification_request_type", requestType);
            spTagNotification.AddParameter("@group_id", groupID);
            spTagNotification.AddParameter("@notification_id", notificationID);
            spTagNotification.AddParameter("@status", status);
            spTagNotification.AddParameter("@ExtraParams", sExtraParams);
            spTagNotification.AddIDListParameter<long>("@users", usersID, "id");
            spTagNotification.AddParameter("@triggerType", triggerType);
            spTagNotification.AddParameter("@mediaID", mediaID);

            bool result = spTagNotification.ExecuteReturnValue<bool>();
        }

        public static void InsertMessage(long notificationId, long notificationRequestId, long userId, List<long> deviceIds, string appName, DateTime publishDate, int notificationMessageType, string message)
        {
            ODBCWrapper.StoredProcedure spInsertMessage = new ODBCWrapper.StoredProcedure("InsertMessage");
            spInsertMessage.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertMessage.AddParameter("@notificationId", notificationId);
            spInsertMessage.AddParameter("@notificationRequestId", notificationRequestId);
            spInsertMessage.AddParameter("@userId", userId);
            spInsertMessage.AddParameter("@appName", appName);
            spInsertMessage.AddParameter("@publishDate", publishDate);
            spInsertMessage.AddParameter("@messageVia", notificationMessageType);
            spInsertMessage.AddParameter("@message", message);
            spInsertMessage.AddIDListParameter<long>("@deviceIds", deviceIds, "Id");

            bool resault = spInsertMessage.ExecuteReturnValue<bool>();
        }

        ///Get All Tags + Tags Values related to mediaID 
        ///
        public static DataTable GetTagsNotificationByMedia(int mediaID, int? useCreateDate)
        {
            ODBCWrapper.StoredProcedure spGetTags = new ODBCWrapper.StoredProcedure("GetTagsNotificationByMedia");
            spGetTags.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetTags.AddParameter("@MediaID", mediaID);
            if (useCreateDate == null)
                useCreateDate = 0;
            spGetTags.AddParameter("@useCreateDate", useCreateDate);

            DataSet ds = spGetTags.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetNotifictaionsByTags(int nGroupID, int TriggerType, Dictionary<int, List<int>> tagDict)
        {
            ODBCWrapper.StoredProcedure spGetNotifications = new ODBCWrapper.StoredProcedure("GetNotifictaionsByTags");
            spGetNotifications.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetNotifications.AddKeyValueListParameter<int, int>("@TagValuesIDs", tagDict, "key", "value");
            spGetNotifications.AddParameter("@GroupID", nGroupID);
            spGetNotifications.AddParameter("@triggerType", TriggerType);


            DataSet ds = spGetNotifications.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetTagsIDsByName(int groupID, Dictionary<string, List<string>> tags)
        {
            ODBCWrapper.StoredProcedure spGetTagsNameByValues = new ODBCWrapper.StoredProcedure("GetTagsIDsByName");
            spGetTagsNameByValues.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetTagsNameByValues.AddKeyValueListParameter<string, string>("@TagValuesIDs", tags, "key", "value");
            spGetTagsNameByValues.AddParameter("@GroupID", groupID);

            DataSet ds = spGetTagsNameByValues.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetTagsNameByIDs(int groupID, Dictionary<int, List<int>> tags)
        {
            ODBCWrapper.StoredProcedure sGetTagsNameByIDs = new ODBCWrapper.StoredProcedure("GetTagsNameByIDs");
            sGetTagsNameByIDs.SetConnectionKey("MAIN_CONNECTION_STRING");
            sGetTagsNameByIDs.AddKeyValueListParameter<int, int>("@TagValues", tags, "sKey", "value");
            sGetTagsNameByIDs.AddParameter("@GroupID", groupID);

            DataSet ds = sGetTagsNameByIDs.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetTagsNameByIDs(int groupID, Dictionary<string, List<int>> tags)
        {
            ODBCWrapper.StoredProcedure sGetTagsNameByIDs = new ODBCWrapper.StoredProcedure("GetTagsNameByIDs");
            sGetTagsNameByIDs.SetConnectionKey("MAIN_CONNECTION_STRING");
            sGetTagsNameByIDs.AddKeyValueListParameter<string, int>("@TagValues", tags, "sKey", "value");
            sGetTagsNameByIDs.AddParameter("@GroupID", groupID);

            DataSet ds = sGetTagsNameByIDs.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetUserNotification(List<long> lUsers)
        {
            return GetUserNotification(null, new List<long>(), 2, lUsers);
        }

        public static DataTable GetUserNotification(int? userID, List<long> notificationIds)
        {
            return GetUserNotification(userID, notificationIds, null, new List<long>());
        }

        public static DataTable GetUserNotification(int? userID, List<long> notificationIds, int byUserID)
        {
            return GetUserNotification(userID, notificationIds, byUserID, new List<long>());
        }

        public static DataTable GetUserNotification(int? userID, List<long> notificationIds, int? byUserID, List<long> lUsers)
        {
            ODBCWrapper.StoredProcedure spGetUsers = new ODBCWrapper.StoredProcedure("GetUserNotificationXML");
            spGetUsers.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetUsers.AddXMLParameter<long>("@notificationIDs", notificationIds, "Id");
            spGetUsers.AddParameter("@user_id", userID);
            if (byUserID == null)
                byUserID = 0;
            spGetUsers.AddParameter("@ByUserID", byUserID);
            spGetUsers.AddXMLParameter<long>("@listUsers", lUsers, "Id");

            DataSet ds = spGetUsers.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;

        }

        //return if there is a notification with those values 
        public static bool IsTagNotificationExists(int groupID, int triggerType, string sKey, string sValue)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsTagNotificationExists");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@sKey", sKey);
            sp.AddParameter("@sValue", sValue);
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@triggerType", triggerType);

            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataTable GetAllNotificationUserSignIn(List<long> lUserIds, int notificationTriggerType, int groupID)
        {
            ODBCWrapper.StoredProcedure spGetUsersTags = new ODBCWrapper.StoredProcedure("GetAllNotificationUserSignIn");
            spGetUsersTags.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetUsersTags.AddIDListParameter<long>("@usersIDs", lUserIds, "id");
            spGetUsersTags.AddParameter("@GroupID", groupID);
            spGetUsersTags.AddParameter("@triggerType", notificationTriggerType);

            DataSet ds = spGetUsersTags.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetAllNotification(int nGroupID, int notificationTriggerType)
        {
            ODBCWrapper.StoredProcedure spGetNotifications = new ODBCWrapper.StoredProcedure("GetAllNotifications");
            spGetNotifications.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetNotifications.AddParameter("@GroupID", nGroupID);
            spGetNotifications.AddParameter("@triggerType", notificationTriggerType);

            DataSet ds = spGetNotifications.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable GetRegularChildGroupsStr(int groupID)
        {
            ODBCWrapper.StoredProcedure spGetUsersTags = new ODBCWrapper.StoredProcedure("GetRegularChildGroupsStr");
            spGetUsersTags.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUsersTags.AddParameter("@GroupID", groupID);

            DataSet ds = spGetUsersTags.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        #region Email Tag Notification

        public static bool IsMediaNeedToBeNotify(int mediaID)
        {
            ODBCWrapper.StoredProcedure spIsMedia = new ODBCWrapper.StoredProcedure("IsMediaNeedToBeNotify");
            spIsMedia.SetConnectionKey("MAIN_CONNECTION_STRING");
            spIsMedia.AddParameter("@mediaID", mediaID);
            bool result = spIsMedia.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataSet GetMediaForEmail(int mediaID)
        {

            ODBCWrapper.StoredProcedure spPicProtocol = new ODBCWrapper.StoredProcedure("GetMediaForEmail");
            spPicProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPicProtocol.AddParameter("@MediaID", mediaID);

            DataSet ds = spPicProtocol.ExecuteDataSet();
            return ds;
        }

        #endregion

        public static DataTable Get_MetasByGroup(int group_id)
        {
            ODBCWrapper.StoredProcedure spPicProtocol = new ODBCWrapper.StoredProcedure("Get_MetasByGroup");
            spPicProtocol.SetConnectionKey("MAIN_CONNECTION_STRING");
            spPicProtocol.AddParameter("@GroupId", group_id);

            DataSet ds = spPicProtocol.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetGroupOperator(long group_id, int triggerType)
        {
            ODBCWrapper.StoredProcedure spGroupOperator = new ODBCWrapper.StoredProcedure("GetGroupOperator");
            spGroupOperator.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGroupOperator.AddParameter("@GroupId", group_id);
            spGroupOperator.AddParameter("@TriggerType", triggerType);
            DataSet ds = spGroupOperator.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetFactory(long groupId)
        {
            ODBCWrapper.StoredProcedure spGetUserPhoneNumber = new ODBCWrapper.StoredProcedure("GetFactory");
            spGetUserPhoneNumber.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetUserPhoneNumber.AddParameter("@GroupID", groupId);
            DataSet ds = spGetUserPhoneNumber.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static bool UserSettings(string userID, int is_device, int is_email, int is_sms, int nGroupID, int is_active, int status)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UserSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@userID", userID);
            sp.AddParameter("@is_device", is_device);
            sp.AddParameter("@is_email", is_email);
            sp.AddParameter("@is_sms", is_sms);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@is_active", is_active);
            sp.AddParameter("@status", status);

            bool result = sp.ExecuteReturnValue<bool>();
            return result;
        }

        public static DataTable GetNotificationDeviceToSend(List<long> notificationsID)
        {
            ODBCWrapper.StoredProcedure spGetUsersTags = new ODBCWrapper.StoredProcedure("GetNotificationDeviceToSend");
            spGetUsersTags.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spGetUsersTags.AddIDListParameter<long>("@notificationsID", notificationsID, "Id");

            DataSet ds = spGetUsersTags.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static DataTable TagsNotificationNotExists(Dictionary<int, List<int>> tagIDs, int groupID, int triggrtType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("TagsNotificationNotExists");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@triggerType", triggrtType);
            sp.AddKeyValueListParameter<int, int>("@TagValues", tagIDs, "key", "value"); ;

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        public static bool InsertNotificationParameter(Dictionary<int, List<int>> tagIDs, int groupID, int triggrtType)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertNotificationParameter");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@triggerType", triggrtType);
            sp.AddKeyValueListParameter<int, int>("@TagValues", tagIDs, "key", "value"); ;

            bool result = sp.ExecuteReturnValue<bool>();

            return result;
        }

        public static DataTable GetAllTagsValueNameByIDs(int nGroupID, List<int> lAllTags)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAllTagsValueNameByIDs");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddIDListParameter("@TagTypes", lAllTags, "Id");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null)
            {
                return ds.Tables[0];
            }
            return null;
        }

        /// <summary>
        /// Update status of several notification requests at notifications_requests table
        /// by calling UpdateNotificationRequestStatus stored procedure. 
        /// </summary>
        /// <param name="requestsIDsList"></param>
        /// <param name="status"></param>
        public static bool UpdateNotificationRequestStatus(int startRequestID, int endRequestID, byte status, long groupID)
        {
            ODBCWrapper.StoredProcedure spUpdateNotificationMessageStatus = new ODBCWrapper.StoredProcedure("UpdateNotificationRequestStatusNotSuccess");
            spUpdateNotificationMessageStatus.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spUpdateNotificationMessageStatus.AddParameter("@StartRequestID", startRequestID);
            spUpdateNotificationMessageStatus.AddParameter("@EndRequestID", endRequestID);
            spUpdateNotificationMessageStatus.AddParameter("@Status", status);
            spUpdateNotificationMessageStatus.AddParameter("@groupID", groupID);
            bool result = spUpdateNotificationMessageStatus.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool InsertNotificationTagRequest(DataTable notification)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("InsertNotifictaionRequestDT");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddDataTableParameter("@notificationDT", notification);
            bool result = spInsert.ExecuteReturnValue<bool>();
            return result;
        }

        public static bool Get_NotificationMessageTypeAndRequestID(long lSiteGuid, long? lNotificationMessageID, long? lNotificationRequestID, ref byte bytMessageType, ref long lNonNullableNotificationRequestID)
        {
            bool res = false;
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_NotificationMessageType");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddNullableParameter<long?>("@NotificationMessageID", lNotificationMessageID);
            sp.AddNullableParameter<long?>("@NotificationRequestID", lNotificationRequestID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["messageVia"] != DBNull.Value && dt.Rows[0]["messageVia"] != null)
                        Byte.TryParse(dt.Rows[0]["messageVia"].ToString(), out bytMessageType);
                    if (lNotificationRequestID.HasValue)
                        lNonNullableNotificationRequestID = lNotificationRequestID.Value;
                    else
                    {
                        if (dt.Rows[0]["notification_request_id"] != DBNull.Value && dt.Rows[0]["notification_request_id"] != null)
                            Int64.TryParse(dt.Rows[0]["notification_request_id"].ToString(), out lNonNullableNotificationRequestID);
                    }
                    res = true;
                }
            }

            return res;
        }

        public static int Get_CountOfUniqueNotifications(long lSiteGuid, byte bytMessageType, byte bytViewStatus)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_CountOfUniqueNotifications");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@SiteGuid", lSiteGuid);
            sp.AddParameter("@MessageType", bytMessageType);
            sp.AddParameter("@ViewStatus", bytViewStatus);
            return sp.ExecuteReturnValue<int>();
        }

        public static bool UpdateNotificationPartnerSettings(int groupID, NotificationPartnerSettings settings)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_NotificationPartnerSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);

            if (settings.IsPushNotificationEnabled.HasValue)
                sp.AddParameter("@push_notification_enabled", settings.IsPushNotificationEnabled.Value);

            if (settings.IsPushSystemAnnouncementsEnabled.HasValue)
                sp.AddParameter("@push_system_announcements_enabled", settings.IsPushSystemAnnouncementsEnabled.Value);

            sp.AddParameter("@date", DateTime.UtcNow);
            if (settings.PushStartHour.HasValue)
                sp.AddParameter("@pushStartHour", settings.PushStartHour.Value);

            if (settings.PushEndHour.HasValue)
                sp.AddParameter("@pushEndHour", settings.PushEndHour.Value);

            if (settings.IsInboxEnabled.HasValue)
                sp.AddParameter("@isInboxEnabled", settings.IsInboxEnabled.Value);

            if (settings.MessageTTLDays.HasValue)
                sp.AddParameter("@messageTTL", settings.MessageTTLDays.Value);

            if (settings.AutomaticIssueFollowNotifications.HasValue)
                sp.AddParameter("@automaticSending", settings.AutomaticIssueFollowNotifications.Value);

            if (settings.TopicExpirationDurationDays.HasValue)
                sp.AddParameter("@topicCleanupExpirationDays", settings.TopicExpirationDurationDays.Value);

            if (settings.IsRemindersEnabled.HasValue)
                sp.AddParameter("@isRemindersEnabled", settings.IsRemindersEnabled.Value);

            if (settings.RemindersPrePaddingSec.HasValue)
                sp.AddParameter("@remindersPrePaddingSec", settings.RemindersPrePaddingSec.Value);

            if (!string.IsNullOrEmpty(settings.PushAdapterUrl))
                sp.AddParameter("@pushAdapterUrl", settings.PushAdapterUrl);

            if (!string.IsNullOrEmpty(settings.SenderEmail))
                sp.AddParameter("@senderEmail", settings.SenderEmail);

            if (!string.IsNullOrEmpty(settings.MailSenderName))
                sp.AddParameter("@mailSenderName", settings.MailSenderName);

            if (!string.IsNullOrEmpty(settings.ChurnMailSubject))
                sp.AddParameter("@churnMailSubject", settings.ChurnMailSubject);

            if (!string.IsNullOrEmpty(settings.ChurnMailTemplateName))
                sp.AddParameter("@churnMailTemplateName", settings.ChurnMailTemplateName);

            return sp.ExecuteReturnValue<bool>();
        }

        public static List<NotificationPartnerSettings> GetNotificationPartnerSettings(int groupID)
        {
            List<NotificationPartnerSettings> settings = new List<NotificationPartnerSettings>();
            bool automaticIssueFollowNotification = true;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_NotificationPartnerSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");

            if (groupID > 0)
                sp.AddParameter("@groupID", groupID);
            else
                sp.AddParameter("@groupID", null);

            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string automaticSending = ODBCWrapper.Utils.GetSafeStr(row, "automatic_sending");

                    if (!string.IsNullOrEmpty(automaticSending))
                        automaticIssueFollowNotification = automaticSending.Equals("1");

                    settings.Add(new NotificationPartnerSettings()
                    {
                        IsPushNotificationEnabled = ODBCWrapper.Utils.GetIntSafeVal(row, "push_notification_enabled") == 1 ? true : false,
                        IsPushSystemAnnouncementsEnabled = ODBCWrapper.Utils.GetIntSafeVal(row, "push_system_announcements_enabled") == 1 ? true : false,
                        PushStartHour = ODBCWrapper.Utils.GetIntSafeVal(row, "push_start_hour"),
                        PushEndHour = ODBCWrapper.Utils.GetIntSafeVal(row, "push_end_hour"),
                        IsInboxEnabled = ODBCWrapper.Utils.GetIntSafeVal(row, "is_inbox_enable") == 1 ? true : false,
                        MessageTTLDays = ODBCWrapper.Utils.GetIntSafeVal(row, "message_ttl"),
                        AutomaticIssueFollowNotifications = automaticIssueFollowNotification,
                        PartnerId = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id"),
                        TopicExpirationDurationDays = ODBCWrapper.Utils.GetIntSafeVal(row, "topic_cleanup_expiration_days"),
                        IsRemindersEnabled = ODBCWrapper.Utils.GetIntSafeVal(row, "is_reminder_enabled") == 1 ? true : false,
                        RemindersPrePaddingSec = ODBCWrapper.Utils.GetIntSafeVal(row, "reminder_offset_sec"),
                        PushAdapterUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "push_adapter_url"),
                        ChurnMailSubject = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "churn_mail_subject"),
                        ChurnMailTemplateName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "churn_mail_template_name"),
                        MailSenderName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "mail_sender_name"),
                        SenderEmail = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "sender_email")
                    });
                }
            }

            return settings;
        }

        public static DataRow GetNotificationSettings(int groupID, int userID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_NotificationSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@userID", userID);
            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            else
                return null;
        }

        public static DataRow Get_MessageAnnouncementWithActiveStatus(int messageAnnouncementId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncementWithActiveStatus");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@ID", messageAnnouncementId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
            }

            return null;
        }

        public static DataRow Get_MessageAnnouncement(long messageAnnouncementId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncementById");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@ID", messageAnnouncementId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
            }

            return null;
        }

        public static List<DataRow> Get_MessageAllAnnouncements(int groupId, int pageSize, int pageIndex)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncements");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@top", pageSize * (pageIndex + 1));
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    int num = pageSize;
                    if (dt.Rows.Count <= pageSize)
                        num = dt.Rows.Count;

                    List<DataRow> ret = new List<DataRow>();

                    for (int i = 0; i < num; i++)
                    {
                        int curr = i + pageSize * pageIndex;
                        if (curr < dt.Rows.Count)
                            ret.Add(dt.Rows[curr]);
                    }

                    return ret;
                }
            }

            return null;
        }

        public static DataRowCollection Get_MessageAnnouncementByAnnouncementId(int announcementId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncementByAnnouncementId");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@Announcement_ID", announcementId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows;
                }
            }

            return null;
        }

        public static DataRowCollection Get_MessageAnnouncementByAnnouncementAndReference(int announcementId, string messageReference)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncementByAnnouncementAndReference");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@Announcement_ID", announcementId);
            sp.AddParameter("@message_reference", messageReference);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows;
                }
            }

            return null;
        }

        public static int Get_MessageAllAnnouncementsCount(int groupId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncements");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return dt.Rows.Count;
                }
            }

            return 0;
        }

        public static DataRow Insert_MessageAnnouncement(int groupId, int recipients, string name, string message, bool enabled, DateTime startTime, string timezone, int updaterId, long announcement_id = 0, string messageReference = null, string resultMsgId = null)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("InsertMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@recipients", recipients);
            spInsert.AddParameter("@name", name);
            spInsert.AddParameter("@message", message);
            spInsert.AddParameter("@start_time", startTime);
            spInsert.AddParameter("@timezone", timezone);
            spInsert.AddParameter("@group_id", groupId);
            spInsert.AddParameter("@updater_id", updaterId);
            spInsert.AddParameter("@is_active", enabled ? 1 : 0);
            spInsert.AddParameter("@result_message_id", resultMsgId);
            spInsert.AddParameter("@update_date", DateTime.UtcNow);
            spInsert.AddParameter("@message_reference", messageReference);
            if (announcement_id != 0)
                spInsert.AddParameter("@announcement_id", announcement_id);

            DataSet ds = spInsert.ExecuteDataSet();
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return null;

            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                return null;

            return dt.Rows[0];
        }

        public static DataRow Update_MessageAnnouncement(int id, int groupId, int recipients, string name, string message, bool enabled, DateTime startTime, string timezone, int updaterId, string resultMsgId = null)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@ID", id);
            spInsert.AddParameter("@recipients", recipients);
            spInsert.AddParameter("@name", name);
            spInsert.AddParameter("@message", message);
            spInsert.AddParameter("@is_active", enabled ? 1 : 0);
            spInsert.AddParameter("@start_time", startTime);
            spInsert.AddParameter("@timezone", timezone);
            spInsert.AddParameter("@updater_id", updaterId);
            spInsert.AddParameter("@result_message_id", resultMsgId);

            DataSet ds = spInsert.ExecuteDataSet();
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0)
                return null;

            DataTable dt = ds.Tables[0];
            if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                return null;

            return dt.Rows[0];
        }

        public static void Update_MessageAnnouncementStatus(int id, int groupId, bool enabled)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@ID", id);
            spInsert.AddParameter("@is_active", enabled ? 1 : 0);
            spInsert.ExecuteDataSet();
        }

        public static void Update_MessageAnnouncementSent(int id, int groupId, int sent)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@ID", id);
            spInsert.AddParameter("@sent", sent);
            spInsert.AddParameter("@response_date", DateTime.UtcNow);
            spInsert.ExecuteDataSet();
        }

        public static void Update_MessageAnnouncementResultMessageId(int id, int groupId, string resultMsgId)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@ID", id);
            spInsert.AddParameter("@result_message_id", resultMsgId);
            spInsert.AddParameter("@response_date", DateTime.UtcNow);
            spInsert.ExecuteDataSet();
        }

        public static void Delete_MessageAnnouncement(long id, int groupId)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@ID", id);
            spInsert.AddParameter("@status", 2);
            spInsert.ExecuteDataSet();
        }

        public static void Update_MessageAnnouncementActiveStatus(int groupId, int messageAnnouncementId, int status)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("UpdateMessageAnnouncementActiveStatus");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@ID", messageAnnouncementId);
            sp.AddParameter("@ActiveStatus", status);
            DataSet ds = sp.ExecuteDataSet();
        }

        public static string Get_AnnouncementExternalIdByRecipients(int groupId, int recipients)
        {
            string ret = string.Empty;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAnnouncementExternalIdByRecipients");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@recipients", recipients);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    return ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "external_id");
                }
            }

            return ret;
        }

        public static int Insert_Announcement(int groupId, string announcementName, string externalAnnouncementId, int messageType, int announcementRecipientsType, string followPhrase = null, string followReference = null)
        {
            ODBCWrapper.StoredProcedure spInsert = new ODBCWrapper.StoredProcedure("Insert_Announcement");
            spInsert.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsert.AddParameter("@group_id", groupId);
            spInsert.AddParameter("@name", announcementName);
            spInsert.AddParameter("@external_id", externalAnnouncementId);
            spInsert.AddParameter("@message_type", messageType);
            spInsert.AddParameter("@recipient_type", announcementRecipientsType);
            spInsert.AddParameter("@status", 1);
            spInsert.AddParameter("@created_at", DateTime.UtcNow);
            spInsert.AddParameter("@follow_phrase", followPhrase);
            spInsert.AddParameter("@follow_reference", followReference);

            int newTransactionID = spInsert.ExecuteReturnValue<int>();
            return newTransactionID;
        }

        //public static DataRowCollection Get_Announcement(int groupId, List<eAnnouncementRecipientsType> recipientsTypes, List<long> announcementIds, bool isAnd = false)
        //{
        //    DataRowCollection rowCollection = null;

        //    ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAnnouncements");
        //    sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");

        //    sp.AddParameter("@group_id", groupId);
        //    sp.AddParameter("@and", Convert.ToInt16(isAnd));

        //    if (announcementIds != null && announcementIds.Count > 0)
        //        sp.AddIDListParameter<long>("@ids", announcementIds, "Id");

        //    if (recipientsTypes != null && recipientsTypes.Count > 0)
        //        sp.AddIDListParameter<int>("@recipient_types", recipientsTypes.Cast<int>().ToList(), "Id");

        //    DataSet ds = sp.ExecuteDataSet();
        //    if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
        //    {
        //        DataTable dt = ds.Tables[0];
        //        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
        //            rowCollection = dt.Rows;
        //    }

        //    return rowCollection;
        //}

        /// <summary>
        /// Retrieve userIds which follows the notification 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="aanouncementId"></param>
        /// <returns></returns>
        public static List<int> GetUsersFollowNotificationView(int groupId, int aanouncementId)
        {
            List<int> userIds = null;
            try
            {
                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_NOTIFICATION, "get_users_notification")
                {
                    startKey = new object[] { groupId, aanouncementId },
                    endKey = new object[] { groupId, aanouncementId },
                    staleState = ViewStaleState.False
                };

                // execute request
                userIds = cbManager.View<int>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get users follows notification. GID: {0}, notification ID: {1}, ex: {2}", groupId, aanouncementId, ex);
            }

            return userIds;
        }

        public static bool SetUserFollowNotificationData(int groupId, int userId, int notificationId)
        {
            bool result = false;
            try
            {
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    result = cbManager.Set(GetUserFollowsNotificationKey(groupId, userId, notificationId), userId);
                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while set user follow notification data. number of tries: {0}/{1}. GID: {2}, notification ID: {3}. userId: {4}",
                             numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            notificationId,
                            userId);
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set user follow notification data. number of tries: {0}/{1}. GID: {2}, notification ID: {3}. userId: {4}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            notificationId,
                            userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while set user follow  notification data.  GID: {0}, notification ID: {1}. userId: {2}. error:{3}",
                  groupId, notificationId, userId, ex);
            }

            return result;
        }

        public static bool RemoveUserFollowNotification(int groupId, int userId, long notificationId)
        {
            bool passed = false;
            string userNotificationItemKey = GetUserFollowsNotificationKey(groupId, userId, notificationId);

            try
            {
                passed = cbManager.Remove(userNotificationItemKey);
                if (passed)
                    log.DebugFormat("Successfully removed {0}", userNotificationItemKey);
                else
                    log.ErrorFormat("Error while removing {0}", userNotificationItemKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing {0}. ex: {1}", userNotificationItemKey, ex);
            }

            return passed;
        }

        public static List<DbAnnouncement> GetAnnouncements(int groupId)
        {
            List<DbAnnouncement> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAnnouncementsByGroupId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<DbAnnouncement>();
                    DbAnnouncement dbAnnouncement = null;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int recipientType = ODBCWrapper.Utils.GetIntSafeVal(row, "recipient_type");
                        string automaticSending = ODBCWrapper.Utils.GetSafeStr(row, "automatic_sending");

                        bool? automaticIssueFollowNotification = null;
                        if (!string.IsNullOrEmpty(automaticSending))
                            automaticIssueFollowNotification = automaticSending.Equals("1");

                        DateTime? lastMessageSentDate = ODBCWrapper.Utils.GetNullableDateSafeVal(row, "last_message_sent_date_sec");
                        long lastMessageSentDateSec = 0;
                        if (lastMessageSentDate != null)
                            lastMessageSentDateSec = ODBCWrapper.Utils.DateTimeToUnixTimestamp((DateTime)lastMessageSentDate);

                        dbAnnouncement = new DbAnnouncement()
                        {
                            ID = ODBCWrapper.Utils.GetIntSafeVal(row, "id"),
                            ExternalId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                            Name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                            FollowPhrase = ODBCWrapper.Utils.GetSafeStr(row, "follow_phrase"),
                            FollowReference = ODBCWrapper.Utils.GetSafeStr(row, "follow_reference"),
                            AutomaticIssueFollowNotification = automaticIssueFollowNotification,
                            RecipientsType = Enum.IsDefined(typeof(eAnnouncementRecipientsType), recipientType) ? (eAnnouncementRecipientsType)recipientType : eAnnouncementRecipientsType.All,
                            LastMessageSentDateSec = lastMessageSentDateSec,
                            QueueName = ODBCWrapper.Utils.GetSafeStr(row, "queue_name")
                        };

                        result.Add(dbAnnouncement);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetAnnouncementsByGroupId. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        public static MessageTemplate SetMessageTemplate(int groupId, MessageTemplate messageTemplate)
        {
            MessageTemplate result = null;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("SetMessageTemplate");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@message", messageTemplate.Message);
                sp.AddParameter("@dateFormat", messageTemplate.DateFormat);
                sp.AddParameter("@assetType", (int)messageTemplate.TemplateType);
                sp.AddParameter("@sound", messageTemplate.Sound);
                sp.AddParameter("@action", messageTemplate.Action);
                sp.AddParameter("@url", messageTemplate.URL);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        result = CreateMessageTemplate(ds.Tables[0].Rows[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetMessageTemplate. groupId: {0}, messageTemplate: {1} . Error {2}", groupId, messageTemplate.ToString(), ex);
            }

            return result;

        }

        public static List<MessageTemplate> GetMessageTemplate(int groupId, MessageTemplateType messageTemplateType)
        {
            List<MessageTemplate> result = new List<MessageTemplate>();
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageTemplate");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@assetType", (int)messageTemplateType);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                            result.Add(CreateMessageTemplate(row));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetMessageTemplate. groupId: {0}, messageTemplateType {1}. Error {2}", groupId, messageTemplateType, ex);
            }
            return result;
        }

        private static MessageTemplate CreateMessageTemplate(DataRow row)
        {
            MessageTemplate result = new MessageTemplate();

            int assetType = ODBCWrapper.Utils.GetIntSafeVal(row, "ASSET_TYPE");

            result = new MessageTemplate()
            {
                Id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID"),
                Message = ODBCWrapper.Utils.GetSafeStr(row, "MESSAGE"),
                DateFormat = ODBCWrapper.Utils.GetSafeStr(row, "DATE_FORMAT"),
                Sound = ODBCWrapper.Utils.GetSafeStr(row, "SOUND"),
                Action = ODBCWrapper.Utils.GetSafeStr(row, "ACTION"),
                URL = ODBCWrapper.Utils.GetSafeStr(row, "URL"),
                TemplateType = Enum.IsDefined(typeof(MessageTemplateType), assetType) ? (MessageTemplateType)assetType : MessageTemplateType.Series
            };
            return result;
        }

        public static DeviceNotificationData GetDeviceNotificationData(int groupId, string udid, ref bool isDocumentExist, bool withLock = false)
        {
            DeviceNotificationData deviceData = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            ulong cas = 0;
            isDocumentExist = true;

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    if (withLock)
                        deviceData = cbManager.Get<DeviceNotificationData>(GetDeviceDataKey(groupId, udid), true, out cas, out status);
                    else
                        deviceData = cbManager.Get<DeviceNotificationData>(GetDeviceDataKey(groupId, udid), out status);

                    if (deviceData == null)
                    {
                        if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            // key doesn't exist - don't try again
                            log.DebugFormat("device notification data with lock wasn't found. key: {0}", GetDeviceDataKey(groupId, udid));
                            isDocumentExist = false;
                            break;
                        }
                        else
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while getting device notification data with lock. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                GetDeviceDataKey(groupId, udid));

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        result = true;
                        deviceData.cas = cas;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully received device notification data with lock. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            GetDeviceDataKey(groupId, udid));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get device notification with lock. key: {0}, ex: {1}", GetDeviceDataKey(groupId, udid), ex);
            }

            return deviceData;
        }

        public static bool SetDeviceNotificationData(int groupId, string udid, DeviceNotificationData newDeviceNotificationData, bool unlock = false)
        {
            bool result = false;
            try
            {
                ulong outCas = 0;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    if (unlock)
                        result = cbManager.Set(GetDeviceDataKey(groupId, udid), newDeviceNotificationData, true, out outCas, 0, newDeviceNotificationData.cas);
                    else
                        result = cbManager.Set(GetDeviceDataKey(groupId, udid), newDeviceNotificationData);

                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while trying to set device notification number of tries: {0}/{1}. GID: {2}, UDID: {3}, data: {4}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            udid,
                            JsonConvert.SerializeObject(newDeviceNotificationData));
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        newDeviceNotificationData.cas = 0;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set device notification. number of tries: {0}/{1}. object {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            JsonConvert.SerializeObject(newDeviceNotificationData));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to set device notification. GID: {0}, UDID: {1}, ex: {2}", groupId, udid, ex);
            }
            return result;
        }

        public static UserNotification GetUserNotificationData(int groupId, int userId, ref bool isDocumentExist, bool withLock = false)
        {
            UserNotification userNotification = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            ulong cas = 0;
            isDocumentExist = true;

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    if (withLock)
                        userNotification = cbManager.Get<UserNotification>(GetUserNotificationKey(groupId, userId), true, out cas, out status);
                    else
                        userNotification = cbManager.Get<UserNotification>(GetUserNotificationKey(groupId, userId), out status);

                    if (userNotification == null)
                    {
                        if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            // key doesn't exist - don't try again
                            log.DebugFormat("user notification data wasn't found. key: {0}", GetUserNotificationKey(groupId, userId));
                            isDocumentExist = false;
                            break;
                        }
                        else
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while getting user notification data. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                GetUserNotificationKey(groupId, userId));

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        result = true;
                        userNotification.cas = cas;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully received user notification data. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            GetUserNotificationKey(groupId, userId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user notification. key: {0}, ex: {1}", GetUserNotificationKey(groupId, userId), ex);
            }

            return userNotification;
        }

        public static bool SetUserNotificationData(int groupId, int userId, UserNotification userNotification, bool unlock = false)
        {
            bool result = false;
            try
            {
                int numOfTries = 0;
                ulong outCas = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    if (unlock)
                        result = cbManager.Set(GetUserNotificationKey(groupId, userId), userNotification, true, out outCas, 0, userNotification.cas);
                    else
                        result = cbManager.Set(GetUserNotificationKey(groupId, userId), userNotification);

                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting user notification data. number of tries: {0}/{1}. GID: {2}, user ID: {3}. data: {4}",
                             numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            userId,
                            JsonConvert.SerializeObject(userNotification));
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        userNotification.cas = 0;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set user notification data. number of tries: {0}/{1}. object {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            JsonConvert.SerializeObject(userNotification));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting user notification data (unlock). GID: {0}, user ID: {1}, ex: {2}", groupId, userId, ex);
            }

            return result;
        }

        public static bool RemoveUserNotificationData(int groupId, int userId, ulong cas = 0)
        {
            bool result = false;
            try
            {
                result = cbManager.Remove(GetUserNotificationKey(groupId, userId), cas);
                if (!result)
                    log.ErrorFormat("Error while removing user notification data. GID: {0}, user ID: {1}.", groupId, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while set user notification data. gid: {0}, user ID: {1}, ex: {2}", groupId, userId, ex);
            }
            return result;
        }

        public static bool RemoveDeviceNotificationData(int groupId, string udid, ulong cas = 0)
        {
            bool result = false;
            try
            {
                result = cbManager.Remove(GetDeviceDataKey(groupId, udid), cas);
                if (!result)
                    log.ErrorFormat("Error while removing device notification data. GID: {0}, UDID: {1}.", groupId, udid);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while removing device notification data. GID: {0}, UDID: {1}, ex: {2}", groupId, udid, ex);
            }
            return result;
        }

        public static UserNotificationSettings UpdateUserNotificationSettings(int groupID, int userId, UserNotificationSettings settings, ref bool isDocumentExist)
        {
            bool result = false;
            UserNotification userNotification = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;
            isDocumentExist = true;
            ulong cas = 0;
            string cbKey = string.Empty;

            try
            {
                int numOfTries = 0;
                cbKey = GetUserNotificationKey(groupID, userId);

                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    userNotification = cbManager.Get<UserNotification>(cbKey, false, out cas, out status);
                    if (userNotification == null)
                    {
                        if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            isDocumentExist = false;

                            // key doesn't exist - don't try again
                            log.DebugFormat("user notification data wasn't found. key: {0}", cbKey);
                            break;
                        }
                        else
                        {
                            // error retrieving document
                            numOfTries++;
                            log.ErrorFormat("Error getting user notification document while trying to update user notification settings. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                cbKey);

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        // document retrieved - update it    
                        UpdateUserNotification(settings, ref userNotification);

                        // insert document to CB
                        if (cbManager.Set<UserNotification>(cbKey, userNotification, false, 0, cas))
                        {
                            result = true;
                            userNotification.cas = cas;

                            // log success on retry
                            if (numOfTries > 0)
                            {
                                numOfTries++;
                                log.DebugFormat("successfully updated user notification settings. number of tries: {0}/{1}. key {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                cbKey);
                            }
                        }
                        else
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while updating user notification settings. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                cbKey);

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to update user notification settings. key: {0}, ex: {1}", cbKey, ex);
            }

            if (userNotification != null && userNotification.Settings != null)
                return userNotification.Settings;
            else
                return null;
        }

        private static void UpdateUserNotification(UserNotificationSettings settings, ref UserNotification userNotification)
        {
            if (settings.EnableInbox.HasValue)
                userNotification.Settings.EnableInbox = settings.EnableInbox.Value;

            if (settings.EnableMail.HasValue)
                userNotification.Settings.EnableMail = settings.EnableMail.Value;

            if (settings.EnablePush.HasValue)
                userNotification.Settings.EnablePush = settings.EnablePush.Value;

            if (settings.FollowSettings != null)
            {
                if (settings.FollowSettings.EnableMail.HasValue)
                    userNotification.Settings.FollowSettings.EnableMail = settings.FollowSettings.EnableMail.Value;

                if (settings.FollowSettings.EnablePush.HasValue)
                    userNotification.Settings.FollowSettings.EnablePush = settings.FollowSettings.EnablePush.Value;
            }
        }

        public static void UnlockDeviceNotificationDocument(int groupId, string udid, ulong cas)
        {
            string docKey = GetDeviceDataKey(groupId, udid);
            try
            {
                if (cbManager.Unlock(docKey, cas))
                    log.DebugFormat("document unlocked {0}", docKey);
                else
                    log.DebugFormat("couldn't unlock document {0}", docKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to unlock device notification object. key: {0}, cas: {1}, ex: {2}", docKey, cas, ex);
            }
        }

        public static void UnlockUserNotificationDocument(int groupId, int userId, ulong cas)
        {
            string docKey = GetUserNotificationKey(groupId, userId);
            try
            {
                if (cbManager.Unlock(docKey, cas))
                    log.DebugFormat("document unlocked {0}", docKey);
                else
                    log.DebugFormat("couldn't unlock document {0}", docKey);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to unlock user notification object. key: {0}, cas: {1}, ex: {2}", docKey, cas, ex);
            }
        }

        public static eOTTAssetTypes GetOttAssetTypByMediaType(int mediaTypeId)
        {
            eOTTAssetTypes assetType = eOTTAssetTypes.None;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += string.Format("SELECT ASSET_TYPE FROM [media_types] WHERE Id = {0}", mediaTypeId);

                if (selectQuery.Execute("query", true) != null && selectQuery.Table("query").DefaultView.Count > 0)
                {
                    var assetTypeId = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0], "ASSET_TYPE");
                    assetType = (eOTTAssetTypes)assetTypeId;
                }
                selectQuery.Finish();
                selectQuery = null;
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error while getting asset type by media Id. mediaTypeId: {0}, ex: {1}", mediaTypeId, ex);
            }

            return assetType;
        }

        public static List<InboxMessage> GetUserMessagesView(int groupId, long userId, bool onlyUnread, long fromDate)
        {
            List<InboxMessage> userMessages = null;
            try
            {
                var startKey = new object[] { groupId, userId, 0, fromDate };
                var endKey = new object[] { groupId, userId, 1, "\uefff" };

                if (onlyUnread)
                    endKey = new object[] { groupId, userId, 0, "\uefff" };

                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_INBOX, "get_user_messages")
                {
                    startKey = startKey,
                    endKey = endKey,
                    staleState = ViewStaleState.False,
                    inclusiveEnd = true
                };

                // execute request
                userMessages = cbManager.View<InboxMessage>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get users inbox message view. GID: {0}, userId ID: {1}, ex: {2}", groupId, userId, ex);
            }

            return userMessages;
        }

        public static InboxMessage GetUserInboxMessage(int groupId, int userId, string messageId)
        {
            InboxMessage userInboxMessage = null;
            Couchbase.IO.ResponseStatus status = Couchbase.IO.ResponseStatus.None;

            try
            {
                bool result = false;
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    userInboxMessage = cbManager.Get<InboxMessage>(GetInboxMessageKey(groupId, userId, messageId), out status);
                    if (userInboxMessage == null)
                    {
                        if (status == Couchbase.IO.ResponseStatus.KeyNotFound)
                        {
                            // key doesn't exist - don't try again
                            log.DebugFormat("user inbox message wasn't found. key: {0}", GetInboxMessageKey(groupId, userId, messageId));
                            break;
                        }
                        else
                        {
                            numOfTries++;
                            log.ErrorFormat("Error while getting user inbox message. number of tries: {0}/{1}. key: {2}",
                                numOfTries,
                                NUM_OF_INSERT_TRIES,
                                GetInboxMessageKey(groupId, userId, messageId));

                            Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                        }
                    }
                    else
                    {
                        result = true;

                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully received user inbox message. number of tries: {0}/{1}. key {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            GetInboxMessageKey(groupId, userId, messageId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user inbox message. key: {0}, ex: {1}", GetInboxMessageKey(groupId, userId, messageId), ex);
            }

            return userInboxMessage;
        }

        public static bool SetUserInboxMessage(int groupId, InboxMessage inboxMessage, int ttlDays)
        {
            bool result = false;
            try
            {
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    result = cbManager.Set(GetInboxMessageKey(groupId, inboxMessage.UserId, inboxMessage.Id), inboxMessage, (uint)TimeSpan.FromDays(ttlDays).TotalSeconds);

                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting inbox message. number of tries: {0}/{1}. GID: {2}, user ID: {3}. data: {4}",
                             numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            inboxMessage.UserId,
                            JsonConvert.SerializeObject(inboxMessage));
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set inbox message. number of tries: {0}/{1}. object {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            JsonConvert.SerializeObject(inboxMessage));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting inbox message. GID: {0}, user ID: {1}, message ID: {2}, ex: {3}", groupId, inboxMessage.UserId, inboxMessage.Id, ex);
            }

            return result;
        }

        public static bool UpdateInboxMessageState(int groupId, int userId, string messageId, eMessageState messageState)
        {
            bool result = false;
            try
            {
                var inboxMessage = GetUserInboxMessage(groupId, userId, messageId);
                if (inboxMessage == null)
                {
                    log.ErrorFormat("couldn't update message state to {0}. inbox message wasn't found. key: {1}", messageState.ToString(), GetInboxMessageKey(groupId, userId, messageId));
                    return false;
                }

                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    // update message
                    inboxMessage.State = messageState;

                    // update document
                    result = cbManager.Set(GetInboxMessageKey(groupId, userId, messageId), inboxMessage);

                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while updating inbox message state to {0}. number of tries: {1}/{2}. GID: {3}, user ID: {4}. data: {5}",
                            messageState.ToString(),
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            userId,
                            JsonConvert.SerializeObject(inboxMessage));
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully updated inbox message to state {0}. number of tries: {1}/{2}. object {3}",
                            messageState.ToString(),
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            JsonConvert.SerializeObject(inboxMessage));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while updating inbox message to state {0}. GID: {1}, user ID: {2}, message ID: {3}, ex: {4}", messageState.ToString(), groupId, userId, messageId, ex);
            }

            return result;
        }

        public static List<string> GetSystemInboxMessagesView(int groupId, long fromDate)
        {
            List<string> messageIds = null;
            try
            {

                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_INBOX, "get_system_messages")
                {
                    startKey = new object[] { groupId, fromDate },
                    endKey = new object[] { groupId, "\uefff" },
                    staleState = ViewStaleState.False,
                    inclusiveEnd = true
                };

                // execute request
                messageIds = cbManager.View<string>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get system inbox message view. GID: {0}, fromDate: {1}, ex: {2}", groupId, fromDate, ex);
            }

            return messageIds;
        }

        public static bool SetSystemAnnouncementMessage(int groupId, InboxMessage inboxMessage, int ttlDays)
        {
            bool result = false;
            try
            {
                int numOfTries = 0;
                while (!result && numOfTries < NUM_OF_INSERT_TRIES)
                {
                    result = cbManager.Set(GetInboxSystemAnnouncementKey(groupId, inboxMessage.Id), inboxMessage, (uint)TimeSpan.FromDays(ttlDays).TotalSeconds);

                    if (!result)
                    {
                        numOfTries++;
                        log.ErrorFormat("Error while setting inbox system message. number of tries: {0}/{1}. GID: {2}. data: {3}",
                             numOfTries,
                            NUM_OF_INSERT_TRIES,
                            groupId,
                            JsonConvert.SerializeObject(inboxMessage));
                        Thread.Sleep(SLEEP_BETWEEN_RETRIES_MILLI);
                    }
                    else
                    {
                        // log success on retry
                        if (numOfTries > 0)
                        {
                            numOfTries++;
                            log.DebugFormat("successfully set inbox system message. number of tries: {0}/{1}. object {2}",
                            numOfTries,
                            NUM_OF_INSERT_TRIES,
                            JsonConvert.SerializeObject(inboxMessage));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while setting inbox system message. GID: {0}, message ID: {1}, ex: {2}", groupId, inboxMessage.Id, ex);
            }

            return result;
        }

        public static List<InboxMessage> GetSystemInboxMessages(int groupId, List<string> messageIds)
        {
            IDictionary<string, InboxMessage> systemInboxMessages = null;
            List<string> requestKeys = new List<string>();
            try
            {
                // create CB requested key list
                foreach (var messageId in messageIds)
                    requestKeys.Add(GetInboxSystemAnnouncementKey(groupId, messageId));

                // get CB documents
                systemInboxMessages = cbManager.GetValues<InboxMessage>(requestKeys, true);
                if (systemInboxMessages == null || systemInboxMessages.Count == 0)
                    log.DebugFormat("user system messages list wasn't found GID: {0}, messageIds: {1}", groupId, string.Join(",", requestKeys));
                else
                    return systemInboxMessages.Values.ToList();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user system messages. messageIds: {0}, ex: {1}", messageIds != null ? string.Join(",", messageIds) : string.Empty, ex);
            }

            return null;
        }

        public static bool UpdateAnnouncement(int groupId, int announcementId, bool? automaticSending, DateTime? lastMessageSentDate = null, string queueName = null)
        {
            int rowCount = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_Announcement");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@announcementId", announcementId);
                if (automaticSending.HasValue)
                    sp.AddParameter("@automaticSending", automaticSending.Value);
                else
                    sp.AddParameter("@automaticSending", DBNull.Value);

                if (lastMessageSentDate.HasValue)
                    sp.AddParameter("@lastMessageSentDateSec", lastMessageSentDate.Value);

                if (!string.IsNullOrEmpty(queueName))
                    sp.AddParameter("@queueName", queueName);

                rowCount = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at UpdateAnnouncement. announcementId: {0}, automaticSending{ {1}", announcementId, automaticSending.HasValue ? automaticSending.Value.ToString() : string.Empty, ex);
            }

            return rowCount > 0;
        }

        public static bool DeleteAnnouncement(int groupId, long announcementId)
        {
            int affectedRows = 0;

            ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("Delete_Announcement");
            spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertUserNotification.AddParameter("@announcement_id", announcementId);
            spInsertUserNotification.AddParameter("@group_id", groupId);
            affectedRows = spInsertUserNotification.ExecuteReturnValue<int>();

            return affectedRows > 0;
        }

        public static List<KeyValuePair<object, int>> GetAmountOfSubscribersPerAnnouncement(int groupId)
        {
            List<KeyValuePair<object, int>> result = null;
            try
            {

                // prepare view request
                ViewManager viewManager = new ViewManager(CB_DESIGN_DOC_NOTIFICATION, "amount_of_subscribers")
                {
                    staleState = ViewStaleState.False,
                    groupLevel = 2,
                    inclusiveEnd = true,
                    reduce = true
                };

                // execute request
                result = cbManager.ViewKeyValuePairs<int>(viewManager);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get amount of subscribers per announcement. GID: {0}, ex: {1}", groupId, ex);
            }

            return result;
        }

        public static void GetEpisodeAssociationTag(int groupId, out string episodeAssociationTag, out int mediaTypeId)
        {
            episodeAssociationTag = string.Empty;
            mediaTypeId = 0;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetEpisodeAssociationTag");
                sp.SetConnectionKey(TVINCI_CONNECTION);
                sp.AddParameter("@groupId", groupId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    episodeAssociationTag = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0], "ASSOCIATION_TAG");
                    mediaTypeId = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0], "id");
                }
                else
                    log.DebugFormat("GetEpisodeAssociationTag. ASSOCIATION_TAG is missing. groupId: {0}.", groupId);

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetEpisodeAssociationTag. groupId: {0}. Error {1}", groupId, ex);
            }
        }

        public static List<DbReminder> GetReminders(int groupId, long reminderId = 0)
        {
            List<DbReminder> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetRemindersByGroupId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@reminderId", reminderId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<DbReminder>();
                    DbReminder dbReminder = null;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        dbReminder = CreateDbReminder(row);
                        result.Add(dbReminder);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetRemindersByGroupId. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        public static DbReminder GetReminderByReferenceId(int groupId, long referenceId)
        {
            DbReminder result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetReminderByReferenceId");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@referenceId", referenceId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    result = CreateDbReminder(ds.Tables[0].Rows[0]);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetRemindersByGroupId. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        public static List<DbReminder> GetReminderByReferenceId(int groupId, List<long> referenceIds)
        {
            List<DbReminder> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetReminderByReferenceIds");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter<long>("@referenceIds", referenceIds, "id");
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<DbReminder>();
                    DbReminder dbReminder = null;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        dbReminder = CreateDbReminder(row);
                        result.Add(dbReminder);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetReminderByReferenceId. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        public static int SetReminder(DbReminder dbReminder)
        {
            int reminderId = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("SetReminder");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", dbReminder.GroupId);
                sp.AddParameter("@id", dbReminder.ID);
                sp.AddParameter("@isSent", dbReminder.IsSent);
                sp.AddParameter("@name", dbReminder.Name);
                sp.AddParameter("@phrase", dbReminder.Phrase);
                sp.AddParameter("@queueId", dbReminder.QueueId);
                sp.AddParameter("@routeName", dbReminder.RouteName);
                sp.AddParameter("@reference", dbReminder.Reference);
                sp.AddParameter("@sendTime", ODBCWrapper.Utils.UnixTimestampToDateTime(dbReminder.SendTime));
                sp.AddParameter("@externalId", dbReminder.ExternalPushId);
                sp.AddParameter("@externalResult", dbReminder.ExternalResult);
                sp.AddParameter("@message", dbReminder.Message);

                reminderId = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetReminder. groupId: {0}, Reminder: {1} . Error {2}", dbReminder.GroupId, JsonConvert.SerializeObject(dbReminder), ex);
            }

            return reminderId;
        }

        public static bool DeleteReminder(int groupId, long reminderId)
        {
            int affectedRows = 0;

            ODBCWrapper.StoredProcedure spInsertUserNotification = new ODBCWrapper.StoredProcedure("DeleteReminder");
            spInsertUserNotification.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            spInsertUserNotification.AddParameter("@reminderId", reminderId);
            spInsertUserNotification.AddParameter("@groupId", groupId);
            affectedRows = spInsertUserNotification.ExecuteReturnValue<int>();

            return affectedRows > 0;
        }

        public static List<DbReminder> GetReminders(int groupId, List<long> remindersIds)
        {
            List<DbReminder> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetRemindersByIds");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter<long>("@remindersIds", remindersIds, "id");

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<DbReminder>();
                    DbReminder dbReminder = null;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        dbReminder = CreateDbReminder(row);
                        result.Add(dbReminder);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetRemindersByIds. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        private static DbReminder CreateDbReminder(DataRow row)
        {
            DateTime? sentDate = ODBCWrapper.Utils.GetNullableDateSafeVal(row, "send_time");
            long sentDateSec = 0;
            if (sentDate != null)
                sentDateSec = ODBCWrapper.Utils.DateTimeToUnixTimestamp((DateTime)sentDate);

            DbReminder result = new DbReminder()
            {
                GroupId = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id"),
                ID = ODBCWrapper.Utils.GetIntSafeVal(row, "id"),
                IsSent = ODBCWrapper.Utils.GetIntSafeVal(row, "is_sent") == 1 ? true : false,
                Name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                Phrase = ODBCWrapper.Utils.GetSafeStr(row, "phrase"),
                QueueId = ODBCWrapper.Utils.GetSafeStr(row, "queue_id"),
                RouteName = ODBCWrapper.Utils.GetSafeStr(row, "route_name"),
                Reference = ODBCWrapper.Utils.GetIntSafeVal(row, "reference"),
                ExternalPushId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                Message = ODBCWrapper.Utils.GetSafeStr(row, "message"),

                SendTime = sentDateSec
            };

            return result;
        }

        public static DbSeriesReminder GetSeriesReminder(int groupId, string seriesId, long? seasonNumber, long epgChannelId)
        {
            DbSeriesReminder result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSeriesReminder");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@seriesId", seriesId);
                sp.AddParameter("@seasonNumber", seasonNumber);
                sp.AddParameter("@epgChannelId", epgChannelId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    result = CreateDbSeriesReminder(ds.Tables[0].Rows[0]);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSeriesReminder. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        private static DbSeriesReminder CreateDbSeriesReminder(DataRow row)
        {
            DbSeriesReminder result = new DbSeriesReminder()
            {
                GroupId = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id"),
                ID = ODBCWrapper.Utils.GetIntSafeVal(row, "id"),
                Name = ODBCWrapper.Utils.GetSafeStr(row, "name"),
                Phrase = ODBCWrapper.Utils.GetSafeStr(row, "phrase"),
                QueueId = ODBCWrapper.Utils.GetSafeStr(row, "queue_id"),
                RouteName = ODBCWrapper.Utils.GetSafeStr(row, "route_name"),
                Reference = ODBCWrapper.Utils.GetIntSafeVal(row, "reference"),
                ExternalPushId = ODBCWrapper.Utils.GetSafeStr(row, "external_id"),
                SeriesId = ODBCWrapper.Utils.GetSafeStr(row, "series_id"),
                SeasonNumber = ODBCWrapper.Utils.GetLongSafeVal(row, "season_number"),
                EpgChannelId = ODBCWrapper.Utils.GetLongSafeVal(row, "epg_channel_id"),
            };

            return result;
        }

        public static int SetSeriesReminder(DbSeriesReminder dbReminder)
        {
            int reminderId = 0;
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("SetSeriesReminder");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", dbReminder.GroupId);
                sp.AddParameter("@id", dbReminder.ID);
                sp.AddParameter("@name", dbReminder.Name);
                sp.AddParameter("@queueId", dbReminder.QueueId);
                sp.AddParameter("@routeName", dbReminder.RouteName);
                sp.AddParameter("@externalId", dbReminder.ExternalPushId);
                sp.AddParameter("@seriesId", dbReminder.SeriesId);
                sp.AddParameter("@seasonNumber", dbReminder.SeasonNumber);
                sp.AddParameter("@epgChannelId", dbReminder.EpgChannelId);

                reminderId = sp.ExecuteReturnValue<int>();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at SetSeriesReminder. groupId: {0}, Reminder: {1} . Error {2}", dbReminder.GroupId, JsonConvert.SerializeObject(dbReminder), ex);
            }

            return reminderId;
        }

        public static List<DbReminder> GetSeriesRemindersBySeasons(int groupId, List<long> seriesRemindersIds, string seriesId, List<long> seasonNumbers, long? epgChannelId)
        {
            List<DbReminder> reminders = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSeriesRemindersBySeasons");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@ids", seriesRemindersIds);
                sp.AddParameter("@seriesId", seriesId);
                sp.AddParameter("@seasonNumbers", seasonNumbers);
                sp.AddParameter("@epgChannelId", epgChannelId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    reminders = new List<DbReminder>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        reminders.Add(CreateDbSeriesReminder(row));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSeriesRemindersBySeasons. groupId: {0}. Error {1}", groupId, ex);
            }

            return reminders;
        }

        public static List<DbReminder> GetSeriesRemindersBySeriesIds(int groupId, List<long> seriesRemindersIds, List<string> seriesIds, long? epgChannelId)
        {
            List<DbReminder> reminders = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSeriesRemindersBySeriesIds");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@ids", seriesRemindersIds);
                sp.AddParameter("@seriesIds", seriesIds);
                sp.AddParameter("@epgChannelId", epgChannelId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    reminders = new List<DbReminder>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        reminders.Add(CreateDbSeriesReminder(row));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSeriesRemindersBySeriesIds. groupId: {0}. Error {1}", groupId, ex);
            }

            return reminders;
        }

        public static bool IsReminderRequired(int groupId, string seriesId, int seasonNumber, long epgChannelId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsReminderRequired");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@GroupID", groupId);
            sp.AddParameter("@SeriesId", seriesId);
            sp.AddParameter("@SeasonNumber", seasonNumber);
            sp.AddParameter("@ChannelId", epgChannelId);

            int rowsFound = sp.ExecuteReturnValue<int>();

            return rowsFound == 0;
        }

        public static List<DbSeriesReminder> GetSeriesReminderBySeries(int groupId, string seriesId, long seasonNum, string epgChannelId)
        {
            List<DbSeriesReminder> reminders = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSeriesReminderBySeries");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@seriesId", seriesId);
                sp.AddParameter("@seasonNum", seasonNum);
                sp.AddParameter("@epgChannelId", epgChannelId);
                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    reminders = new List<DbSeriesReminder>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        reminders.Add(CreateDbSeriesReminder(row));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSeriesReminderBySeries. groupId: {0}. Error {1}", groupId, ex);
            }

            return reminders;
        }

        public static List<DbSeriesReminder> GetSeriesReminders(int groupId, List<long> remindersIds)
        {
            List<DbSeriesReminder> result = null;

            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSeriesRemindersByIds");
                sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter<long>("@remindersIds", remindersIds, "id");

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    result = new List<DbSeriesReminder>();
                    DbSeriesReminder dbReminder = null;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        dbReminder = CreateDbSeriesReminder(row);
                        result.Add(dbReminder);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error at GetSeriesReminders. groupId: {0}. Error {1}", groupId, ex);
            }

            return result;
        }

        public static ulong IncreasePushCounter(int partnerId, int userId, bool withTTL)
        {
            ulong counter = 0;
            try
            {
                if (!withTTL)
                    counter = cbManager.Increment(GetUserPushKey(partnerId, userId), 1);
                else
                {
                    ulong docTTL = TCMClient.Settings.Instance.GetValue<ulong>("push_message.ttl_seconds");
                    if (docTTL == 0)
                        docTTL = TTL_USER_PUSH_COUNTER_DOCUMENT_SECONDS;

                    counter = cbManager.Increment(GetUserPushKey(partnerId, userId), 1, docTTL);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while trying to increase user push counter. partner ID: {0}, user ID: {1}, EX: {2}", partnerId, userId, ex);
            }
            return counter;
        }

        public static bool IsUserPushDocExists(int partnerId, int userId)
        {
            bool exists = false;
            try
            {
                if (cbManager.Get<string>(GetUserPushKey(partnerId, userId)) != null)
                    exists = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying check if user push document . key: {0}, ex: {1}", GetUserPushKey(partnerId, userId), ex);
            }

            return exists;
        }
    }
}






