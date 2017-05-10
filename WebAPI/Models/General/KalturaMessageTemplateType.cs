
namespace WebAPI.Models.General
{
    /// <summary>
    /// Message template type
    /// </summary>
    [KalturaIntEnum]
    public enum KalturaMessageTemplateType
    {
        Series = 0,
        Reminder = 1,
        Churn = 2,
        SeriesReminder = 3
    }
}