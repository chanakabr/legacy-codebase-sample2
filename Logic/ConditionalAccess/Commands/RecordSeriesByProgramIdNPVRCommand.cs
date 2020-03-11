using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class RecordSeriesByProgramIdNPVRCommand : BaseNPVRCommand
    {
        public bool NewVersion {get;set;}

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.RecordSeriesByProgramID(siteGuid, assetID, Version);
        }
    }
}
