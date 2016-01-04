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
using EpgBL;
using ApiObjects.Response;

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
                            oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                            oMediaMarkResponse.m_sDescription = "Media id not a number";
                            oMediaMarkResponse.status = new Status((int)eResponseStatus.BadSearchRequest, "Asset id is not a number");
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
                        oMediaMarkResponse.m_sStatus = "Error";
                        oMediaMarkResponse.m_sDescription = "invalid asset type";
                    }
                }
                else
                {
                    oMediaMarkResponse = new MediaMarkResponse();
                    oMediaMarkResponse.m_sDescription = Catalog.GetMediaPlayResponse(MediaPlayResponse.OK);
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                return oMediaMarkResponse;
            }
            catch (Exception ex)
            {
                log.Error(String.Concat("MediaMarkRequest.GetResponse. ", oBaseRequest.ToString()), ex);

                oMediaMarkResponse.m_sStatus = "ERROR"; 
                oMediaMarkResponse.status = new Status((int)eResponseStatus.Error, ex.ToString());
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
            int nMediaDuration = 0;
            DateTime dNow = DateTime.UtcNow;
            string sSessionID = string.Empty;
            int nPlatform = 0;
            int nSwhoosh = 0;
            int fileDuration = 0;

            MediaPlayActions mediaMarkAction;

            if (assetType == eAssetTypes.EPG)
            {
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(this.m_nGroupID);
                List<EpgCB> lEpgProg = epgBL.GetEpgs(new List<string>(){this.m_oMediaPlayRequestData.m_sAssetID});
                if (lEpgProg != null && lEpgProg.Count > 0)
                    fileDuration = Convert.ToInt32((lEpgProg.First().EndDate - lEpgProg.First().StartDate).TotalSeconds);

                if (fileDuration == 0)
                {
                    oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    oMediaMarkResponse.m_sDescription = "Program doesn't exist";
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.BadSearchRequest, "Program doesn't exist");
                    return oMediaMarkResponse;
                }
            }

            Int32.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);

            oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.MEDIA_MARK);
            oMediaMarkResponse.status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (this.m_oFilter != null)
            {
                Int32.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }
            int nCountryID = 0;


            if (Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                if (Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                {
                    oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ERROR);
                    oMediaMarkResponse.m_sDescription = "Anonymous User Can't watch nPVR";
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.UserNotAllowed, "Anonymous User Can't watch nPVR");
                }
                else
                {
                    bool isError = false;
                    bool isConcurrent = false; // for future use
                    HandleNpvrEpgPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, ref nExitFull, ref nSendToFriend, ref nLoad,
                                          ref nFirstPlay, ref isConcurrent, ref isError, ref nSwhoosh, ref fileDuration, assetType);
                }
            }
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
                            this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), (int)assetType, nDomainID, playType);
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

            MediaPlayActions mediaMarkAction;

            Int32.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);


            oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.MEDIA_MARK);
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
            if (Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                if (!Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                {
                    bool isError = false;
                    bool isConcurrent = false;
                    HandleMediaPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, ref nExitFull, ref nSendToFriend, ref nLoad,
                                          ref nFirstPlay, ref isConcurrent, ref isError, ref nSwhoosh, ref fileDuration, ref nMediaTypeID);
                    if (isConcurrent)
                    {
                        isTerminateRequest = true;
                        oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.CONCURRENT);
                        oMediaMarkResponse.status = new Status((int)eResponseStatus.ConcurrencyLimitation, "Concurrent play");
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
                if (nActionID == 0 && this.m_oMediaPlayRequestData.m_sAction.Length > 0)
                {
                    nActionID = Catalog.GetMediaActionID(m_oMediaPlayRequestData.m_sAction);
                }

                if (mediaId != 0)
                {
                    if (nFirstPlay != 0 || nPlay != 0 || nLoad != 0 || nPause != 0 || nStop != 0 || nFull != 0 || nExitFull != 0 || nSendToFriend != 0 || nPlayTime != 0 || nFinish != 0 || nSwhoosh != 0)
                    {
                        CatalogDAL.Insert_MediaMarkHitActionData(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, mediaId,
                            m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID, nMediaDuration, nCountryID, nPlayerID, nFirstPlay, nPlay, nLoad, nPause,
                            nStop, nFull, nExitFull, nSendToFriend, m_oMediaPlayRequestData.m_nLoc, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid,
                            m_oMediaPlayRequestData.m_sUDID, nSwhoosh, 0);
                    }
                }

                if (nActionID != 0)
                {
                    CatalogDAL.Insert_NewWatcherMediaAction(nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, mediaId, m_oMediaPlayRequestData.m_nMediaFileID, m_nGroupID,
                                                            nCDNID, nActionID, nCountryID, nPlayerID, m_oMediaPlayRequestData.m_nLoc, nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID);

                    if (IsFirstPlay(nActionID))
                    {
                        Task.Factory.StartNew(() => WriteFirstPlay(mediaId, m_oMediaPlayRequestData.m_nMediaFileID,
                            m_nGroupID, nMediaTypeID, nPlayTime, m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID));
                    }
                }
                else
                {
                    oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ACTION_NOT_RECOGNIZED);
                    oMediaMarkResponse.status = new Status((int)eResponseStatus.BadSearchRequest, "Action not recognized");
                }
            }

            return oMediaMarkResponse;
        }

        private void WriteFirstPlay(int mediaID, int mediaFileID, int groupID, int mediaTypeID, int playTime,
            string siteGuid, string udid, int platform, int countryID)
        {
            ApiDAL.Update_MediaViews(mediaID, mediaFileID);
            if (!Catalog.InsertStatisticsRequestToES(groupID, mediaID, mediaTypeID, Catalog.STAT_ACTION_FIRST_PLAY, playTime))
            {
                log.Error("Error - " + String.Concat("Failed to write firstplay into stats index. Req: ", ToString()));
            }
        }

        private bool IsFirstPlay(int actionId)
        {
            return actionId == 4;
        }

        private void HandleMediaPlayAction(MediaPlayActions mediaMarkAction, int nCountryID, int nPlatform, ref int nActionID, ref int nPlay, ref int nStop, ref int nPause, ref int nFinish, ref int nFull, ref int nExitFull,
                                           ref int nSendToFriend, ref int nLoad, ref int nFirstPlay, ref bool isConcurrent, ref bool isError, ref int nSwoosh, ref int fileDuration, ref int mediaTypeId)
        {
            int mediaId = int.Parse(m_oMediaPlayRequestData.m_sAssetID);

            if (mediaId != 0)
            {
                nActionID = (int)mediaMarkAction;
            }

            int nDomainID = 0;

            switch (mediaMarkAction)
            {
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
                            this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID))
                        {
                            isConcurrent = true;
                        }
                        else
                        {
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID);
                        }
                        break;
                    }
                case MediaPlayActions.STOP:
                    {
                        nStop = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, domainId);
                        break;
                    }
                case MediaPlayActions.PAUSE:
                    {
                        nPause = 1;
                        if (mediaId != 0)
                        {
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, domainId);
                        }
                        break;
                    }
                case MediaPlayActions.FINISH:
                    {
                        nFinish = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, 0, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, domainId);
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
                            this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID))
                        {
                            isConcurrent = true;
                        }
                        else
                        {
                            if (Catalog.IsGroupUseFPNPC(this.m_nGroupID)) //FPNPC -  on First Play create New Play Cycle
                            {
                                CatalogDAL.GetOrInsert_PlayCycleKey(this.m_oMediaPlayRequestData.m_sSiteGuid, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_sUDID, nPlatform, nCountryID, 0, this.m_nGroupID, true);
                            }
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, nDomainID);
                        }
                        break;
                    }
                case MediaPlayActions.BITRATE_CHANGE:
                    {
                        nActionID = 40;
                        int siteGuid = 0;
                        int status = 1;
                        int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out siteGuid);
                        CatalogDAL.Insert_NewMediaFileVideoQuality(0, siteGuid, string.Empty, mediaId, this.m_oMediaPlayRequestData.m_nMediaFileID,
                                                                   this.m_oMediaPlayRequestData.m_nAvgBitRate, this.m_oMediaPlayRequestData.m_nCurrentBitRate, this.m_oMediaPlayRequestData.m_nTotalBitRate,
                                                                   0, 0, nPlatform, nCountryID, status, this.m_nGroupID);
                        break;
                    }
                case MediaPlayActions.SWOOSH:
                    {
                        nSwoosh = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_sAssetID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, fileDuration, mediaMarkAction.ToString(), mediaTypeId, domainId);
                        break;
                    }
            }
        }
    }
}
