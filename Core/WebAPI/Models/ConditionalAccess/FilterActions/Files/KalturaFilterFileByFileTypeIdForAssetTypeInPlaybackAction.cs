using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdForAssetTypeInPlaybackAction : KalturaFilterFileByFileTypeIdForAssetTypeAction, IKalturaFilterFileInPlayback
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInPlayback;
        }
    }
}