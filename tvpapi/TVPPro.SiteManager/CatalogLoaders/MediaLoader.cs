using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.Request;
using Core.Catalog;
using Core.Catalog.Response;
using ApiObjects.SearchObjects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class MediaLoader : MultiMediaLoader
    {
        public List<int> MediaIDs { get; set; }

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private MediaCache m_oMediaCache;

        #region Constructors
        public MediaLoader(int mediaID, int groupID, string userIP, string picSize) :
            this(new List<int>() { mediaID }, groupID, userIP, picSize)
        {
        }
        public MediaLoader(int mediaID, string userName, string userIP, string picSize) :
            this(mediaID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, picSize)
        {
        }

        public MediaLoader(List<int> mediaIDs, int groupID, string userIP, string picSize) :
            base(groupID, userIP, 0, 0, picSize)
        {
            MediaIDs = mediaIDs;
        }

        public MediaLoader(List<int> mediaIDs, string userName, string userIP, string picSize) :
            this(mediaIDs, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaUpdateDateRequest()
            {
                m_lMediaIds = MediaIDs
            };
        }

        protected override object Process()
        {
            List<BaseObject> lMediaObj = null;
            if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
            {
                m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
            }
            else if (m_oResponse == null)// No Response from Catalog, gets medias from cache
            {
                List<SearchResult> lMediaIDs = new List<SearchResult>();
                foreach (int id in MediaIDs)
                {
                    lMediaIDs.Add(new SearchResult() { assetID = id, UpdateDate = DateTime.MinValue });
                }
                m_oMediaCache = new MediaCache(lMediaIDs, GroupID, m_sUserIP, m_oFilter);

            }
            if (m_oMediaCache != null)
            {
                m_oMediaCache.BuildRequest();
                lMediaObj = (List<BaseObject>)m_oMediaCache.Execute();
            }
            return lMediaObj;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaUpdateDateRequest":
                        MediaUpdateDateRequest updateDateRequest = obj as MediaUpdateDateRequest;
                        sText.AppendFormat("MediaUpdateDateRequest: GroupID = {0}, PageIndex = {1}, PageSize = {2}, ", updateDateRequest.m_nGroupID, updateDateRequest.m_nPageIndex, updateDateRequest.m_nPageSize);
                        sText.Append(CatalogHelper.IDsToString(updateDateRequest.m_lMediaIds, "MediaIDs"));
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIdsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse for Media: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIdsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }
    }
}
