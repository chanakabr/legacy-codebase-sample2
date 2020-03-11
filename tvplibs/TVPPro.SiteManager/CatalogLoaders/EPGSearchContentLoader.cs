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
    public class EPGSearchContentLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string SearchText { get; set; }
        public List<long> EPGChannelIDs { get; set; }

        #region Constructors

        public EPGSearchContentLoader(int groupID, string userIP, int pageSize, int pageIndex, string searchText)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            SearchText = searchText;
        }

        public EPGSearchContentLoader(string userName, string userIP, int pageSize, int pageIndex, string searchText)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, searchText)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {

            m_oRequest = new EpgSearchRequest()
            {
                m_sSearch = SearchText,
                m_oEPGChannelIDs = EPGChannelIDs,
            };
        }

        public virtual object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = ((EpgProgramsResponse)m_oResponse).lEpgList;
            }
            else
            {
                retVal = new List<BaseObject>();
            }
            return retVal;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGSearchContentRequest":
                        EPGSearchContentRequest searchRequest = obj as EPGSearchContentRequest;
                        sText.AppendFormat("EPGSearchContentRequest: GroupID = {0}, PageIndex = {1}, PageSize = {2}, searchText = {3} ", searchRequest.m_nGroupID, searchRequest.m_nPageIndex, searchRequest.m_nPageSize, searchRequest.m_sSearch);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgProgramsResponse":
                        EpgProgramsResponse searchResponse = obj as EpgProgramsResponse;
                        sText.AppendFormat("EpgSearchResponse: TotalItems = {0}, ", searchResponse.m_nTotalItems);
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ISupportPaging method
        public bool TryGetItemsCount(out long count)
        {
            count = 0;

            if (m_oResponse == null)
                return false;

            count = m_oResponse.m_nTotalItems;

            return true;
        }
        #endregion

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
