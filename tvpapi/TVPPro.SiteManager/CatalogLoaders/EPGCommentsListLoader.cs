using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Phx.Lib.Log;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Objects;
using Core.Catalog.Response;
using Core.Catalog.Request;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class EPGCommentsListLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int EpgProgramID { get; set; }

        #region Constructors
        public EPGCommentsListLoader(int epgProgramID, int language, int groupID, string userIP, int pageSize, int pageIndex)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            EpgProgramID = epgProgramID;
            Language = language;
        }

        public EPGCommentsListLoader(int epgProgramID, int language, string userName, string userIP, int pageSize, int pageIndex)
            : this(epgProgramID, language, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex)
        {
        }

        public EPGCommentsListLoader(int epgProgramID, int groupID, int language, string userIP, int pageSize, int pageIndex, string picSize, Provider provider)
            : this(epgProgramID, language, groupID, userIP, pageSize, pageIndex)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new EpgCommentsListRequest()
            {
                m_nEpgProgramID = EpgProgramID
            };
        }

        public object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = ExecuteEPGCommentsAdapter(m_oResponse as CommentsListResponse);
            }
            else
            {
                retVal = new List<EPGComment>();
            }
            return retVal;
        }

        private List<EPGComment> ExecuteEPGCommentsAdapter(CommentsListResponse commentsListRespons)
        {
            List<EPGComment> retVal = new List<EPGComment>();
            List<string> commentsTypesList = new List<string>();
            foreach (Comments comment in commentsListRespons.m_lComments)
            {

                EPGComment epgComment = new EPGComment()
                {
                    ContentText = comment.m_sContentText,
                    CreateDate = comment.m_dCreateDate,
                    EPGProgramID = comment.m_nAssetID,
                    Header = comment.m_sHeader,
                    ID = comment.Id,
                    Language = comment.m_nLang,
                    LanguageName = comment.m_sLangName,
                    Writer = comment.m_sWriter,
                    UserPicURL = comment.m_sUserPicURL
                };
                retVal.Add(epgComment);
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.EpgCommentsListRequest":
                        EpgCommentsListRequest commentsListRequest = obj as EpgCommentsListRequest;
                        sText.AppendFormat("EpgCommentsListRequest: EpgProgramID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", commentsListRequest.m_nEpgProgramID, commentsListRequest.m_nGroupID, commentsListRequest.m_nPageIndex, commentsListRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.CommentsListResponse":
                        CommentsListResponse commentsListResponse = obj as CommentsListResponse;
                        sText.AppendFormat("EpgCommentsListResponse: TotalItems = {0}, ", commentsListResponse.m_nTotalItems);
                        sText.AppendLine(commentsListResponse.m_lComments.ToStringEx());
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
