using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Reflection;
using ApiObjects.Notification;
using System.Threading;
using System.Text.RegularExpressions;
using KLogMonitor;
using KlogMonitorHelper;
using Core.Users;

namespace Core.Notification
{
    /// <summary>
    /// Singleton class that responsible for executing the flow of
    /// notifications logic.
    /// </summary>
    public class NotificationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string deafultDateFormat = "dd/MM/yyyy HH:mm:ss";
        private const string deafultEmailDateFormat = "dd-MMM-yyyy";


        #region private Memebers
        private object lockObj = new object();
        private const string NOTIFICATION_MANAGER_LOG_FILE = "NotificationManager";
        #endregion

        #region private constructor
        private NotificationManager()
        {
        }
        #endregion

        #region public Methods

        /// <summary>
        /// Fetch notifictaion requests with status "NotStarted" from db (the number of requests determined according to numOfRequests param) 
        /// filtered by group id and update the fetched requests to status "InProgress".
        /// afterwards handle each request logic by calling HanldeOneRequest() method.
        /// </summary>
        /// <param name="numOfRequests"></param>
        /// <param name="groupID"></param>
        /// <param name="triggerType"> if 0 mean all notification</param>
        public void HandleRequests(int numOfRequests, long groupID)
        {
            try
            {
                log.Debug("HanldeRequests - Start : group_id = " + groupID.ToString());
                List<NotificationRequest> requestsList = null;
                int startRequestID = 0;
                int endRequestID = 0;

                lock (lockObj) // locks the operation of fetching bulk of requests from db and update their status to "InProgress".
                {
                    requestsList = GetNotificationRequests(numOfRequests, groupID, NotificationRequestStatus.NotStarted, ref startRequestID, ref endRequestID);
                    if (requestsList != null && requestsList.Count > 0)
                    {
                        log.Debug("HanldeRequests - Num Of Requests=" + numOfRequests.ToString() + ",GroupID=" + groupID.ToString());
                        List<long> requestsIDs = requestsList.Select(request => request.ID).ToList();
                        UpdateNotificationRequests(requestsIDs, NotificationRequestStatus.InProgress);
                    }
                }
                if (requestsList != null && requestsList.Count > 0)
                {
                    // save monitor and logs context data
                    ContextData contextData = new ContextData();

                    Task[] tasks = new Task[requestsList.Count];
                    //send push notification to device + send SMS
                    for (int i = 0; i < requestsList.Count; i++)
                    {
                        int j = i;
                        tasks[j] = Task.Factory.StartNew(() =>
                            {
                                // load monitor and logs context data
                                contextData.Load();

                                HandleOneRequest(requestsList[j]);
                            });
                    }
                    Task.WaitAll(tasks);
                }
                //Update all the request that was't Collected during the above
                UpdateNotRelevantNotificationRequests(startRequestID, endRequestID, NotificationRequestStatus.NotRelevant, groupID);

            }
            catch (Exception ex)
            {
                log.Error("HanldeRequests Exception - NotificationManager", ex);
            }
        }

        private void UpdateNotRelevantNotificationRequests(int startRequestID, int endRequestID, NotificationRequestStatus requestStatus, long groupID)
        {
            byte status = (Byte)(requestStatus);
            NotificationDal.UpdateNotificationRequestStatus(startRequestID, endRequestID, status, groupID);
        }


        /// <summary>
        /// Get Notification object by group id and  trigger type by querying
        /// notifications table at the db.        
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        public ApiObjects.Notification.Notification GetNotification(long groupID, NotificationTriggerType triggerType)
        {
            try
            {
                ApiObjects.Notification.Notification objNotification = null;

                DataTable dtNotification = null;
                switch (triggerType)
                {
                    case NotificationTriggerType.Renewal:
                    case NotificationTriggerType.PaymentFailure:
                        dtNotification = NotificationDal.GetNotifictaionByGroupAndTriggerType(groupID, (int)triggerType);
                        if (dtNotification != null && dtNotification.Rows.Count > 0)
                        {
                            objNotification = CreateNotificationObject(dtNotification.DefaultView[0]);
                        }
                        break;
                    default:
                        break;
                }

                return objNotification;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Faild creat Notification at GetNotification Exception = {0}. groupID={1},triggerType={1} ", ex.Message, groupID, triggerType));
                return null;
            }
        }

        /// <summary>
        /// Get all notifications that related to specific mediaID by tags
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="notificationTriggerType"></param>
        /// <param name="key"></param>
        /// <param name="nMediaID"></param>
        /// <returns></returns>
        /// 
        public List<FollowUpTagNotification> GetNotifications(int nGroupID, NotificationTriggerType notificationTriggerType, int nMediaID)
        {
            try
            {
                DataTable dtNotification = null;
                List<FollowUpTagNotification> lNotifications = new List<FollowUpTagNotification>();
                Dictionary<long, FollowUpTagNotification> dNotifications = new Dictionary<long, FollowUpTagNotification>();
                FollowUpTagNotification objNotification = null;

                log.Debug("GetNotifications - " + string.Format("Notification Follow Up, groupID:{0} , mediaID:{1}", nGroupID, nMediaID));

                # region Get All Tags+Tag Values Related To media

                DataTable dtMediaTags = DAL.NotificationDal.GetTagsNotificationByMedia(nMediaID, 1);    // get tags+tag values from TVinciDB (that their updatedate >= yesterday)
                Dictionary<int, List<int>> tagMediaDict = null;
                Dictionary<string, List<TagIDValue>> tempMediaTagsDict = null;//get the ID+ text values
                Dictionary<string, List<TagIDValue>> tagsDict = null;//get the ID+ text values
                TagIDValue tagsDictValue = null;

                if (dtMediaTags != null && dtMediaTags.DefaultView.Count > 0) //Run of all Media Tags
                {
                    foreach (DataRow row in dtMediaTags.Rows)
                    {
                        if (tagMediaDict == null)
                            tagMediaDict = new Dictionary<int, List<int>>();
                        if (tempMediaTagsDict == null)
                            tempMediaTagsDict = new Dictionary<string, List<TagIDValue>>();

                        int tag_type_id = ODBCWrapper.Utils.GetIntSafeVal(row["tag_type_id"]);
                        string sTag_type_id = ODBCWrapper.Utils.GetSafeStr(row["tag_type_id"]);
                        int tag_id = ODBCWrapper.Utils.GetIntSafeVal(row["tag_id"]);
                        string tag_type_name = ODBCWrapper.Utils.GetSafeStr(row["tag_type_name"]);
                        string tag_value = ODBCWrapper.Utils.GetSafeStr(row["value"]);
                        tagsDictValue = new TagIDValue(tag_type_name, tag_id, tag_value);

                        if (tagMediaDict.ContainsKey(tag_type_id))
                        {
                            tagMediaDict[tag_type_id].Add(tag_id);
                        }
                        else
                        {
                            tagMediaDict.Add(tag_type_id, new List<int> { tag_id });
                        }
                        if (tempMediaTagsDict.ContainsKey(sTag_type_id))
                        {
                            tempMediaTagsDict[sTag_type_id].Add(tagsDictValue);
                        }
                        else
                        {
                            tempMediaTagsDict.Add(sTag_type_id, new List<TagIDValue> { tagsDictValue });
                        }
                    }
                }

                if (tagMediaDict == null) // there was no notification to add
                    return null;
                #endregion

                #region Build List of FollowUpByTag notifications

                dtNotification = NotificationDal.GetNotifictaionsByTags(nGroupID, (int)notificationTriggerType, tagMediaDict); // get all notifications that related to one tag (at least) from the dictionary above

                var oNotificationIds = from row in dtNotification.AsEnumerable()
                                       select row.Field<long>("notification_id");
                var oListNotificationIds = oNotificationIds.ToList();
                List<long> lNotificationIds = new List<long>();
                foreach (var vNId in oListNotificationIds)
                {
                    long nId = ODBCWrapper.Utils.GetIntSafeVal(vNId);
                    if (!lNotificationIds.Contains(nId))
                        lNotificationIds.Add(nId);
                }

                if (dtNotification != null && dtNotification.DefaultView.Count > 0) //Run of all notifications (that include tag/s from media)
                {
                    for (int i = 0; i < dtNotification.DefaultView.Count; i++)
                    {
                        long notification_id = ODBCWrapper.Utils.GetLongSafeVal(dtNotification.Rows[i]["notification_id"]);
                        if (!dNotifications.Keys.Contains<long>(notification_id))
                        {
                            int notification_type = ODBCWrapper.Utils.GetIntSafeVal(dtNotification.Rows[i]["notification_type"]);
                            string message_text = ODBCWrapper.Utils.GetSafeStr(dtNotification.Rows[i]["message_text"]);
                            string sms_message_text = ODBCWrapper.Utils.GetSafeStr(dtNotification.Rows[i]["sms_message_text"]);
                            string pull_message_text = ODBCWrapper.Utils.GetSafeStr(dtNotification.Rows[i]["pull_message_text"]);
                            string title = ODBCWrapper.Utils.GetSafeStr(dtNotification.Rows[i]["title"]);
                            int status = ODBCWrapper.Utils.GetIntSafeVal(dtNotification.Rows[i]["status"]);
                            int is_broadcast = ODBCWrapper.Utils.GetIntSafeVal(dtNotification.Rows[i]["is_broadcast"]);
                            bool bIs_broadcast = false;
                            if (is_broadcast != 0)
                                bIs_broadcast = true;

                            DateTime create_date = ODBCWrapper.Utils.GetDateSafeVal(dtNotification.Rows[i]["create_date"]);
                            string sKey = ODBCWrapper.Utils.GetSafeStr(dtNotification.Rows[i]["sKey"]); // == tag_type_id
                            int nKey = ODBCWrapper.Utils.GetIntSafeVal(dtNotification.Rows[i]["sKey"]); // == tag_type_id
                            long group_id = ODBCWrapper.Utils.GetLongSafeVal(dtNotification.Rows[i]["group_id"]);

                            List<int> lValues = null;
                            bool addAllMediaValues = false;
                            //If column is null - take the tag values from the MEDIA !! 
                            //If column is NOT null - take value only if exsits both media and notification 
                            //if (ODBCWrapper.Utils.GetIntSafeVal(dtNotification.Rows[i]["value"]) != 0)
                            // {
                            DataRow[] drValues = dtNotification.Select("notification_id = " + notification_id.ToString());
                            lValues = new List<int>();
                            tagsDict = new Dictionary<string, List<TagIDValue>>();
                            if (drValues != null && drValues.Count() > 0)
                            {
                                foreach (DataRow item in drValues) // all tag values per tag
                                {
                                    //Check if tagType+TagValue equel to media tag values
                                    int nTagValue = ODBCWrapper.Utils.GetIntSafeVal(item["value"]);
                                    if (nTagValue == 0)
                                    {
                                        // add at the end of loop - all values of media !!!!
                                        addAllMediaValues = true;
                                        tagsDict = GetAllTagsFromMedia(tagMediaDict, tempMediaTagsDict, tagsDict, sKey, nKey, lValues);
                                    }
                                    if (!addAllMediaValues)
                                    {
                                        if (tagMediaDict.ContainsKey(nKey) && tagMediaDict[nKey].Contains(nTagValue)) //exect match 
                                        {
                                            if (!lValues.Contains(nTagValue))
                                                lValues.Add(nTagValue);
                                        }

                                        List<TagIDValue> lDicValue = tempMediaTagsDict[sKey]; // get all values by key forom media tags Dictionary
                                        List<TagIDValue> lTagIDValue = new List<TagIDValue>();
                                        foreach (TagIDValue tagIDValue in lDicValue)
                                        {
                                            if (tagIDValue.tagValueId == ODBCWrapper.Utils.GetIntSafeVal(item["value"])) // exect match 
                                            {
                                                TagIDValue tagIDValueFind = lTagIDValue.Find(delegate(TagIDValue t)
                                                {
                                                    return (t.tagValueId == tagIDValue.tagValueId && t.tagTypeName == tagIDValue.tagTypeName && t.tagValueName == tagIDValue.tagValueName);
                                                }
                                                      );
                                                if (tagIDValueFind == null)
                                                    lTagIDValue.Add(tagIDValue);
                                            }
                                        }

                                        if (tagsDict == null)
                                            tagsDict = new Dictionary<string, List<TagIDValue>>();

                                        if (!tagsDict.ContainsKey(sKey))
                                        {
                                            tagsDict.Add(sKey, lTagIDValue);
                                        }
                                    }
                                }
                            }
                            if (drValues == null || drValues.Count() == 0) // take the tag values from the MEDIA !! 
                            {
                                tagsDict = GetAllTagsFromMedia(tagMediaDict, tempMediaTagsDict, tagsDict, sKey, nKey, lValues);
                            }
                            // }
                            //Add notification to list only if necessary
                            if (lValues != null && lValues.Count > 0 && tagsDict != null && tagsDict.Count > 0)
                            {
                                NotificationTag oNotificationTag = new NotificationTag(nMediaID, lValues, tagsDict);
                                oNotificationTag.notificationsID = lNotificationIds;
                                objNotification = new FollowUpTagNotification(notification_id, (NotificationMessageType)notification_type, (int)NotificationTriggerType.FollowUpByTag, group_id, message_text, sms_message_text, pull_message_text, title, true, status, bIs_broadcast, create_date, null, sKey, oNotificationTag);
                                dNotifications.Add(notification_id, objNotification);
                                log.Debug("GetNotifications - " + string.Format("Notification Insert Media to tempTable , groupID:{0} , mediaID:{1}", nGroupID, nMediaID));
                            }
                        }
                    }
                    lNotifications = dNotifications.Values.ToList<FollowUpTagNotification>();
                }
                else
                {
                    lNotifications = null;
                }
                #endregion

                return lNotifications;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed create List<FollowUpTagNotification>  at GetNotifications , Exception ={0}, groupID={1}, triggerType={2}, mediaID={3}", ex.Message, nGroupID, notificationTriggerType, nMediaID));
                return null;
            }
        }

