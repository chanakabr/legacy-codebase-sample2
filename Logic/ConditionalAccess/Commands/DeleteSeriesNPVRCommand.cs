using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class DeleteSeriesNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.DeleteNPVR(siteGuid, assetID, true, Version);
        }
    }
}
