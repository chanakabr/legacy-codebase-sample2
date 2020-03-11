using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class AssetsBookmarksLoader : CatalogRequestManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<AssetBookmarkRequest> AssetsToRequest { get; set; }        

        public AssetsBookmarksLoader(int groupID, string userIP, string siteGuid, string udid, List<AssetBookmarkRequest> assets)
            : base(groupID, userIP, 0 , 0)
        {
            SiteGuid = siteGuid;
            DeviceId = udid;
            AssetsToRequest = assets;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new AssetsBookmarksRequest
            {
                Data = new AssetsBookmarksRequestData()
                {
                    Assets = AssetsToRequest
                }                
            };
        }

        public object Execute()
        {
            AssetsBookmarksResponse response = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                response = m_oResponse as AssetsBookmarksResponse;
            }
            return response;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                if (obj is AssetsBookmarksRequest)
                {
                    sText.AppendFormat("AssetsBookmarksRequest: groupID = {0}, userIP = {1}, SiteGuid = {2}, UDID = {3}", GroupID, m_sUserIP, SiteGuid, DeviceId);
                }
                else if (obj is AssetsBookmarksResponse)
                {
                    AssetsBookmarksResponse assetStatsResponse = obj as AssetsBookmarksResponse;
                    sText.AppendFormat("AssetsBookmarksResponse");
                }
            }
            logger.Debug(sText.ToString());
        }

    }
}