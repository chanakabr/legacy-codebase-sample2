using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataEntities;
using Phx.Lib.Log;
using System.Reflection;
using Core.Catalog.Request;
using Core.Catalog.Response;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class MediaCommentsListLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int MediaID { get; set; }
        public eOrderComments OrderBy { get; set; }

        public enum eOrderComments
        {
            None,
            CommentNumber
        }

        #region Constructors
        public MediaCommentsListLoader(int mediaID, int groupID, string userIP, int pageSize, int pageIndex)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            MediaID = mediaID;
        }

        public MediaCommentsListLoader(int mediaID, string userName, string userIP, int pageSize, int pageIndex)
            : this(mediaID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex)
        {
        }

        public MediaCommentsListLoader(int mediaID, int groupID, string userIP, int pageSize, int pageIndex, string picSize, Provider provider)
            : this(mediaID, groupID, userIP, pageSize, pageIndex)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new CommentsListRequest()
            {
                m_nMediaID = MediaID
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
                retVal = ExecuteCommentsAdapter(m_oResponse as CommentsListResponse);
            }
            else
            {
                if (!FailOverManager.Instance.SafeMode)
                    FailOverManager.Instance.AddRequest(true);
                retVal = new MediaComments();
            }
            return retVal;
        }

        //protected virtual MediaComments ExecuteCommentsAdapter(CommentsListResponse commentsListRespons)
        protected virtual Object ExecuteCommentsAdapter(CommentsListResponse commentsListRespons)
        {
            MediaComments retVal = new MediaComments();
            List<string> commentsTypesList = new List<string>();
            foreach (Comments comment in commentsListRespons.m_lComments)
            {
                CommentContext context = new CommentContext(
                    string.Empty,
                    comment.m_dCreateDate.ToString(),
                    comment.m_sWriter,
                    comment.m_sHeader,
                    comment.m_sSubHeader,
                    comment.m_sContentText,
                    comment.m_nAssetID.ToString(),
                    comment.m_sUserPicURL);

                retVal.commentsList.Add(context);

                dsComments.CommentsRow itemRow = retVal.commentsDS.Comments.NewCommentsRow();

                itemRow.Type = string.Empty;
                comment.m_dCreateDate.ToString();
                itemRow.Writer = comment.m_sWriter;
                itemRow.Header = comment.m_sHeader;
                itemRow.SubHeader = comment.m_sSubHeader;
                itemRow.Content = comment.m_sContentText;
                itemRow.MediaId = comment.m_nAssetID.ToString();

                retVal.commentsDS.Comments.AddCommentsRow(itemRow);

                if (!commentsTypesList.Contains(itemRow.Type))
                {
                    commentsTypesList.Add(itemRow.Type);

                    dsComments.CommentTypesRow commentTypeRow = retVal.commentsDS.CommentTypes.NewCommentTypesRow();
                    commentTypeRow.Type = itemRow.Type;
                    retVal.commentsDS.CommentTypes.AddCommentTypesRow(commentTypeRow);
                }
            }

            switch (OrderBy)
            {
                case eOrderComments.CommentNumber:
                    retVal.commentsList = retVal.commentsList.OrderBy(commentItem => commentItem.Date).ToList();
                    break;
                case eOrderComments.None:
                default:
                    retVal.commentsList.Reverse();
                    break;
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
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.CommentsListRequest":
                        CommentsListRequest commentsListRequest = obj as CommentsListRequest;
                        sText.AppendFormat("CommentsListRequest: MediaID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", commentsListRequest.m_nMediaID, commentsListRequest.m_nGroupID, commentsListRequest.m_nPageIndex, commentsListRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.CommentsListResponse":
                        CommentsListResponse commentsListResponse = obj as CommentsListResponse;
                        sText.AppendFormat("ChannelDetailsResponse: TotalItems = {0}, ", commentsListResponse.m_nTotalItems);
                        sText.AppendLine(commentsListResponse.m_lComments.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
            //logger.Info(sText.ToString());
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
