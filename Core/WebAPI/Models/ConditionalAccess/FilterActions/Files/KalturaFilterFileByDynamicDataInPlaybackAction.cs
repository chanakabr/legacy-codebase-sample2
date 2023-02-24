using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByDynamicDataInPlaybackAction : KalturaFilterFileByDynamicDataAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByDynamicDataInPlayback;
        }
    }
}