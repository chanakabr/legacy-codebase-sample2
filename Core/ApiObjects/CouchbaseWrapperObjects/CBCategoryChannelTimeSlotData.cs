namespace ApiObjects.CouchbaseWrapperObjects
{
    public class CBCategoryChannelTimeSlotData
    {
        public long CategoryId { get; set; }
        public long ChannelId { get; set; }
        public int ChanneType { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public static string GetKey(long categoryId, long channelId, int channeType)
        {
            return $"category_{categoryId}_channel_{channelId}_type_{channeType}_timeslot";
        }
    }
}