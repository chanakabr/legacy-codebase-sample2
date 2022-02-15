using System;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Start date offset action
    /// </summary>
    [Serializable]
    public partial class KalturaStartDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.START_DATE_OFFSET;
        }
    }
}