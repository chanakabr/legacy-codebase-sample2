using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders 
{
    public class NPVRSeriesLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(NPVRSeriesLoader));

        public RecordedEPGOrderObj RecordedEPGOrderObj { get; set; }

        #region ctr

        public NPVRSeriesLoader(int groupID, string userIP, string siteGuid, int pageSize, int pageIndex, RecordedEPGOrderObj recordedEPGOrderObj)
                : base(groupID, userIP, pageSize, pageIndex)
            {
                RecordedEPGOrderObj = recordedEPGOrderObj;
                SiteGuid = siteGuid;
            }

        public NPVRSeriesLoader(string userName, string userIP, string siteGuid, int pageSize, int pageIndex, RecordedEPGOrderObj recordedEPGOrderObj)
                : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, siteGuid, pageSize, pageIndex, recordedEPGOrderObj)
            {
            }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new NPVRSeriesRequest()
            {
                m_sSiteGuid = SiteGuid,
                m_oOrderObj = RecordedEPGOrderObj
            };
        }

        public object Execute()
        {
            NPVRSeriesResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as NPVRSeriesResponse;
            }
            return retVal.recordedSeries;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                if (obj is NPVRSeriesRequest)
                {
                    sText.AppendFormat("NPVRSeriesRequest: groupID = {0}, userIP = {1}", GroupID, m_sUserIP);
                }
                else if (obj is NPVRSeriesResponse)
                {
                    NPVRRetrieveResponse assetStatsResponse = obj as NPVRRetrieveResponse;
                    sText.AppendFormat("NPVRSeriesResponse");
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
