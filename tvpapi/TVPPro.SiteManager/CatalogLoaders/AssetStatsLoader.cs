using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Log;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class AssetStatsLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<int> AssetIDs { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public StatsType AssetType { get; set; }

        #region Constructors

        public AssetStatsLoader(int groupID, string userIP, int pageSize, int pageIndex, List<int> assetIDs, StatsType assetType, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            AssetIDs = assetIDs;
            StartTime = startTime;
            EndTime = endTime;
            AssetType = assetType;
        }

        public AssetStatsLoader(string userName, string userIP, int pageSize, int pageIndex, List<int> assetIDs, StatsType assetType, DateTime startTime, DateTime endTime)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, assetIDs, assetType, startTime, endTime)
        {
        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new AssetStatsRequest()
            {
                m_nAssetIDs = AssetIDs,
                m_dStartDate = StartTime,
                m_dEndDate = EndTime,
                m_type = AssetType
            };
        }

        public object Execute()
        {
            AssetStatsResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as AssetStatsResponse;
            }
            return retVal != null && retVal.m_lAssetStat != null && retVal.m_lAssetStat.Count > 0 ? retVal.m_lAssetStat : null;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetStatsRequest":
                        sText.AppendFormat("AssetStatsRequest: groupID = {0}, userIP = {1}, MediaIDs = {2}, StartTime = {3}, EndTime = {4}",
                            GroupID, m_sUserIP, AssetIDs != null & AssetIDs.Count > 0 ? string.Join(",", AssetIDs.Select(id => id.ToString()).ToArray()) : string.Empty, StartTime, EndTime);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.AssetStatsResponse":
                        AssetStatsResponse assetStatsResponse = obj as AssetStatsResponse;
                        sText.AppendFormat("AssetStatsResponse");
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
