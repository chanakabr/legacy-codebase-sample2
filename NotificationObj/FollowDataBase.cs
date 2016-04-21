using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationObj
{
    public class FollowDataBase
    {
        public long AnnouncementId;
        public int Status;
        public string Title;
        public long Timestamp;
        protected string _followPhrase;

        public virtual string FollowPhrase
        {
            get { return _followPhrase; }
            set { _followPhrase = value; }
        }        
    }
}
