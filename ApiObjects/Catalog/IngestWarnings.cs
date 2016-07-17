
namespace ApiObjects.Catalog
{
    public enum IngestWarnings
    {
        MissingEntryId,
        MissingAction,
        NotRecognizedItemType,
        NotRecognizedWatchPermissionRule,
        NotRecognizedGeoBlockRule,
        NotRecognizedDeviceRule,
        NotRecognizedPlayersRule,
        FailedDownloadPic,
        UpdateIndexFailed,
        ErrorExportChannel,
        MediaIdNotExist,
        EPGSchedIdNotExist,
        EPGSProgramDatesError
    }
}
