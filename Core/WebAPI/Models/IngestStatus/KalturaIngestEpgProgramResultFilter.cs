using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    public partial class KalturaIngestEpgProgramResultFilter : KalturaFilter<KalturaIngestEpgProgramResultOrderBy>
    {
        public override KalturaIngestEpgProgramResultOrderBy GetDefaultOrderByValue()
        {
            return KalturaIngestEpgProgramResultOrderBy.NONE;
        }
    }
}