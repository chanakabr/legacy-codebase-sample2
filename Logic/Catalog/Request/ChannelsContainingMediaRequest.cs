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
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using TVinciShared;
using CachingProvider.LayeredCache;

namespace Core.Catalog.Request
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

                List<int> channels = Utils.GetChannelsContainingMedia(request.m_nGroupID, request.m_nMediaID);
                if (channels != null && channels.Count > 0)
                {
                    HashSet<int> channelsMap = new HashSet<int>(channels);
                    foreach (int item in request.m_lChannles)
                    {
                        if (channelsMap.Contains(item))
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

        
    }
}