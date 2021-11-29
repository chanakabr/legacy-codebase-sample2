using System;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaBlockPlaybackAction : KalturaAssetRuleAction
    {
        public KalturaBlockPlaybackAction()
        {
            this.Type = KalturaRuleActionType.BLOCK_PLAYBACK;
        }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.BLOCK_PLAYBACK;
        }
    }
}