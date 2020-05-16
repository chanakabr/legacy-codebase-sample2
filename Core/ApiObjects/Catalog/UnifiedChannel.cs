namespace ApiObjects
{
    public class UnifiedChannel
    {
        public long Id { get; set; }

        public UnifiedChannelType Type { get; set; }


    }

    public class UnifiedChannelInfo : UnifiedChannel
    {
        public string Name { get; set; }
        public TimeSlot TimeSlot { get; set; }
    }

    public enum UnifiedChannelType
    {
        Internal,
        External
    }
}