using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Kaltura External Recording ResponseProfile Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaExternalRecordingResponseProfileFilter : KalturaFilter<KalturaExternalRecordingResponseProfileOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaExternalRecordingResponseProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaExternalRecordingResponseProfileOrderBy.NONE;
        }
    }

    public enum KalturaExternalRecordingResponseProfileOrderBy
    {
        NONE
    }
}