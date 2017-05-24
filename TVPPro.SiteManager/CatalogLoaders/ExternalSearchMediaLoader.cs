using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class ExternalSearchMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string Query { get; set; }
        public List<int> MediaTypes { get; set; }
        public string RequestId { get; set; }
        public Status Status { get; set; }

        #region Constructors
        public ExternalSearchMediaLoader(string query, List<int> mediaTypes, int groupID, string userIP, int pageSize, int pageIndex)
            : base(groupID, userIP, pageSize, pageIndex, "0")
        {
            MediaTypes = mediaTypes;
            Query = query;
        }

        public ExternalSearchMediaLoader(string query, List<int> mediaTypes, string userName, string userIP, int pageSize, int pageIndex)
            : this(query, mediaTypes, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex)
        {
        }
        #endregion

        public override string GetLoaderCachekey()
        {
            StringBuilder key = new StringBuilder();
            key.AppendFormat("external_search_index{0}_size{1}_group{2}", PageIndex, PageSize, GroupID);
            if (MediaTypes != null && MediaTypes.Count > 0)
                key.AppendFormat("_mt={0}", string.Join(",", MediaTypes.Select(type => type.ToString()).ToArray()));
            return key.ToString();

        }
        
        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaSearchExternalRequest()
            {
                m_nMediaTypes = MediaTypes,
                m_sQuery = Query,
                m_sDeviceID = m_oFilter.m_sDeviceId
            };
        }

        public virtual object Execute()
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaSearchExternalRequest":
                        MediaSearchExternalRequest searchRquest = obj as MediaSearchExternalRequest;
                        sText.AppendFormat("MediaExternalSearchRequest: Query = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", searchRquest.m_sQuery, searchRquest.m_nGroupID, searchRquest.m_nPageIndex, searchRquest.m_nPageSize);
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
