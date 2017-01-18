using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Core.Notification
{
    //public class NotificationTagRequest : NotificationRequest
    //{
       
    //    //public ExtraParams oExtraParams { get; set; }
    //    public List<long> usersID { get; set; }

    //    public NotificationTagRequest(long id, long notificationID, NotificationRequestStatus status, long groupID, long userID, DateTime createdDate, NotificationRequestType type, NotificationMessageType messageType, string messageText,
    //        string smsMessageText, string pullMessageText, string title, NotificationRequestAction[] actions, Dictionary<string, List<int>> dTagDict, int nMediaID, string sPicURL, string sTemplateEmail, List<long> lUsersID)
    //        : base(id, notificationID, status, groupID, userID, createdDate, type, messageType, messageText,smsMessageText,pullMessageText, title, actions, NotificationTriggerType.FollowUpByTag)
    //    {
    //        oExtraParams = new ExtraParams();
    //        oExtraParams.mediaID = nMediaID;
    //        oExtraParams.TagDict = dTagDict;
    //        oExtraParams.mediaPicURL = sPicURL;
    //        oExtraParams.templateEmail = sTemplateEmail;

    //        usersID = lUsersID;
    //    }

    //    public NotificationTagRequest(FollowUpTagNotification followUp)
    //        : base( (Notification)followUp, 0)
    //    {
    //        if (followUp.notificationTag != null && !string.IsNullOrEmpty(followUp.key))
    //        {
    //            oExtraParams.TagDict = new Dictionary<string, List<int>>();
    //            oExtraParams.TagDict[followUp.key] = followUp.notificationTag.tagValues;               
    //            oExtraParams.mediaID = followUp.notificationTag.mediaID;
    //            oExtraParams.mediaPicURL = followUp.notificationTag.mediaPicURL;
    //            oExtraParams.templateEmail = followUp.notificationTag.templateEmail;
    //        }          
    //    }



    //}

    //[Serializable]
    //public class ExtraParams
    //{
    //    //parameters for notification with specific values 
    //    [DataMember]
    //    public Dictionary<string, List<int>> TagDict { get; set; }
    //    [DataMember]
    //    public Dictionary<int, List<TagIDValue>> dTagDict { get; set; }
    //    [DataMember]
    //    public int mediaID { get;set; }
    //    [DataMember]
    //    public List<long> notificationsID { get; set; }
    //    [ScriptIgnore]
    //    public string mediaPicURL { get; set; }
    //    [ScriptIgnore]
    //    public string templateEmail { get; set; }
    //    [ScriptIgnore]
    //    public string subjectEmail { get; set; }
       
    //    public ExtraParams()
    //    {
    //    }
    //}







}
  