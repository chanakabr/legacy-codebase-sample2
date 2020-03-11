namespace ApiObjects.PlaybackAdapter
{
    public class PlaybackPluginData
    {
    }

    public class BumperPlaybackPluginData : PlaybackPluginData
    {
        public string URL { get; set; }

        public string StreamerType { get; set; }
    }
}