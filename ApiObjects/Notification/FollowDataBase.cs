using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [DataContract]
    public class FollowDataBase
    {
        [DataMember]
        public int GroupId;

        [DataMember]
        public long AnnouncementId;

        [DataMember]
        public int Status;

        [DataMember]
        public string Title;

        [DataMember]
        public long Timestamp;

        [DataMember]
        public string FollowReference;

        [DataMember]
        public int Type;

        [DataMember]
        public string FollowPhrase;

        public FollowDataBase()
        {
        }

        public FollowDataBase(int groupId, string followPhrase)
        {
            this.GroupId = groupId;
            this.FollowPhrase = followPhrase;
        }
    }
}
