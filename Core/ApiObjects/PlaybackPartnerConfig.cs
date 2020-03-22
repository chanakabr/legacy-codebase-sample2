using System;

namespace ApiObjects
{
    public class PlaybackPartnerConfig
    {
        public long? VodDefaultAdapter { get; set; }

        public long? EpgDefaultAdapter { get; set; }

        public long? RecordingDefaultAdapter { get; set; }

        public bool SetUnchangedProperties(PlaybackPartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (this.VodDefaultAdapter.HasValue) // TODO Anat: remove 0? -1 
            {
                needToUpdate = true;
            }
            else
            {
                this.VodDefaultAdapter = oldConfig.VodDefaultAdapter;
            }

            if (this.EpgDefaultAdapter.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.EpgDefaultAdapter = oldConfig.EpgDefaultAdapter;
            }

            if (this.RecordingDefaultAdapter.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                this.RecordingDefaultAdapter = oldConfig.RecordingDefaultAdapter;
            }

            return needToUpdate;
        }
    }
}