using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [Serializable]
    [DataContract]
    public class ChannelViewsRequest : BaseRequest, IRequestImp
    {
        public ChannelViewsRequest(Int32 nPageSize, Int32 nPageIndex, string sIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sIP, nGroupID, oFilter, sSignature, sSignString)
        { }

        public ChannelViewsRequest()
        { }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelViewsRequest request = (ChannelViewsRequest)oBaseRequest;

            if (request == null || request.m_nGroupID == 0)
                throw new Exception("request object is null or Required variables is null");

            string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
            if (sCheckSignature != request.m_sSignature)
                throw new Exception("Signatures dosen't match");

            ChannelViewsResponse response = new ChannelViewsResponse();

            try
            {
                ApiObjects.SearchObjects.ISearcher searcher = Bootstrapper.GetInstance<ApiObjects.SearchObjects.ISearcher>();
                List<ChannelViewsResult> channelViewsResult = Utils.GetChannelViewsResult(request.m_nGroupID);

                if (channelViewsResult != null)
                {
                    int nValidNumberOfMediasRange = request.m_nPageSize;

                    if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(channelViewsResult.Count, request.m_nPageIndex, ref nValidNumberOfMediasRange))
                    {
                        if (nValidNumberOfMediasRange > 0)
                        {
                            channelViewsResult = channelViewsResult.GetRange(request.m_nPageSize * request.m_nPageIndex, nValidNumberOfMediasRange);
                        }
                    }
                    else
                    {
                        channelViewsResult.Clear();
                    }

                    response.ChannelViews = channelViewsResult;
                    response.m_nTotalItems = channelViewsResult.Count;
                }
            }
            catch
            {

            }

            return response;
        }
    }
}
