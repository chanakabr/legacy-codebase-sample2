namespace ApiObjects.PlaybackAdapter
{
    public class PluginData
    {
    }

    public class DrmPlaybackPluginData : PluginData
    {
        public ApiObjects.PlaybackAdapter.DrmSchemeName Scheme { get; set; }

        public string LicenseURL { get; set; }
    }

    public class FairPlayPlaybackPluginData : DrmPlaybackPluginData
    {
        public string Certificate { get; set; }
    }

    public class CustomDrmPlaybackPluginData : DrmPlaybackPluginData
    {
        public string Data { get; set; }
    }
}
