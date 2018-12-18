namespace ApiObjects.PlaybackAdapter
{
    public enum PlaybackAdapterAdsPolicy
    {
        NO_ADS,
        KEEP_ADS
    }

    public enum PlaybackAdapterDrmSchemeName
    {
        PLAYREADY_CENC,
        WIDEVINE_CENC,
        FAIRPLAY,
        WIDEVINE,
        PLAYREADY,
        CUSTOM_DRM
    }

    public enum PlaybackAdapterRuleActionType
    {
        BLOCK,
        START_DATE_OFFSET,
        END_DATE_OFFSET,
        USER_BLOCK,
        ALLOW_PLAYBACK,
        BLOCK_PLAYBACK,
        APPLY_DISCOUNT_MODULE,
        APPLY_PLAYBACK_ADAPTER
    }
}
