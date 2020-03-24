namespace ApiObjects
{
    public class PlaybackPartnerConfig
    {
        public DefaultPlayback DefaultPlayback { get; set; }
    }

    public class DefaultPlayback
    {
        public long VodAdapter { get; set; }

        public long EpgAdapter { get; set; }

        public long RecordingAdapter { get; set; }
    }
}