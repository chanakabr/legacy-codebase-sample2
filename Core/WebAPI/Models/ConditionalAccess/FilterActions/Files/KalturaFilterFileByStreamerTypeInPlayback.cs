using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByStreamerTypeInPlayback : KalturaFilterFileByStreamerTypeAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByStreamerTypeInPlayback;
        }
    }
}