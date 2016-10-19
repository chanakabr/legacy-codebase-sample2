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
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using ApiObjects.Statistics;
using Catalog.Cache;
using GroupsCacheManager;
using System.Threading.Tasks;
using ApiObjects;
using Catalog.Response;
using KLogMonitor;
using ApiObjects.PlayCycle;
using KlogMonitorHelper;

namespace Catalog.Request
{

    [DataContract]
    public class MediaHitRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public MediaPlayRequestData m_oMediaPlayRequestData;

        public MediaHitRequest()
            : base()
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
                    oMediaHitRequest = oBaseRequest as MediaHitRequest;
                    
                    if (oMediaHitRequest.m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.MEDIA) // Media
                    {
                        oMediaHitResponse = ProcessMediaHitRequest(oMediaHitRequest);
                    }
                    else if (oMediaHitRequest.m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.NPVR) //Npvr
                    {
                        oMediaHitResponse = ProcessNpvrHitRequest(oMediaHitRequest);
                    }

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
                log.Error("MediaHitRequest.GetResponse", ex);
                throw ex;
            }
        }

        private MediaHitResponse ProcessNpvrHitRequest(MediaHitRequest oMediaHitRequest)
        {
            MediaHitResponse oMediaHitResponse = new MediaHitResponse();

            int nPlayTime = 30;
            int nMediaDuration = 0;
            DateTime dNow = DateTime.UtcNow;
            int fileDuration = 0;

            string sSessionID = string.Empty;

            int nPlatform = 0;

            MediaPlayActions action;

            if (m_oMediaPlayRequestData.m_nLoc > 0)
            {
                nPlayTime = m_oMediaPlayRequestData.m_nLoc;
            }
            int.TryParse(m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);

            if (this.m_oFilter != null)
            {
                int.TryParse(m_oFilter.m_sPlatform, out nPlatform);
            }


            bool resultParse = Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out action);

            int nSiteGuid;
            int.TryParse(m_oMediaPlayRequestData.m_sSiteGuid, out nSiteGuid);

            if (m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.MEDIA || m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.EPG)
            {
                int t;
                if (!int.TryParse(m_oMediaPlayRequestData.m_sAssetID, out t))
                {
                    oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    oMediaHitResponse.m_sDescription = "Media id not a number";
                    return oMediaHitResponse;
                }
            }

            //anonymous user - can't play npvr
            if (Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
            {
                oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                oMediaHitResponse.m_sDescription = "Anonymous User Can't watch nPVR";
            }
            else
            {
                if (!resultParse || action != MediaPlayActions.BITRATE_CHANGE)
                {
                    Catalog.UpdateFollowMe(m_nGroupID, m_oMediaPlayRequestData.m_sAssetID, m_oMediaPlayRequestData.m_sSiteGuid, nPlayTime, m_oMediaPlayRequestData.m_sUDID, fileDuration, 
                        MediaPlayResponse.HIT.ToString(), (int)eAssetTypes.NPVR, 0, ApiObjects.ePlayType.NPVR);
                }

                oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.HIT);
            }
            return oMediaHitResponse;
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
            int nFinish = 0;
            int nFull = 0;
            int nExitFull = 0;
            int nSendToFriend = 0;
            int nPlayTime = 0;
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
            int nCountryID = 0;
            int nSiteGuid;
            int fileDuration = 0;
            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);
            string playCycleKey = string.Empty;
            MediaPlayActions action;
            List<Task> tasks = new List<Task>();
            ContextData contextData = new ContextData();

            if (m_oMediaPlayRequestData.m_nLoc > 0)
                nPlayTime = m_oMediaPlayRequestData.m_nLoc;

            int.TryParse(m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);

            if (this.m_oFilter != null)
                int.TryParse(m_oFilter.m_sPlatform, out nPlatform);

            if (!Catalog.GetMediaMarkHitInitialData(m_oMediaPlayRequestData.m_sSiteGuid, m_sUserIP, mediaId, m_oMediaPlayRequestData.m_nMediaFileID,
                ref nCountryID, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nMediaTypeID, ref nBillingTypeID, ref fileDuration))
            {
                throw new Exception(String.Concat("Failed to bring initial data from DB. Req: ", ToString()));
            }

            bool isLinearChannel = false;
            var group = new GroupManager().GetGroup(this.m_nGroupID);

            if (group != null)
            {
                // make sure media types list is initialized
                group.GetMediaTypes();

                if (group.linearChannelMediaTypes != null && group.linearChannelMediaTypes.Contains(nMediaTypeID))
                {
                    isLinearChannel = true;
                }
            }

            bool resultParse = Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out action);
            int.TryParse(m_oMediaPlayRequestData.m_sSiteGuid, out nSiteGuid);

            //non-anonymous user
            if (nSiteGuid > 0)
            {
                // Get from CB and insert into MediaEOH
                PlayCycleSession playCycleSession = CatalogDAL.GetUserPlayCycle(m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID, m_oMediaPlayRequestData.m_sUDID, nPlatform);
                if (playCycleSession != null)
                {
                    domainId = playCycleSession.DomainID;
                    playCycleKey = playCycleSession.PlayCycleKey;
                }
                else
                {
                    playCycleKey = CatalogDAL.GetOrInsert_PlayCycleKey(m_oMediaPlayRequestData.m_sSiteGuid, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, 0, m_nGroupID, true);
                }
                
                tasks.Add(Task.Factory.StartNew(() => Catalog.WriteMediaEohStatistics(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID,
                                                                            nMediaDuration, nCountryID, nPlayerID, nFirstPlay, nPlay, nLoad, nPause, nStop, nFinish, nFull, nExitFull, nSendToFriend,
                                                                            m_oMediaPlayRequestData.m_nLoc, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid,
                                                                            m_oMediaPlayRequestData.m_sUDID, playCycleKey, nSwhoosh, contextData)));

                if (!resultParse || action != MediaPlayActions.BITRATE_CHANGE)
                {
                    bool isFirstPlay = action == MediaPlayActions.FIRST_PLAY;
                    Catalog.UpdateFollowMe(m_nGroupID, m_oMediaPlayRequestData.m_sAssetID, m_oMediaPlayRequestData.m_sSiteGuid,
                        nPlayTime, m_oMediaPlayRequestData.m_sUDID, fileDuration, action.ToString(), nMediaTypeID, domainId, ePlayType.MEDIA, isFirstPlay, isLinearChannel);
                }

                if (m_oMediaPlayRequestData.m_nAvgBitRate > 0)
                {
                    int siteGuid = 0;
                    int.TryParse(m_oMediaPlayRequestData.m_sSiteGuid, out siteGuid);
                }
            }
            //if this is not a bit rate change, log for mediahit for statistics
            if (!resultParse || action != MediaPlayActions.BITRATE_CHANGE)
            {
                tasks.Add(Task.Factory.StartNew(() => WriteLiveViews(mediaHitRequest.m_nGroupID, mediaId, nMediaTypeID, nPlayTime, contextData)));
            }

            oMediaHitResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.HIT);
            if (tasks != null && tasks.Count > 0)
            {
                Task.WaitAll(tasks.ToArray());
            }

            return oMediaHitResponse;
        }

        private void WriteLiveViews(int groupID, int mediaID, int mediaTypeID, int playTime, ContextData context)
        {
            try
            {
                context.Load();
                int parentGroupID = CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, Catalog.STAT_ACTION_MEDIA_HIT, playTime, false))
                    log.Error("Error - " + String.Concat("Failed to write mediahit into stats index. M ID: ", mediaID, " MT ID: ", mediaTypeID));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in WriteLiveViews, mediaID: {0}, groupID: {1}, mediaTypeID: {2}, playTime: {3}, Exception: {4}", mediaID,
                    groupID, mediaTypeID, playTime, ex);
            }
        }

    }
}
