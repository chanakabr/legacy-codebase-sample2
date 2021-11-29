using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaAssetUserRuleBlockAction : KalturaAssetUserRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.USER_BLOCK;
        }
    }
}