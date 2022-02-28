namespace WebAPI.Models.IngestStatus
{
    public enum KalturaIngestEpgProgramResultOrderBy
    {
        NONE,
        EXTERNAL_PROGRAM_ID_DESC,
        EXTERNAL_PROGRAM_ID_ASC,
        LINEAR_CHANNEL_ID_DESC,
        LINEAR_CHANNEL_ID_ASC,
        INDEX_IN_FILE_DESC,
        INDEX_IN_FILE_ASC,
        START_DATE_DESC,
        START_DATE_ASC,
        SEVERITY_DESC,
        SEVERITY_ASC
    }
}