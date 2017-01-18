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
    [DataContract]
    public class MediaFilesRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<int> m_lMediaFileIDs;

        /*
         * ************************* IMPORTANT **************************************
         * 1. Currently if the list of co guids is not null or empty, the catalog will pass to the DB just the first co guid.
         * 2. Co Guids are strings, and it is very expensive to search if the input list of co guids appears in either co_guid column or
         * 3. alt_co_guid column.
         * 4. Due to collation problems in the local DB, I chose to pass just the first co guid to the select query.
         * 5. In order to solve this problem we should either:
         *      a. Index media files on ElasticSearch OR
         *      b. Index the alt_co_guid column in the SQL Server and use a Caching mechanism, since the Get_MediaFilesDetails SP is pretty
         *          heavy.
         * 6. This request was created due to the alternative url task (July 2014) and currently it serves just the ConditionalAccess module
         * 7. If you wish to expose this call through the TVPAPI you must first solve the indexing problem.
         * 8. The idea behind the decision of exposing list of co guids and not just one, is to extend the funcationality later just on
         * 9. the Catalog side and not on any other modules.
         * 
         */ 
        [DataMember]
        public List<string> m_lCoGuids;

        public MediaFilesRequest()
            : base()
        {

        }


        protected override void CheckRequestValidness()
        {
            if ((m_lMediaFileIDs == null || m_lMediaFileIDs.Count == 0) && (m_lCoGuids == null || m_lCoGuids.Count == 0))
                throw new ArgumentException("No Media File IDs or Media Co Guids were provided.");
              
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaFilesResponse res = new MediaFilesResponse();
            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                List<FileMedia> files = CatalogLogic.GetMediaFilesDetails(m_nGroupID, m_lMediaFileIDs, m_lCoGuids != null && m_lCoGuids.Count > 0 ? m_lCoGuids[0] : string.Empty);

                for (int i = 0; i < files.Count; i++)
                {
                    MediaFileObj mfObj = new MediaFileObj();
                    mfObj.m_oFile = files[i];
                    res.m_lObj.Add(mfObj);
                }

                res.m_nTotalItems = res.m_lObj.Count;

            }
            catch (Exception ex)
            {
                log.Error("Exception at GetMediaFilesByIDs", ex);
                throw ex;
            }

            return res;
        }
    }
}
