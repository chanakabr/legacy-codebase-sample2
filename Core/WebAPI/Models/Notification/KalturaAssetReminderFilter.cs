namespace WebAPI.Models.Notification
{
    public partial class KalturaAssetReminderFilter : KalturaReminderFilter<KalturaAssetReminderOrderBy>
    {
        public override KalturaAssetReminderOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetReminderOrderBy.RELEVANCY_DESC;
        }
    }
}