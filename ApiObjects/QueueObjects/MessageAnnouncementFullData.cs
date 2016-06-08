using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class MessageAnnouncementFullData : QueueObject
    {       
        #region Data Members
        
        private string Message;
        private string Url;
        private string Sound;
        private string Category;
        long StartTime;

        #endregion

        public MessageAnnouncementFullData(int groupId, string message, string url, string sound, string category, long startTime)
        {
            GroupId = groupId;
            Message = message;
            Url = url;
            Sound = sound;
            Category = category;
            StartTime = startTime;
        }
    }
}
