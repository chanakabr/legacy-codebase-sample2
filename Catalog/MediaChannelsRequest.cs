using ApiObjects.SearchObjects;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    public class MediaChannelsRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public int m_nMediaID;


        public MediaChannelsRequest()
            : base()
        { }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                MediaChannelsRequest request = oBaseRequest as MediaChannelsRequest;
                List<SearchResult> lMedias = new List<SearchResult>();

                MediaChannelsResponse response = new MediaChannelsResponse();

                _logger.Info(string.Format("{0}: {1}", "MediaChannelsRequest Start At", DateTime.Now));


                if (request == null)
                    throw new ArgumentNullException("request object is null or Required variables is null");

                CheckSignature(request);

                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                if (searcher != null)
                {
                    List<int> lChannels = searcher.GetMediaChannels(request.m_nGroupID, request.m_nMediaID);

                    if (lChannels != null && lChannels.Count > 0)
                    {
                        response.m_nChannelIDs.AddRange(lChannels);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }

        }


    }
}
