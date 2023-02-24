using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByDynamicDataInDiscoveryAction : KalturaFilterFileByDynamicDataAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByDynamicDataInDiscovery;
        }
    }
}