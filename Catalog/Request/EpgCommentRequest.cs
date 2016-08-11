using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using KLogMonitor;
using Tvinci.Core.DAL;

namespace Catalog.Request
{

    [DataContract]
    public class EpgCommentRequest : CommentRequest
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public EpgCommentRequest()
            : base()
        {
        }

        override protected bool PostComment(BaseRequest oBaseRequest)
        {
            try
            {
                EpgCommentRequest request = (EpgCommentRequest)oBaseRequest;

                int nIsActive = request.m_bAutoActive == true ? 1 : 0;
                DateTime? createdDate = null;
                long insertedId = CatalogDAL.InsertEpgComment(request.m_nAssetID, request.m_oFilter.m_nLanguage, request.m_sWriter,
                    request.m_nGroupID, request.m_sUserIP, request.m_sHeader, request.m_sSubHeader, request.m_sContentText,
                    request.m_sSiteGuid, request.m_sUDID, request.m_sCountry, nIsActive, ref createdDate);

                return insertedId > 0;
            }
            catch (Exception ex)
            {
                log.Error("EpgCommentRequest", ex);
                throw ex;
            }
        }
    }
}
