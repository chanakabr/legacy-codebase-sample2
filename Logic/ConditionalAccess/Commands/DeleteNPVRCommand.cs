using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class DeleteNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.DeleteNPVR(siteGuid, assetID, false, Version);
        }
    }
}
