using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.TVMDataLoader.Protocols.CommentsSave;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Manager;
using TVPPro.SiteManager.Objects;

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

        public static List<Comment> GetMediaComments(int mediaID, int groupID, int pageSize, int pageIndex)
        {            
            List<Comment> retVal = null;
            MediaComments mediaComments = (new MediaCommentsListLoader(mediaID, groupID, SiteHelper.GetClientIP(), pageSize, pageIndex)).Execute() as MediaComments;
          
            if (mediaComments != null && mediaComments.commentsList.Count > 0)
            {
                retVal = new List<Comment>();
                foreach (CommentContext context in mediaComments.commentsList)
                {
                    retVal.Add(parseCommentContextToComment(context));
                }
            }
            return retVal;
        }

        private static Comment parseCommentContextToComment(CommentContext context)
        {
            Comment comment = new Comment(context.Writer, context.Header, context.Date, context.Content, context.UserPicURL);
            return comment;
        }

        public static bool SaveMediaComments(int groupID, PlatformType platform, string siteGuid, string udid, string language, string country, int mediaId, string writer, string header, string subHeader, string content, bool autoActive)
        {
            int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguageDBID(language);
            CommentResponse response = new MediaCommentLoader(groupID, SiteHelper.GetClientIP(), ilanguage, siteGuid, udid, mediaId, content, country, header, subHeader, writer, autoActive).Execute() as CommentResponse;
            bool retVal = response != null ? response.eStatusComment == StatusComment.SUCCESS : false;
            return retVal; 
        }

        public static StatusComment AddEPGComment(int groupId, PlatformType platform, string language, string siteGuid, string udid, int epgProgramID, string contentText, string country, string header, string subHeader, string writer, bool autoActive)
        {
            int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupId, platform).GetLanguageDBID(language);
            CommentResponse response = new TVPPro.SiteManager.CatalogLoaders.EPGCommentLoader(groupId, SiteHelper.GetClientIP(), ilanguage, siteGuid, udid, epgProgramID, contentText, country, header, subHeader, writer, autoActive).Execute() as CommentResponse;
            if (response != null)
                return response.eStatusComment;
            else
                return StatusComment.FAIL;
        }

        public static List<EPGComment> GetEPGCommentsList(int groupId, PlatformType platform, string language, int epgProgramID, int pageSize, int pageIndex)
        {
            int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupId, platform).GetLanguageDBID(language);
            return new TVPPro.SiteManager.CatalogLoaders.EPGCommentsListLoader(epgProgramID, ilanguage, groupId, SiteHelper.GetClientIP(), pageSize, pageIndex).Execute() as List<TVPPro.SiteManager.Objects.EPGComment>;
        }
    }
}
