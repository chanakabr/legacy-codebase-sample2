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

namespace Catalog
{
    [DataContract]
    public class MediaMarkRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            try
            {
                MediaMarkResponse oMediaMarkResponse = null;
                MediaMarkRequest oMediaMarkRequest = null;

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    oMediaMarkRequest = (MediaMarkRequest)oBaseRequest;
                    oMediaMarkResponse = ProcessMediaMarkRequest(oMediaMarkRequest);
                }
                else
                {
                    oMediaMarkResponse = new MediaMarkResponse();
                    oMediaMarkResponse.m_sDescription = Catalog.GetMediaPlayResponse(MediaPlayResponse.OK);

                }

                return oMediaMarkResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(String.Concat("MediaMarkRequest.GetResponse. ", oBaseRequest.ToString()), ex);
                throw ex;
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

            MediaPlayActions mediaMarkAction;

            Int32.TryParse(this.m_oMediaPlayRequestData.m_sMediaDuration, out nMediaDuration);


            oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.MEDIA_MARK);

            if (this.m_oFilter != null)
            {
                Int32.TryParse(this.m_oFilter.m_sPlatform, out nPlatform);
            }
            int nCountryID = 0;
            //int nCountryID = Catalog.GetCountryIDByIP(m_sUserIP);
            //Catalog.GetMediaPlayData(m_oMediaPlayRequestData.m_nMediaID, m_oMediaPlayRequestData.m_nMediaFileID,
            //                         ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nBillingTypeID, ref nMediaTypeID
            //);
            if (!Catalog.GetMediaMarkHitInitialData(m_sUserIP, m_oMediaPlayRequestData.m_nMediaID, m_oMediaPlayRequestData.m_nMediaFileID,
                ref nCountryID, ref nOwnerGroupID, ref nCDNID, ref nQualityID, ref nFormatID, ref nMediaTypeID, ref nBillingTypeID))
            {
                throw new Exception(String.Concat("Failed to bring initial data from DB. Req: ", ToString()));
            }

            bool isTerminateRequest = false;
            if (Enum.TryParse(m_oMediaPlayRequestData.m_sAction.ToUpper().Trim(), out mediaMarkAction))
            {
                if (Catalog.IsAnonymousUser(m_oMediaPlayRequestData.m_sSiteGuid))
                {
                    isTerminateRequest = true;
                    if (mediaMarkAction == MediaPlayActions.FIRST_PLAY)
                    {
                        CatalogDAL.Insert_NewPlayCycleKey(m_nGroupID, m_oMediaPlayRequestData.m_nMediaID, m_oMediaPlayRequestData.m_nMediaFileID, m_oMediaPlayRequestData.m_sSiteGuid, nPlatform, m_oMediaPlayRequestData.m_sUDID, nCountryID, Guid.NewGuid().ToString());
                    }
                }
                else
                {
                    bool isError = false;
                    bool isConcurrent = false;
                    HandleMediaPlayAction(mediaMarkAction, nCountryID, nPlatform, ref nActionID, ref nPlay, ref nStop, ref nPause, ref nFinish, ref nFull, ref nExitFull, ref nSendToFriend, ref nLoad,
                                          ref nFirstPlay,/* ref sPlayCycleKey,*/ ref isConcurrent, ref isError, ref nSwhoosh);
                    if (isConcurrent)
                    {
                        isTerminateRequest = true;
                        oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.CONCURRENT);

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

                if (m_oMediaPlayRequestData.m_nMediaID != 0)
                {
                    if (nFirstPlay != 0 || nPlay != 0 || nLoad != 0 || nPause != 0 || nStop != 0 || nFull != 0 || nExitFull != 0 || nSendToFriend != 0 || nPlayTime != 0 || nFinish != 0 || nSwhoosh != 0)
                    {
                        //if (string.IsNullOrEmpty(sPlayCycleKey))
                        //{
                        //    sPlayCycleKey = Catalog.GetLastPlayCycleKey(m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_nMediaID, m_oMediaPlayRequestData.m_nMediaFileID, m_oMediaPlayRequestData.m_sUDID, m_nGroupID, nPlatform, nCountryID);
                        //}
                        //CatalogDAL.Insert_NewMediaEoh(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, m_oMediaPlayRequestData.m_nMediaID, m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID, nMediaDuration, nCountryID, nPlayerID,
                        //                              nFirstPlay, nPlay, nLoad, nPause, nStop, nFull, nExitFull, nSendToFriend, m_oMediaPlayRequestData.m_nLoc, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser, nPlatform,
                        //                              m_oMediaPlayRequestData.m_sSiteGuid, m_oMediaPlayRequestData.m_sUDID, sPlayCycleKey, nSwhoosh);
                        CatalogDAL.Insert_MediaMarkHitActionData(nWatcherID, sSessionID, m_nGroupID, nOwnerGroupID, m_oMediaPlayRequestData.m_nMediaID,
                            m_oMediaPlayRequestData.m_nMediaFileID, nBillingTypeID, nCDNID, nMediaDuration, nCountryID, nPlayerID, nFirstPlay, nPlay, nLoad, nPause,
                            nStop, nFull, nExitFull, nSendToFriend, m_oMediaPlayRequestData.m_nLoc, nQualityID, nFormatID, dNow, nUpdaterID, nBrowser, nPlatform, m_oMediaPlayRequestData.m_sSiteGuid,
                            m_oMediaPlayRequestData.m_sUDID, nSwhoosh, 0);
                    }
                }

                if (nActionID != 0)
                {
                    CatalogDAL.Insert_NewWatcherMediaAction(nWatcherID, sSessionID, nBillingTypeID, nOwnerGroupID, nQualityID, nFormatID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_nGroupID,
                                                            nCDNID, nActionID, nCountryID, nPlayerID, this.m_oMediaPlayRequestData.m_nLoc, nBrowser, nPlatform, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID);

                    if (IsFirstPlay(nActionID)) // update only when first_play
                    {
                            ApiDAL.Update_MediaViews(this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID);
                    }
                }
                else
                {
                    oMediaMarkResponse.m_sStatus = Catalog.GetMediaPlayResponse(MediaPlayResponse.ACTION_NOT_RECOGNIZED);
                }
            }

            return oMediaMarkResponse;
        }

        private bool IsFirstPlay(int actionId)
        {
            return actionId == 4;
        }

        private void HandleMediaPlayAction(MediaPlayActions mediaMarkAction, int nCountryID, int nPlatform, ref int nActionID, ref int nPlay, ref int nStop, ref int nPause, ref int nFinish, ref int nFull, ref int nExitFull,
                                           ref int nSendToFriend, ref int nLoad, ref int nFirstPlay, /*ref string sPlayCycleKey, */ref bool isConcurrent, ref bool isError, ref int nSwoosh)
        {
            if (this.m_oMediaPlayRequestData.m_nMediaID != 0)
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
                        CatalogDAL.Insert_NewPlayerErrorMessage(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID, this.m_oMediaPlayRequestData.m_nLoc, nPlatform,
                                                                nSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_sErrorCode, this.m_sErrorMessage);

                        isError = true;
                        break;

                    }
                case MediaPlayActions.PLAY:
                    {
                        nPlay = 1;
                        if (Catalog.IsConcurrent(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_nGroupID, ref nDomainID, this.m_oMediaPlayRequestData.m_nMediaID,
                            this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID))
                        {
                            isConcurrent = true;
                        }
                        else
                        {
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, nDomainID);
                        }
                        break;
                    }
                case MediaPlayActions.STOP:
                    {
                        nStop = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID);
                        break;
                    }
                case MediaPlayActions.PAUSE:
                    {
                        nPause = 1;
                        if (this.m_oMediaPlayRequestData.m_nMediaID != 0)
                        {
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID);
                        }
                        break;
                    }
                case MediaPlayActions.FINISH:
                    {
                        nFinish = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, 0, this.m_oMediaPlayRequestData.m_sUDID);
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
                        if (Catalog.IsConcurrent(this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_sUDID, this.m_nGroupID, ref nDomainID, this.m_oMediaPlayRequestData.m_nMediaID,
                            this.m_oMediaPlayRequestData.m_nMediaFileID, nPlatform, nCountryID))
                        {
                            isConcurrent = true;
                        }
                        else
                        {   
                            Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID, nDomainID);
                        }
                        break;
                    }
                case MediaPlayActions.BITRATE_CHANGE:
                    {
                        nActionID = 40;
                        int siteGuid = 0;
                        int status = 1;
                        int.TryParse(this.m_oMediaPlayRequestData.m_sSiteGuid, out siteGuid);
                        CatalogDAL.Insert_NewMediaFileVideoQuality(0, siteGuid, string.Empty, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_nMediaFileID,
                                                                   this.m_oMediaPlayRequestData.m_nAvgBitRate, this.m_oMediaPlayRequestData.m_nCurrentBitRate, this.m_oMediaPlayRequestData.m_nTotalBitRate,
                                                                   0, 0, nPlatform, nCountryID, status, this.m_nGroupID);
                        break;
                    }
                case MediaPlayActions.SWOOSH:
                    {
                        nSwoosh = 1;
                        Catalog.UpdateFollowMe(this.m_nGroupID, this.m_oMediaPlayRequestData.m_nMediaID, this.m_oMediaPlayRequestData.m_sSiteGuid, this.m_oMediaPlayRequestData.m_nLoc, this.m_oMediaPlayRequestData.m_sUDID);
                        break;
                    }
            }
        }
    }
}
