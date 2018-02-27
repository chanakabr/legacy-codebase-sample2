using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class CancelNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.CancelNPVR(siteGuid, assetID, false, Version);
        }
    }
}
