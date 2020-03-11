using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class MediaLastPositionLoader : CatalogRequestManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string UDID { get; set; }
        public int MediaID { get; set; }
        public string NPVRID { get; set; }

        public MediaLastPositionLoader(int groupID, string userIP, string siteGuid, string udid, int mediaID, string npvrID)
        {
            SiteGuid = siteGuid;
            UDID = udid;
            MediaID = mediaID;
            NPVRID = npvrID;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaLastPositionRequest()
            {
                data = new MediaLastPositionRequestData()
                {
                    m_nMediaID = MediaID,
                    m_sNpvrID = NPVRID,
                    m_sSiteGuid = SiteGuid,
                    m_sUDID = UDID
                }
            };
        }

        public object Execute()
        {
            MediaMarkObject retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = ResponseToMediaMarkObject();
            }
            return retVal;
        }

        private MediaMarkObject ResponseToMediaMarkObject()
        {
            MediaMarkObject mediaMarkObject = null;
            if (m_oResponse != null)
            {
                MediaLastPositionResponse response = m_oResponse as MediaLastPositionResponse;
                mediaMarkObject = new MediaMarkObject()
                {
                    eStatus = response.m_sStatus != "BAD_REQUEST" || response.m_sStatus != "INVALID_PARAMS" ? MediaMarkObjectStatus.OK : MediaMarkObjectStatus.FAILED,
                    nGroupID = GroupID,
                    nLocationSec = response.Location,
                    nMediaID = MediaID,
                    sDeviceID = UDID,
                    sDeviceName = string.Empty,
                    sSiteGUID = SiteGuid
                };
            }
            return mediaMarkObject;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                if (obj is MediaLastPositionRequest)
                {
                    sText.AppendFormat("MediaLastPositionRequest: groupID = {0}, userIP = {1}, SiteGuid = {2}, UDID = {3}, MediaID = {4}, NPVRID = {5}", GroupID, m_sUserIP, SiteGuid, UDID, MediaID, NPVRID);
                }
                else if (obj is MediaLastPositionResponse)
                {
                    MediaLastPositionResponse assetStatsResponse = obj as MediaLastPositionResponse;
                    sText.AppendFormat("MediaLastPositionResponse");
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
