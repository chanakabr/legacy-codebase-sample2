using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class MediaFilesRequest : BaseRequest
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public List<int> m_lMediaFileIDs;
        [DataMember]
        public List<string> m_lCoGuids;

        public MediaFilesRequest()
            : base()
        {

        }

        public MediaFilesResponse GetMediaFilesByIDs()
        {
            MediaFilesResponse res = new MediaFilesResponse();
            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                List<FileMedia> files = Catalog.GetMediaFilesDetails(m_nGroupID, m_lMediaFileIDs, m_lCoGuids != null && m_lCoGuids.Count > 0 ? m_lCoGuids[0] : string.Empty);

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
                _logger.Error("Exception at GetMediaFilesByIDs", ex);
                throw ex;
            }

            return res;
        }

        private void CheckRequestValidness()
        {
            if ((m_lMediaFileIDs == null || m_lMediaFileIDs.Count == 0) && (m_lCoGuids == null || m_lCoGuids.Count == 0))
                throw new ArgumentException("No Media File IDs or Media Co Guids were provided.");
              
        }
    }


}
