using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    /// <summary>
    /// Gets information of several types of assets in one request
    /// </summary>
    [DataContract]
    public class AssetInfoRequest : BaseRequest, IRequestImp
    {

        #region Data Members

        /// <summary>
        /// List of media Ids to get their info
        /// </summary>
        [DataMember]
        public List<long> mediaIds;

        /// <summary>
        /// List of Epg Ids to get their info
        /// </summary>
        [DataMember]
        public List<long> epgIds;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the request with the list of request assets
        /// </summary>
        /// <param name="nPageSize"></param>
        /// <param name="nPageIndex"></param>
        /// <param name="sUserIP"></param>
        /// <param name="nGroupID"></param>
        /// <param name="oFilter"></param>
        /// <param name="sSignature"></param>
        /// <param name="sSignString"></param>
        /// <param name="sSiteGuid"></param>
        /// <param name="mediaIds"></param>
        /// <param name="epgIds"></param>
        public AssetInfoRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, Filter oFilter, string sSignature, 
            string sSignString, string sSiteGuid,
            List<long> mediaIds, List<long> epgIds)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString, sSiteGuid)
        {
            this.mediaIds = mediaIds;
            this.epgIds = epgIds;
        }

        
        #endregion

        #region Public Methods

        /// <summary>
        /// Main method: gets the information of the assets
        /// </summary>
        /// <param name="oBaseRequest"></param>
        /// <returns></returns>
        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            AssetInfoResponse response = new AssetInfoResponse();

            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                //List<EPGChannelProgrammeObject> epgs = Catalog.GetEpgsByGroupAndIDs(parentGroupID, epgIds);

                response.epgList = Catalog.GetEPGProgramInformation(epgIds, this.m_nGroupID);

                //response = Catalog.GetEPGProgramsFromCB(sro.m_resultIDs.Select(item => item.assetID).ToList<int>(), m_nGroupID, true, m_nChannelIDs);

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Failed at GetResponse of AssetInfoRequest. ex = {0}, ST = {1}", ex.Message, ex.StackTrace), "AssetInfoRequest");
                throw ex;
            }

            return response;
        }

        #endregion
    }
}
