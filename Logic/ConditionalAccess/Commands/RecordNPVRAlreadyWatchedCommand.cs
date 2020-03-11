using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class RecordNPVRAlreadyWatchedCommand : BaseNPVRCommand
    {
        public int alreadyWatched { get; set; }

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.SetAssetAlreadyWatchedStatus(siteGuid, assetID, alreadyWatched, Version);
        }
    }   
}
