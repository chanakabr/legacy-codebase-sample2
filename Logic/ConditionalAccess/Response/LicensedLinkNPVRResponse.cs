using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class LicensedLinkNPVRResponse : NPVRResponse
    {
        public string mainUrl;

        public DrmPlaybackPluginData drm;
    }
}
