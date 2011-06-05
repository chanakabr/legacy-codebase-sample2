using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;

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

        public static List<Comment> GetMediaComments(long mediaID, int pageSize, int pageIndex)
        {
            List<Comment> retVal = null;
            MediaComments mediaComments = (new TVMCommentsLoader(mediaID.ToString())).Execute();
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


    }
}
