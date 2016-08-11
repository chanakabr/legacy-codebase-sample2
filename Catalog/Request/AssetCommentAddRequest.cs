using ApiObjects.Response;
using Catalog.Cache;
using Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Catalog.Request
{
    public class AssetCommentAddRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public ApiObjects.eAssetType assetType;
        [DataMember]
        public string m_sWriter;
        [DataMember]
        public string m_sHeader;
        [DataMember]
        public string m_sSubHeader;
        [DataMember]
        public string m_sContentText;
        [DataMember]
        public string m_sUDID;
        [DataMember]
        public string m_sCountry;
        [DataMember]
        public bool m_bAutoActive;
        [DataMember]
        public Int32 m_nAssetID;

        public AssetCommentAddRequest(): base()
        {
        }

        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            AssetCommentResponse response = new AssetCommentResponse();

            try
            {
                AssetCommentAddRequest request = (AssetCommentAddRequest)baseRequest;

                if (request == null)
                {
                    response.Status.Message = "request object is null";
                    return (BaseResponse)response;
                }

                CheckSignature(request);


                if (string.IsNullOrEmpty(request.m_sCountry))
                {
                    request.m_sCountry = TVinciShared.WS_Utils.GetIP2CountryCode(request.m_sUserIP);
                }

                // insert comment
                int nIsActive = request.m_bAutoActive == true ? 1 : 0;
                long id = 0;
                DateTime? createdDate = null;
                switch (request.assetType)
	            {
                    case ApiObjects.eAssetType.MEDIA:
                        id = CatalogDAL.InsertMediaComment(request.m_sSiteGuid, request.m_nAssetID, request.m_sWriter, request.m_nGroupID, nIsActive, 0, request.m_sUserIP,
                                                      request.m_sHeader,request.m_sSubHeader, request.m_sContentText, request.m_oFilter.m_nLanguage, request.m_sUDID, ref createdDate);
                        break;
                    case ApiObjects.eAssetType.PROGRAM:
                        id = CatalogDAL.InsertEpgComment(request.m_nAssetID, request.m_oFilter.m_nLanguage, request.m_sWriter,request.m_nGroupID, request.m_sUserIP, request.m_sHeader,
                                                    request.m_sSubHeader, request.m_sContentText, request.m_sSiteGuid, request.m_sUDID, request.m_sCountry, nIsActive, ref createdDate);
                        break;
                    default:
                        break;
                }
                
                if (id == 0 || !createdDate.HasValue)
                {
                    response.Status.Message = "No ID or created_date returned from the insert stored procedure";
                    return response;
                }

                response.AssetComment = new Comments()
                {
                    Id = (int)id,
                    m_nAssetID = request.m_nAssetID,
                    m_sAssetType = request.assetType.ToString(),
                    m_sWriter = request.m_sWriter,
                    m_sHeader = request.m_sHeader,
                    m_sSubHeader = request.m_sSubHeader,
                    m_sContentText = request.m_sContentText,
                    m_dCreateDate = createdDate.Value,
                    m_sSiteGuid = request.m_sSiteGuid,
                    m_nLang = request.m_oFilter.m_nLanguage,
                    m_Action = "comment"                    
                };                

                if (!WriteCommentToES(response.AssetComment))
                {
                    log.DebugFormat("Failed WriteCommentToES for comment with ID: {0}, assetType: {1}", id, request.assetType.ToString());
                    response.Status = new Status((int)eResponseStatus.Error, "Failed adding comment to ES");
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                return response;


            }
            catch (Exception ex)
            {
                log.Error("Failed AssetCommentAddRequest", ex);                
                throw ex;
            }

        }

        private bool WriteCommentToES(Comments comment)
        {
            bool bResult = false;

            try
            {
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(m_nGroupID);
                comment.m_nGroupID = nParentGroupID;

                string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(comment);
                ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
                Guid guid = Guid.NewGuid();

                bResult = esApi.InsertRecord(ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nParentGroupID), ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), sJson);
            }

            catch (Exception ex)
            {
                log.Error("Failed WriteCommentToES on AssetCommentAddRequest", ex);
                bResult = false;
            }

            return bResult;
        }

    }
}
