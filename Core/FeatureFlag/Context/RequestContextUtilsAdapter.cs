using KalturaRequestContext;

namespace FeatureFlag.Context
{
    public class RequestContextUtilsAdapter : IFeatureFlagContext
    {
        private readonly IRequestContextUtils _requestContextUtils;

        public RequestContextUtilsAdapter(IRequestContextUtils requestContextUtils)
        {
            _requestContextUtils = requestContextUtils;
        }
        
        public long? GetPartnerId()
        {
            return _requestContextUtils.GetPartnerId();
        }

        public long? GetUserId()
        {
            return _requestContextUtils.GetUserId();
        }
    }
}
