using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Log;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class BundleMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int bundleId { get; set; }
        public string mediaType { get; set; }
        public OrderObj orderObj { get; set; }
        public eBundleType bundleType { get; set; }

        #region CTOR

        public BundleMediaLoader(int bundleId, string mediaType, OrderObj order, int groupID, string userIP, int pageSize, int pageIndex, string picSize, eBundleType bundleType)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            this.bundleId = bundleId;
            this.mediaType = mediaType;
            this.orderObj = order;
            this.bundleType = bundleType;

        }

        public BundleMediaLoader(int bundleId, string mediaType, OrderObj order, string userName, string userIP, int pageSize, int pageIndex, string picSize, eBundleType bundleType)
            : this(bundleId, mediaType, order, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize, bundleType)
        {

        }

        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new BundleMediaRequest()
            {
                m_eBundleType = bundleType,
                m_nBundleID = bundleId,
                m_oOrderObj = orderObj,
                m_sMediaType = mediaType
            };
        }

        protected override void Log(string message, object obj)
        {
            if (!string.IsNullOrEmpty(message) && obj != null)
            {
                StringBuilder log = new StringBuilder();
                log.AppendLine(message);
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.BundleMediaRequest":
                        BundleMediaRequest bundleRequest = obj as BundleMediaRequest;
                        log.AppendFormat("BundleMediaRequest: BundleID = {0}, GroupID = {1}, BundleType = {2}", bundleRequest.m_nBundleID, bundleRequest.m_nGroupID, bundleRequest.m_eBundleType);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIdsResponse = obj as MediaIdsResponse;
                        log.AppendFormat("MediaIdsResponse: TotalItemsInBundle = {0}, ", mediaIdsResponse.m_nTotalItems);
                        break;
                }

                if (logger != null)
                {
                    logger.Info(log.ToString());
                }
            }
        }

        public override string GetLoaderCachekey()
        {

            return string.Format("bundle_media_bundleId{0}mediaType{1}orderDir{2}orderBy{3}orderValue{4}index{5}size{6}group{7}",
                                    bundleId, mediaType, orderObj.m_eOrderDir, orderObj.m_eOrderBy,
                                    string.IsNullOrEmpty(orderObj.m_sOrderValue) ? string.Empty : orderObj.m_sOrderValue,
                                    PageIndex, PageSize, GroupID);
        }
    }
}
