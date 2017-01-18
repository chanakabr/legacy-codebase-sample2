using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using Tvinci.Core.DAL;
using ApiObjects.Catalog;

namespace Core.Catalog.Request
{
    [DataContract]
    abstract public class CommentRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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


        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            CommentResponse response = new CommentResponse();

            CommentRequest cr = oBaseRequest as CommentRequest;

            if (cr == null)
                throw new ArgumentException("request object is null or Required variables is null");

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

            return response;
        }

        private bool WriteCommentToES(CommentRequest oCommentReq)
        {
            bool bResult = false;

            CatalogCache catalogCache = CatalogCache.Instance();
            int nParentGroupID = catalogCache.GetParentGroup(oCommentReq.m_nGroupID);

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
                m_nGroupID = nParentGroupID,
            };

            string sJson = Newtonsoft.Json.JsonConvert.SerializeObject(comment);
            ElasticSearch.Common.ElasticSearchApi esApi = new ElasticSearch.Common.ElasticSearchApi();
            Guid guid = Guid.NewGuid();

            bResult = esApi.InsertRecord(ElasticSearch.Common.Utils.GetGroupStatisticsIndex(nParentGroupID), ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), sJson);

            return bResult;
        }

        abstract protected bool PostComment(BaseRequest oBaseRequest);
    }
}
