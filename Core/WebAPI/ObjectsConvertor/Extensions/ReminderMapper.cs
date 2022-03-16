using Nest;
using WebAPI.Models.Notifications;

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