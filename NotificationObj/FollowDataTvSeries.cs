using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NotificationObj
{
    [DataContract]
    public class FollowDataTvSeries : FollowDataBase
    {
        [DataMember]
        public int AssetId;

        public FollowDataTvSeries(int groupId, string followPhrase) : base(groupId, followPhrase) { }
    }
}
