namespace ApiObjects
{
    public class PlaybackPartnerConfig
    {
        public DefaultPlaybackAdapters DefaultAdapters { get; set; }

        public bool SetUnchangedProperties(PlaybackPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.DefaultAdapters != null)
            {
                needToUpdate = true;
            }
            else
            {
                this.DefaultAdapters = oldConfig.DefaultAdapters;
            }

            return needToUpdate;
        }
    }

    public class DefaultPlaybackAdapters
    {
        public long MediaAdapterId { get; set; }

        public long EpgAdapterId { get; set; }

        public long RecordingAdapterId { get; set; }
    }
}