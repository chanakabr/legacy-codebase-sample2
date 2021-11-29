using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByAudioCodecInDiscoveryAction : KalturaFilterFileByAudioCodecAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByAudioCodecInDiscovery;
        }
    }
}