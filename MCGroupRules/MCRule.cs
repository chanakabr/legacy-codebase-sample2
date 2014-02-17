using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCGroupRules
{
    public class MCRule
    {
        public int RuleID { get; set; }
        public RuleType RuleType { get; set; }
        public int GroupID { get; set; }
        public int FrequencyID { get; set; }
        public int SubscriptionID { get; set; }


        public MCRule(int groupid, int ruleid, RuleType type, int frequencyID, int subscriptionid)
        {
            RuleID = ruleid;
            RuleType = type;
            GroupID = groupid;
            FrequencyID = frequencyID;
            SubscriptionID = subscriptionid;
        }
    }

    public enum RuleType
    {
        HaventWatched = 1,
        PaymentFailed = 2,
        RegistrationDate = 3,
        SubscriptionTypeAfterEnd = 4,
        SubscriptionTypeBeforeEnd = 5,
        HaventWatchedSinceRegistration= 6,
        SubscriptionEnded = 7,
        SocialInviteTriggered = 8
    }
}

