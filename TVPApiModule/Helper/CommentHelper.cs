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

        public static List<Comment> GetMediaComments(int mediaID, int commentType, int groupID, int pageSize, int pageIndex)
        {            
            List<Comment> retVal = null;
            MediaComments mediaComments = (new CommentsListLoader(mediaID, commentType, groupID, SiteHelper.GetClientIP(), pageSize, pageIndex)).Execute() as MediaComments;
          
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
            Comment comment = new Comment(context.Writer, context.Header, context.Date, context.Content);
            return comment;
        }

        public static bool SaveMediaComments(string TVMUser, string TVMPass, string siteGuid, string udid, int mediaId, string writer, string header, string subHeader, string content, bool autoActive)
        {
            bool retVal = false;
            retVal = (new TVMCommentsSave(TVMUser, TVMPass, mediaId.ToString(), writer, header, subHeader, content, autoActive) { DeviceUDID = udid, SiteGuid = siteGuid }).Execute();
            return retVal;
        }

        public static StatusEpgComment AddEPGComment(int groupId, PlatformType platform, string language, string siteGuid, string udid, int epgProgramID, TVPPro.SiteManager.Helper.CatalogEnums.EPGCommentType commentType, DateTime publishDate, string contentText, string country, string header, string subHeader, string writer)
        {

            int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupId, platform).GetLanguageDBID(language);
            EpgCommentResponse response = new TVPPro.SiteManager.CatalogLoaders.EPGCommentLoader(groupId, SiteHelper.GetClientIP(), ilanguage, siteGuid, udid, epgProgramID, commentType.ToString(), publishDate, contentText, country, header, subHeader, writer).Execute() as EpgCommentResponse;
            if (response != null)
                return response.eStatusEpgComment;
            else
                return StatusEpgComment.FAIL;
        }

        public static List<EPGComment> GetEPGCommentsList(int groupId, PlatformType platform, string language, int epgProgramID, TVPPro.SiteManager.Helper.CatalogEnums.EPGCommentType commentType, int pageSize, int pageIndex)
        {
            int ilanguage = TextLocalizationManager.Instance.GetTextLocalization(groupId, platform).GetLanguageDBID(language);
            return new TVPPro.SiteManager.CatalogLoaders.EPGCommentsListLoader(epgProgramID, (int)commentType, ilanguage, groupId, SiteHelper.GetClientIP(), pageSize, pageIndex).Execute() as List<TVPPro.SiteManager.Objects.EPGComment>;
        }
    }
}
