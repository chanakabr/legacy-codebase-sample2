using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class MediaHitLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int AvgBitRate { get; set; }
        public int CurrentBitRate { get; set; }
        public int Location { get; set; }
        public int MediaFileID { get; set; }
        public int MediaID { get; set; }
        public int TotalBitRate { get; set; }
        public string Action { get; set; }
        public string MediaDuration { get; set; }
        public string SiteGuid { get; set; }
        public string UDID { get; set; }
        public string NPVRID { get; set; }

        #region Constructors
        public MediaHitLoader(int groupID, string userIP, string siteGuid, string udid, int mediaID, int mediaFileID, string npvrID, int avgBitRate, int currentBitRate, int location, int totalBitRate, string action, string mediaDuration)
            : base(groupID, userIP, 0, 0)
        {
            AvgBitRate = avgBitRate;
            CurrentBitRate = currentBitRate;
            Location = location;
            MediaFileID = mediaFileID;
            MediaID = mediaID;
            NPVRID = npvrID;
            TotalBitRate = totalBitRate;
            Action = action;
            MediaDuration = MediaDuration;
            SiteGuid = siteGuid;
            UDID = udid;
        }

        public MediaHitLoader(string userName, string userIP, string siteGuid, string udid, int mediaID, int mediaFileID, string npvrID, int avgBitRate, int currentBitRate, int location, int totalBitRate, string action, string mediaDuration)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, siteGuid, udid, mediaID, mediaFileID, npvrID, avgBitRate, currentBitRate, location, totalBitRate, action, mediaDuration)
        {
        }

        public MediaHitLoader(int groupID, string userIP, string siteGuid, string udid, int mediaID, int mediaFileID, string npvrID, int avgBitRate, int currentBitRate, int location, int totalBitRate, string action, string mediaDuration, Provider provider)
            : this(groupID, userIP, siteGuid, udid, mediaID, mediaFileID, npvrID, avgBitRate, currentBitRate, location, totalBitRate, action, mediaDuration)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaHitRequest()
            {
                m_oMediaPlayRequestData = new MediaPlayRequestData()
                {
                    m_nAvgBitRate = AvgBitRate,
                    m_nCurrentBitRate = CurrentBitRate,
                    m_nLoc = Location,
                    m_nMediaFileID = MediaFileID,
                    m_sAssetID = string.IsNullOrEmpty(NPVRID) ? MediaID.ToString() : NPVRID,
                    m_nTotalBitRate = TotalBitRate,
                    m_sAction = Action,
                    m_sMediaDuration = MediaDuration,
                    m_sSiteGuid = SiteGuid,
                    m_sUDID = UDID,
                    m_eAssetType = string.IsNullOrEmpty(NPVRID) ? eAssetTypes.MEDIA : eAssetTypes.NPVR,
                },
                m_sSiteGuid = SiteGuid
            };
        }

        public object Execute()
        {
            MediaHitResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as MediaHitResponse;
            }
            return retVal != null ? retVal.m_sStatus : null;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaHitRequest":
                        sText.AppendFormat("MediaHitRequest: groupID = {0}, userIP = {1}, siteGuid = {2}, udid = {3}, mediaID = {4}, mediaFileID = {5}, avgBitRate = {6}, currentBitRate = {7}, location = {8}, totalBitRate = {9}, action = {10}, mediaDuration = {11}",
                            GroupID, m_sUserIP, SiteGuid, UDID, MediaID, MediaFileID, AvgBitRate, CurrentBitRate, Location, TotalBitRate, Action, MediaDuration);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaHitResponse":
                        MediaHitResponse mediaHitResponse = obj as MediaHitResponse;
                        sText.AppendFormat("MediaHitResponse: Status = {0}, ", mediaHitResponse.m_sStatus);
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ILoaderAdapter not implemented methods
        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}
