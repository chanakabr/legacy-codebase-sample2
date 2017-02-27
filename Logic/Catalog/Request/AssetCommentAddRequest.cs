using ApiObjects.Response;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.Request
{
    [DataContract]
    public class AssetCommentAddRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public ApiObjects.eAssetType assetType;
        [DataMember]
        public string writer;
        [DataMember]
        public string header;
        [DataMember]
        public string subHeader;
        [DataMember]
        public string contentText;
        [DataMember]
        public string udid;
        [DataMember]
        public Int32 assetId;

        public AssetCommentAddRequest(): base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest baseRequest)
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
                string country = string.Empty;

                if (!string.IsNullOrEmpty(request.m_sUserIP))
                {
                    country = Utils.GetIP2CountryName(request.m_nGroupID, request.m_sUserIP);
                }

                // insert comment
                DataRow dr = ODBCWrapper.Utils.GetTableSingleRowColumnsByParamValue("groups", "group_id", request.m_nGroupID.ToString(), new List<string>() { "ENABLE_COMMENT_AUTOMATICALLY" });
                int nIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "ENABLE_COMMENT_AUTOMATICALLY", 1);
                long id = 0;
                DateTime? createdDate = null;
                switch (request.assetType)
	            {
                    case ApiObjects.eAssetType.MEDIA:
                        id = CatalogDAL.InsertMediaComment(request.m_sSiteGuid, request.assetId, request.writer, request.m_nGroupID, nIsActive, 0, request.m_sUserIP,
                                                      request.header,request.subHeader, request.contentText, request.m_oFilter.m_nLanguage, request.udid, ref createdDate);
                        break;
                    case ApiObjects.eAssetType.PROGRAM:
                        id = CatalogDAL.InsertEpgComment(request.assetId, request.m_oFilter.m_nLanguage, request.writer, request.m_nGroupID, request.m_sUserIP, request.header,
                                                    request.subHeader, request.contentText, request.m_sSiteGuid, request.udid, country, nIsActive, ref createdDate);
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
                    m_nAssetID = request.assetId,                    
                    m_sAssetType = CatalogDAL.Get_MediaTypeIdByMediaId(request.assetId).ToString(),
                    AssetType = request.assetType,
                    m_sWriter = request.writer,
                    m_sHeader = request.header,
                    m_sSubHeader = request.subHeader,
                    m_sContentText = request.contentText,
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
