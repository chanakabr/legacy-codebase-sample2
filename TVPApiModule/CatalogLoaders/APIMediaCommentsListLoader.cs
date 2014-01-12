using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;

namespace TVPApiModule.CatalogLoaders
{
    [Serializable]
    public class APIMediaCommentsListLoader : MediaCommentsListLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        #region Constructors

        public APIMediaCommentsListLoader(int groupID, PlatformType platform, string udid, string language, int mediaID, string userIP, int pageSize, int pageIndex)
            : base(mediaID, groupID, userIP, pageSize, pageIndex)
        {
            Platform = platform.ToString();
            DeviceId = udid;
            Culture = language;
        }

        protected override Object ExecuteCommentsAdapter(CommentsListResponse commentsListRespons)
        {
            List<Comment> retVal = new List<Comment>();

            foreach (var comment in commentsListRespons.m_lComments)
            {
                retVal.Add(new Comment(comment.m_sWriter, comment.m_sHeader, comment.m_dCreateDate.ToString(), comment.m_sContentText));
            }

            return retVal;
        }

        #endregion
    }
}
