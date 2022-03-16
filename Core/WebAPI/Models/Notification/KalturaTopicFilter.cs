using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public partial class KalturaTopicFilter : KalturaFilter<KalturaTopicOrderBy>
    {
        public override KalturaTopicOrderBy GetDefaultOrderByValue()
        {
            return KalturaTopicOrderBy.NONE;
        }
    }
}