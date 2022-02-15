using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaAccessControlBlockAction : KalturaAssetRuleAction
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.BLOCK;
        }
    }
}