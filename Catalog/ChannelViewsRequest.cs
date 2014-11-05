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
        {

        }

        protected override void CheckRequestValidness()
        {
            if (m_nGroupID == 0)
                throw new ArgumentException("request object is null or Required variables is null");
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelViewsResponse response = new ChannelViewsResponse();
            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                List<ChannelViewsResult> channelViewsResult = Catalog.GetChannelViewsResult(m_nGroupID);

                if (channelViewsResult != null && channelViewsResult.Count > 0)
                {
                    int nValidNumberOfMediasRange = m_nPageSize;

                    if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(channelViewsResult.Count, m_nPageIndex, ref nValidNumberOfMediasRange))
                    {
                        if (nValidNumberOfMediasRange > 0)
                        {
                            channelViewsResult = channelViewsResult.GetRange(m_nPageSize * m_nPageIndex, nValidNumberOfMediasRange);
                        }
                    }
                    else
                    {
                        channelViewsResult.Clear();
                    }

                    response.ChannelViews = channelViewsResult;
                    response.m_nTotalItems = channelViewsResult.Count;
                }
                else
                {
                    Logger.Logger.Log("Error", String.Concat("GetChannelViewsResult returned no items. Req: ", ToString()), "ChannelViewsRequest");
                }
            }
            catch(Exception ex)
            {
                Logger.Logger.Log("Exception", String.Concat("Exception at ChannelViewsRequest. Req: ", ToString(), " Ex Msg: ", ex.Message, " ST: ", ex.StackTrace), "ChannelViewsRequest");
                throw ex;
            }

            return response;
        }
    }
}
