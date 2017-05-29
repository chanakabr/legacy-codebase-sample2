using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    [DataContract]
    public class InterestNotification
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ExternalId { get; set; }

        [DataMember]
        public MessageTemplateType TemplateType { get; set; }

        [DataMember]
        public eAssetTypes AssetType { get; set; }

        [DataMember]
        public long LastMessageSentDateSec { get; set; }

        [DataMember]
        public string QueueName { get; set; }

        [DataMember]
        public string TopicNameValue { get; set; }

        [DataMember]
        public int TopicInterestId { get; set; }
    }
}
