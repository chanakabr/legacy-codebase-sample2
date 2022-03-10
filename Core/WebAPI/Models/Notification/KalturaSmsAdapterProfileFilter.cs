using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaSmsAdapterProfileFilter : KalturaFilter<KalturaSmsAdapterProfileOrderBy>
    {
        public override KalturaSmsAdapterProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaSmsAdapterProfileOrderBy.NONE;
        }
    }
}