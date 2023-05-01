using WebAPI.Models.Notification;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AnnouncementMapper
    {
        public static long getStartTime(this KalturaAnnouncement model)
        {
            return model.StartTime.HasValue ? (long)model.StartTime : 0;
        }

        public static int getId(this KalturaAnnouncement model)
        {
            return model.Id.HasValue ? (int)model.Id : 0;
        }

        public static bool getEnabled(this KalturaAnnouncement model)
        {
            return model.Enabled.HasValue ? (bool)model.Enabled : true;
        }
    }
}
