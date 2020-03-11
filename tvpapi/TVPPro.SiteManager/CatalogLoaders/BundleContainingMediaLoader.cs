using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Core.Catalog;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using ApiObjects.Catalog;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class BundleContainingMediaLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public eBundleType BundleType { get; set; }
        public int BundleID { get; set; }
        public int MediaID { get; set; }
        public string MediaType { get; set; }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new BundleContainingMediaRequest()
            {
                m_eBundleType = BundleType,
                m_nBundleID = BundleID,
                m_nMediaID = MediaID,
                m_sMediaType = MediaType
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.BundleContainingMediaRequest":
                        BundleContainingMediaRequest bundleContainingMediaRequest = obj as BundleContainingMediaRequest;
                        log.AppendFormat("BundleMediaRequest: BundleID = {0}, GroupID = {1}, BundleType = {2}", bundleContainingMediaRequest.m_nBundleID, bundleContainingMediaRequest.m_nGroupID, bundleContainingMediaRequest.m_eBundleType);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.ContainingMediaResponse":
                        ContainingMediaResponse containingMediasResponse = obj as ContainingMediaResponse;
                        log.AppendFormat("MediaIdsResponse: MediaContained = {0}, ", containingMediasResponse.m_bContainsMedia);
                        break;
                }

                if (logger != null)
                {
                    logger.Info(log.ToString());
                }
            }
        }

        #region ILoaderAdapter

        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute()
        {
            bool retVal = false;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                ContainingMediaResponse response = m_oResponse as ContainingMediaResponse;
                retVal = response != null ? response.m_bContainsMedia : false;
            }
            return retVal;
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
