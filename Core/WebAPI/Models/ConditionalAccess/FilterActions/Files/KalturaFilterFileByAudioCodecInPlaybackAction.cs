using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByAudioCodecInPlaybackAction : KalturaFilterFileByAudioCodecAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByAudioCodecInPlayback;
        }
    }
}