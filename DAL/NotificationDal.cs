using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.Notification;
using CouchbaseManager;
using Newtonsoft.Json;
using KLogMonitor;
using System.Reflection;
using ApiObjects;


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

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.NOTIFICATION);

        private static string GetDeviceDataKey(int groupId, string udid)
        {
            return string.Format("device_data_{0}_{1}", groupId, udid);
        }

        private static string GetUserNotificationKey(int groupId, string userId)
        {
            return string.Format("user_notification_{0}_{1}", groupId, userId);
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

        public static bool UpdateNotificationPartnerSettings(int groupID, bool? push_notification_enabled, bool? push_system_announcements_enabled)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_NotificationPartnerSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            if (push_notification_enabled != null)
            {
                sp.AddParameter("@push_notification_enabled", push_notification_enabled);
            }
            if (push_system_announcements_enabled != null)
            {
                sp.AddParameter("@push_system_announcements_enabled", push_system_announcements_enabled);
            }
            sp.AddParameter("@date", DateTime.UtcNow);
            return sp.ExecuteReturnValue<bool>();
        }

        public static DataRow GetNotificationPartnerSettings(int groupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_NotificationPartnerSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            DataTable dt = sp.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                return dt.Rows[0];
            else
                return null;
        }

        public static bool UpdateNotificationSettings(int groupID, string userId, bool? push_notification_enabled)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_NotificationSettings");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
            sp.AddParameter("@groupID", groupID);
            sp.AddParameter("@userId", userId);
            if (push_notification_enabled != null)
            {
                sp.AddParameter("@push_notification_enabled", push_notification_enabled);
            }
            sp.AddParameter("@date", DateTime.UtcNow);
            sp.AddParameter("@updater_id", userId);

            return sp.ExecuteReturnValue<bool>();
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

        public static DataRow Get_MessageAnnouncement(int messageAnnouncementId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetMessageAnnouncement");
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

        public static int Insert_MessageAnnouncement(int groupId, int recipients, string name, string message, bool enabled, DateTime startTime, string timezone, int updaterId, string resultMsgId = null)
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
            int newTransactionID = spInsert.ExecuteReturnValue<int>();
            return newTransactionID;
        }

        public static void Update_MessageAnnouncement(int id, int groupId, int recipients, string name, string message, bool enabled, DateTime startTime, string timezone, int updaterId, string resultMsgId = null)
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
            spInsert.ExecuteDataSet();
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

        public static void Delete_MessageAnnouncement(int id, int groupId)
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
            sp.AddParameter("@status", status);
            DataSet ds = sp.ExecuteDataSet();
        }

        public static DeviceNotificationData GetDeviceNotificationData(int groupId, string udid)
        {
            DeviceNotificationData deviceData = null;
            try
            {
                string deviceString = cbManager.Get<string>(GetDeviceDataKey(groupId, udid));
                if (!string.IsNullOrEmpty(deviceString))
                    deviceData = JsonConvert.DeserializeObject<DeviceNotificationData>(deviceString);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get device notification. gid: {0}, udid: {1}, ex: {2}", groupId, udid, ex);
            }

            return deviceData;
        }

        public static UserNotification GetUserNotificationData(int groupId, string userId)
        {
            UserNotification userNotification = null;
            try
            {
                string userNotificationString = cbManager.Get<string>(GetUserNotificationKey(groupId, userId));
                if (!string.IsNullOrEmpty(userNotificationString))
                    userNotification = JsonConvert.DeserializeObject<UserNotification>(userNotificationString);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get user notification. gid: {0}, user ID: {1}, ex: {2}", groupId, userId, ex);
            }

            return userNotification;
        }

        public static bool SetUserNotificationData(int groupId, string userId, UserNotification userNotification)
        {
            bool result = false;
            try
            {
                result = cbManager.Set(GetUserNotificationKey(groupId, userId), JsonConvert.SerializeObject(userNotification));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while set user notification data. gid: {0}, user ID: {1}, ex: {2}", groupId, userId, ex);
            }

            return result;
        }

        public static string Get_AnnouncementExternalIdByRecipients(int recipients)
        {
            string ret = string.Empty;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAnnouncementExternalIdByRecipients");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");
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

        public static DataRowCollection Get_Announcement(List<eAnnouncementRecipientsType> recipientsTypes, List<long> announcementIds)
        {
            DataRowCollection rowCollection = null;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetAnnouncements");
            sp.SetConnectionKey("MESSAGE_BOX_CONNECTION_STRING");

            if (announcementIds != null && announcementIds.Count > 0)
                sp.AddIDListParameter<long>("@IDs", announcementIds, "Id");

            if (recipientsTypes != null && recipientsTypes.Count > 0)
                sp.AddIDListParameter<int>("@recipientTypes", recipientsTypes.Cast<int>().ToList(), "Id");

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    rowCollection = dt.Rows;
            }

            return rowCollection;
        }

        public static bool SetDeviceNotificationData(int groupId, string udid, DeviceNotificationData newDeviceNotificationData)
        {
            bool result = false;
            try
            {
                result = cbManager.Set(GetDeviceDataKey(groupId, udid), JsonConvert.SerializeObject(newDeviceNotificationData));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to set device notification. gid: {0}, udid: {1}, ex: {2}", groupId, udid, ex);
            }

            return result;
        }

      


    }
}
