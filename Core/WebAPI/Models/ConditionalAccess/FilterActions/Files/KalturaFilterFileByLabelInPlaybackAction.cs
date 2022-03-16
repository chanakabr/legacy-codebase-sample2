using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByLabelInPlaybackAction : KalturaFilterFileByLabelAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByLabelInPlayback;
        }
    }
}