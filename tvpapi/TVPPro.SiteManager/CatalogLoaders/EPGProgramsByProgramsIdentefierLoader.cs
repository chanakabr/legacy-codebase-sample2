using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGProgramsByProgramsIdentefierLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<string> PIDs { get; set; }
        public int Duration { get; set; }
        public Language Lang { get; set; }

        #region Constructors

        public EPGProgramsByProgramsIdentefierLoader(int groupID, string userIP, int pageSize, int pageIndex, List<string> pids, int duration, Language lang)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            PIDs = pids;
            Duration = duration;
            Lang = lang;

        }

        public EPGProgramsByProgramsIdentefierLoader(string userName, string userIP, int pageSize, int pageIndex, List<string> pids, int duration, Language lang)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, pids, duration, lang)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EPGProgramsByProgramsIdentefierRequest()
            {
                duration = Duration,
                eLang = Lang,
                pids = PIDs
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
                retVal = new List<EPGChannelProgrammeObject>();
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGProgramsByProgramsIdentefierRequest":
                        EPGProgramsByProgramsIdentefierRequest request = obj as EPGProgramsByProgramsIdentefierRequest;
                        sText.AppendFormat("EPGProgramsByScidsRequest: GroupID = {0}, PageIndex = {1}, PageSize = {2}, duration = {3}, eLang = {4}, num of pids = {5}", request.m_nGroupID, request.m_nPageIndex, request.m_nPageSize, request.duration, request.eLang, request.pids.Count);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgProgramsResponse":
                        EpgProgramsResponse response = obj as EpgProgramsResponse;
                        sText.AppendFormat("EpgProgramsResponse: TotalItems = {0}, ", response.m_nTotalItems);

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
