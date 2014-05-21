using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using Logger;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using ApiObjects.Statistics;

namespace Catalog
{
   
    [DataContract]
    public class MediaHitRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        

        [DataMember]
        public MediaPlayRequestData m_oMediaPlayRequestData;

        public MediaHitRequest() : base()                      
        {
          
        }


        public MediaHitRequest(MediaHitRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            this.m_oMediaPlayRequestData = m.m_oMediaPlayRequestData; 
        }


        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                MediaHitResponse oMediaHitResponse = null;
                MediaHitRequest oMediaHitRequest = null;

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    oMediaHitRequest = (MediaHitRequest)oBaseRequest;
                    oMediaHitResponse = ProcessMediaHitRequest(oMediaHitRequest);
                }
                else
                {
                    oMediaHitResponse = new MediaHitResponse();
                    oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR); 
                    oMediaHitResponse.m_sDescription = "Null request";
                }

                return (BaseResponse)oMediaHitResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("MediaHitRequest.GetResponse", ex);
                throw ex;
            }
        }




        private MediaHitResponse ProcessMediaHitRequest(MediaHitRequest mediaHitRequest)
        {
            MediaHitResponse oMediaHitResponse = new MediaHitResponse();


            int nCDNID = 0;       
            int nPlay = 0;
            int nFirstPlay = 0;
            int nLoad = 0;
            int nPause = 0;
            int nStop = 0;
            int nFull = 0;
            int nExitFull = 0;        
            int nSendToFriend = 0;
            int nPlayTime = 30;
            int nMediaDuration = 0;
            DateTime dNow = DateTime.UtcNow;           
            int nUpdaterID = 0;
            int nOwnerGroupID = 0;
            int nQualityID = 0;
            int nFormatID = 0;
            int nMediaTypeID = 0;
            int nBillingTypeID = 0;        
            int nBrowser = 0;           
            int nWatcherID = 0;           
            string sSessionID = string.Empty; 
            int nPlayerID = 0;
            int nPlatform = 0;
            int nSwhoosh = 0;

            MediaPlayActions action;

            if (this.m_oMediaPlayRequestData.m_nLoc > 0)
            {
                nPlayTime = this.m_oMediaPlayRequestData.m_nLoc;
            }
            int.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);
            if (this.m_oFilter != null)
            {
                int.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }

                        int nCountryID = Catalog.GetCountryIDByIP(this.m_sUserIP);
            
            Catalog.GetMediaPlayData(this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nBillingTypeID, ref nMediaTypeID);

            Group oGroup = GroupsCache.Instance.GetGroup(mediaHitRequest.m_nGroupID);
            bool resultParse = Enum.TryParse(this.m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out action);

            //we only record channel/series views on hits that are of type play/first_play actions
            if (oGroup != null && resultParse && (action == MediaPlayActions.PLAY || action == MediaPlayActions.FIRST_PLAY))
            {
                MediaView view = new MediaView() { GroupID = oGroup.m_nParentGroupID, MediaID = mediaHitRequest.m_oMediaPlayRequestData.m_nMediaID, Location = nPlayTime, MediaType = mediaHitRequest.m_oMediaPlayRequestData.m_sMediaTypeId, Action = mediaHitRequest.m_oMediaPlayRequestData.m_sAction, Date = DateTime.UtcNow };
                WriteLiveViewsToES(view);                       
            }

            string sPlayCycleKey = Catalog.GetLastPlayCycleKey(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_sUDID, this.m_nGroupID, nPlatform, nCountryID);                      

            CatalogDAL.Insert_NewMediaEoh(nWatcherID, sSessionID, this.m_nGroupID, nOwnerGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID,
                                          nMediaDuration, nCountryID, nPlayerID, nFirstPlay, nPlay, nLoad, nPause, nStop, nFull, nExitFull, nSendToFriend, nPlayTime, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser,
                                          nPlatform, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, sPlayCycleKey, nSwhoosh);


            

            if (!resultParse || (resultParse == true && action != MediaPlayActions.BITRATE_CHANGE))
            {
                Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, nPlayTime, this.m_oMediaPlayRequestData.m_sUDID);
            }

            if (this.m_oMediaPlayRequestData.m_nAvgBitRate > 0)
            {
                int siteGuid = 0;
                int status = 1;
                int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out siteGuid);
                CatalogDAL.Insert_NewMediaFileVideoQuality(nWatcherID, siteGuid, sSessionID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, 
                                                           this.m_oMediaPlayRequestData.m_nAvgBitRate, this.m_oMediaPlayRequestData.m_nCurrentBitRate, this.m_oMediaPlayRequestData.m_nTotalBitRate,
                                                           nPlayTime, nBrowser, nPlatform, nCountryID, status, this.m_nGroupID);
            }
            oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.HIT); 
            return oMediaHitResponse;         
        }

        private bool WriteLiveViewsToES(MediaView oMediaView )
        {
            bool bRes = false;
            ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi();

            string sJsonView = Newtonsoft.Json.JsonConvert.SerializeObject(oMediaView);
            string index = Utils.GetStatisticsIndexAlias(oMediaView.GroupID);

            if (oESApi.IndexExists(index) && !string.IsNullOrEmpty(sJsonView))
            {
                Guid guid = Guid.NewGuid();

                bRes = oESApi.InsertRecord(index, Utils.ES_STATISTICS_TYPE, guid.ToString(), sJsonView);

                if (!bRes)
                    _logger.Error(string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}", index, Utils.ES_STATISTICS_TYPE, sJsonView));
            }

            return bRes;
        }
    }
}
