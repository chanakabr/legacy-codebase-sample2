using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    [Serializable]
    public class NotificationTag
    {

        #region public Properties

        public int mediaID { get; set; }
        public List<int> tagValues { get; set; }
        public string mediaPicURL { get; set; }
        public string templateEmail { get; set; }
        public List<long> notificationsID { get; set; }
        public Dictionary<string, List<TagIDValue>> tagValueDict { get; set; }

        #endregion;

        public NotificationTag(int nMediaID, List<int> dTagValues)
        {
            mediaID = nMediaID;
            tagValues = dTagValues;
        }

        public NotificationTag(int nMediaID, List<int> dTagValues, Dictionary<string, List<TagIDValue>> dtagValueDict)
        {
            mediaID = nMediaID;
            tagValues = dTagValues;
            tagValueDict = dtagValueDict;
        }

        public NotificationTag(int nMediaID, List<int> dTagValues, string sURLPic, string sTemplateEmail)
        {
            mediaID = nMediaID;
            tagValues = dTagValues;
            mediaPicURL = sURLPic;
            templateEmail = sTemplateEmail;
        }

        public NotificationTag(int nMediaID, List<int> dTagValues, string sURLPic, string sTemplateEmail, List<long> lnotificationsID)
        {
            mediaID = nMediaID;
            tagValues = dTagValues;
            mediaPicURL = sURLPic;
            templateEmail = sTemplateEmail;
            notificationsID = lnotificationsID;
        }
    }
}
