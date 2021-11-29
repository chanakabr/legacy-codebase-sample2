using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdInPlaybackAction : KalturaFilterFileByFileTypeIdAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdInPlayback;
        }
    }
}