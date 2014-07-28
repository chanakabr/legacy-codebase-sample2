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
        public string m_sCoGuid;

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

                List<FileMedia> files = Catalog.GetMediaFilesDetails(m_nGroupID, m_lMediaFileIDs, m_sCoGuid);

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
            if (m_lMediaFileIDs == null || (m_lMediaFileIDs.Count == 0 && string.IsNullOrEmpty(m_sCoGuid)) || m_nGroupID < 1)
            {
                throw new ArgumentException("Request is invalid");
            }
              
        }
        /*Get Media Details By MediasIds*/
        //public MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest)
        //{
        //    MediaResponse mediaResponse = new MediaResponse();
        //    List<MediaObj> lMediaObj = new List<MediaObj>();
        //    MediaObj oMediaObj = new MediaObj();

        //    try
        //    {

        //        CheckRequestValidness(mediaRequest);

        //        CheckSignature(mediaRequest);

        //        Catalog.CompleteDetailsForMediaResponse(mediaRequest, ref mediaResponse, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex, mediaRequest.m_nPageSize * mediaRequest.m_nPageIndex + mediaRequest.m_nPageSize);

        //        return mediaResponse;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex.Message, ex);
        //        throw ex;
        //    }
        //}
    }


}
