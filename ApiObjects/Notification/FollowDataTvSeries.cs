using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [DataContract]
    public class FollowDataTvSeries : FollowDataBase
    {
        [DataMember]
        public int AssetId;

        public FollowDataTvSeries()
            : base()
        {
        }

        public FollowDataTvSeries(int groupId, string followPhrase) : base(groupId, followPhrase) { }
    }
}
