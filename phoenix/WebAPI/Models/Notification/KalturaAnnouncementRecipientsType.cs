
namespace WebAPI.Models.Notifications
{
    /// <summary>
    /// Announcement recipients: All=0/LoggedIn=1/Guests=2/Other=3
    /// </summary>
    public enum KalturaAnnouncementRecipientsType
    {
        All = 0,
        LoggedIn = 1,
        Guests = 2,
        Other = 3
    }
}