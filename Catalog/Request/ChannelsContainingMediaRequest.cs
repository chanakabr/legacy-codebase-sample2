using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using Catalog.Cache;
using Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using TVinciShared;
using CachingProvider.LayeredCache;

namespace Catalog.Request
{
    [DataContract]
    public class ChannelsContainingMediaRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());        

        [DataMember]
        public List<int> m_lChannles;
        [DataMember]
        public int m_nMediaID;

        public ChannelsContainingMediaRequest()
            : base()
        {
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelsContainingMediaResponse response = new ChannelsContainingMediaResponse();
            try
            {
                ChannelsContainingMediaRequest request = (ChannelsContainingMediaRequest)oBaseRequest;

                if (request == null || request.m_lChannles == null || request.m_lChannles.Count == 0)
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                List<int> channels = null;
                List<string> invalidationKeys = new List<string>()
                {
                    LayeredCacheConfigNames.CHANNELS_INVALIDATION_KEY
                };

                string key = LayeredCacheKeys.GetChannelsContainingMediaKey(m_nMediaID);

                bool cacheResult = LayeredCache.Instance.Get<List<int>>(key, ref channels, GetMediaChannels, new Dictionary<string, object>() { { "groupId", request.m_nGroupID }, { "mediaId", request.m_nMediaID } },
                    request.m_nGroupID, LayeredCacheConfigNames.CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME, invalidationKeys);

                if (cacheResult && channels != null && channels.Count > 0)
                {
                    Dictionary<int, int> dChannels = channels.ToDictionary<int, int>(item => item);

                    foreach (int item in request.m_lChannles)
                    {
                        if (dChannels.ContainsKey(item))
                        {
                            response.m_lChannellList.Add(item);
                        }
                    }
                }

                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AllChannelsContainingMediaRequest failed ex={0} ", ex.Message);
                return null;
            }
        }

        private Tuple<List<int>, bool> GetMediaChannels(Dictionary<string, object> funcParams)
        {
            bool res = false;
            List<int> result = new List<int>();
            try
            {
                if (funcParams != null && funcParams.Count == 2)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("mediaId"))
                    {
                        int? groupId, mediaId;
                        groupId = funcParams["groupId"] as int?;
                        mediaId = funcParams["mediaId"] as int?;

                        ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
                        if (searcher != null)
                        {
                            if (searcher is ElasticsearchWrapper)
                            {
                                if (groupId.HasValue && mediaId.HasValue)
                                {
                                    result = searcher.GetMediaChannels(groupId.Value, mediaId.Value);
                                    res = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetMediaChannels faild params : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<List<int>, bool>(result, res);
        }

    }
}