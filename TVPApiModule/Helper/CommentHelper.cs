using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.CommentsSave;

/// <summary>
/// Summary description for CommentHelper
/// </summary>
/// 

namespace TVPApi
{
    public class CommentHelper
    {
        public CommentHelper()
        {
            
        }

        public static List<Comment> GetMediaComments(string TVMUser, string TVMPass, long mediaID, int pageSize, int pageIndex)
        {            
            List<Comment> retVal = null;
            MediaComments mediaComments = (new TVMCommentsLoader(TVMUser, TVMPass, mediaID.ToString())).Execute();
            if (mediaComments != null && mediaComments.commentsList.Count > 0)
            {
                int startIndex = (pageIndex) * pageSize;
                IEnumerable<CommentContext> pagedComments = PagingHelper.GetPagedData<CommentContext>(startIndex, pageSize, mediaComments.commentsList);
                retVal = new List<Comment>();
                foreach (CommentContext context in pagedComments)
                {
                    retVal.Add(parseCommentContextToComment(context));
                }


            }
            return retVal;
        }

        private static Comment parseCommentContextToComment(CommentContext context)
        {
            Comment comment = new Comment(context.Writer, context.Header, context.Date, context.Content);
            return comment;
        }


        public static bool SaveMediaComments(string TVMUser, string TVMPass, int mediaId, string writer, string header, string subHeader, string content, bool autoActive)
        {
            bool retVal = false;
            retVal = (new TVMCommentsSave(TVMUser, TVMPass, mediaId.ToString(), writer, header, subHeader, content, autoActive)).Execute();
            return retVal;
        }

    }
}
