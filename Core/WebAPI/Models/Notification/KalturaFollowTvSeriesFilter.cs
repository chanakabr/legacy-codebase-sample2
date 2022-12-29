using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaFollowTvSeriesFilter : KalturaFilter<KalturaFollowTvSeriesOrderBy>
    {
        public override KalturaFollowTvSeriesOrderBy GetDefaultOrderByValue()
        {
            return KalturaFollowTvSeriesOrderBy.START_DATE_DESC;
        }
    }
}