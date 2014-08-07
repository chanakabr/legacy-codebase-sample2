using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Catalog.Cache;
using Logger;
using Tvinci.Core.DAL;

namespace Catalog
{
    [DataContract]
    abstract public class CommentRequest : BaseRequest, IRequestImp
    {
        protected static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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


        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CommentResponse response = new CommentResponse();

            CommentRequest cr = oBaseRequest as CommentRequest;

            if (cr == null)
                throw new Exception("request object is null or Required variables is null");

            CheckSignature(cr);


            if (string.IsNullOrEmpty(cr.m_sCountry))
            {
                cr.m_sCountry = TVinciShared.WS_Utils.GetIP2CountryCode(cr.m_sUserIP);
            }


            bool bInsert = this.PostComment(oBaseRequest);

            if (bInsert)
            {
                bool b = WriteCommentToES(cr);
            }

            response.eStatusComment = bInsert ? StatusComment.SUCCESS : StatusComment.FAIL;

            return (BaseResponse)response;
        }

        private bool WriteCommentToES(CommentRequest oCommentReq)
        {
            bool bResult = false;

            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(oCommentReq.m_nGroupID);

            if (group != null)
            {
                Comments comment = new Comments()
                {
                    m_dCreateDate = DateTime.UtcNow,
                    m_nAssetID = oCommentReq.m_nAssetID,
                    m_sContentText = oCommentReq.m_sContentText,
                    m_sHeader = oCommentReq.m_sHeader,
                    m_sSubHeader = oCommentReq.m_sSubHeader,
                    m_sSiteGuid = oCommentReq.m_sSiteGuid,
                    m_sWriter = oCommentReq.m_sWriter,
                    m_nLang = oCommentReq.m_oFilter.m_nLanguage,
                    m_sAssetType = CatalogDAL.Get_MediaTypeIdByMediaId(oCommentReq.m_nAssetID).ToString(),
                    m_Action = "comment",
                    m_nGroupID = group.m_nParentGroupID,
                };

                string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(comment);
                ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
                Guid guid = Guid.NewGuid();

                bResult = esApi.InsertRecord(ElasticSearch.Common.Utils.GetGroupStatisticsIndex(group.m_nParentGroupID), ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), sJson);
            }

            return bResult;
        }

        abstract protected bool PostComment(BaseRequest oBaseRequest);
    }
}
