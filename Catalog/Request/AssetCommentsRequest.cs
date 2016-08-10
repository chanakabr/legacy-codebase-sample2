using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Catalog.Cache;
using Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Catalog.Request
{
    public class AssetCommentsRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 assetId;
        [DataMember]
        public OrderObj orderObj;
        [DataMember]
        public ApiObjects.eAssetType assetType;


        public AssetCommentsRequest()
            : base()
        {
        }
        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
             AssetCommentsListResponse response = new AssetCommentsListResponse();
            try
            {
                AssetCommentsRequest request = (AssetCommentsRequest)oBaseRequest;               
                response.status = new ApiObjects.Response.Status();
              
                int pageIndex = request.m_nPageIndex;
                int pageSize = request.m_nPageSize;

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                if (request.assetType == ApiObjects.eAssetType.PROGRAM)
                {
                    EpgCommentsListRequest epgRequest = new EpgCommentsListRequest()
                    {
                        domainId = request.domainId,
                        m_dServerTime = request.m_dServerTime,
                        m_nEpgProgramID = request.assetId,
                        m_nGroupID = request.m_nGroupID,
                        m_nPageIndex = request.m_nPageIndex,
                        m_nPageSize = request.m_nPageSize,
                        m_oFilter = request.m_oFilter,
                        m_sSignature = request.m_sSignature,
                        m_sSignString = request.m_sSignString,
                        m_sSiteGuid = request.m_sSiteGuid,
                        m_sUserIP = request.m_sUserIP
                    };

                    CommentsListResponse commentResponse = (CommentsListResponse)epgRequest.GetResponse(epgRequest);
                    response.Comments = commentResponse != null ?  commentResponse.m_lComments : null;
                    response.m_nTotalItems = commentResponse.m_nTotalItems;
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                if (request.assetType == ApiObjects.eAssetType.MEDIA)
                {
                    CommentsListRequest mediaRequest = new CommentsListRequest()
                    {
                        domainId = request.domainId,
                        m_dServerTime = request.m_dServerTime,
                        m_nMediaID = request.assetId,
                        m_nGroupID = request.m_nGroupID,
                        m_nPageIndex = request.m_nPageIndex,
                        m_nPageSize = request.m_nPageSize,
                        m_oFilter = request.m_oFilter,
                        m_sSignature = request.m_sSignature,
                        m_sSignString = request.m_sSignString,
                        m_sSiteGuid = request.m_sSiteGuid,
                        m_sUserIP = request.m_sUserIP
                    };
                    CommentsListResponse commentResponse = (CommentsListResponse)mediaRequest.GetResponse(mediaRequest);
                    response.Comments = commentResponse != null ? commentResponse.m_lComments : null;
                    response.m_nTotalItems = commentResponse.m_nTotalItems;
                    response.status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                    
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                 response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed get asset comments");
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
