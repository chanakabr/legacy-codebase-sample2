using System;
using System.Collections.Generic;

namespace ApiObjects.Rules
{
    [Serializable]
    public class ChannelCondition : RuleCondition
    {
        public List<long> ChannelIds { get; set; }

        public ChannelCondition()
        {
            Type = RuleConditionType.Channel;
        }
    }
}