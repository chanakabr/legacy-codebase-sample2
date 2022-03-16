using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByQualityInDiscoveryAction : KalturaFilterFileByQualityAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleActionType.FilterFileByQualityInDiscovery;
        }
    }
}