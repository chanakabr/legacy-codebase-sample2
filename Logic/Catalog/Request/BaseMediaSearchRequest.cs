using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [DataContract]
    public abstract class BaseMediaSearchRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public bool m_bExact;
        [DataMember]
        public OrderObj m_oOrderObj;
        [DataMember]
        public List<Int32> m_nMediaTypes;
        [DataMember]
        public Int32 m_nMediaID; 

        public BaseMediaSearchRequest()
            : base()
        {
        }

        public BaseMediaSearchRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {

        }

        public BaseMediaSearchRequest(MediaSearchFullRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {

        }

        public BaseMediaSearchRequest(MediaSearchRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {

        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BaseMediaSearchRequest request  = (BaseMediaSearchRequest)oBaseRequest;
                MediaIdsResponse oMediaResponse = new MediaIdsResponse();

                if (request == null)
                    throw new ArgumentNullException("request object is null or Required variables is null");

                CheckSignature(oBaseRequest);

                //GetMediaIds With Searcher
                int nTotalItems = 0;
                bool isLucene = false;
                List<SearchResult> mediaIds = CatalogLogic.GetMediaIdsFromSearcher(request, ref nTotalItems, ref isLucene);
                oMediaResponse.m_nTotalItems = nTotalItems;

                if (nTotalItems > 0)
                {
                    if (isLucene)
                    {
                        //Complete max updatedate per mediaId
                        List<SearchResult> lMediaRes = Utils.GetMediaUpdateDate(mediaIds);
                        lMediaRes = Utils.GetMediaForPaging(lMediaRes, request);
                        oMediaResponse.m_nMediaIds = new List<SearchResult>(lMediaRes);
                    }
                    else //ElasticSearch
                    {
                        oMediaResponse.m_nMediaIds = mediaIds;
                    }

                   
                    
                 
                }
                
                return (BaseResponse)oMediaResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }
    }
}
