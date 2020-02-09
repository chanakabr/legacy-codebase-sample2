using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class RuleAction
    {
        [DataMember]
        public RuleActionType Type { get; protected set; }

        [DataMember]
        public string Description { get; set; }
    }
}