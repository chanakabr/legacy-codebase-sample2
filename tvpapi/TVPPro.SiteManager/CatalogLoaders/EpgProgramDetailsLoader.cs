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
using Core.Catalog;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class EpgProgramDetailsLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<int> PIDs { get; set; }

        #region Constructors

        public EpgProgramDetailsLoader(int groupID, string userIP, int pageSize, int pageIndex, List<int> pids)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            PIDs = pids;

        }

        public EpgProgramDetailsLoader(string userName, string userIP, int pageSize, int pageIndex, List<int> pids)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, pids)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgProgramDetailsRequest()
            {
                m_lProgramsIds = PIDs
            };
        }

        public virtual object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oResponse);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = ((EpgProgramResponse)m_oResponse).m_lObj;
            }
            else
            {
                retVal = new List<ProgramObj>();
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgProgramDetailsRequest":
                        EPGProgramsByProgramsIdentefierRequest request = obj as EPGProgramsByProgramsIdentefierRequest;
                        sText.AppendFormat($"EPGProgramsByScidsRequest: GroupID = {request.m_nGroupID}, PageIndex = {request.m_nPageIndex}, PageSize = { request.m_nPageSize}, num of pids = {request.pids.Count}");
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
