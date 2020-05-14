namespace ApiObjects.CouchbaseWrapperObjects
{
    public class CBCategoryTimeSlotData
    {
        public long Id { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public static string GetCategoryTimeSlotDataKey(long id)
        {
            return $"category_timeslot_{id}";
        }
    }
}