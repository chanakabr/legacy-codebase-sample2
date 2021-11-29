using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdForAssetTypeInDiscoveryAction : KalturaFilterFileByFileTypeIdForAssetTypeAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdForAssetTypeInDiscovery;
        }
    }
}