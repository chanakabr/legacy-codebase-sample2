using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByVideoCodecInDiscoveryAction : KalturaFilterFileByVideoCodecAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByVideoCodecInDiscovery;
        }
    }
}