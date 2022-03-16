using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByQualityInPlaybackAction : KalturaFilterFileByQualityAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByQualityInPlayback;
        }
    }
}