        private static Dictionary<string, List<TagIDValue>> GetAllTagsFromMedia(Dictionary<int, List<int>> tagMediaDict, Dictionary<string, List<TagIDValue>> tempMediaTagsDict, Dictionary<string, List<TagIDValue>> tagsDict, string sKey, int nKey, List<int> lValues)
        {
            if (tagMediaDict.ContainsKey(nKey)) //notification parameter to ALL 
            {
                foreach (int nValue in tagMediaDict[nKey])
                {
                    if (lValues == null)
                        lValues = new List<int>();

                    if (!lValues.Contains(nValue))
                        lValues.Add(nValue);
                }
            }
            if (tempMediaTagsDict.ContainsKey(sKey))
            {
                if (tagsDict == null)
                    tagsDict = new Dictionary<string, List<TagIDValue>>();

                if (!tagsDict.ContainsKey(sKey))
                {
                    // get all values by key forom media tagws Dictionary
                    tagsDict.Add(sKey, tempMediaTagsDict[sKey]);
                }
                else
                {
                    foreach (TagIDValue tagIDValue in tempMediaTagsDict[sKey])
                    {
                        TagIDValue tagIDValueFind = tagsDict[sKey].Find(delegate(TagIDValue t)
                        {
                            return (t.tagValueId == tagIDValue.tagValueId && t.tagTypeName == tagIDValue.tagTypeName && t.tagValueName == tagIDValue.tagValueName);
                        }
                              );
                        if (tagIDValueFind == null)
                            tagsDict[sKey].Add(tagIDValue);
                    }
                }

            }
            return tagsDict;
        }

