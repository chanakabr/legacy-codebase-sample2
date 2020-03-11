using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class RecordSeriesByNameNPVRCommand : BaseNPVRCommand
    {
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.RecordSeriesByName(siteGuid, assetID, Version);
        }
    }
}
