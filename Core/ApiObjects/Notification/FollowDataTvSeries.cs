using ApiObjects.Base;
using System.Runtime.Serialization;

namespace ApiObjects.Notification
{
    [DataContract]
    public class FollowDataTvSeries : FollowDataBase, ICrudHandeledObject
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
