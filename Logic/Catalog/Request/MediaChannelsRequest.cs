using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    public class MediaChannelsRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public int m_nMediaID;


        public MediaChannelsRequest()
            : base()
        { }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                MediaChannelsRequest request = oBaseRequest as MediaChannelsRequest;
                List<SearchResult> lMedias = new List<SearchResult>();

                MediaChannelsResponse response = new MediaChannelsResponse();


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
                log.Error(ex.Message, ex);
                throw ex;
            }

        }


    }
}
