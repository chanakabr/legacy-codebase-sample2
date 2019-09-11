using ApiObjects.Base;

namespace ApiObjects
{
    public class EventNotificationActionFilter : ICrudFilter
    {
        public string Id { get; set; }
        public long? ObjectId { get; set; }
        public string  ObjectType { get; set; }
    }
}