using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using NotificationObj;
using ApiObjects;
using WS_API;
using WS_Users;
using Users;

namespace NotificationInterface
{
    public class EmailNotification : NotificationBase, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public EmailNotification()
            : base()
        {
        }
        public virtual void Send(NotificationRequest oNotificationRequest)
        {
            //get Notification Message Object for EMAIL
            NotificationMessage emailMessage = GetEmailMessage(oNotificationRequest);
            log.Info("EmailNotification.Send " + emailMessage.MessageText);
            SendMail(emailMessage);
        }

        private static NotificationMessage GetEmailMessage(NotificationRequest request)
        {
            NotificationMessageType messageType = request.MessageType;
            NotificationRequestAction[] actions = request.Actions;
            string appName = GetAppNameFromConfig(request.GroupID);
            NotificationMessage emailMessage = new NotificationMessage(messageType, request.NotificationID, request.ID, request.UserID, NotificationMessageStatus.NotStarted, request.MessageText, request.Title, appName,
                string.Empty, 0, request.Actions, request.oExtraParams, request.GroupID);
            return emailMessage;
        }

        private static void SendMail(NotificationMessage message)
        {
            try
            {
                #region Get all user Details by userID frm WS_USERS

                UsersService usersService = new UsersService();
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";
                int nGroupID = ODBCWrapper.Utils.GetIntSafeVal(message.nGroupID); //TVinciShared.LoginManager.GetLoginGroupID();                
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "00000", "users", sIP, ref sWSUserName, ref sWSPass);
                UserResponseObject userObj = usersService.GetUserData(sWSUserName, sWSPass, ODBCWrapper.Utils.GetSafeStr(message.UserID), sIP);

                #endregion

                API clientAPI = new API();
                //Build mailRequest from the message 
                EmailNotificationRequest emailRequest = new EmailNotificationRequest();

                emailRequest.m_eMailType = eMailTemplateType.Notification;
                if (userObj != null && userObj.m_user != null && userObj.m_user.m_oBasicData != null)
                {
                    emailRequest.m_sFirstName = userObj.m_user.m_oBasicData.m_sFirstName;
                    emailRequest.m_sLastName = userObj.m_user.m_oBasicData.m_sLastName;
                    emailRequest.m_sSenderTo = userObj.m_user.m_oBasicData.m_sEmail; //"liat.r@tvinci.com";
                }

                #region user following tags

                string userFollowTags = UserFollowTags(message, nGroupID);
                emailRequest.m_userFollowTags = userFollowTags;

                #endregion


                if (message.TagNotificationParams != null)
                {
                    #region Build email request by tag parameters

                    //get all tags for media
                    DataTable dtMediaTags = DAL.NotificationDal.GetTagsNotificationByMedia(message.TagNotificationParams.mediaID, null);
                    List<TagPair> lMediaTagsMetas = new List<TagPair>();
                    Dictionary<string, string> tags = new Dictionary<string, string>();
                    if (dtMediaTags != null && dtMediaTags.DefaultView.Count > 0)
                    {
                        foreach (DataRow dr in dtMediaTags.Rows)
                        {
                            string tagType = ODBCWrapper.Utils.GetSafeStr(dr["tag_type_name"]);
                            string tagValue = ODBCWrapper.Utils.GetSafeStr(dr["value"]);
                            TagPair tagPair = new TagPair();
                            tagPair.key = tagType.Replace("(", string.Empty).Replace(")", string.Empty);
                            tagPair.value = tagValue;

                            if (tags.ContainsKey(tagPair.key))
                                tags[tagPair.key] = tags[tagPair.key] + " , " + tagPair.value;
                            else
                                tags.Add(tagPair.key, tagPair.value);
                        }
                        foreach (KeyValuePair<string, string> item in tags)
                        {
                            TagPair tagPair = new TagPair();
                            tagPair.key = item.Key;
                            tagPair.value = item.Value;
                            lMediaTagsMetas.Add(tagPair);
                        }
                    }

                    //get all tags related to notifications 
                    emailRequest.m_sTagToFollow = TagsNotification(message);

                    //get media details for media
                    GetMediaDetails(message, ref emailRequest, lMediaTagsMetas);

                    #endregion

                    emailRequest.m_sTemplateName = message.TagNotificationParams.templateEmail;
                    emailRequest.m_emailKey = ODBCWrapper.Utils.GetSafeStr(ODBCWrapper.Utils.GetTableSingleVal("groups_operators", "EmailKey", "group_id", "=", message.nGroupID, "MESSAGE_BOX_CONNECTION_STRING"));
                }

                //call send mail template service by api service
                sWSUserName = "";
                sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(nGroupID, "Mailer", "api", sIP, ref sWSUserName, ref sWSPass);
                bool bSend = clientAPI.SendMailTemplate(sWSUserName, sWSPass, emailRequest);
                message.Status = NotificationMessageStatus.Successful;
            }

