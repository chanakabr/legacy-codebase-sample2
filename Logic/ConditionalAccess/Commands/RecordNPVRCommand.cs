using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class RecordNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.RecordNPVR(siteGuid, assetID, false, Version);
        }
    }
}
