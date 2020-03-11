using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using TVPPro.SiteManager.Manager;


namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class UserSocialMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int SocialAction { get; set; }
        public int SocialPlatform { get; set; }

        #region Constructors
        public UserSocialMediaLoader(string siteGuid, int socialAction, int socialPlatform, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            SiteGuid = siteGuid;
            SocialAction = socialAction;
            SocialPlatform = socialPlatform;
        }

        public UserSocialMediaLoader(string siteGuid, int socialAction, int socialPlatform, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(siteGuid, socialAction, socialPlatform, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new UserSocialMediasRequest()
            {
                m_nSocialAction = SocialAction,
                m_nSocialPlatform = SocialPlatform,
            };
        }

        public override string GetLoaderCachekey()
        {
            return string.Format("social_siteguid{0}_action{1}_platform{2}_index{3}_size{4}_group_{5}", SiteGuid, SocialAction, SocialPlatform, PageIndex, PageSize, GroupID);
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.UserSocialMediasRequest":
                        UserSocialMediasRequest userSocialMediasRequest = obj as UserSocialMediasRequest;
                        sText.AppendFormat("UserSocialMediasRequest: SocialAction = {0}, SocialPlatform = {1}, GroupID = {2}, PageIndex = {3}, PageSize = {4}",
                            userSocialMediasRequest.m_nSocialAction, userSocialMediasRequest.m_nSocialPlatform, userSocialMediasRequest.m_nGroupID, userSocialMediasRequest.m_nPageIndex, userSocialMediasRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIDsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse: TotalItems = {0}, ", mediaIDsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIDsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }


    }
}
