using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Core.Catalog.Request
{
    /// <summary>
    /// Gets information of several types of assets in one request
    /// </summary>
    [DataContract]
    public class AssetInfoRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        /// <summary>
        /// Management data
        /// </summary>
        [DataMember]
        public bool ManagementData;

        [DataMember]
        public bool AllowPartialResponse;

        #endregion

        #region Ctor

        public AssetInfoRequest()
            : base()
        {
        }

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
            string sSignString, string sSiteGuid, int domainId,
            List<long> mediaIds, List<long> epgIds)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString, sSiteGuid, domainId)
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
        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            AssetInfoResponse response = new AssetInfoResponse();

            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                List<BaseObject> assetsToRetrieve = new List<BaseObject>();
                if (epgIds != null && epgIds.Count > 0)
                {
                    foreach (long epgId in epgIds)
                    {
                        assetsToRetrieve.Add(new BaseObject() { AssetId = epgId.ToString(), AssetType = ApiObjects.eAssetTypes.EPG });
                    }
                }

                if (mediaIds != null && mediaIds.Count > 0)
                {
                    foreach (long mediaId in mediaIds)
                    {
                        assetsToRetrieve.Add(new BaseObject() { AssetId = mediaId.ToString(), AssetType = ApiObjects.eAssetTypes.MEDIA });
                    }
                }

                if (assetsToRetrieve.Count > 0)
                {
                    List<BaseObject> assets = Core.Catalog.Utils.GetOrderedAssets(m_nGroupID, assetsToRetrieve, this.m_oFilter, this.ManagementData);
                    if (assets.Count > 0)
                    {
                        foreach (BaseObject asset in assets)
                        {
                            if (asset.AssetType == ApiObjects.eAssetTypes.MEDIA)
                            {
                                response.mediaList.Add(asset as MediaObj);
                            }
                            else if (asset.AssetType == ApiObjects.eAssetTypes.EPG)
                            {
                                response.epgList.Add(asset as ProgramObj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Failed at GetResponse of AssetInfoRequest. ex = {0}, ST = {1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return response;
        }

        #endregion
    }
}
