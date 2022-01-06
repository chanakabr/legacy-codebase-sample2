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
using ApiObjects.Response;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class MediaAutoCompleteLoader : CatalogRequestManager, ILoaderAdapter
    {
        #region Members

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string SearchText { get; set; }

        public List<int> MediaTypes { get; set; }

        #endregion

        #region CTOR

        public MediaAutoCompleteLoader(int groupID, string userIP, int pageSize, int pageIndex, string searchText, List<int> mediaTypes)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            SearchText = searchText;
            MediaTypes = mediaTypes;
        }

        public MediaAutoCompleteLoader(string userName, string userIP, int pageSize, int pageIndex, string searchText, List<int> mediaTypes)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, searchText, mediaTypes)
        {
        }

        #endregion

        #region CatalogRequestManager

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new MediaAutoCompleteRequest()
            {
                m_sPrefix = SearchText,
                m_MediaTypes = MediaTypes
            };
        }

        protected override void Log(string message, object obj)
        {
            if (!string.IsNullOrEmpty(message) && obj != null)
            {
                StringBuilder log = new StringBuilder();
                log.AppendLine(message);
                if (obj is MediaAutoCompleteRequest)
                {
                    MediaAutoCompleteRequest mediaAutoCompleteRequest = obj as MediaAutoCompleteRequest;
                    log.AppendFormat("BundleMediaRequest: m_sPrefix = {0}", mediaAutoCompleteRequest.m_sPrefix);
                }
                else if (obj is MediaAutoCompleteResponse)
                {
                    MediaAutoCompleteResponse mediaAutoCompleteResponse = obj as MediaAutoCompleteResponse;
                    string msgLog = string.Empty;
                    if (mediaAutoCompleteResponse.lResults != null)
                    {
                        msgLog = string.Format("MediaAutoCompleteResponse: lResults", string.Join(", ", mediaAutoCompleteResponse.lResults.ToArray()));
                    }
                    else
                    {
                        msgLog = "MediaAutoCompleteResponse: No data returned";
                    }

                    log.Append(msgLog);
                }

                if (logger != null)
                {
                    logger.Info(log.ToString());
                }
            }
        }

        #endregion

        public object Execute()
        {
            List<string> retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = (m_oResponse as MediaAutoCompleteResponse).lResults;
            }
            return retVal;
        }

        #region ILoaderAdapter

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
