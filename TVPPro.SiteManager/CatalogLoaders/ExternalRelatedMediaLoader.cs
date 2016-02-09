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
    public class ExternalRelatedMediaLoader : RelatedMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string FreeParam { get; set; }
        public string RequestId { get; set; }
        public Status Status { get; set; }

        #region Constructors
        public ExternalRelatedMediaLoader(int mediaID, List<int> mediaTypes, int groupID, string userIP, int pageSize, int pageIndex, string picSize, string freeParam = null)
            : base(mediaID, mediaTypes, groupID, userIP, pageSize, pageIndex, picSize)
        {
            MediaID = mediaID;
            MediaTypes = mediaTypes;
            FreeParam = freeParam;
        }

        public ExternalRelatedMediaLoader(int mediaID, List<int> mediaTypes, string userName, string userIP, int pageSize, int pageIndex, string picSize, string freeParam = null)
            : base(mediaID, mediaTypes, userName, userIP, pageSize, pageIndex, picSize)
        {
            FreeParam = freeParam;
        }
        #endregion

        public override string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_related_mediaid{0}_index{1}_size{2}_group{3}", MediaID, PageIndex, PageSize, GroupID);
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            return key.ToString();
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaRelatedExternalRequest()
            {
                m_nMediaTypes = MediaTypes,
                m_nMediaID = MediaID,
                m_sFreeParam = FreeParam,
                m_nUtcOffset = UtcOffset
            };
        }

        public override object Execute()
        {
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            List<BaseObject> lObj = null;

            m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            {
                Log("Got:", m_oResponse);
                lObj = (List<BaseObject>)Process();
            }

            MediaIdsStatusResponse response = m_oResponse as MediaIdsStatusResponse;

            if (response != null)
            {
                this.RequestId = response.RequestId;
                this.Status = response.Status;
            }

            return lObj;
        }
        
        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaRelatedExternalRequest":
                        MediaRelatedExternalRequest searchRquest = obj as MediaRelatedExternalRequest;
                        sText.AppendFormat("MediaExternalSearchRequest: MediaId = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", searchRquest.m_nMediaID, searchRquest.m_nGroupID, searchRquest.m_nPageIndex, searchRquest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsStatusResponse":
                        MediaIdsStatusResponse mediaIdsResponse = obj as MediaIdsStatusResponse;
                        sText.AppendFormat("MediaIdsResponse for Ralated: TotalItems = {0}, ", mediaIdsResponse.m_nTotalItems);
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
