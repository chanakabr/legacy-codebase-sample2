using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using Phx.Lib.Log;
using System.Reflection;
using Core.Catalog.Response;
using Core.Catalog.Request;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class RelatedMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int MediaID { get; set; }
        public List<int> MediaTypes { get; set; }

        #region Constructors
        public RelatedMediaLoader(int mediaID, List<int> mediaTypes, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            MediaID = mediaID;
            MediaTypes = mediaTypes;
        }

        public RelatedMediaLoader(int mediaID, List<int> mediaTypes, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(mediaID, mediaTypes, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaRelatedRequest()
            {
                m_nMediaTypes = MediaTypes,
                m_nMediaID = MediaID
            };
        }

        public override string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();
            key.AppendFormat("related_mediaid{0}_index{1}_size{2}_group{3}", MediaID, PageIndex, PageSize, GroupID);
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            return key.ToString();
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaRelatedRequest":
                        MediaRelatedRequest relatedRquest = obj as MediaRelatedRequest;
                        sText.AppendFormat("MediaRelatedRequest: MediaID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", relatedRquest.m_nMediaID, relatedRquest.m_nGroupID, relatedRquest.m_nPageIndex, relatedRquest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIdsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse for Ralated: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIdsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
            //logger.Info(sText.ToString());
        }
    }
}
