using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using TVinciShared;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using ApiObjects.SearchObjects;
using Catalog.Response;
using KLogMonitor;

namespace Catalog.Request
{
    /**************************************************************************************
   * return : Return all medias that share the same values Like the mediaID that was send
   * *************************************************************************************/
    [DataContract]
    public class MediaRelatedRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nMediaID;
        [DataMember]
        public List<Int32> m_nMediaTypes;

        public MediaRelatedRequest()
            : base()
        {
            m_nMediaTypes = new List<Int32>();
        }

        public MediaRelatedRequest(Int32 nMediaID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, List<Int32> nMediaTypes)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nMediaID = nMediaID;
            m_nMediaTypes = nMediaTypes;
            m_nMediaTypes = new List<Int32>();
        }

        public MediaRelatedRequest(MediaRelatedRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            m_nMediaID = m.m_nMediaID;
            m_nMediaTypes = m.m_nMediaTypes;
            m_nMediaTypes = new List<Int32>();
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaSearchRequest oMediaRequest;
            MediaIdsResponse oMediaResponse = new MediaIdsResponse();
            MediaRelatedRequest request = oBaseRequest as MediaRelatedRequest;

            Filter oFilter = new Filter();
            try
            {
                //Build  MediaSearchRequest object
                if (request == null || request.m_nMediaID == 0 || request.m_oFilter == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                bool bIsMainLang = Utils.IsLangMain(request.m_nGroupID, request.m_oFilter.m_nLanguage);
                oMediaRequest = Catalog.BuildMediasRequest(request.m_nMediaID, bIsMainLang, request.m_oFilter, ref oFilter, request.m_nGroupID, request.m_nMediaTypes, request.m_sSiteGuid);

                oMediaRequest.m_oFilter = oFilter;
                oMediaRequest.m_nMediaID = request.m_nMediaID;
                oMediaRequest.m_sSignString = request.m_sSignString;
                oMediaRequest.m_sSignature = request.m_sSignature;
                oMediaRequest.m_nPageSize = request.m_nPageSize;
                oMediaRequest.m_nPageIndex = request.m_nPageIndex;
                oMediaRequest.m_sUserIP = request.m_sUserIP;
                oMediaRequest.m_bAnd = false;
                oMediaRequest.m_bExact = true;

                //GetMediaIds With Searcher service
                int nTotalItems = 0;
                bool isLucene = false;
                List<SearchResult> lSearchResults = Catalog.GetMediaIdsFromSearcher(oMediaRequest, ref nTotalItems, ref isLucene);

                if (lSearchResults != null)
                {
                    oMediaResponse.m_nTotalItems = nTotalItems;
                    if (isLucene)
                    {
                        List<SearchResult> lMediaRes = Utils.GetMediaUpdateDate(lSearchResults);
                        lMediaRes = Utils.GetMediaForPaging(lMediaRes, request);
                        oMediaResponse.m_nMediaIds = new List<SearchResult>(lMediaRes);
                    }
                    else //ElasticSearch
                    {
                        oMediaResponse.m_nMediaIds = lSearchResults;
                    }
                }

                return (BaseResponse)oMediaResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetMediaRelated", ex);
                throw ex;
            }
        }
    }
}
