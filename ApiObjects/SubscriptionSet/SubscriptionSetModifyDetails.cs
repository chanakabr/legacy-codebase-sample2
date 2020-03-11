using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SubscriptionSet
{
    [Serializable]
    public class SubscriptionSetModifyDetails
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("UserId")]
        public string UserId { get; set; }

        [JsonProperty("DomainId")]
        public long DomainId { get; set; }

        [JsonProperty("SubscriptionId")]
        public long SubscriptionId { get; set; }

        [JsonProperty("PreviousSubscriptionId")]
        public long PreviousSubscriptionId { get; set; }

        [JsonProperty("UDID")]
        public string UDID { get; set; }

        [JsonProperty("UserIp")]
        public string UserIp { get; set; }

        [JsonProperty("Type")]
        public SubscriptionSetModifyType Type { get; set; }        

        public SubscriptionSetModifyDetails()
        {
            Id = 0;
            GroupId = 0;
            UserId = string.Empty;
            DomainId = 0;
            SubscriptionId = 0;
            UDID = string.Empty;
            UserIp = string.Empty;
            Type = SubscriptionSetModifyType.Unknown;
        }

        public SubscriptionSetModifyDetails(long id, int groupId, string userId, long domainId, long subscriptionId, string previousSubscriptionId, string udid, string userIp, SubscriptionSetModifyType type)
        {
            Id = id;
            GroupId = groupId;
            UserId = userId;
            DomainId = domainId;
            long prevSubId = 0;
            if (long.TryParse(previousSubscriptionId, out prevSubId) && prevSubId > 0)
            {
                PreviousSubscriptionId = prevSubId;
            }

            SubscriptionId = subscriptionId;
            UDID = udid;
            UserIp = userIp;
            Type = type;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.AppendFormat("Id: {0}, ", Id);
            sb.AppendFormat("GroupId: {0}, ", GroupId);
            sb.AppendFormat("UserId: {0}, ", string.IsNullOrEmpty(UserId) ? string.Empty : UserId);
            sb.AppendFormat("DomainId: {0}, ", DomainId);
            sb.AppendFormat("SubscriptionId: {0}, ", SubscriptionId);
            sb.AppendFormat("PreviousSubscriptionId: {0}, ", PreviousSubscriptionId);
            sb.AppendFormat("UDID: {0}, ", string.IsNullOrEmpty(UDID) ? string.Empty : UDID);
            sb.AppendFormat("UserIp: {0}, ", string.IsNullOrEmpty(UserIp) ? string.Empty : UserIp);
            sb.AppendFormat("Type: {0}, ", Type.ToString());

            return sb.ToString();
        }

    }
}
