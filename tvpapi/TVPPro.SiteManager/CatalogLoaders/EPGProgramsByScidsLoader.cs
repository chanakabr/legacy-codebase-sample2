using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects;
using TVPPro.SiteManager.Manager;
using Core.Catalog;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGProgramsByScidsLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<string> SCIDs { get; set; }
        public int Duration { get; set; }
        public Language Lang { get; set; }

        #region Constructors

        public EPGProgramsByScidsLoader(int groupID, string userIP, int pageSize, int pageIndex, List<string> scids, int duration, Language lang)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            SCIDs = scids;
            Duration = duration;
            Lang = lang;

        }

        public EPGProgramsByScidsLoader(string userName, string userIP, int pageSize, int pageIndex, List<string> scids, int duration, Language lang)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, scids, duration, lang)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EPGProgramsByScidsRequest()
            {
                duration = Duration,
                eLang = Lang,
                scids = SCIDs
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGProgramsByScidsRequest":
                        EPGProgramsByScidsRequest request = obj as EPGProgramsByScidsRequest;
                        sText.AppendFormat($"EPGProgramsByScidsRequest: GroupID = {request.m_nGroupID}, PageIndex = {request.m_nPageIndex}, PageSize = {request.m_nPageSize}, duration = {request.duration}, eLang = {request.eLang}, num of scids = {request.scids.Count}");
                        break; 
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgProgramsResponse":
                        EpgProgramsResponse response = obj as EpgProgramsResponse;
                        sText.AppendFormat($"EpgProgramsResponse: TotalItems = {response.m_nTotalItems}, ");

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
