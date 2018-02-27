using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class CancelSeriesNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.CancelNPVR(siteGuid, assetID, true, Version);
        }
    }
}
