
namespace WebAPI.Models.Notifications
{
    /// <summary>
    /// Announcement status: NotSent=0/Sending=1/Sent=2/Aborted=3
    /// </summary>
    public enum KalturaAnnouncementStatus
    {
        NotSent = 0,
        Sending = 1,
        Sent = 2,
        Aborted = 3
    }
}