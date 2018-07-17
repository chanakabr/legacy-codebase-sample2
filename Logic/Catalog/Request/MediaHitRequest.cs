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
using Core.Catalog.Cache;
using GroupsCacheManager;
using System.Threading.Tasks;
using ApiObjects;
using Core.Catalog.Response;
using KLogMonitor;
using ApiObjects.PlayCycle;
using KlogMonitorHelper;
using ApiObjects.Catalog;
using ApiObjects.MediaMarks;
using Core.Users;
using ConfigurationManager;
using ApiObjects.Response;

namespace Core.Catalog.Request
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

        public override BaseResponse GetResponse(BaseRequest baseRequest)
        {
            try
            {
                MediaHitResponse mediaHitResponse = null;
                MediaHitRequest mediaHitRequest = null;

                CheckSignature(baseRequest);

                if (baseRequest != null)
                {
                    mediaHitRequest = baseRequest as MediaHitRequest;
                    this.m_oMediaPlayRequestData = mediaHitRequest.m_oMediaPlayRequestData;

                    if (mediaHitRequest.m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.MEDIA) // Media
                    {
                        mediaHitResponse = ProcessMediaHitRequest();
                    }
                    else if (mediaHitRequest.m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.NPVR) //Npvr
                    {
                        mediaHitResponse = ProcessNpvrHitRequest();
                    }
                }
                else
                {
                    mediaHitResponse = new MediaHitResponse();
                    mediaHitResponse.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    mediaHitResponse.m_sDescription = "Null request";
                }

                return (BaseResponse)mediaHitResponse;
            }
            catch (Exception ex)
            {
                log.Error("MediaHitRequest.GetResponse", ex);
                throw ex;
            }
        }

        private MediaHitResponse ProcessNpvrHitRequest()
        {
            MediaHitResponse response = new MediaHitResponse();
                        
            int locationSec = 30;
            if (m_oMediaPlayRequestData.m_nLoc > 0)
            {
                locationSec = m_oMediaPlayRequestData.m_nLoc;
            }
            
            MediaPlayActions action;
            bool resultParse = Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out action);

            if (m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.MEDIA || m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.EPG)
            {
                int t;
                if (!int.TryParse(m_oMediaPlayRequestData.m_sAssetID, out t))
                {
                    response.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    response.m_sDescription = "Media id not a number";
                    return response;
                }
            }
            
            long recordingId = 0;
            int fileDuration = 0;
            int assetId = 0;

            if (m_oMediaPlayRequestData.m_eAssetType == eAssetTypes.NPVR)
            {
                assetId = int.Parse(this.m_oMediaPlayRequestData.m_sAssetID);
                NPVR.INPVRProvider npvr;

                if (!NPVR.NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(this.m_nGroupID, out npvr, null) &&
                    !CatalogLogic.GetNPVRMarkHitInitialData(assetId, ref recordingId, ref fileDuration, this.m_nGroupID, this.domainId))
                {
                    response.m_sStatus = eResponseStatus.RecordingNotFound.ToString();
                    response.m_sDescription = "Recording doesn't exist";
                    return response;
                }
            }

            //anonymous user - can't play npvr
            if (CatalogLogic.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
            {
                response.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                response.m_sDescription = "Anonymous User Can't watch nPVR";
            }
            else
            {
                if (!resultParse || action != MediaPlayActions.BITRATE_CHANGE)
                {
                    int platform = 0;
                    if (this.m_oFilter != null)
                        int.TryParse(m_oFilter.m_sPlatform, out platform);

                    int countryId = 0;
                    if (!ApplicationConfiguration.CatalogLogicConfiguration.ShouldUseHitCache.Value)
                    {
                        countryId = Utils.GetIP2CountryId(this.m_nGroupID, this.m_sUserIP);
                    }

                    bool isLinearChannel = IsLinearChannel((int)this.m_oMediaPlayRequestData.m_eAssetType);

                    DevicePlayData devicePlayData = 
                        m_oMediaPlayRequestData.GetOrCreateDevicePlayData(assetId, MediaPlayActions.HIT, this.m_nGroupID, isLinearChannel, ePlayType.NPVR, 0, 
                                                                          recordingId, platform, countryId);

                    if (devicePlayData == null)
                    {
                        response.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                        response.m_sDescription = "No devicePlayData";
                        return response;
                    }

                    this.domainId = devicePlayData.DomainId;

                    if (!m_oMediaPlayRequestData.IsReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref devicePlayData))
                    {
                        response.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.CONCURRENT);
                        if (devicePlayData.TimeStamp > DateTime.UtcNow.ToUnixTimestamp() - 65)
                        {
                            devicePlayData.TimeStamp -= 70;
                            CatalogDAL.UpdateOrInsertDevicePlayData(devicePlayData, false, eExpirationTTL.Long);
                        }
                        return response;
                    }

                    CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, locationSec, fileDuration, MediaPlayActions.HIT, eExpirationTTL.Short,
                                                this.m_oMediaPlayRequestData.IsReportingMode, (int)eAssetTypes.NPVR, false, false);
                }

                response.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.HIT);
            }

            return response;
        }

        private MediaHitResponse ProcessMediaHitRequest()
        {
            MediaHitResponse mediaHitResponse = new MediaHitResponse();

            // for Statistics only
            int nCDNID = 0;
            int nOwnerGroupID = 0;
            int nQualityID = 0;
            int nFormatID = 0;
            int nBillingTypeID = 0;
            int countryId = 0;
            
            List<Task> tasks = new List<Task>();
            ContextData contextData = new ContextData();

            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);

            int fileDuration = 0; // from db
            int mediaTypeId = 0;
            if (!CatalogLogic.GetMediaMarkHitInitialData(m_oMediaPlayRequestData.m_sSiteGuid, this.m_sUserIP, mediaId, m_oMediaPlayRequestData.m_nMediaFileID,
                                                         ref countryId, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref mediaTypeId, 
                                                         ref nBillingTypeID, ref fileDuration, this.m_nGroupID))
            {
                throw new Exception(String.Concat("Failed to bring initial data from DB. Req: ", ToString()));
            }

            bool isLinearChannel = this.IsLinearChannel(mediaTypeId);
            
            MediaPlayActions action;
            bool resultParse = Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out action);
            
            int locationSec = 0;
            if (m_oMediaPlayRequestData.m_nLoc > 0)
            {
                locationSec = m_oMediaPlayRequestData.m_nLoc;
            }
            
            //non-anonymous user
            if (!CatalogLogic.IsAnonymousUser(this.m_oMediaPlayRequestData.m_sSiteGuid))
            {
                bool isFirstPlay = false;
                if (!resultParse || action == MediaPlayActions.HIT)
                {
                    isFirstPlay = action == MediaPlayActions.FIRST_PLAY;
                }

                string playCycleKey = string.Empty;

                int platform = 0;
                if (this.m_oFilter != null)
                {
                    int.TryParse(m_oFilter.m_sPlatform, out platform);
                }

                var currDevicePlayData = m_oMediaPlayRequestData.GetOrCreateDevicePlayData(mediaId, action, this.m_nGroupID, isLinearChannel, ePlayType.MEDIA, 
                                                                                           this.domainId, 0, platform, countryId);
                if (currDevicePlayData == null)
                {
                    mediaHitResponse.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    mediaHitResponse.m_sDescription = "No devicePlayData";
                    return mediaHitResponse;
                }

                this.domainId = currDevicePlayData.DomainId;

                if (!m_oMediaPlayRequestData.IsReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref currDevicePlayData))
                {
                    mediaHitResponse.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.CONCURRENT);
                    if (currDevicePlayData.TimeStamp > DateTime.UtcNow.ToUnixTimestamp() - 65)
                    {
                        currDevicePlayData.TimeStamp -= 70;
                        CatalogDAL.UpdateOrInsertDevicePlayData(currDevicePlayData, false, eExpirationTTL.Long);
                    }
                    return mediaHitResponse;
                }

                int nPlay = 0;
                int nFirstPlay = 0;
                int nLoad = 0;
                int nPause = 0;
                int nStop = 0;
                int nFinish = 0;
                int nFull = 0;
                int nExitFull = 0;
                int nSendToFriend = 0;
                DateTime dNow = DateTime.UtcNow;
                int nUpdaterID = 0;
                int nBrowser = 0;
                int nWatcherID = 0;
                string sSessionID = string.Empty;
                int nPlayerID = 0;
                int nSwhoosh = 0;
                int nMediaDuration = 0;
                int.TryParse(m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);
                tasks.Add(Task.Run(() => CatalogLogic.WriteMediaEohStatistics(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, 
                                                                              nBillingTypeID, nCDNID, nMediaDuration, countryId, nPlayerID, nFirstPlay, nPlay, nLoad, nPause, 
                                                                              nStop, nFinish, nFull, nExitFull, nSendToFriend, m_oMediaPlayRequestData.m_nLoc, nQualityID, 
                                                                              nFormatID, dNow, nUpdaterID, nBrowser, platform, m_oMediaPlayRequestData.m_sSiteGuid, 
                                                                              m_oMediaPlayRequestData.m_sUDID, playCycleKey, nSwhoosh, contextData)));
                
                if (!resultParse || action == MediaPlayActions.HIT)
                {
                    CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, action, eExpirationTTL.Short, 
                                                m_oMediaPlayRequestData.IsReportingMode,
                                                mediaTypeId, isFirstPlay, isLinearChannel);
                }

                if (m_oMediaPlayRequestData.m_nAvgBitRate > 0)
                {
                    int siteGuidUnkown = 0;
                    int.TryParse(m_oMediaPlayRequestData.m_sSiteGuid, out siteGuidUnkown);
                }
            }

            //if this is not a bit rate change, log for mediahit for statistics
            if ((!resultParse || action == MediaPlayActions.HIT) && TvinciCache.GroupsFeatures.GetGroupFeatureStatus(m_nGroupID, GroupFeature.CROWDSOURCE))
            {
                tasks.Add(Task.Run(() => WriteLiveViews(this.m_nGroupID, mediaId, mediaTypeId, locationSec, contextData)));
            }

            mediaHitResponse.m_sStatus = CatalogLogic.GetMediaPlayResponse(MediaPlayResponse.HIT);
            if (tasks != null && tasks.Count > 0)
            {
                Task.WaitAll(tasks.ToArray());
            }

            return mediaHitResponse;
        }

        private void WriteLiveViews(int groupID, int mediaID, int mediaTypeID, int playTime, ContextData context)
        {
            try
            {
                context.Load();
                int parentGroupID = CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, CatalogLogic.STAT_ACTION_MEDIA_HIT, playTime, false))
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
