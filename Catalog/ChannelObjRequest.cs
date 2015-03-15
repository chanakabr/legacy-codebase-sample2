using Catalog.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GroupsCacheManager;

namespace Catalog
{
    [DataContract]
    public class ChannelObjRequest : BaseRequest, IRequestImp
    {
        [DataMember]
        public int ChannelId { get; set; }

        public ChannelObjRequest(Int32 nChannelID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, ApiObjects.SearchObjects.OrderObj oOrderObj)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            ChannelId = nChannelID;
        }

                public ChannelObjRequest(ChannelObjRequest c)
            : base(c.m_nPageSize, c.m_nPageIndex, c.m_sUserIP, c.m_nGroupID, c.m_oFilter, c.m_sSignature, c.m_sSignString)
        {
            ChannelId = c.ChannelId;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelObjRequest request = (ChannelObjRequest)oBaseRequest;
            ChannelObjResponse response = new ChannelObjResponse();
            Group group = null;
            Channel channel = null;

            GroupManager groupManager = new GroupManager();

            CatalogCache catalogCache = CatalogCache.Instance();            
            int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);

            groupManager.GetGroupAndChannel(request.ChannelId, nParentGroupID, ref group, ref channel);

            if (channel != null)
            {
                response = new ChannelObjResponse
                {
                    ChannelObj = channel,
                };
            }
            return (BaseResponse)response;
        }
    }
}