            catch (Exception ex)
            {
                log.Error(string.Format("{0}Exception = {1}", "SendMail failed", ex.ToString()));
                log.Error("SendMail - " + string.Format("Exception = {0}", ex.ToString()), ex);
                message.Status = NotificationMessageStatus.Failed;
            }
        }

        private static void GetMediaDetails(NotificationMessage message, ref EmailNotificationRequest emailRequest, List<TagPair> lMediaTagsMetas)
        {
            //get Media Details from Tvinci DB 
            DataSet ds = DAL.NotificationDal.GetMediaForEmail(message.TagNotificationParams.mediaID);
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0] != null) // media Details
                {
                    emailRequest.m_mediaId = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0]["id"]);
                    string mediaName = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0]["mediaName"]);
                    emailRequest.m_sMediaName = mediaName;
                    emailRequest.m_sSenderFrom = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0]["mail_ret_add"]);
                    //
                    DateTime startDate = ODBCWrapper.Utils.GetDateSafeVal(ds.Tables[0].Rows[0], "START_DATE");
                    DateTime catalogStartDate = ODBCWrapper.Utils.GetDateSafeVal(ds.Tables[0].Rows[0], "CATALOG_START_DATE");

                    try
                    {
                        emailRequest.m_startDate = Utils.ExtractDate(startDate, message.TagNotificationParams.dateFormat);
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                    }

                    try
                    {
                        emailRequest.m_catalogStartDate = Utils.ExtractDate(catalogStartDate, message.TagNotificationParams.dateFormat);
                    }
                    catch (Exception ex)
                    {
                        log.Error("", ex);
                    }

                    int group_id = ODBCWrapper.Utils.GetIntSafeVal(ds.Tables[0].Rows[0]["group_id"]);
                    DataTable dtMetaNames = DAL.NotificationDal.Get_MetasByGroup(group_id);
                    //get all metas for media 
                    lMediaTagsMetas = GetMetas(lMediaTagsMetas, ds, group_id, dtMetaNames);
                    emailRequest.m_tagList = lMediaTagsMetas;
                }
                if (ds.Tables[1] != null && ds.Tables[1].DefaultView.Count > 0) //Media  Pictures
                {
                    string sPicSize = "full";//default picture size
                    foreach (DataRow drPicSize in ds.Tables[1].Rows) // get the first picSize that bigger than 200X200
                    {
                        string picSize = ODBCWrapper.Utils.GetSafeStr(drPicSize["PicSize"]);
                        string[] aPicSize = picSize.Split('X');
                        if (int.Parse(aPicSize[0]) > 150 && int.Parse(aPicSize[0]) < 300 && int.Parse(aPicSize[1]) > 150 && int.Parse(aPicSize[1]) < 300)
                        {
                            if ((int.Parse(aPicSize[0]) / 16 * 9) == int.Parse(aPicSize[1]))
                            {
                                sPicSize = picSize;
                                break;
                            }
                        }
                    }
                    DataRow[] drPic = ds.Tables[1].Select("PicSize = '" + sPicSize + "'");
                    if (drPic != null && drPic.Length > 0)
                    {
                        emailRequest.m_mediaPicURL = ODBCWrapper.Utils.GetSafeStr(drPic[0]["m_sURL"]);
                    }
                }
            }
        }

        private static List<TagPair> GetMetas(List<TagPair> lMediaTagsMetas, DataSet ds, int group_id, DataTable dtMetaNames)
        {
            DataRow[] drMetaNames = dtMetaNames.Select("Id = " + group_id);
            //get all metas
            foreach (DataRow drmetaPair in drMetaNames)
            {
                string metaName = ODBCWrapper.Utils.GetSafeStr(drmetaPair["value"]);
                string metaFiled = ODBCWrapper.Utils.GetSafeStr(drmetaPair["columnname"]).Replace("_NAME", string.Empty);
                string metaValue = string.Empty;
                try
                {
                    if (!string.IsNullOrEmpty(metaFiled))
                    {
                        metaValue = ODBCWrapper.Utils.GetSafeStr(ds.Tables[0].Rows[0][metaFiled]);
                        if (!string.IsNullOrEmpty(metaValue))
                        {
                            TagPair metaPair = new TagPair();
                            metaPair.key = metaName;
                            metaPair.value = metaValue;
                            lMediaTagsMetas.Add(metaPair);
                        }
                    }
                }
                catch
                {
                }
            }
            return lMediaTagsMetas;
        }

        private static string TagsNotification(NotificationMessage message)
        {
            List<string> tagsName = new List<string>();
            string sFollowByTags = string.Empty;
            if (message != null && message.TagNotificationParams != null)
            {
                foreach (KeyValuePair<string, List<TagIDValue>> item in message.TagNotificationParams.dTagDict)
                {
                    string tagType = string.Empty;
                    if (item.Value != null && item.Value.Count() > 0)
                    {
                        if (tagsName.Contains(item.Value[0].tagTypeName))
                            continue;
                        tagsName.Add((item.Value[0].tagTypeName));
                    }
                }
            }
            if (tagsName.Count > 0)
                sFollowByTags = string.Join(",", tagsName);
            return sFollowByTags;
        }



        private static string UserFollowTags(NotificationMessage message, int nGroupID)
        {
            //get all notifications that user follow
            Dictionary<int, List<int>> lTag = GetUserNotifications(message.UserID);

            Dictionary<string, List<string>> notificationTags = null;
            //get name for all tagType + tagValues
            notificationTags = NotificationManager.GetTagsNameByIDs(lTag, nGroupID);

            string userFollowTags = string.Empty;
            List<string> valueList = null;
            List<string> followList = new List<string>();

            foreach (KeyValuePair<string, List<string>> tag in notificationTags)
            {
                valueList = new List<string>();
                foreach (string tagValue in tag.Value)
                {
                    if (!valueList.Contains(tagValue))
                        valueList.Add(tagValue);
                }
                followList.Add(string.Format("{0} : {1}", tag.Key, string.Join(",", valueList)));

            }

            string followstr = string.Join(" , ", followList.ToArray());
            return followstr;
        }

        private static Dictionary<int, List<int>> GetUserNotifications(long luserID)
        {
            try
            {

                int userID = 0;
                try
                {
                    userID = Convert.ToInt32(luserID);
                }
                catch
                {
                }
                DataTable dtUserTags = DAL.NotificationDal.GetUserNotification(userID, new List<long>(), 1);
                Dictionary<int, List<int>> lTag = new Dictionary<int, List<int>>();
                foreach (DataRow dr in dtUserTags.Rows)
                {
                    int tagTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr["sKey"]);
                    if (lTag.Keys.Contains<int>(tagTypeId))
                        continue;
                    DataRow[] result = dtUserTags.Select("sKey = " + tagTypeId);
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
                return lTag;
            }
            catch (Exception ex)
            {
                log.Error("GetUserNotifications - " + string.Format("Exception = {0}", ex.ToString()), ex);
                return null;
            }
        }


    }
}
