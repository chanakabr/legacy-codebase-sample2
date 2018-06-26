namespace Core.ConditionalAccess
{
    public class DrmPlaybackPluginData
    {
        public DrmSchemeName scheme { get; set; }

        public string data { get; set; }
    }

    public enum DrmSchemeName
    {
        PLAYREADY_CENC,
        WIDEVINE_CENC,
        FAIRPLAY,
        WIDEVINE,
        PLAYREADY,
        CUSTOM_DRM,
    }
}
