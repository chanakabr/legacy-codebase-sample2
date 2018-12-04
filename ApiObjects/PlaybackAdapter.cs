namespace ApiObjects
{
    public class PlaybackAdapter
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string AdapterUrl { get; set; }
        public string ExternalIdentifier { get; set; }
        public string SharedSecret { get; set; }
        public string Ksql { get; set; }
        public string Settings { get; set; }
    }
}
