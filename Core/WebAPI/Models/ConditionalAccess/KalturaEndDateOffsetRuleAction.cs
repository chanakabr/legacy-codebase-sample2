using System;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// End date offset action
    /// </summary>
    [Serializable]
    public partial class KalturaEndDateOffsetRuleAction : KalturaTimeOffsetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.END_DATE_OFFSET;
        }
    }
}