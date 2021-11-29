using System;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    [Serializable]
    public partial class KalturaFilterFileByLabelInDiscoveryAction : KalturaFilterFileByLabelAction, IKalturaFilterFileInDiscovery
    {
        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.FilterFileByLabelInDiscovery;
        }
    }
}