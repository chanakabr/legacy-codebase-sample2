using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class MessageAnnouncementFullData : QueueObject
    {
        public string Message;
        public string Url;
        public string Sound;
        public string Category;
        public long StartTime;

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
