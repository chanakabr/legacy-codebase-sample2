using System;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public partial class KalturaSeriesRecordingFilter : KalturaFilter<KalturaSeriesRecordingOrderBy>
    {  
        public override KalturaSeriesRecordingOrderBy GetDefaultOrderByValue()
        {
            return KalturaSeriesRecordingOrderBy.START_DATE_DESC;
        }
    }
}