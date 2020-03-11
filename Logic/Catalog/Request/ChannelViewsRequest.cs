using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;
using KLogMonitor;
using Tvinci.Core.DAL;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class ChannelViewsRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ChannelViewsRequest(Int32 nPageSize, Int32 nPageIndex, string sIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString, List<Int32> nMediaTypes)
            : base(nPageSize, nPageIndex, sIP, nGroupID, oFilter, sSignature, sSignString)
        {

        }

        public ChannelViewsRequest(Int32 nPageSize, Int32 nPageIndex, string sIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sIP, nGroupID, oFilter, sSignature, sSignString)
        {

        }

        public ChannelViewsRequest()
        {

        }

        protected override void CheckRequestValidness()
        {
            if (m_nGroupID == 0)
                throw new ArgumentException("request object is null or Required variables is null");
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelViewsResponse response = new ChannelViewsResponse();

            CheckRequestValidness();
            CheckSignature(this);

            try
            {
                //Get Linear Media Type
                List<int> nMediaTypes = CatalogDAL.Get_LinearMediaType(m_nGroupID);

                List<ChannelViewsResult> channelViewsResult = Utils.GetChannelViewsResult(m_nGroupID, nMediaTypes);

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
                    log.Error("Error - " + String.Concat("GetChannelViewsResult returned no items. Req: ", ToString()));
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - " + String.Concat("Exception at ChannelViewsRequest. Req: ", ToString(), " Ex Msg: ", ex.Message, " ST: ", ex.StackTrace), ex);
                throw ex;
            }

            return response;
        }
    }
}
