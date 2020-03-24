namespace ApiObjects
{
    public class PlaybackPartnerConfig
    {
        public DefaultPlayback DefaultPlayback { get; set; }

        public bool SetUnchangedProperties(PlaybackPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.DefaultPlayback != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.DefaultPlayback = oldConfig.DefaultPlayback;
            }

            return needToUpdate;
        }
    }

    public class DefaultPlayback
    {
        public long VodAdapter { get; set; }

        public long EpgAdapter { get; set; }

        public long RecordingAdapter { get; set; }
    }
}