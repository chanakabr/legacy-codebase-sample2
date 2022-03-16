using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByFileTypeIdInDiscoveryAction : KalturaFilterFileByFileTypeIdAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByFileTypeIdInDiscovery;
        }
    }
}