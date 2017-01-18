using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using KLogMonitor;
using Tvinci.Core.DAL;
using Core.Catalog.Response;

namespace Core.Catalog.Request
{

    [DataContract]
    public class MediaCommentRequest : CommentRequest
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public MediaCommentRequest()
            : base()
        {
        }

        override protected bool PostComment(BaseRequest oBaseRequest)
        {
            try
            {
                MediaCommentRequest request = (MediaCommentRequest)oBaseRequest;

                int nIsActive = request.m_bAutoActive == true ? 1 : 0;
                DateTime? createdDate = null;
                long insertedId = CatalogDAL.InsertMediaComment(request.m_sSiteGuid, request.m_nAssetID, request.m_sWriter,
                    request.m_nGroupID, nIsActive, 0, request.m_sUserIP, request.m_sHeader,
                    request.m_sSubHeader, request.m_sContentText, request.m_oFilter.m_nLanguage, request.m_sUDID, ref createdDate);

                return insertedId > 0;
            }
            catch (Exception ex)
            {
                log.Error("MediaCommentRequest", ex);
                throw ex;
            }
        }
    };
}
