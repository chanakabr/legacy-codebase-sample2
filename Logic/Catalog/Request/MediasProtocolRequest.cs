using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using Core.Catalog.Response;
using KLogMonitor;


namespace Core.Catalog.Request
{
    [DataContract]
    public class MediasProtocolRequest : BaseRequest, IMediasProtocolRequest
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<Int32> m_lMediasIds;

        public MediasProtocolRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
        }
        public MediasProtocolRequest()
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            return GetMediasByIDs(oBaseRequest as MediasProtocolRequest);
        }

        /*Get Media Details By MediasIds*/
        public MediaResponse GetMediasByIDs(MediasProtocolRequest mediaRequest)
        {
            MediaResponse mediaResponse = new MediaResponse();
            List<MediaObj> lMediaObj = new List<MediaObj>();
            MediaObj oMediaObj = new MediaObj();

            try
            {

                CheckRequestValidness();

                CheckSignature(this);

                CatalogLogic.CompleteDetailsForMediaResponse(this, ref mediaResponse, m_nPageSize * m_nPageIndex, m_nPageSize * m_nPageIndex + m_nPageSize);

                return mediaResponse;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }

        protected override void CheckRequestValidness()
        {
            if (m_lMediasIds == null || m_lMediasIds.Count == 0
                || m_oFilter == null)
            {
                throw new ArgumentException("At least one of the arguments is not valid");
            }
        }
    }
}
