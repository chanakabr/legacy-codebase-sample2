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
using System.Threading.Tasks;
using ApiObjects;
using Catalog.Response;
using KLogMonitor;
using KlogMonitorHelper;
using EpgBL;
using ApiObjects.Response;
using ApiObjects.PlayCycle;
using GroupsCacheManager;

namespace Catalog.Request
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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            MediaMarkResponse oMediaMarkResponse = null;

            try
            {
                MediaMarkRequest oMediaMarkRequest = null;

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    oMediaMarkRequest = oBaseRequest as MediaMarkRequest;

                    eAssetTypes assetType = oMediaMarkRequest.m_oMediaPlayRequestData.m_eAssetType;

                    if (assetType == eAssetTypes.MEDIA || assetType == eAssetTypes.EPG)
                    {
                        int t;
                        if (!int.TryParse(m_oMediaPlayRequestData.m_sAssetID, out t))
                        {
                            oMediaMarkResponse.status = new Status((int)eResponseStatus.InvalidAssetId, "Invalid Asset id");
                            return oMediaMarkResponse;
                        }
                    }

                    if (assetType == eAssetTypes.MEDIA) // Media MArk
                    {
                        oMediaMarkResponse = ProcessMediaMarkRequest(oMediaMarkRequest);
                    }
                    else if (assetType == eAssetTypes.EPG || assetType == eAssetTypes.NPVR)// EPG or Npvr Mark
                    {
                        oMediaMarkResponse = ProcessNpvrEpgMarkRequest(oMediaMarkRequest);
                    }
                    else
                    {
                        oMediaMarkResponse = new MediaMarkResponse();
                        oMediaMarkResponse.status = new Status((int)eResponseStatus.InvalidAssetType, "invalid asset type");
                    }
                }
                else
                {
                    oMediaMarkResponse = new MediaMarkResponse();
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                return oMediaMarkResponse;
            }
            catch (Exception ex)
            {
                log.Error(String.Concat("MediaMarkRequest.GetResponse. ", oBaseRequest.ToString()), ex);

                oMediaMarkResponse.status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return oMediaMarkResponse;
            }
        }

        private MediaMarkResponse ProcessNpvrEpgMarkRequest(MediaMarkRequest oMediaMarkRequest)
        {
            MediaMarkResponse oMediaMarkResponse = new MediaMarkResponse();
            eAssetTypes assetType = oMediaMarkRequest.m_oMediaPlayRequestData.m_eAssetType;

            int nActionID = 0;
            int nPlay = 0;
            int nStop = 0;
            int nPause = 0;
            int nFinish = 0;
            int nFull = 0;
            int nExitFull = 0;
            int nSendToFriend = 0;
            int nLoad = 0;
            int nFirstPlay = 0;
            int nPlatform = 0;
            int nSwhoosh = 0;
            int fileDuration = 0;

            MediaPlayActions mediaMarkAction;

            if (assetType == eAssetTypes.EPG)
            {
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(this.m_nGroupID);
                List<EpgCB> lEpgProg = epgBL.GetEpgs(new List<string>() { this.m_oMediaPlayRequestData.m_sAssetID });
                if (lEpgProg != null && lEpgProg.Count > 0)
                    fileDuration = Convert.ToInt32((lEpgProg.First().EndDate - lEpgProg.First().StartDate).TotalSeconds);
                else
                {
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.ProgramDoesntExist, "Program doesn't exist");
                    return oMediaMarkResponse;
                }
            }

            // check action
            if (!Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                oMediaMarkResponse.status = new Status((int)eResponseStatus.ActionNotRecognized, "Action not recognized");
                return oMediaMarkResponse;
            }

            // check anonymous user
            if (Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
            {
                oMediaMarkResponse.status = new Status((int)eResponseStatus.UserNotAllowed, "Anonymous User Can't watch nPVR");
                return oMediaMarkResponse;
            }

            oMediaMarkResponse.status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (this.m_oFilter != null)
            {
                Int32.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }
            int nCountryID = 0;

            bool isError = false;
            bool isConcurrent = false; // for future use
            HandleNpvrEpgPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, ref nExitFull, ref nSendToFriend, ref nLoad,
                                    ref nFirstPlay, ref isConcurrent, ref isError, ref nSwhoosh, ref fileDuration, assetType);

            return oMediaMarkResponse;
        }


        /* no concurrency check for NPVR play , only UpdateFollowMe */
        private void HandleNpvrEpgPlayAction(MediaPlayActions mediaMarkAction, int nCountryID, int nPlatform, ref int nActionID, ref int nPlay, ref int nStop, ref int nPause,
            ref int nFinish, ref int nFull, ref int nExitFull, ref int nSendToFriend, ref int nLoad, ref int nFirstPlay, ref bool isConcurrent,
            ref bool isError, ref int nSwoosh, ref int fileDuration, eAssetTypes assetType)
        {
            int nDomainID = 0;
            ePlayType playType = assetType == eAssetTypes.EPG ? ePlayType.EPG : ePlayType.NPVR;

            switch (mediaMarkAction)
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
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
                        break;
                    }
                case MediaPlayActions.PLAY:
                    {
                        nPlay = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
                        break;
                    }
                case MediaPlayActions.STOP:
                    {
                        nStop = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
                        break;
                    }
                case MediaPlayActions.PAUSE:
                    {
                        nPause = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
                        break;
                    }
                case MediaPlayActions.FINISH:
                    {
                        nFinish = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, 0, this.m_oMediaPlayRequestData.m_sUDID,
                            fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
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
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType, true);
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
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc,
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
                        break;
                    }
            }
        }

        private MediaMarkResponse ProcessMediaMarkRequest(MediaMarkRequest mediaMarkRequest)
        {
            MediaMarkResponse oMediaMarkResponse = new MediaMarkResponse();

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
            PlayCycleSession playCycleSession = null;
            string playCycleKey = string.Empty;
            MediaPlayActions mediaMarkAction;

            Int32.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);

            oMediaMarkResponse.status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (this.m_oFilter != null)
            {
                Int32.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }
            int nCountryID = 0;

            if (!Catalog.GetMediaMarkHitInitialData(m_oMediaPlayRequestData.m_sSiteGuid, m_sUserIP, mediaId, m_oMediaPlayRequestData.m_nMediaFileID,
                ref nCountryID, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nMediaTypeID, ref nBillingTypeID, ref fileDuration))
            {
                throw new Exception(String.Concat("Failed to bring initial data from DB. Req: ", ToString()));
            }

            bool isTerminateRequest = false;

            if (string.IsNullOrEmpty(m_oMediaPlayRequestData.m_sAction))
            {
                oMediaMarkResponse = new MediaMarkResponse()
                {
                    status = new Status((int)eResponseStatus.Error,
                        "m_sAction is null or empty")
                };

                return oMediaMarkResponse;
            }

            if (Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                if (!Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                {
                    bool isError = false;
                    bool isConcurrent = false;
                    playCycleSession = HandleMediaPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, ref nExitFull, ref nSendToFriend, ref nLoad,
                                          ref nFirstPlay, ref isConcurrent, ref isError, ref nSwhoosh, ref fileDuration, ref nMediaTypeID);
                    if (isConcurrent)
                    {
                        isTerminateRequest = true;
                        oMediaMarkResponse.status = new Status((int)eResponseStatus.ConcurrencyLimitation, "Concurrent play limitation");
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
                    nActionID = Catalog.GetMediaActionID(m_oMediaPlayRequestData.m_sAction);
                }

                List<Task> tasks = new List<Task>();
                ContextData contextData = new ContextData();

                if (mediaId != 0)
                {
                    if (nFirstPlay != 0 || nPlay != 0 || nLoad != 0 || nPause != 0 || nStop != 0 || nFull != 0 || nExitFull != 0 || nSendToFriend != 0 || nPlayTime != 0 || nFinish != 0 || nSwhoosh != 0 || nActionID == (int)MediaPlayActions.HIT)
                    {
                        //  Insert into MediaEOH                        
                        if (playCycleSession != null)
                        {
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
                    }
                }

                if (nActionID != -1)
                {
                    if (nActionID != (int)MediaPlayActions.HIT)
                    {                        
                        tasks.Add(Task.Factory.StartNew(() => Catalog.WriteNewWatcherMediaActionLog(nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, mediaId,
                                                                                        m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID, nCDNID, nActionID, nCountryID, nPlayerID, m_oMediaPlayRequestData.m_nLoc,
                                                                                        nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, contextData)));
                    }
                    else if (TvinciCache.GroupsFeatures.GetGroupFeatureStatus(m_nGroupID, GroupFeature.CROWDSOURCE))
                    // log for mediahit for statistics
                    {
                        tasks.Add(Task.Factory.StartNew(() => WriteLiveViews(m_nGroupID, mediaId, nMediaTypeID, nPlayTime, contextData)));
                    }

                    if (IsFirstPlay(nActionID))
                    {                        
                        tasks.Add(Task.Factory.StartNew(() => WriteFirstPlay(mediaId, m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID, nMediaTypeID, nPlayTime,
                                        m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, contextData)));
                    }
                }
                else
                {
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.ActionNotRecognized, "Action not recognized");
                }

                if (tasks != null && tasks.Count > 0)
                {
                    Task.WaitAll(tasks.ToArray());
                }
            }

            return oMediaMarkResponse;
        }

        private void WriteLiveViews(int groupID, int mediaID, int mediaTypeID, int playTime, ContextData context)
        {
            try
            {
                context.Load();
                int parentGroupID = Cache.CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, Catalog.STAT_ACTION_MEDIA_HIT, playTime, false))
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
                ApiDAL.Update_MediaViews(mediaID, mediaFileID);

                int parentGroupID = Cache.CatalogCache.Instance().GetParentGroup(groupID);

                if (!ElasticSearch.Utilities.ESStatisticsUtilities.InsertMediaView(parentGroupID, mediaID, mediaTypeID, Catalog.STAT_ACTION_FIRST_PLAY, playTime, true))
                {
                    log.Error("Error - " + String.Concat("Failed to write firstplay into stats index. Req: ", ToString()));
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

        private PlayCycleSession HandleMediaPlayAction(MediaPlayActions mediaMarkAction, int nCountryID, int nPlatform, ref int nActionID, ref int nPlay, ref int nStop, ref int nPause, ref int nFinish, ref int nFull, ref int nExitFull,
                                           ref int nSendToFriend, ref int nLoad, ref int nFirstPlay, ref bool isConcurrent, ref bool isError, ref int nSwoosh, ref int fileDuration, ref int mediaTypeId)
        {
            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);

            if (mediaId != 0)
            {
                nActionID = (int)mediaMarkAction;
            }

            bool isLinearChannel = false;
            var group = new GroupManager().GetGroup(this.m_nGroupID);

            if (group != null)
            {
                // make sure media types list is initialized
                group.GetMediaTypes();

                if (group.linearChannelMediaTypes != null && group.linearChannelMediaTypes.Contains(mediaTypeId))
                {
                    isLinearChannel = true;
                }
            }

            int nDomainID = 0;
            PlayCycleSession playCycleSession = CatalogDAL.GetUserPlayCycle(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_nGroupID, this.m_oMediaPlayRequestData.m_sUDID, nPlatform);
            if (playCycleSession != null && playCycleSession.DomainID > 0)
            {
                nDomainID = playCycleSession.DomainID;
            }

            switch (mediaMarkAction)
            {
                case MediaPlayActions.HIT:
                {
                    Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration,
                        mediaMarkAction.ToString(), mediaTypeId, nDomainID, ePlayType.MEDIA, false, isLinearChannel);
                    break;
                }
                case MediaPlayActions.ERROR:
                {
                    int nErrorCode = 0;
                    int.TryParse(this.m_sErrorCode, out nErrorCode);
                    int nSiteGuid = 0;
                    int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out nSiteGuid);
                    CatalogDAL.Insert_NewPlayerErrorMessage(this.m_nGroupID, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_nLoc, nPlatform,
                                                            nSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_sErrorCode, this.m_sErrorMessage);

                    isError = true;
                    break;

                }
                case MediaPlayActions.PLAY:
                {
                    nPlay = 1;
                    if (Catalog.IsConcurrent(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_nGroupID, ref nDomainID, mediaId,
                        this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID, playCycleSession))
                    {
                        isConcurrent = true;
                    }
                    else
                    {
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid,
                            this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(),
                            mediaTypeId, nDomainID, ePlayType.MEDIA, false, isLinearChannel);
                    }
                    break;
                }
                case MediaPlayActions.STOP:
                {
                    nStop = 1;
                    Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID,
                        ePlayType.MEDIA, false, isLinearChannel);
                    break;
                }
                case MediaPlayActions.PAUSE:
                {
                    nPause = 1;
                    if (mediaId != 0)
                    {
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, 
                            this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID,
                            ePlayType.MEDIA, false, isLinearChannel);
                    }
                    break;
                }
                case MediaPlayActions.FINISH:
                {
                    nFinish = 1;
                    Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, 0, this.m_oMediaPlayRequestData.m_sUDID,
                        fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID,
                            ePlayType.MEDIA, true, isLinearChannel);
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
                    if (Catalog.IsConcurrent(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_nGroupID, ref nDomainID, mediaId,
                        this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID, playCycleSession))
                    {
                        isConcurrent = true;
                    }
                    else
                    {
                        if (Catalog.IsGroupUseFPNPC(this.m_nGroupID)) //FPNPC -  on First Play create New Play Cycle
                        {
                            playCycleSession = null;
                            // Insert to CB if user is not anonymous                            
                            if (!Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                            {
                                playCycleSession = CatalogDAL.InsertPlayCycleSession(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_nGroupID, this.m_oMediaPlayRequestData.m_sUDID, nPlatform, 0, nDomainID);
                            }
                            // We still insert to DB incase needed by other process                            
                            if (playCycleSession != null && !string.IsNullOrEmpty(playCycleSession.PlayCycleKey))
                            {
                                CatalogDAL.InsertPlayCycleKey(this.m_oMediaPlayRequestData.m_sSiteGuid, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, 0, this.m_nGroupID, playCycleSession.PlayCycleKey);
                            }
                            else
                            {
                                CatalogDAL.GetOrInsert_PlayCycleKey(this.m_oMediaPlayRequestData.m_sSiteGuid, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, 0, this.m_nGroupID, true);
                            }
                        }

                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid,
                            this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID,
                            ePlayType.MEDIA, true, isLinearChannel);
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
                    Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid,
                        this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID,
                        ePlayType.MEDIA, false, isLinearChannel);
                    break;
                }
                }

            return playCycleSession;
        }
    }
}
