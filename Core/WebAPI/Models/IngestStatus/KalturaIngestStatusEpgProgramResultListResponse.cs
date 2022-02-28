using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.IngestStatus
{
    [ListResponse("IngestStatusEpgProgramResult")]
    public partial class KalturaIngestStatusEpgProgramResultListResponse : KalturaListResponse<KalturaIngestEpgProgramResult>
    {
    }
}