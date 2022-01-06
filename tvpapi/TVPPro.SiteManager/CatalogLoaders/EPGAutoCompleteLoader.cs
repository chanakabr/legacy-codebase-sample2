using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Phx.Lib.Log;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGAutoCompleteLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SearchText { get; set; }

        #region Constructors

        public EPGAutoCompleteLoader(int groupID, string userIP, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            SearchText = searchText;
            StartTime = startTime;
            EndTime = endTime;
        }

        public EPGAutoCompleteLoader(string userName, string userIP, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, searchText, startTime, endTime)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgAutoCompleteRequest()
            {
                m_dEndDate = EndTime,
                m_dStartDate = StartTime,
                m_sSearch = SearchText,
            };
        }

        public object Execute()
        {
            List<string> retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse == null ? null : (m_oResponse as EpgAutoCompleteResponse).m_sList;
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgAutoCompleteRequest":
                        EpgCommentRequest request = obj as EpgCommentRequest;
                        sText.AppendFormat("EpgAutoCompleteRequest: SearchText = {0}, GroupID = {1}, StartTime = {2}, EndTime = {3}", SearchText, GroupID, StartTime, EndTime);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgAutoCompleteResponse":
                        EpgAutoCompleteResponse response = obj as EpgAutoCompleteResponse;
                        sText.AppendFormat("EpgAutoCompleteResponse: TotalItems = {0}, ", response.m_nTotalItems);
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
