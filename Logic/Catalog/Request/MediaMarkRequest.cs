using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.MediaMarks;
using ApiObjects.PlayCycle;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using ConfigurationManager;
using Core.Catalog.Response;
using Core.Users;
using EpgBL;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.Request
{
    [DataContract]
    public class MediaMarkRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());         

        [DataMember]
        public MediaPlayRequestData m_oMediaPlayRequestData;
        [DataMember]
        public string m_sMediaCDN;
        [DataMember]
        public string m_sErrorCode;
        [DataMember]
        public string m_sErrorMessage;

        public MediaMarkRequest()
            : base()
        {

        }

        public MediaMarkRequest(MediaMarkRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            this.m_oMediaPlayRequestData = m.m_oMediaPlayRequestData;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaMarkResponse response = null;

            try
            {
                MediaMarkRequest oMediaMarkRequest = null;

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    oMediaMarkRequest = oBaseRequest as MediaMarkRequest;
                    this.m_oMediaPlayRequestData = oMediaMarkRequest.m_oMediaPlayRequestData;
                    this.domainId = oMediaMarkRequest.domainId;

                    eAssetTypes assetType = oMediaMarkRequest.m_oMediaPlayRequestData.m_eAssetType;

                    if (assetType == eAssetTypes.MEDIA || assetType == eAssetTypes.EPG)
                    {
                        int t;
                        if (!int.TryParse(m_oMediaPlayRequestData.m_sAssetID, out t))
                        {
                            response = new MediaMarkResponse();
                            response.status.Set((int)eResponseStatus.InvalidAssetId, "Invalid Asset id");
                            return response;
                        }
                    }
                    
                    if (assetType == eAssetTypes.MEDIA) // Media MArk
                    {
                        response = ProcessMediaMarkRequest();
                    }
                    else if (assetType == eAssetTypes.EPG || assetType == eAssetTypes.NPVR)// EPG or Npvr Mark
                    {
                        response = ProcessNpvrEpgMarkRequest();
                    }
                    else
                    {
                        response = new MediaMarkResponse();
                        response.status.Set((int)eResponseStatus.InvalidAssetType, "invalid asset type");
                    }
                }
                else
                {
                    response = new MediaMarkResponse();
                    response.status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                return response;
            }
            catch (Exception ex)
            {
                log.Error(String.Concat("MediaMarkRequest.GetResponse. ", oBaseRequest.ToString()), ex);

                response.status.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
        }

        private MediaMarkResponse ProcessNpvrEpgMarkRequest()
        {
            MediaMarkResponse mediaMarkResponse = new MediaMarkResponse();
            eAssetTypes assetType = this.m_oMediaPlayRequestData.m_eAssetType;
            
            int fileDuration = 0;
            long recordingId = 0;
            long linearChannelMediaId = 0;

            if (assetType == eAssetTypes.EPG)
            {
                #region EPG

                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(this.m_nGroupID);
                List<EpgCB> epgProgramList = epgBL.GetEpgs(new List<string>() { this.m_oMediaPlayRequestData.m_sAssetID });

                if (epgProgramList != null && epgProgramList.Count > 0)
                {
                    var firstEpg = epgProgramList.First();
                    fileDuration = Convert.ToInt32((firstEpg.EndDate - firstEpg.StartDate).TotalSeconds);

                    ApiObjects.TimeShiftedTv.Recording recording = null;
                    EPGChannelProgrammeObject programObject = null;

                    Core.ConditionalAccess.Utils.GetMediaIdForAsset(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, assetType, this.m_sSiteGuid, null, 
                                                                    this.m_oMediaPlayRequestData.m_sUDID, out linearChannelMediaId, out recording, out programObject);
                }
                else
                {
                    mediaMarkResponse.status.Set((int)eResponseStatus.ProgramDoesntExist, "Program doesn't exist");
                    return mediaMarkResponse;
                } 

                #endregion
            }
            else if (assetType == eAssetTypes.NPVR)
            {
                #region NPVR

                NPVR.INPVRProvider npvrProvider;

                // First check if group has old NPVR implementation
                if (NPVR.NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(this.m_nGroupID, out npvrProvider, null))
                {
                    // old, external implementation
                    recordingId = long.Parse(this.m_oMediaPlayRequestData.m_sAssetID);
                }
                else
                {
                    // new, internal implentation
                    if (!CatalogLogic.GetNPVRMarkHitInitialData(long.Parse(this.m_oMediaPlayRequestData.m_sAssetID), ref fileDuration, 
                        ref recordingId, this.m_nGroupID, this.domainId))
                    {
                        mediaMarkResponse.status.Set((int)eResponseStatus.RecordingNotFound, "Recording doesn't exist");
                        return mediaMarkResponse;
                    }
                }
                
                #endregion
            }

            // check action
            MediaPlayActions mediaMarkAction;
            if (!Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                mediaMarkResponse.status.Set((int)eResponseStatus.ActionNotRecognized, "Action not recognized");
                return mediaMarkResponse;
            }

            // check anonymous user
            if (CatalogLogic.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
            {
                mediaMarkResponse.status.Set((int)eResponseStatus.UserNotAllowed, "Anonymous User Can't watch nPVR");
                return mediaMarkResponse;
            }

            mediaMarkResponse.status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            
            bool isError = false;
            bool isConcurrent = false;

            HandleNpvrEpgPlayAction(mediaMarkAction, ref isConcurrent, ref isError, fileDuration, assetType, this.m_oMediaPlayRequestData.ProgramId, 
                                    this.m_oMediaPlayRequestData.IsReportingMode, recordingId, linearChannelMediaId);
            if (isConcurrent)
            {
                mediaMarkResponse.status.Set((int)eResponseStatus.ConcurrencyLimitation, "Concurrent play limitation");
            }
            else if(isError)
            {
                mediaMarkResponse.status.Set((int)eResponseStatus.Error, "Error when handling media mark request");
            }

            return mediaMarkResponse;
        }

        /// <summary>
        /// no concurrency check for NPVR play , only UpdateFollowMe
        /// </summary>
        /// <param name="mediaPlayAction"></param>
        /// <param name="countryId"></param>
        /// <param name="platform"></param>
        /// <param name="nActionID"></param>
        /// <param name="nPlay"></param>
        /// <param name="nStop"></param>
        /// <param name="nPause"></param>
        /// <param name="nFinish"></param>
        /// <param name="nFull"></param>
        /// <param name="nExitFull"></param>
        /// <param name="nSendToFriend"></param>
        /// <param name="nLoad"></param>
        /// <param name="firstPlay"></param>
        /// <param name="isConcurrent"></param>
        /// <param name="isError"></param>
        /// <param name="nSwoosh"></param>
        /// <param name="fileDuration"></param>
        /// <param name="assetType"></param>
        /// <param name="recordingId"></param>
        /// <param name="linearChannelMediaId"></param>
        private void HandleNpvrEpgPlayAction(MediaPlayActions mediaPlayAction, ref bool isConcurrent, ref bool isError, int fileDuration, eAssetTypes assetType, 
                                             long programId, bool isReportingMode, long recordingId = 0, long linearChannelMediaId = 0)
        {
            int assetId = int.Parse(this.m_oMediaPlayRequestData.m_sAssetID);
            int mediaTypeId = (int)assetType;
            ePlayType playType = assetType == eAssetTypes.EPG ? ePlayType.EPG : ePlayType.NPVR;
            eExpirationTTL ttl = this.GetDevicePlayDataTTL(mediaPlayAction);

            int platform = 0;
            if (this.m_oFilter != null)
                int.TryParse(m_oFilter.m_sPlatform, out platform);

            int countryId = 0;
            if (!ApplicationConfiguration.CatalogLogicConfiguration.ShouldUseHitCache.Value)
            {
                countryId = Utils.GetIP2CountryId(this.m_nGroupID, this.m_sUserIP);
            }

            DevicePlayData devicePlayData = 
                this.m_oMediaPlayRequestData.GetOrCreateDevicePlayData(assetId, mediaPlayAction, this.m_nGroupID, (mediaPlayAction == MediaPlayActions.FIRST_PLAY), 
                                                                       playType, this.domainId, recordingId.ToString(), platform, countryId, ttl);
            
            switch (mediaPlayAction)
            {
                case MediaPlayActions.ERROR:
                    {
                        int nErrorCode = 0;
                        int.TryParse(this.m_sErrorCode, out nErrorCode);
                        int nSiteGuid = 0;
                        int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out nSiteGuid);
                        isError = true;
                        break;
                    }
                case MediaPlayActions.HIT:
                    {
                        CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                    ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        break;
                    }
                case MediaPlayActions.PLAY:
                    {
                        if ((playType == ePlayType.NPVR || playType == ePlayType.EPG) && !isReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref devicePlayData))
                        {
                            isConcurrent = true;
                        }
                        
                        if (!isConcurrent)
                        {
                            CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                        ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        }
                        
                        break;
                    }
                case MediaPlayActions.STOP:
                    {
                        CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                    ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        break;
                    }
                case MediaPlayActions.PAUSE:
                    {
                        CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                    ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        break;
                    }
                case MediaPlayActions.FINISH:
                    {
                        CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, 0, fileDuration, mediaPlayAction,
                                                    ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        break;
                    }
                case MediaPlayActions.FIRST_PLAY:
                    {
                        if ((playType == ePlayType.NPVR || playType == ePlayType.EPG) && !isReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref devicePlayData))
                        {
                            isConcurrent = true;
                        }
                        
                        if (!isConcurrent)
                        {
                            CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                        ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, true, false, recordingId);
                        }
                        break;
                    }
                case MediaPlayActions.SWOOSH:
                    {
                        CatalogLogic.UpdateFollowMe(devicePlayData, this.m_nGroupID, this.m_oMediaPlayRequestData.m_nLoc, fileDuration, mediaPlayAction,
                                                    ttl, this.m_oMediaPlayRequestData.IsReportingMode, mediaTypeId, false, false, recordingId);
                        break;
                    }
                default:
                    break;
            }
        }

        private eExpirationTTL GetDevicePlayDataTTL(MediaPlayActions mediaPlayAction)
        {
            eExpirationTTL ttl;

            if (mediaPlayAction == MediaPlayActions.PAUSE)
            {
                ttl = eExpirationTTL.Long;
            }
            else
            {
                ttl = eExpirationTTL.Short;
            }

            return ttl;
        }

        private MediaMarkResponse ProcessMediaMarkRequest()
        {
            MediaMarkResponse response = new MediaMarkResponse();

            int nCDNID = 0;
            int nActionID = -1;
            int nPlay = 0;
            int nStop = 0;
            int nPause = 0;
            int nFinish = 0;
            int nFull = 0;
            int nExitFull = 0;
            int nSendToFriend = 0;
            int nLoad = 0;
            int nFirstPlay = 0;
            string sPlayCycleKey = string.Empty;
            int nMediaDuration = 0;
            DateTime dNow = DateTime.UtcNow;
            int nPlayerID = 0;
            int nWatcherID = 0;
            int nPlayTime = 0;
            string sSessionID = string.Empty;
            int nBrowser = 0;
            int nUpdaterID = 0;
            int nOwnerGroupID = 0;
            int nQualityID = 0;
            int nFormatID = 0;
            int nMediaTypeID = 0;
            int nBillingTypeID = 0;
            int nPlatform = 0;
            int nSwhoosh = 0;
            int fileDuration = 0;
            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);
            DevicePlayData devicePlayData = null;
            string playCycleKey = string.Empty;
            MediaPlayActions mediaMarkAction;

            Int32.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);

            response.status.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (this.m_oFilter != null)
            {
                Int32.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }
            int nCountryID = 0;

            if (!CatalogLogic.GetMediaMarkHitInitialData(m_oMediaPlayRequestData.m_sSiteGuid, m_sUserIP, mediaId, m_oMediaPlayRequestData.m_nMediaFileID,
                ref nCountryID, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nMediaTypeID, ref nBillingTypeID, ref fileDuration, m_nGroupID))
            {
                throw new Exception(String.Concat("Failed to bring initial data from DB. Req: ", ToString()));
            }

            bool isTerminateRequest = false;

            if (string.IsNullOrEmpty(m_oMediaPlayRequestData.m_sAction))
            {
                response.status.Set((int)eResponseStatus.Error, "m_sAction is null or empty");
                return response;
            }

            if (Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                if (!CatalogLogic.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                {
                    bool isError = false;
                    bool isConcurrent = false;
                    devicePlayData = HandleMediaPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, 
                                                             ref nExitFull, ref nSendToFriend, ref nLoad, ref nFirstPlay, ref isConcurrent, ref isError, ref nSwhoosh, 
                                                             ref fileDuration, ref nMediaTypeID, this.m_oMediaPlayRequestData.ProgramId, 
                                                             this.m_oMediaPlayRequestData.IsReportingMode);
                    if (isConcurrent)
                    {
                        isTerminateRequest = true;
                        response.status.Set((int)eResponseStatus.ConcurrencyLimitation, "Concurrent play limitation");
                    }
                    else
                    {
                        if (isError)
                        {
                            isTerminateRequest = true;
                        }
                    }
                }
            }

            if (!isTerminateRequest)
            {
                if (nActionID == -1 && this.m_oMediaPlayRequestData.m_sAction.Length > 0)
                {
                    nActionID = CatalogLogic.GetMediaActionID(m_oMediaPlayRequestData.m_sAction);
                }

                List<Task> tasks = new List<Task>();
                ContextData contextData = new ContextData();

                if (mediaId != 0)
                {
                    if (nFirstPlay != 0 || nPlay != 0 || nLoad != 0 || nPause != 0 || nStop != 0 || nFull != 0 || nExitFull != 0 || nSendToFriend != 0 || nPlayTime != 0 || nFinish != 0 || nSwhoosh != 0 || nActionID == (int)MediaPlayActions.HIT)
                    {
                        //  Insert into MediaEOH                        
                        if (devicePlayData != null)
                        {
                            playCycleKey = devicePlayData.PlayCycleKey;
                        }
                        else
                        {
                            playCycleKey = CatalogDAL.GetOrInsertPlayCycleKey(m_oMediaPlayRequestData.m_sSiteGuid, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, 0, m_nGroupID, true);
                        }

                        tasks.Add(Task.Run(() => CatalogLogic.WriteMediaEohStatistics(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID,
                                                        nMediaDuration, nCountryID, nPlayerID, nFirstPlay, nPlay, nLoad, nPause, nStop, nFinish, nFull, nExitFull, nSendToFriend,
                                                        m_oMediaPlayRequestData.m_nLoc, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid,
                                                        m_oMediaPlayRequestData.m_sUDID, playCycleKey, nSwhoosh, contextData)));
                    }
                }

                if (nActionID != -1)
                {
                    if (nActionID != (int)MediaPlayActions.HIT)
                    {                        
                        tasks.Add(Task.Run(() => CatalogLogic.WriteNewWatcherMediaActionLog(nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, mediaId,
                                                                                        m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID, nCDNID, nActionID, nCountryID, nPlayerID, m_oMediaPlayRequestData.m_nLoc,
                                                                                        nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, contextData)));
                    }
                    else if (TvinciCache.GroupsFeatures.GetGroupFeatureStatus(m_nGroupID, GroupFeature.CROWDSOURCE))
                    // log for mediahit for statistics
                    {
                        tasks.Add(Task.Run(() => WriteLiveViews(m_nGroupID, mediaId, nMediaTypeID, nPlayTime, contextData)));
                    }

                    if (IsFirstPlay(nActionID))
                    {                        
                        tasks.Add(Task.Run(() => WriteFirstPlay(mediaId, m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID, nMediaTypeID, nPlayTime,
                                        m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, contextData)));
                    }
                }
                else
                {
                    response.status.Set((int)eResponseStatus.ActionNotRecognized, "Action not recognized");
                }

                if (tasks != null && tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }

            return response;
        }

        private void WriteLiveViews(int groupID, int mediaID, int mediaTypeID, int playTime, ContextData context)
        {
            try
            {
                context.Load();
                int parentGroupID = Cache.CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, CatalogLogic.STAT_ACTION_MEDIA_HIT, playTime, false))
                    log.Error("Error - " + String.Concat("Failed to write mediahit into stats index. M ID: ", mediaID, " MT ID: ", mediaTypeID));
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in WriteLiveViews, mediaID: {0}, groupID: {1}, mediaTypeID: {2}, playTime: {3}, Exception: {4}", mediaID,
                    groupID, mediaTypeID, playTime, ex);
            }
        }

        private void WriteFirstPlay(int mediaID, int mediaFileID, int groupID, int mediaTypeID, int playTime,
            string siteGuid, string udid, int platform, int countryID, ContextData context)
        {
            try
            {
                context.Load();
                //ApiDAL.Update_MediaViews(mediaID, mediaFileID);  // -https://kaltura.atlassian.net/browse/BEO-4390

                int parentGroupID = Cache.CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, CatalogLogic.STAT_ACTION_FIRST_PLAY, playTime, true))
                {
                    log.Error("Error - " + String.Concat("Failed to write firstplay into stats index. Req: ", ToString()));
                }

                int userId = 0;
                if (int.TryParse(siteGuid, out userId) && userId > 0)
                {
                    string invalidationKey = LayeredCacheKeys.GetUserWatchedMediaIdsInvalidationKey(userId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on GetUserWatchedMediasInvalidationKey key = {0}", invalidationKey);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in WriteFirstPlay, mediaID: {0}, mediaFileID: {1}, groupID: {2}, mediaTypeID: {3}, playTime: {4}, siteGuid: {5}, udid: {6}, platform: {7}, countryID: {8}, Exception: {9}", mediaID,
                    mediaFileID, groupID, mediaTypeID, playTime, siteGuid, udid, platform, countryID, ex);
            }
        }

        private bool IsFirstPlay(int actionId)
        {
            return actionId == 4;
        }

        private DevicePlayData HandleMediaPlayAction(MediaPlayActions mediaPlayAction, int countryId, int platform, ref int nActionID, ref int nPlay, 
                                                       ref int nStop, ref int nPause, ref int nFinish, ref int nFull, ref int nExitFull, ref int nSendToFriend, 
                                                       ref int nLoad, ref int nFirstPlay, ref bool isConcurrent, ref bool isError, ref int nSwoosh, ref int fileDuration, 
                                                       ref int mediaTypeId, long programId, bool isReportingMode)
        {
            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);

            if (mediaId != 0)
            {
                nActionID = (int)mediaPlayAction;
            }

            bool isLinearChannel = this.IsLinearChannel(mediaTypeId);
            
            int locationSec = 0;
            if (m_oMediaPlayRequestData.m_nLoc > 0)
                locationSec = m_oMediaPlayRequestData.m_nLoc;

            eExpirationTTL ttl = this.GetDevicePlayDataTTL(mediaPlayAction);

            var currDevicePlayData = m_oMediaPlayRequestData.GetOrCreateDevicePlayData(mediaId, mediaPlayAction, this.m_nGroupID, isLinearChannel, ePlayType.MEDIA,
                                                                                       this.domainId, string.Empty, platform, countryId, ttl);

            if (currDevicePlayData.DomainId > 0)
            {
                domainId = currDevicePlayData.DomainId;
            }

            this.domainId = currDevicePlayData.DomainId;
            
            switch (mediaPlayAction)
            {
                case MediaPlayActions.HIT:
                    {
                        if (!isReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref currDevicePlayData))
                        {
                            isConcurrent = true;
                        }
                        else
                        {
                            CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                        false, isLinearChannel);
                        }

                        break;
                    }
                case MediaPlayActions.ERROR:
                    {
                        CatalogDAL.Insert_NewPlayerErrorMessage(this.m_nGroupID, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID, locationSec, platform, 
                                                                int.Parse(this.m_oMediaPlayRequestData.m_sSiteGuid), this.m_oMediaPlayRequestData.m_sUDID, this.m_sErrorCode, 
                                                                this.m_sErrorMessage);

                        isError = true;
                        break;
                    }
                case MediaPlayActions.PLAY:
                    {
                        nPlay = 1;
                        if (!isReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref currDevicePlayData))
                        { 
                            isConcurrent = true;
                        }
                        else
                        {
                            CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                        false, isLinearChannel);
                        }
                        break;
                    }
                case MediaPlayActions.STOP:
                    {
                        nStop = 1;
                        CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                    false, isLinearChannel);
                        break;
                    }
                case MediaPlayActions.PAUSE:
                    {
                        nPause = 1;
                        if (mediaId != 0)
                        {
                            CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                        false, isLinearChannel);
                        }
                        break;
                    }
                case MediaPlayActions.FINISH:
                    {
                        nFinish = 1;
                        CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, 0, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                    true, isLinearChannel);
                        break;
                    }
                case MediaPlayActions.FULL_SCREEN:
                    {
                        nFull = 1;
                        break;
                    }
                case MediaPlayActions.FULL_SCREEN_EXIT:
                    {
                        nExitFull = 1;
                        break;
                    }
                case MediaPlayActions.SEND_TO_FRIEND:
                    {
                        nSendToFriend = 1;
                        break;
                    }
                case MediaPlayActions.LOAD:
                    {
                        nLoad = 1;
                        break;
                    }
                case MediaPlayActions.FIRST_PLAY:
                    {
                        nFirstPlay = 1;
                        
                        if (!isReportingMode && CatalogLogic.IsConcurrent(this.m_nGroupID, ref currDevicePlayData))
                        {
                            isConcurrent = true;
                        }
                        else
                        {
                            CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                        true, isLinearChannel);
                        }

                        break;
                    }
                case MediaPlayActions.BITRATE_CHANGE:
                    {
                        nActionID = 40;
                        int siteGuid = 0;
                        int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out siteGuid);
                        break;
                    }
                case MediaPlayActions.SWOOSH:
                    {
                        nSwoosh = 1;
                        CatalogLogic.UpdateFollowMe(currDevicePlayData, this.m_nGroupID, locationSec, fileDuration, mediaPlayAction, ttl, isReportingMode, mediaTypeId,
                                                    false, isLinearChannel);
                        break;
                    }
            }

            return currDevicePlayData;
        }
    }
}
