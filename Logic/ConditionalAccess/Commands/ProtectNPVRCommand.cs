using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class ProtectNPVRCommand : BaseNPVRCommand
    {
        public bool isProtect;

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.SetNPVRProtectionStatus(siteGuid, assetID, false, isProtect, Version);
        }
    }
}