        /// <summary>
        /// Insert one notifictaion reques to the db (notifications_requests table),
        /// according to NotificationRequest param.
        /// </summary>
        /// <param name="request"></param>
        public void InsertNotificationRequest(NotificationRequest request)
        {
            try
            {
                InsertNotificationRequest(request.Type, request.GroupID, request.UserID, request.NotificationID, request.Status, request.TriggerType);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed to InsertNotificationRequest Exception = {0}, requestType={1}, requestGroupID={2}, requestUserID={3}, requestNotificationID={4}, requestStatus={5}, requestTriggerType={6}",
                    ex.Message, request.Type, request.GroupID, request.UserID, request.NotificationID, request.Status, request.TriggerType));
            }
        }


        /// <summary>
        /// Insert one notifictaion reques to the db (notifications_requests table),
        /// according to the params of the methods that represents the relevant fields at notifications_requests table.
        /// </summary>
        /// <param name="requestType"></param>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="notificationID"></param>
        /// <param name="status"></param>
        public void InsertNotificationRequest(NotificationRequestType requestType, long groupID, long userID, long notificationID, NotificationRequestStatus status, NotificationTriggerType triggerType)
        {
            NotificationDal.InsertNotificationRequest((int)requestType, groupID, userID, notificationID, (byte)status, (int)triggerType);
        }

        /// <summary>
        /// Insert notification to DB
        /// </summary>
        /// <param name="notification"></param>
        public void InsertNotificationTagRequest(NotificationTagRequest notification)
        {
            try
            {
                //to json
                string json = TVinciShared.JSONUtils.ToJSON(notification.oExtraParams);
                NotificationDal.InsertNotificationTagRequest((int)notification.Type, notification.GroupID, notification.NotificationID, (int)notification.Status, notification.usersID, json, (int)notification.TriggerType, notification.oExtraParams.mediaID);
                //insert document to couchbase
                //TvinciNotificationBL notificationBL = new TvinciNotificationBL(notification.GroupID);
                //notificationBL.InsertNotificationRequest(notification);               
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed InsertNotificationTagRequest Exception = {0}", ex.Message));
            }
        }
        /// <summary>
        /// Insert all notifications as a datatable in one insert to DB
        /// </summary>
        /// <param name="notification"></param>
        public void InsertNotificationTagRequest(DataTable notification)
        {
            try
            {
                NotificationDal.InsertNotificationTagRequest(notification);
                try
                {
                    log.Debug("InsertNotificationTagRequest - " + string.Format("Insert to DB {0} recoreds ", notification.Rows.Count));
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed InsertNotificationTagRequest Exception = {0}", ex.Message));
            }
        }
        /// <summary>
        /// Create NotificationRequest object according to Notification object and userID.
        /// </summary>
        /// <param name="objNotitfication"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public NotificationRequest BuildNotificationRequest(ApiObjects.Notification.Notification objNotitfication, string siteGuid)
        {
            try
            {
                long userID = 0;
                try
                {
                    userID = long.Parse(siteGuid);
                }
                catch
                {
                }

                NotificationRequest request;
                switch (objNotitfication.TriggerType)
                {
                    case (int)NotificationTriggerType.PaymentFailure:
                    case (int)NotificationTriggerType.Renewal:
                        request = new NotificationRequest(objNotitfication, userID);
                        return request;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed BuildNotificationRequest Exception = {0}", ex.Message));
                return null;
            }
        }

        /// <summary>
        ///  Build one notification request for tag notifications 
        /// </summary>
        /// <param name="lNotifications"></param>
        /// <returns></returns>
        /// 
        public DataTable BuildTagNotificationsRequest(List<FollowUpTagNotification> lNotifications)
        {
            try
            {
                DataTable dtNotification = null;
                NotificationTagRequest objRequest = null;
                FollowUpTagNotification objNotification = null;
                Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
                Dictionary<string, List<TagIDValue>> dictfull = new Dictionary<string, List<TagIDValue>>();
                List<long> notificationsIds = new List<long>();
                #region userPArameters
                ExtraParams userExtraParams = null;
                Dictionary<string, List<TagIDValue>> userDictFull = null;
                Dictionary<string, List<int>> userDict = null;
                List<long> userNotificationsIds = null;
                #endregion
                if (lNotifications != null && lNotifications.Count > 0)
                {
                    log.Debug("BuildTagNotificationsRequest - Start");

                    //Build basic  notification request 
                    objNotification = new FollowUpTagNotification(lNotifications[0].ID, lNotifications[0].MessageType, lNotifications[0].TriggerType, lNotifications[0].GroupID, lNotifications[0].MessageText, lNotifications[0].SmsMessageText,
                        lNotifications[0].PullMessageText, lNotifications[0].Title, lNotifications[0].IsActive, lNotifications[0].Status, lNotifications[0].IsBroadcast, lNotifications[0].CreatedDate, lNotifications[0].Actions, string.Empty, null);

                    log.Debug("BuildTagNotificationsRequest - " + string.Format("objNotification ID={0},MessageText={1},GroupID={2},CreatedDate={3}  ", objNotification.ID, objNotification.MessageText, objNotification.GroupID, objNotification.CreatedDate));

                    #region build notification request dictionary

                    foreach (FollowUpTagNotification objN in lNotifications)
                    {
                        notificationsIds.Add(objN.ID);
                        if (objN.notificationTag != null)
                        {
                            string sKey = ODBCWrapper.Utils.GetSafeStr(objN.key);
                            if (dict.ContainsKey(sKey))
                            {
                                dict[sKey] = objN.notificationTag.tagValues;
                                if (objN.notificationTag.tagValueDict != null && objN.notificationTag.tagValueDict[sKey] != null)
                                    dictfull[sKey] = objN.notificationTag.tagValueDict[sKey];
                                else
                                    dictfull[sKey] = null;
                            }
                            else
                            {
                                dict.Add(sKey, null);
                                dict[sKey] = objN.notificationTag.tagValues;
                                dictfull.Add(sKey, null);
                                if (objN.notificationTag.tagValueDict != null)
                                    dictfull[sKey] = objN.notificationTag.tagValueDict[sKey];
                            }
                        }
                    }
                    #endregion

                    objRequest = new NotificationTagRequest(objNotification);
                    objRequest.oExtraParams = new ExtraParams();
                    objRequest.oExtraParams.TagDict = dict;
                    objRequest.oExtraParams.dTagDict = dictfull;
                    objRequest.oExtraParams.mediaID = lNotifications[0].notificationTag.mediaID;
                    objRequest.oExtraParams.notificationsID = lNotifications[0].notificationTag.notificationsID;

                    log.Debug("BuildTagNotificationsRequest - " + string.Format(" MediaID={0}", objRequest.oExtraParams.mediaID));

                    #region get all sign in users to one of the notification ids
                    DataTable dtUsers = DAL.NotificationDal.GetUserNotification(null, notificationsIds);
                    if (dtUsers != null && dtUsers.DefaultView.Count > 0)
                    {
                        long lUserID = 0;
                        objRequest.usersID = new List<long>();
                        for (int i = 0; i < dtUsers.DefaultView.Count; i++)
                        {
                            lUserID = ODBCWrapper.Utils.GetLongSafeVal(dtUsers.Rows[i]["user_id"]);
                            if (!objRequest.usersID.Contains(lUserID))
                                objRequest.usersID.Add(lUserID);
                        }
                    }
                    #endregion

                    #region Create specific notification per user
                    //get to each User the specific notification that he subscribe to it ([GetUserNotification])
                    DataTable userSubscribe = DAL.NotificationDal.GetUserNotification(objRequest.usersID);
                    //create specific NotificationTagRequest to each user
                    dtNotification = GetNotificationTagRequestTable();

                    int previousUserId = 0;
                    foreach (DataRow drUserSubscribe in userSubscribe.Rows)
                    {
                        if (previousUserId != ODBCWrapper.Utils.GetIntSafeVal(drUserSubscribe["user_id"]))
                        {
                            log.Debug("BuildTagNotificationsRequest - " + string.Format(" UserID={0}", previousUserId));
                            userDictFull = new Dictionary<string, List<TagIDValue>>();
                            userDict = new Dictionary<string, List<int>>();
                            userNotificationsIds = new List<long>();

                            previousUserId = ODBCWrapper.Utils.GetIntSafeVal(drUserSubscribe["user_id"]);
                            DataRow[] drSubscibes = userSubscribe.Select("user_id=" + previousUserId);
                            //need to find the user subscribe that match the media tags 
                            foreach (DataRow subscribe in drSubscibes)
                            {
                                string sKey = ODBCWrapper.Utils.GetSafeStr(subscribe["sKey"]);
                                int nUserTagValue = 0;
                                string sUserTagValue = string.Empty;
                                int nNotificationID = 0;
                                List<TagIDValue> tagIDValueList = null;
                                if (objRequest.oExtraParams.dTagDict.ContainsKey(sKey)) //key must be contains in dictFull (which created before)
                                {
                                    tagIDValueList = dictfull[sKey];
                                    if (tagIDValueList != null)
                                    {
                                        foreach (TagIDValue tagIDValue in tagIDValueList)
                                        {
                                            nUserTagValue = ODBCWrapper.Utils.GetIntSafeVal(subscribe["value"]);
                                            sUserTagValue = ODBCWrapper.Utils.GetSafeStr(subscribe["value"]);
                                            nNotificationID = ODBCWrapper.Utils.GetIntSafeVal(subscribe["notification_id"]);
                                            if (tagIDValue.tagValueId == nUserTagValue)
                                            {
                                                if (objRequest.oExtraParams.TagDict.ContainsKey(sKey) && objRequest.oExtraParams.TagDict[sKey].Contains(nUserTagValue)) //match !
                                                {

                                                    if (!userDict.ContainsKey(sKey))
                                                    {
                                                        userDict.Add(sKey, new List<int> { nUserTagValue });
                                                        userNotificationsIds.Add(nNotificationID);
                                                    }
                                                    else if (!userDict[sKey].Contains(nUserTagValue))
                                                        userDict[sKey].Add(nUserTagValue);


                                                    if (!userDictFull.ContainsKey(sKey))
                                                    {
                                                        userDictFull.Add(sKey, objRequest.oExtraParams.dTagDict[sKey]);
                                                    }
                                                    else
                                                    {
                                                        //get all tagValues that user subscribe to 
                                                        TagIDValue tagIDValueFind = userDictFull[sKey].Find(delegate(TagIDValue t)
                                                        {
                                                            return (t.tagValueId == tagIDValue.tagValueId && t.tagTypeName == tagIDValue.tagTypeName && t.tagValueName == tagIDValue.tagValueName);
                                                        }
                                                              );
                                                        if (tagIDValueFind == null)
                                                            userDictFull[sKey].Add(tagIDValue);
                                                    }
                                                }
                                            }
                                            else if (nUserTagValue == 0) //User subscribe to all values 
                                            {
                                                if (!userDict.ContainsKey(sKey)) //notification parameter to ALL 
                                                {
                                                    userDict.Add(sKey, objRequest.oExtraParams.TagDict[sKey]);
                                                    userNotificationsIds.Add(nNotificationID);
                                                }
                                                else
                                                {
                                                    foreach (int value in objRequest.oExtraParams.TagDict[sKey])
                                                    {
                                                        if (!userDict[sKey].Contains(value))
                                                            userDict[sKey].Add(value);
                                                    }
                                                }

                                                if (!userDictFull.ContainsKey(sKey))
                                                {
                                                    userDictFull.Add(sKey, objRequest.oExtraParams.dTagDict[sKey]);
                                                }
                                                else
                                                {
                                                    TagIDValue tagIDValueFind = userDictFull[sKey].Find(delegate(TagIDValue t)
                                                    {
                                                        return (t.tagValueId == tagIDValue.tagValueId && t.tagTypeName == tagIDValue.tagTypeName && t.tagValueName == tagIDValue.tagValueName);
                                                    }
                                                              );
                                                    if (tagIDValueFind == null)
                                                        userDictFull[sKey].Add(tagIDValue);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (userNotificationsIds != null && userNotificationsIds.Count > 0)
                            {
                                userExtraParams = new ExtraParams();
                                userExtraParams.TagDict = userDict;
                                userExtraParams.dTagDict = userDictFull;
                                userExtraParams.mediaID = lNotifications[0].notificationTag.mediaID;
                                userExtraParams.notificationsID = userNotificationsIds;
                                //insert this media to notification 
                                FillNotificationDataTable(objNotification, userExtraParams, dtNotification, previousUserId);
                                string sUserExtraParams = string.Empty;
                                try
                                {
                                    sUserExtraParams = TVinciShared.JSONUtils.ToJSON(userExtraParams);
                                }
                                catch (Exception ex)
                                {
                                    log.Error("", ex);
                                }
                                log.Debug("BuildTagNotificationsRequest - " + string.Format("insert user notification request user_id ={0}, extraParams={1}", previousUserId, sUserExtraParams));
                            }
                        }
                    }
                    #endregion
                }
                // return objRequest;
                return dtNotification;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed BuildTagNotificationsRequest Exception={0}", ex.Message));
                return null;
            }
        }
        private static void FillNotificationDataTable(FollowUpTagNotification objNotification, ExtraParams userExtraParams, DataTable dtNotification, int previousUserId)
        {
            DataRow userRow = dtNotification.NewRow();
            userRow["notification_request_type"] = objNotification.TriggerType;
            userRow["group_id"] = objNotification.GroupID;
            userRow["user_id"] = previousUserId;
            userRow["notification_id"] = userExtraParams.notificationsID[0];
            userRow["status"] = objNotification.Status;
            userRow["created_date"] = objNotification.CreatedDate;
            userRow["parameters"] = TVinciShared.JSONUtils.ToJSON(userExtraParams);
            userRow["trigger_type"] = (int)NotificationTriggerType.FollowUpByTag;
            userRow["media_id"] = userExtraParams.mediaID;

            dtNotification.Rows.Add(userRow);
        }

        private DataTable GetTagsNameByIDs(DataRow[] drtagDict, long groupID)
        {
            try
            {
                Dictionary<int, List<int>> tags = new Dictionary<int, List<int>>();
                int key = 0;
                int value = 0;
                int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(groupID);
                foreach (DataRow item in drtagDict)
                {
                    key = ODBCWrapper.Utils.GetIntSafeVal(item["sKey"]);
                    value = ODBCWrapper.Utils.GetIntSafeVal(item["value"]);
                    if (tags.ContainsKey(key))
                    {
                        tags[key].Add(value);
                    }
                    else
                    {
                        tags.Add(key, new List<int>() { value });
                    }
                }

                DataTable dtTagName = DAL.NotificationDal.GetTagsNameByIDs(nGroupID, tags);
                return dtTagName;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed GetTagsNameByIDs Exception={0}", ex.Message));
                return null;
            }
        }
        #region notification per user
        private DataTable GetNotificationTagRequestTable()
        {
            try
            {
                DataTable dtNotification = new DataTable();
                dtNotification.Columns.Add("notification_request_type", typeof(int));
                dtNotification.Columns.Add("group_id", typeof(long));
                dtNotification.Columns.Add("user_id", typeof(long));
                dtNotification.Columns.Add("notification_id", typeof(long));
                dtNotification.Columns.Add("status", typeof(int));
                dtNotification.Columns.Add("created_date", typeof(DateTime));
                dtNotification.Columns.Add("parameters", typeof(string));
                dtNotification.Columns.Add("trigger_type", typeof(int));
                dtNotification.Columns.Add("media_id", typeof(string));

                return dtNotification;
            }
            catch (Exception ex)
            {
                log.Error("GetNotificationTagRequestTable - create DataTable failed", ex);
                return null;
            }
        }
        #endregion

        #region notification per user
        //private DataTable GetNotificationTagRequestTable()
        //{
        //    try
        //    {
        //        DataTable dtNotification = new DataTable();

        //        dtNotification.Columns.Add("id", typeof(long));
        //        dtNotification.Columns.Add("notificationID", typeof(long));
        //        dtNotification.Columns.Add("status", typeof(NotificationRequestStatus));
        //        dtNotification.Columns.Add("userID", typeof(long));
        //        dtNotification.Columns.Add("createdDate", typeof(DateTime));
        //        dtNotification.Columns.Add("type", typeof(NotificationRequestType));
        //        dtNotification.Columns.Add("messageType", typeof(NotificationMessageType));
        //        dtNotification.Columns.Add("messageText", typeof(string));
        //        dtNotification.Columns.Add("smsMessageText", typeof(string));
        //        dtNotification.Columns.Add("pullMessageText", typeof(string));
        //        dtNotification.Columns.Add("title", typeof(string));
        //        dtNotification.Columns.Add("actions", typeof(NotificationRequestAction[]));
        //        //dtNotification.Columns.Add("dTagDict", typeof(Dictionary<string, List<int>>));
        //        //dtNotification.Columns.Add("nMediaID", typeof(int));
        //        //dtNotification.Columns.Add("sPicURL", typeof(string));
        //        //dtNotification.Columns.Add("sTemplateEmail", typeof(string));
        //        dtNotification.Columns.Add("oExtraParams", typeof(ExtraParams));
        //        dtNotification.Columns.Add("triggerType", typeof(int));
        //        dtNotification.Columns.Add("groupID" , typeof(long));
        //        dtNotification.Columns.Add("isActive", typeof(bool));
        //        dtNotification.Columns.Add("isBroadcast", typeof(bool));

        //        //dtNotification.Columns.Add("notificationsID", typeof(List<long>));
        //        return dtNotification;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }  
        //}
        #endregion

        /// <summary>
        /// Get notifications messages by device UDID and userID
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="sDeviceUDID"></param>
        /// <param name="messageType"></param>
        /// <param name="status"></param>
        /// <param name="messageCount"></param>
        /// <returns></returns>      
        public List<NotificationMessage> GetDeviceNotifications(long groupID, string siteGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, int? messageCount)
        {
            try
            {
                List<NotificationMessage> res = null;
                long userID = 0;
                int intUserID = 0;
                try
                {
                    userID = long.Parse(siteGuid);
                    intUserID = int.Parse(siteGuid);
                }
                catch
                {
                }
                long DeviceID = 0;
                if (!string.IsNullOrEmpty(sDeviceUDID))
                {
                    DataTable dtDevicetoUses = UsersDal.GetDevicesToUsers(groupID, userID);
                    foreach (DataRow dr in dtDevicetoUses.Rows)
                    {
                        if (dr["device_udid"].ToString() == sDeviceUDID)
                        {
                            long.TryParse(dr["device_id"].ToString(), out DeviceID);
                        }
                    }
                }
                byte? tempViewStatus = null;
                if (viewStatus != NotificationMessageViewStatus.All)
                    tempViewStatus = (byte)(viewStatus);

                DataTable dtDeviceNotification = NotificationDal.GetDeviceNotification(groupID, userID, DeviceID, tempViewStatus, messageCount, (int)notificationType);
                if (dtDeviceNotification != null && dtDeviceNotification.Rows.Count > 0)
                {
                    res = new List<NotificationMessage>();
                    foreach (DataRow dr in dtDeviceNotification.Rows)
                    {
                        long notificationMessageID = ODBCWrapper.Utils.GetLongSafeVal(dr["notificationMessageID"].ToString());
                        long notificationID = ODBCWrapper.Utils.GetLongSafeVal(dr["notificationID"].ToString());
                        long notificationRequestID = ODBCWrapper.Utils.GetLongSafeVal(dr["notificationRequestID"].ToString());
                        long nGroupID = ODBCWrapper.Utils.GetLongSafeVal(dr["group_id"].ToString());
                        string messageText = ODBCWrapper.Utils.GetSafeStr(dr["messageText"].ToString());
                        string title = ODBCWrapper.Utils.GetSafeStr(dr["title"].ToString());
                        DateTime publishDate = ODBCWrapper.Utils.GetDateSafeVal(dr["publishDate"].ToString());
                        string appName = ODBCWrapper.Utils.GetSafeStr(dr["appName"].ToString());
                        NotificationMessageType NotificationType = (NotificationMessageType)Enum.Parse(typeof(NotificationMessageType), ODBCWrapper.Utils.GetSafeStr(dr["notificationType"].ToString()));
                        NotificationMessageViewStatus ViewStatus = (NotificationMessageViewStatus)Enum.Parse(typeof(NotificationMessageViewStatus), ODBCWrapper.Utils.GetSafeStr(dr["viewStatus"].ToString()));
                        NotificationMessageStatus nms = (NotificationMessageStatus)Enum.Parse(typeof(NotificationMessageStatus), ODBCWrapper.Utils.GetSafeStr(dr["NotificationMessageStatus"].ToString()));
                        string sTagParams = ODBCWrapper.Utils.GetSafeStr(dr["parameters"].ToString());
                        ExtraParams tagParams = null;
                        if (!string.IsNullOrEmpty(sTagParams))
                        {
                            //tagParams = TVinciShared.JSONUtils.JsonToObject<ExtraParams>(sTagParams);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            tagParams = js.Deserialize<ExtraParams>(sTagParams);
                            //Dictionary<string, List<string>> tagNames = GetTagsNameByIDs(tagParams.TagDict, int.Parse(nGroupID.ToString()));
                        }
                        NotificationMessage temp = new NotificationMessage(NotificationType, notificationID, notificationRequestID, notificationMessageID, userID, nms, messageText, title, publishDate, appName, DeviceID, sDeviceUDID, null, ViewStatus, tagParams, nGroupID);
                        res.Add(temp);
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDeviceNotifications Exception = {0}", ex.Message));
                return null;
            }
        }

        public bool SetNotificationMessageViewStatus(string siteGuid, long? notificationRequestID, long? notificationMessageID, NotificationMessageViewStatus viewStatus, int nGroupID)
        {
            try
            {
                long lUserID = 0;
                if (viewStatus != NotificationMessageViewStatus.All && !string.IsNullOrEmpty(siteGuid) && Int64.TryParse(siteGuid, out lUserID) && lUserID > 0)
                {
                    long lNotificationRequestID = 0;
                    if (IsRequestToMarkPushOrPullNotificationAsRead(lUserID, notificationRequestID, notificationMessageID, viewStatus, ref lNotificationRequestID))
                    {
                        /*
                         * When a device sends us that a user read a push/pull notification we need:
                         *  a. Update for all devices assigned to user that he read this notification so he won't get different
                         *     badge numbers (badge number == the number of notifications pending appearing on the device application
                         *     icon) in different devices.
                         *  b. Send a new push notification to update the badge number on his application icon.
                         */

                        return HandleDeviceNotificationReadEvent(lUserID, lNotificationRequestID, viewStatus, nGroupID);
                    }
                    return NotificationDal.UpdateNotificationMessageViewStatus(lUserID, notificationRequestID, notificationMessageID, (byte)viewStatus);
                }
            }
            catch (Exception ex)
            {
                #region Logging
                log.Error(string.Format("Failed SetNotificationMessageViewStatus Exception = {0} , Site Guid {1} , Notification request ID: {2} , Notification Msg ID: {3} , View Status: {4}", ex.Message, siteGuid, notificationRequestID.HasValue ? notificationRequestID.Value.ToString() : "null", notificationMessageID.HasValue ? notificationMessageID.Value.ToString() : "null", viewStatus.ToString()));
                #endregion
            }
            return false;
        }

        private bool IsRequestToMarkPushOrPullNotificationAsRead(long lUserID, long? lNotificationRequestID, long? lNotificationMsgID, NotificationMessageViewStatus viewStatus, ref long lNonNullableNotificationRequestID)
        {
            byte bytMessageType = Byte.MaxValue;
            NotificationMessageType eMsgType = NotificationMessageType.All;
            if (viewStatus == NotificationMessageViewStatus.Read && (lNotificationRequestID != null || lNotificationMsgID != null) && NotificationDal.Get_NotificationMessageTypeAndRequestID(lUserID, lNotificationMsgID, lNotificationRequestID, ref bytMessageType, ref lNonNullableNotificationRequestID) && lNonNullableNotificationRequestID > 0 && bytMessageType != Byte.MaxValue && Enum.IsDefined(typeof(NotificationMessageType), bytMessageType))
            {
                eMsgType = (NotificationMessageType)bytMessageType;
                if (eMsgType == NotificationMessageType.Push || eMsgType == NotificationMessageType.Pull)
                    return true;
            }
            return false;

        }

        private bool HandleDeviceNotificationReadEvent(long lSiteGuid, long lNotificationRequestID, NotificationMessageViewStatus eNewViewStatus, int nGroupID)
        {
            if (NotificationDal.UpdateNotificationMessageViewStatus(lSiteGuid, lNotificationRequestID, null, (byte)eNewViewStatus))
            {
                #region Logging
                log.Debug("HandleDeviceNotificationReadEvent - " + string.Format("Updating view status in DB successful. Site Guid: {0} , Notification request id: {1} , View status: {2}", lSiteGuid, lNotificationRequestID, eNewViewStatus.ToString()));
                #endregion
                UpdateBadgeAtUserDevices(lSiteGuid, nGroupID);
                return true;
            }
            return false;
        }

        private void UpdateBadgeAtUserDevices(long lSiteGuid, int nGroupID)
        {
            NotificationRequest oRequestForBadgeUpdate = CreateNotificationRequestForBadgeUpdate(lSiteGuid, nGroupID);
            BadgeUpdateInner(oRequestForBadgeUpdate);
        }

        private NotificationRequest CreateNotificationRequestForBadgeUpdate(long lSiteGuid, int nGroupID)
        {
            NotificationRequest nr = new NotificationRequest(0, 0, NotificationRequestStatus.NotStarted, nGroupID, lSiteGuid, DateTime.UtcNow, NotificationRequestType.Simple, NotificationMessageType.Push, string.Empty, string.Empty, string.Empty, string.Empty, null, NotificationTriggerType.BadgeUpdate);
            nr.sendVia.is_email = 0;
            nr.sendVia.is_sms = 0;
            nr.sendVia.is_device = 1;

            return nr;
        }

        private void BadgeUpdateInner(NotificationRequest oNotificationRequestObj)
        {
            if (oNotificationRequestObj != null)
            {
                try
                {
                    #region Logging Start
                    log.Debug("BadgeUpdateInner - " + string.Format("Entering try block: Managed Thread ID: {0} , NotificationRequest object: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, oNotificationRequestObj.ToString()));
                    #endregion
                    IFactoryImp f = new FactoryImp(oNotificationRequestObj.GroupID);
                    IRequestImp imp = f.GetTypeImp(eSenderObjectType.Device);
                    imp.Send(oNotificationRequestObj);
                    #region Logging Success
                    log.Debug("BadgeUpdateInner - " + string.Format("Try block completed successfully: Managed Thread ID: {0} , NotificationRequest object: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, oNotificationRequestObj.ToString()));
                    #endregion
                }
                catch (Exception ex)
                {
                    #region Logging
                    log.Error("BadgeUpdateInner - " + string.Format("Exception. Exception msg: {0} , Managed thread ID: {1} , NotificationRequest obj: {2}", ex.Message, System.Threading.Thread.CurrentThread.ManagedThreadId, oNotificationRequestObj.ToString()), ex);
                    #endregion
                }
            }
            else
            {
                #region Logging
                log.Debug("BadgeUpdateInner - " + string.Format("Incorrect format of input. Managed thread ID: {0} , Input is null", System.Threading.Thread.CurrentThread.ManagedThreadId));
                #endregion
            }
        }

        public bool FollowUpByTag(string siteGuid, Dictionary<string, List<string>> tags, int groupID)
        {
            try
            {
                long userID = 0;
                try
                {
                    userID = long.Parse(siteGuid);
                }
                catch
                {
                }

                //translate all tags value to it's id.
                Dictionary<int, List<int>> tagIDs = GetTagIDs(tags, groupID);
                //check if tagValue exsits for tagType Null option only !!!
                DataTable dt = DAL.NotificationDal.TagsNotificationNotExists(tagIDs, groupID, (int)NotificationTriggerType.FollowUpByTag);
                //if not - create it 
                if (dt != null && dt.DefaultView.Count > 0)
                {
                    Dictionary<int, List<int>> tagIDsToInsert = new Dictionary<int, List<int>>();
                    int tagID = 0;
                    int tagValue = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        tagID = ODBCWrapper.Utils.GetIntSafeVal(row["sKey"]);
                        tagValue = ODBCWrapper.Utils.GetIntSafeVal(row["value"]);
                        if (tagIDsToInsert.ContainsKey(tagID))
                            tagIDsToInsert[tagID].Add(tagValue);
                        else
                            tagIDsToInsert.Add(tagID, new List<int> { tagValue });
                    }

                    bool result = DAL.NotificationDal.InsertNotificationParameter(tagIDsToInsert, groupID, (int)NotificationTriggerType.FollowUpByTag);
                }

                return NotificationDal.InsertUserNotification(userID, tagIDs, groupID);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("FollowUpByTag Exception = {0}", ex.Message));
                return false;
            }
        }

        public bool UserSettings(UserSettings userSettings, int nGroupID)
        {
            try
            {
                int is_sms = 1;
                int is_email = 1;
                int is_device = 1;

                if (userSettings == null)
                    return false;
                long userID = 0;
                try
                {
                    userID = long.Parse(userSettings.siteGuid);
                }
                catch
                {
                }

                if (userSettings.sendVia != null)
                {
                    is_sms = userSettings.sendVia.is_sms;
                    is_email = userSettings.sendVia.is_email;
                    is_device = userSettings.sendVia.is_device;
                }
                return NotificationDal.UserSettings(userSettings.siteGuid, is_device, is_email, is_sms, nGroupID, 1, 1);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("UserSettings Exception = {0}", ex.Message));
                return false;
            }
        }

        public bool UnsubscribeFollowUpByTag(string siteGuid, Dictionary<string, List<string>> tags, int groupID)
        {
            try
            {
                long userID = 0;
                try
                {
                    userID = long.Parse(siteGuid);
                }
                catch
                {
                }

                Dictionary<int, List<int>> tagIDs = GetTagIDs(tags, groupID);
                return NotificationDal.UpdateUserNotification(userID, tagIDs, groupID, 2);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("UnsubscribeFollowUpByTag Exception = {0}", ex.Message));
                return false;
            }
        }

        //get all user subscriptions tag notifications
        public Dictionary<string, List<string>> GetUserStatusSubscriptions(string siteGuid, int groupID)
        {
            try
            {
                Dictionary<string, List<string>> userSubscriptionsTags = null;
                Dictionary<int, List<int>> lTag = new Dictionary<int, List<int>>();
                //get all user subscriptions notifications
                int userID = 0;
                try
                {
                    userID = int.Parse(siteGuid);
                }
                catch
                {
                }

                DataTable dtTag = DAL.NotificationDal.GetUserNotification(userID, new List<long>(), 1);
                //get the TagNames by the notifications ids (and the tagTypeID+TagValuesIDs)
                if (dtTag == null || dtTag.DefaultView.Count == 0)
                    return null;

                foreach (DataRow dr in dtTag.Rows)
                {
                    int tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr["sKey"]);
                    if (lTag.Keys.Contains<int>(tagTypeId))
                        continue;
                    DataRow[] result = dtTag.Select("sKey = " + tagTypeId);
                    List<int> tagValues = new List<int>();
                    foreach (DataRow row in result)
                    {
                        tagValues.Add(ODBCWrapper.Utils.GetIntSafeVal(row["value"]));
                    }
                    if (tagValues.Count > 0)
                    {
                        lTag.Add(tagTypeId, tagValues);
                    }
                }
                userSubscriptionsTags = GetTagsNameByIDs(lTag, groupID);
                return userSubscriptionsTags;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUserStatusSubscriptions Exception = {0}", ex.Message));
                return null;
            }
        }

        //get all  notifications by NotificationTriggerType + groupid
        public Dictionary<string, List<string>> GetAllTagNotifications(int nGroupID)
        {
            try
            {
                int tagTypeId = 0;
                Dictionary<string, List<string>> notificationTags = null;
                Dictionary<string, List<string>> notificationTagsAll = null;
                Dictionary<int, List<int>> lTag = new Dictionary<int, List<int>>();
                List<int> lAllTags = new List<int>();

                DataTable dtNotifications = DAL.NotificationDal.GetAllNotification(nGroupID, (int)NotificationTriggerType.FollowUpByTag);

                //get the TagNames by the notifications ids (and the tagTypeID+TagValuesIDs)
                if (dtNotifications == null || dtNotifications.DefaultView.Count == 0)
                    return null;

                DataRow[] result = dtNotifications.Select("related_to_parameters = 0"); // only notification related to all tag values
                foreach (DataRow dr in result)
                {
                    tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr["sKey"]);
                    if (!lAllTags.Contains(tagTypeId))
                        lAllTags.Add(tagTypeId);
                }
                //Get All tag values for tagType 
                DataTable dtTagValues;
                if (lAllTags.Count > 0)
                {
                    dtTagValues = DAL.NotificationDal.GetAllTagsValueNameByIDs(nGroupID, lAllTags);
                    foreach (DataRow dr in dtTagValues.Rows)
                    {
                        if (notificationTagsAll == null)
                            notificationTagsAll = new Dictionary<string, List<string>>();
                        string tagType = ODBCWrapper.Utils.GetSafeStr(dr["tagTypeName"]);
                        if (notificationTagsAll.Keys.Contains<string>(tagType))
                            continue;
                        result = dtTagValues.Select("tagTypeName = '" + tagType + "'");
                        List<string> sTagValues = new List<string>();
                        foreach (DataRow row in result)
                        {
                            sTagValues.Add(ODBCWrapper.Utils.GetSafeStr(row["tagValueName"]));
                        }
                        if (sTagValues.Count > 0)
                        {
                            notificationTagsAll.Add(tagType, sTagValues);
                        }
                    }
                }
                result = dtNotifications.Select("related_to_parameters = 1"); // only notification related to specific tag values
                foreach (DataRow dr in result)//dtNotifications.Rows)
                {
                    tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr["sKey"]);
                    if (lTag.Keys.Contains<int>(tagTypeId))
                        continue;
                    result = dtNotifications.Select("sKey = " + tagTypeId);
                    List<int> tagValues = new List<int>();
                    foreach (DataRow row in result)
                    {
                        tagValues.Add(ODBCWrapper.Utils.GetIntSafeVal(row["value"]));
                    }
                    if (tagValues.Count > 0)
                    {
                        lTag.Add(tagTypeId, tagValues);
                    }
                }
                notificationTags = GetTagsNameByIDs(lTag, nGroupID);

                foreach (KeyValuePair<string, List<string>> nItem in notificationTags)
                {
                    if (!notificationTagsAll.ContainsKey(nItem.Key))
                        notificationTagsAll.Add(nItem.Key, nItem.Value);
                }
                return notificationTagsAll;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAllTagNotifications Exception = {0}", ex.Message));
                return null;
            }
        }

        //return true/false if there is a tag notification for specific tag values 
        public bool IsTagNotificationExists(string tagType, string tagValue, int nGroupID)
        {
            try
            {

                Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
                if (!string.IsNullOrEmpty(tagValue))
                    tags.Add(tagType, new List<string>() { tagValue });
                else
                    tags.Add(tagType, null);
                Dictionary<int, List<int>> tagNames = GetTagIDs(tags, nGroupID);
                string tagTypeID = string.Empty;
                string tagValueID = string.Empty;

                if (tagNames != null)
                {
                    tagTypeID = tagNames.Keys.First<int>().ToString();
                    if (tagNames.Values != null && tagNames.Values.Count > 0)
                        tagValueID = tagNames.Values.First<List<int>>()[0].ToString();
                }
                bool result = DAL.NotificationDal.IsTagNotificationExists(nGroupID, (int)NotificationTriggerType.FollowUpByTag, tagTypeID, tagValueID);
                return result;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("IsTagNotificationExists Exception = {0}", ex.Message));
                return false;
            }
        }

        public static Dictionary<string, List<string>> GetTagsNameByIDs(Dictionary<int, List<int>> tags, int groupID)
        {
            try
            {
                Dictionary<string, List<string>> tagsNames = new Dictionary<string, List<string>>();
                string tagType = string.Empty;
                int tagTypeID = 0;

                DataTable dtTagName = DAL.NotificationDal.GetTagsNameByIDs(groupID, tags);
                if (dtTagName != null && dtTagName.DefaultView.Count > 0)
                {
                    foreach (DataRow dr in dtTagName.Rows)
                    {
                        tagType = ODBCWrapper.Utils.GetSafeStr(dr["tagTypeName"]);
                        tagTypeID = ODBCWrapper.Utils.GetIntSafeVal(dr["tagTypeID"]);
                        if (tagsNames.Keys.Contains<string>(tagType))
                            continue;
                        DataRow[] result = dtTagName.Select("tagTypeID = " + tagTypeID);
                        List<string> tagValues = new List<string>();
                        foreach (DataRow row in result)
                        {
                            tagValues.Add(ODBCWrapper.Utils.GetSafeStr(row["tagValueName"]));
                        }
                        if (tagValues.Count > 0)
                        {
                            tagsNames.Add(tagType, tagValues);
                        }
                    }
                }
                return tagsNames;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetTagsNameByIDs Exception = {0}", ex.Message));
                return null;
            }
        }

        #endregion

        #region private Methods

        private static Dictionary<int, List<int>> GetTagIDs(Dictionary<string, List<string>> tags, int groupID)
        {
            Dictionary<int, List<int>> tagIDs = new Dictionary<int, List<int>>();
            int tagTypeId = 0;

            DataTable dtTag = DAL.NotificationDal.GetTagsIDsByName(groupID, tags);
            if (dtTag != null && dtTag.DefaultView.Count > 0)
            {
                foreach (DataRow dr in dtTag.Rows)
                {
                    tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr["tagTypeID"]);
                    if (tagIDs.Keys.Contains<int>(tagTypeId))
                        continue;
                    DataRow[] result = dtTag.Select("tagTypeID = " + tagTypeId);
                    List<int> tagValues = new List<int>();
                    foreach (DataRow row in result)
                    {
                        tagValues.Add(ODBCWrapper.Utils.GetIntSafeVal(row["tagValueID"]));
                    }
                    if (tagValues.Count > 0)
                    {
                        tagIDs.Add(tagTypeId, tagValues);
                    }
                }
            }
            return tagIDs;
        }

        /// <summary>
        /// Update one notification request status at the db (notifications_requests table).
        /// </summary>
        /// <param name="requestID"></param>
        /// <param name="requestStatus"></param>
        private void UpdateOneNotificationRequest(long requestID, NotificationRequestStatus requestStatus)
        {
            List<long> requestsList = new List<long>();
            requestsList.Add(requestID);
            byte status = (Byte)(requestStatus);
            NotificationDal.UpdateNotificationRequestStatus(requestsList, status);
        }

        /// <summary>
        /// Update several notification requests status at the db (notifications_requests table).
        /// The requests represent by requestsIDsList param that contains the IDs of the requests.
        /// </summary>
        /// <param name="requestsIDsList"></param>
        /// <param name="requestStatus"></param>
        private void UpdateNotificationRequests(List<long> requestsIDsList, NotificationRequestStatus requestStatus)
        {
            byte status = (Byte)(requestStatus);
            NotificationDal.UpdateNotificationRequestStatus(requestsIDsList, status);
        }

        /// <summary>
        ///  Create list of NotificationRequest objects by querying the db when the filter
        ///  is by groupID and status params , the number of requests determined by the numOfRequests param.
        /// </summary>
        /// <param name="numOfRequests"></param>
        /// <param name="groupID"></param>
        /// <param name="requestStatus"></param>
        /// <returns></returns>
        private List<NotificationRequest> GetNotificationRequests(int numOfRequests, long groupID, NotificationRequestStatus requestStatus, ref int startRequestID, ref int endRequestID)
        {
            try
            {
                List<NotificationRequest> requestsList = new List<NotificationRequest>();
                byte status = (Byte)(requestStatus);

                DataSet dsRequests = NotificationDal.GetNotificationRequests(numOfRequests, groupID, null, status);
                DataTable dtRequests = null;
                DataTable dtRequestsActions = null;
                DataTable dtRequestParameters = null;

                if (dsRequests != null)
                {
                    dtRequests = dsRequests.Tables[0];
                    // return min and max id for later use 
                    if (dtRequests != null && dtRequests.DefaultView.Count > 0)
                    {
                        var query = dtRequests.AsEnumerable().Cast<DataRow>().Min(x => x["ID"]);
                        startRequestID = ODBCWrapper.Utils.GetIntSafeVal(query);
                        query = dtRequests.AsEnumerable().Cast<DataRow>().Max(x => x["ID"]);
                        endRequestID = ODBCWrapper.Utils.GetIntSafeVal(query);
                        log.Debug("GetNotificationRequests - " + string.Format("Run from id={0} to id={1}", startRequestID, endRequestID));
                    }

                    dtRequestsActions = dsRequests.Tables[1];
                    if (dsRequests.Tables.Count == 3 && dsRequests.Tables[2] != null)
                        dtRequestParameters = dsRequests.Tables[2]; // table for the relevant values for keys in specific TriggerType notifications.
                }

                requestsList = GetNotificationRequestsList(dtRequests, dtRequestsActions, dtRequestParameters);
                return requestsList;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed create the NotificationRequest List at GetNotificationRequests Exception = {0}.  groupID = {1}, NotificationRequestStatus = {2}",
                                            ex.Message, groupID, requestStatus));
                return null;
            }
        }

        /// <summary>
        /// Create list of NotificationRequest objects according to datatable of requests and datatable
        /// of actions attached to each request.
        /// </summary>
        /// <param name="dtRequests"></param>
        /// <param name="dtRequestsActions"></param>
        /// <returns></returns>
        private List<NotificationRequest> GetNotificationRequestsList(DataTable dtRequests, DataTable dtRequestsActions, DataTable dtRequestParameters)
        {
            List<NotificationRequest> requestsList = new List<NotificationRequest>();
            Dictionary<long, List<NotificationRequestAction>> requestsActionsDict = GetRequestsActionsDictionary(dtRequestsActions);
            Dictionary<long, string> requestsParametersDict = GetRequestsValuesDictionary(dtRequestParameters);

            #region get all tags that user sign in to
            var oUser = from row in dtRequests.AsEnumerable()
                        select row.Field<long>("user_id");
            var oUserList = oUser.ToList();

            List<long> lUserIds = new List<long>();
            lUserIds.AddRange(oUserList);

            DataTable dtTagsPerUSer = DAL.NotificationDal.GetAllNotificationUserSignIn(lUserIds, (int)NotificationTriggerType.FollowUpByTag, ODBCWrapper.Utils.GetIntSafeVal(dtRequests.Rows[0]["group_id"]));
            #endregion

            foreach (DataRowView drRequest in dtRequests.DefaultView)
            {
                bool continueWithRequest = false;

                //check if media is active and if it's start date <= today if so then continue 
                string json = ODBCWrapper.Utils.GetSafeStr(drRequest["parameters"]);

                if (!string.IsNullOrEmpty(json))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    ExtraParams extraParams = js.Deserialize<ExtraParams>(json);
                    if (extraParams != null)
                    {
                        continueWithRequest = DAL.NotificationDal.IsMediaNeedToBeNotify(extraParams.mediaID);
                        if (!continueWithRequest) // media start date > today or media is not active / status is not 1 
                            continue;
                    }
                }

                NotificationRequestAction[] actions = null;
                string sValues = string.Empty;
                long requestID = ODBCWrapper.Utils.GetLongSafeVal(drRequest["ID"]);
                if (requestsActionsDict != null && requestsActionsDict.ContainsKey(requestID) == true)
                {
                    actions = requestsActionsDict[requestID].ToArray();
                }

                if (requestsParametersDict != null && requestsParametersDict.ContainsKey(requestID) == true)
                {
                    sValues = requestsParametersDict[requestID];
                }

                NotificationRequest request = CreateNotificationRequestObject(drRequest, requestID, actions, sValues, dtTagsPerUSer);

                if (request != null)
                    requestsList.Add(request);
            }
            return requestsList;
        }

        /// <summary>
        /// Create dictionary of actions according to datatable of actions,
        /// the key is request id and the value is list of NotificationRequestAction objects belong to each request.
        /// </summary>
        /// <param name="dtRequestsActions"></param>
        /// <returns></returns>
        private Dictionary<long, List<NotificationRequestAction>> GetRequestsActionsDictionary(DataTable dtRequestsActions)
        {
            Dictionary<long, List<NotificationRequestAction>> requestsActionsDict = new Dictionary<long, List<NotificationRequestAction>>();
            foreach (DataRowView drAction in dtRequestsActions.DefaultView)
            {
                long requestID = ODBCWrapper.Utils.GetLongSafeVal(drAction["notificationRequestID"]);
                if (requestsActionsDict.ContainsKey(requestID) == false)
                {
                    requestsActionsDict.Add(requestID, new List<NotificationRequestAction>());
                }
                long actionID = ODBCWrapper.Utils.GetLongSafeVal(drAction["ID"]);
                string actionText = ODBCWrapper.Utils.GetSafeStr(drAction["text"]);
                string actionLink = ODBCWrapper.Utils.GetSafeStr(drAction["link"]);
                NotificationRequestAction action = new NotificationRequestAction(actionID, actionText, actionLink);
                requestsActionsDict[requestID].Add(action);
            }
            return requestsActionsDict;
        }

        /// <summary>
        /// Create dictionary of parameters according to datatable of values,
        /// the key is notification id and the value is list of values belong to each notification.
        /// </summary>
        /// <param name="dtRequestsActions"></param>
        /// <returns></returns>
        private Dictionary<long, string> GetRequestsValuesDictionary(DataTable dtRequestsParameters)
        {
            Dictionary<long, string> requestsParametersDict = new Dictionary<long, string>();
            foreach (DataRowView drParam in dtRequestsParameters.DefaultView)
            {
                long requestID = ODBCWrapper.Utils.GetLongSafeVal(drParam["notificationRequestID"]);
                if (!requestsParametersDict.ContainsKey(requestID))
                    requestsParametersDict.Add(requestID, string.Empty);
                requestsParametersDict[requestID] = ODBCWrapper.Utils.GetSafeStr(drParam["parameters"]);
            }
            return requestsParametersDict;
        }

        /// <summary>
        /// Create NotificationRequest object from DataRowView object that represent record at 
        /// notifications_requests table and dictionary of actions.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="requestsActionsDict"></param>
        /// <returns></returns>
        private NotificationRequest CreateNotificationRequestObject(DataRowView dr, long requestID, NotificationRequestAction[] actions, string values, DataTable dtTagsPerUSer)
        {
            long notificationID = ODBCWrapper.Utils.GetLongSafeVal(dr["notificationID"]);
            NotificationRequestStatus status = (NotificationRequestStatus)(ODBCWrapper.Utils.GetIntSafeVal(dr["status"]));
            int groupID = ODBCWrapper.Utils.GetIntSafeVal(dr["group_id"]);
            long userID = ODBCWrapper.Utils.GetLongSafeVal(dr["user_id"]);
            DateTime createdDate = ODBCWrapper.Utils.GetDateSafeVal(dr["created_date"]);
            NotificationRequestType requestType = (NotificationRequestType)(ODBCWrapper.Utils.GetIntSafeVal(dr["notification_request_type"]));
            NotificationMessageType messageType = (NotificationMessageType)(ODBCWrapper.Utils.GetIntSafeVal(dr["notification_type"]));
            string messageText = ODBCWrapper.Utils.GetSafeStr(dr["message_text"]);
            string smsMessageText = ODBCWrapper.Utils.GetSafeStr(dr["sms_message_text"]);
            string pull_message_text = ODBCWrapper.Utils.GetSafeStr(dr["pull_message_text"]);
            string title = ODBCWrapper.Utils.GetSafeStr(dr["title"]);
            string sKey = ODBCWrapper.Utils.GetSafeStr(dr["sKey"]);

            DateTime startDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "startDate");
            DateTime catalogStartDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "catalogStartDate");

            string notificationDateFormat = ODBCWrapper.Utils.GetSafeStr(dr, "notification_date_format");


            log.Debug("CreateNotificationRequestObject - " + string.Format("Start userID={0}", userID));

            //add is email / push / sms
            int is_email = ODBCWrapper.Utils.GetIntSafeVal(dr["is_email"]);
            int is_sms = ODBCWrapper.Utils.GetIntSafeVal(dr["is_sms"]);
            int is_device = ODBCWrapper.Utils.GetIntSafeVal(dr["is_device"]);

            //user specific values for SendVia - default value = 1 
            DataRow[] drUsers = dtTagsPerUSer.Select("user_id=" + userID);
            int is_user_email = 1;
            int is_user_sms = 1;
            int is_user_device = 1;
            if (drUsers != null && drUsers.Length > 0)
            {
                is_user_email = ODBCWrapper.Utils.GetIntSafeVal(drUsers[0]["is_user_email"]);
                is_user_sms = ODBCWrapper.Utils.GetIntSafeVal(drUsers[0]["is_user_sms"]);
                is_user_device = ODBCWrapper.Utils.GetIntSafeVal(drUsers[0]["is_user_device"]);
            }
            int emailNotify = 0;
            int smsNotify = 0;
            int deviceNotify = 0;

            if (is_email == 1 && is_user_email == 1)
                emailNotify = 1;
            if (is_sms == 1 && is_user_sms == 1)
                smsNotify = 1;
            if (is_device == 1 && is_user_device == 1)
                deviceNotify = 1;

            SendVia oSendVia = new SendVia(emailNotify, smsNotify, deviceNotify);
            string notificationEmailTemplate = ODBCWrapper.Utils.GetSafeStr(dr["notification_email_template"]);
            string notificationEmailSubject = ODBCWrapper.Utils.GetSafeStr(dr["subject"]);
            NotificationTriggerType eTriggerType = (NotificationTriggerType)(ODBCWrapper.Utils.GetIntSafeVal(dr["trigger_type"]));

            //Get the NotificationTag Object from the json string we got from DB
            // values The JSON data from the POST
            #region extraParams for tag notification
            JavaScriptSerializer js = new JavaScriptSerializer();
            ExtraParams extraParams = js.Deserialize<ExtraParams>(values);
            log.Debug("CreateNotificationRequestObject - " + string.Format(" NotificationReques userID={0} ,createdDate={1}, extraParams={2}", userID, createdDate.ToShortDateString(), values));
            string picURL = string.Empty;
            if (extraParams != null)
            {
                //sms/device/email
                if (extraParams.notificationsID != null && extraParams.notificationsID.Count > 0)
                {
                    DataTable dtSendVia = DAL.NotificationDal.GetNotificationDeviceToSend(extraParams.notificationsID);

                    foreach (DataRow drSendVia in dtSendVia.Rows)
                    {
                        if (emailNotify == 0)
                            emailNotify = ODBCWrapper.Utils.GetIntSafeVal(drSendVia["is_email"]);
                        if (smsNotify == 0)
                            smsNotify = ODBCWrapper.Utils.GetIntSafeVal(drSendVia["is_sms"]);
                        if (deviceNotify == 0)
                            deviceNotify = ODBCWrapper.Utils.GetIntSafeVal(drSendVia["is_device"]);
                    }
                    if (emailNotify == 0 || is_user_email == 0)
                        emailNotify = 0;
                    if (is_sms == 0 || is_user_sms == 0)
                        smsNotify = 0;
                    if (is_device == 0 || is_user_device == 0)
                        deviceNotify = 0;

                    oSendVia = new SendVia(emailNotify, smsNotify, deviceNotify);
                }

                if (extraParams.mediaID != 0)
                {
                    extraParams.templateEmail = notificationEmailTemplate;
                    extraParams.subjectEmail = notificationEmailSubject;
                    extraParams.dateFormat = string.IsNullOrEmpty(notificationDateFormat) ? deafultEmailDateFormat : notificationDateFormat;
                    // A Notification About New Media XXXX
                    string mediaName = ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("media", "name", extraParams.mediaID, "MAIN_CONNECTION_STRING"));
                    ReplaceText(ref messageText, "{mediaName}", mediaName);
                    ReplaceText(ref smsMessageText, "{mediaName}", mediaName);
                    ReplaceText(ref pull_message_text, "{mediaName}", mediaName);

                    // replace the startDatae +catalogStartDate                   
                    ReplaceDatesInMessageText(startDate, catalogStartDate, notificationDateFormat, ref messageText);
                    ReplaceDatesInMessageText(startDate, catalogStartDate, notificationDateFormat, ref smsMessageText);
                    ReplaceDatesInMessageText(startDate, catalogStartDate, notificationDateFormat, ref pull_message_text);

                    //replace FirstName + LastName
                    ReplaceFirstLastNames(groupID, userID, ref messageText, ref smsMessageText, ref pull_message_text);


                    //call GetTagsNotificationByMedia (Tvinci DB) and comper the relevant tags
                    DataTable dt = DAL.NotificationDal.GetTagsNotificationByMedia(extraParams.mediaID, 0); // get all tags related to media 

                    if (dt == null) // no request needed
                        return null;


                    if (extraParams.TagDict != null && extraParams.TagDict.Count() > 0)
                    {
                        // bool bUserMustBeNotify = false;
                        string metaTag = string.Empty;
                        string metaName = string.Empty;
                        string metaValues = string.Empty;
                        List<string> metaValueList = null;
                        List<string> metaTagList = new List<string>();

                        foreach (KeyValuePair<string, List<TagIDValue>> item in extraParams.dTagDict)
                        {
                            metaName = string.Empty;
                            metaValueList = new List<string>();
                            foreach (TagIDValue itemValue in item.Value)
                            {
                                if (string.IsNullOrEmpty(metaName))
                                {
                                    metaName = itemValue.tagTypeName;
                                }
                                metaValueList.Add(itemValue.tagValueName);
                            }
                            metaValues = string.Join(",", metaValueList.ToArray());

                            metaTag = string.Format("{0} : {1}", metaName, metaValues);
                            metaTagList.Add(metaTag);
                        }
                        metaTag = string.Join(",", metaTagList.ToArray());

                        ReplaceText(ref messageText, "{MetaTag}", metaTag);
                        ReplaceText(ref smsMessageText, "{MetaTag}", metaTag);
                        ReplaceText(ref pull_message_text, "{MetaTag}", metaTag);

                        NotificationRequest request = new NotificationRequest(requestID, notificationID, status, groupID, userID, createdDate, requestType, messageType, messageText, smsMessageText, pull_message_text, title, actions, eTriggerType, oSendVia, extraParams);
                        log.Debug("CreateNotificationRequestObject - " + string.Format("End userID={0}", userID));
                        return request;
                    }
                }
            }
            #endregion
            else // for notification with no extraParams
            {
                NotificationRequest request = new NotificationRequest(requestID, notificationID, status, groupID, userID, createdDate, requestType, messageType, messageText, smsMessageText, pull_message_text, title, actions, eTriggerType, oSendVia, extraParams);
                return request;
            }
            return null;
        }

        private void ReplaceText(ref string messageText, string matchString, string replaceWithString)
        {
            messageText = String.Format("{0}", messageText.Replace(matchString, replaceWithString));
        }

        private void ReplaceFirstLastNames(int groupID, long userID, ref string messageText, ref string smsMessageText, ref string pull_message_text)
        {
            try
            {
                int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(groupID); //TVinciShared.LoginManager.GetLoginGroupID();                
                UserResponseObject userObj = Core.Users.Module.GetUserData(groupID, ODBCWrapper.Utils.GetSafeStr(userID), string.Empty);

                if (userObj != null && userObj.m_user != null && userObj.m_user.m_oBasicData != null)
                {
                    ReplaceName(ref messageText, userObj.m_user.m_oBasicData);
                    ReplaceName(ref smsMessageText, userObj.m_user.m_oBasicData);
                    ReplaceName(ref pull_message_text, userObj.m_user.m_oBasicData);
                }
            }
            catch (Exception ex)
            {
                log.Error("ReplaceFirstLastNames - " + string.Format("failed replace names in message with groupID={0}, messageText={1}, ex={2}", groupID, messageText, ex.Message), ex);
            }
        }

        private void ReplaceName(ref string messageText, UserBasicData user)
        {
            ReplaceText(ref messageText, "{FirstName}", user.m_sFirstName);
            ReplaceText(ref messageText, "{LastName}", user.m_sLastName);
        }

        private void ReplaceDatesInMessageText(DateTime startDate, DateTime catalogStartDate, string notificationDateFormat, ref string messageText)
        {
            try
            {
                // get start and end insex for this substring that we ae looking for  
                string matchText = "{StartDate";
                ReplaceDates(startDate, notificationDateFormat, matchText, ref messageText);
                matchText = "{CatalaogStartDate";
                ReplaceDates(startDate, notificationDateFormat, matchText, ref messageText);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
        }

        private static void ReplaceDates(DateTime startDate, string notificationDateFormat, string matchText, ref string messageText)
        {
            int last = 0;
            string dateString = string.Empty;
            string startDateFormat = string.Empty;
            string sStartDate = string.Empty;

            Match mc = Regex.Match(messageText, matchText);
            while (mc != null && mc.Index >= 0 && mc.Success)
            {
                last = messageText.IndexOf("}", mc.Index);
                dateString = messageText.Substring(mc.Index, last - mc.Index + 1);
                string[] startDateSplit = dateString.Split(',');
                if (startDateSplit != null && startDateSplit.Count() == 2)
                {
                    startDateFormat = startDateSplit[1].TrimEnd().TrimStart().Replace("}", "");
                }
                if (string.IsNullOrEmpty(startDateFormat))
                {
                    startDateFormat = string.IsNullOrEmpty(notificationDateFormat) ? deafultDateFormat : notificationDateFormat;
                }
                //create the date in the right format 
                try
                {
                    if (!string.IsNullOrEmpty(dateString))
                    {
                        sStartDate = Utils.ExtractDate(startDate, startDateFormat);
                        messageText = String.Format("{0}", messageText.Replace(dateString, sStartDate));
                    }
                }
                catch (Exception exStartDate)
                {
                    log.Error("ReplaceDatesInMessageText - " + string.Format("failed replace datetime in message with format={0}, date={1}, ex={2} ", startDateFormat, dateString, exStartDate.Message), exStartDate);
                    sStartDate = Utils.ExtractDate(startDate, deafultDateFormat);
                    messageText = String.Format("{0}", messageText.Replace(dateString, sStartDate));
                }

                mc = Regex.Match(messageText, matchText);
            }
        }

        /// <summary>
        /// Create Notification object from DataRowView object that represent record at 
        /// at notifications table.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private ApiObjects.Notification.Notification CreateNotificationObject(DataRowView dr)
        {
            long ID = ODBCWrapper.Utils.GetLongSafeVal(dr["ID"]);
            NotificationMessageType messageType = (NotificationMessageType)(ODBCWrapper.Utils.GetIntSafeVal(dr["notification_type"]));
            int triggerType = ODBCWrapper.Utils.GetIntSafeVal(dr["trigger_type"]);
            long groupID = ODBCWrapper.Utils.GetLongSafeVal(dr["group_id"]);
            string messageText = ODBCWrapper.Utils.GetSafeStr(dr["message_text"]);
            string smsMessageText = ODBCWrapper.Utils.GetSafeStr(dr["sms_message_text"]);
            string pullMessageText = ODBCWrapper.Utils.GetSafeStr(dr["pull_message_text"]);
            string title = ODBCWrapper.Utils.GetSafeStr(dr["title"]);
            bool isActive = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]));
            int status = ODBCWrapper.Utils.GetIntSafeVal(dr["status"]);
            bool isBroadcast = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(dr["is_broadcast"]));
            DateTime createdDate = ODBCWrapper.Utils.GetDateSafeVal(dr["create_date"]);
            NotificationRequestAction[] actions = null; //TBD:Fetch it from db  
            string key = ODBCWrapper.Utils.GetSafeStr(dr["sKey"]);
            ApiObjects.Notification.Notification objNotification = null;

            objNotification = new ApiObjects.Notification.Notification(ID, messageType, triggerType, groupID, messageText, smsMessageText, pullMessageText, title, isActive, status, isBroadcast, createdDate, actions, key);

            return objNotification;
        }

        /// <summary>
        /// Process flow of one NotificationRequest object by obtaining the suitable implementor
        /// object and use it to get the list of NotificationMessage and procees this list forward.
        /// </summary>
        /// <param name="request"></param>
        private void HandleOneRequest(NotificationRequest request)
        {
            NotificationRequestStatus status = NotificationRequestStatus.Successful;
            NotificationRequestStatus[] statuses = new NotificationRequestStatus[4];
            try
            {
                if (request == null || request.sendVia == null)
                    return;
                int media = 0;
                if (request.oExtraParams != null)
                    media = request.oExtraParams.mediaID;
                //create pull notification 
                try
                {
                    log.Debug("HandleOneRequest - " + string.Format(" Pull request Group ={0}, UserID ={1} ", request.GroupID, request.UserID, media));
                    IFactoryImp f = new FactoryImp(request.GroupID);
                    IRequestImp imp = f.GetTypeImp(eSenderObjectType.Pull);
                    imp.Send(request);
                    statuses[0] = NotificationRequestStatus.Successful;
                    log.Debug("HandleOneRequest - " + string.Format("Pull request Finish {0} ", "Successful"));
                }
                catch (Exception ex)
                {
                    statuses[0] = NotificationRequestStatus.Failed;
                    log.Error("HanldeOneRequest failed send Pull Notification to DB", ex);
                }

                SendVia oSendVia = request.sendVia;
                if (oSendVia.is_device == 1)
                {
                    try
                    {
                        log.Debug("HandleOneRequest " + string.Format("Device request Group ={0}, UserID ={1} ", request.GroupID, request.UserID, media));
                        IFactoryImp f = new FactoryImp(request.GroupID);
                        IRequestImp imp = f.GetTypeImp(eSenderObjectType.Device);
                        imp.Send(request);
                        statuses[1] = NotificationRequestStatus.Successful;
                        log.Debug("HandleOneRequest - " + string.Format("Device request Finish {0} ", "Successful"));
                    }
                    catch (Exception ex)
                    {
                        statuses[1] = NotificationRequestStatus.Failed;
                        log.Error("HandleOneRequest - " + string.Format("failed send Push to Device  {0} ", ex.Message), ex);
                        log.Error(string.Format("HanldeOneRequest failed send Push to Device {0}", ex.Message));
                    }
                }

                if (oSendVia.is_email == 1)
                {
                    try
                    {
                        if (request != null)
                        {
                            log.Debug("HandleOneRequest - " + string.Format("Email request Group ={0}, UserID ={1} ", request.GroupID, request.UserID, media));
                            IFactoryImp f = new FactoryImp(request.GroupID);
                            IRequestImp imp = f.GetTypeImp(eSenderObjectType.Email);
                            imp.Send(request);
                            statuses[1] = NotificationRequestStatus.Successful;
                            log.Debug("HandleOneRequest - " + string.Format("Email request Finish {0} ", "Successful"));
                        }
                    }
                    catch (Exception ex)
                    {
                        statuses[1] = NotificationRequestStatus.Failed;
                        log.Error("HandleOneRequest - " + string.Format("failed send Email  {0} ", ex.Message), ex);
                    }
                }
                //Wait for next version 
                //if (oSendVia.is_sms == 1)
                //{   
                //    try
                //    {
                //    IFactoryImp f = new FactoryImp(request.GroupID);
                //    IRequestImp imp = f.GetTypeImp(eSenderObjectType.SMS);
                //    imp.Send(request);
                //    statuses[3] = NotificationRequestStatus.Successful;
                //    }
                //    catch (Exception ex)
                //    {
                //        statuses[3] = NotificationRequestStatus.Failed;
                //        log.Error(string.Format("HanldeOneRequest failed send SMS {0}", ex.Message));
                //    }
                //}
            }
            catch
            {
                status = NotificationRequestStatus.Failed;
            }
            try
            {
                if (statuses[0] == NotificationRequestStatus.Failed && statuses[1] == NotificationRequestStatus.Failed && statuses[2] == NotificationRequestStatus.Failed) // next version  && statuses[3] == NotificationRequestStatus.Failed)
                    status = NotificationRequestStatus.Failed;
                UpdateOneNotificationRequest(request.ID, status);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("HanldeOneRequest Failed UpdateOneNotificationRequest requestID={0}, status={1}, Exception = {2}", request.ID, status, ex.Message));
            }
        }
        #endregion

        #region singleton Property
        public static NotificationManager Instance
        {
            get
            {
                NotificationManager retInstance = Nested.instance;
                return retInstance;
            }
        }
        #endregion

        #region classes
        class Nested
        {
            internal static readonly NotificationManager instance = new NotificationManager();
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }
        }
        #endregion
    }
}