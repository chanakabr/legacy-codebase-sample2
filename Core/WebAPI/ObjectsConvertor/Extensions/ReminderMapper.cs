using WebAPI.Models.Notification;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ReminderMapper
    {
        public static  int getId(this KalturaReminder model)
        {
            return model.Id.HasValue ? (int)model.Id : 0;
        }
    }
}