using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaApplyFreePlaybackAction : KalturaBusinessModuleRuleAction
    {
        public KalturaApplyFreePlaybackAction()
        {
            this.Type = KalturaRuleActionType.APPLY_FREE_PLAYBACK;
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_FREE_PLAYBACK;
        }
    }
}