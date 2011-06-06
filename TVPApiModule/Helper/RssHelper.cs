using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using System.Web;

namespace TVPApiModule.Helper
{
    public class RssHelper
    {
        public static string GetSiteBaseURL(bool isSecure, int groupID, PlatformType platform)
        {
            if ( ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.Site.ApplicativeBaseUri.UsePermanentURL)
            {
                return string.Concat(((isSecure) ? ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.Site.ApplicativeBaseUri.SecureBaseUri : ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.Site.ApplicativeBaseUri.BaseUri).TrimEnd('/'), '/');
            }
            else
            {
                return string.Concat((isSecure) ? "https://" : "http://", HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.ApplicationPath.TrimEnd('/'), '/');
            }
        }

        public static string GetRssPath(int GroupId, object ChannelId, object Pic, PlatformType platform)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(int.Parse(GroupId.ToString()), platform).TechnichalConfiguration.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}",
                        ConfigManager.GetInstance().GetConfig(int.Parse(GroupId.ToString()), platform).TechnichalConfiguration.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false, GroupId, platform),
                        Pic.ToString());
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path full media path (not the item page url) to the relevent GroupID  and ChannelID
        /// </summary>

        public static string GetRssPath(int GroupId, object ChannelId, object Pic, string fileQuality, string fileFormat, PlatformType platform)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupId, platform).TechnichalConfiguration.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}&quality={5}&format={6}",
                        ConfigManager.GetInstance().GetConfig(int.Parse(GroupId.ToString()), platform).TechnichalConfiguration.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false, GroupId, platform),
                        Pic.ToString(), fileQuality, fileFormat);
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path full media path (not the item page url) to the relevent GroupID  and ChannelID
        /// </summary>

        public static string GetRssPathWithMediaIDS(int GroupId, object MediaIDStr, object MediaTagStr, object Pic, string fileQuality, string fileFormat, PlatformType platform)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupId, platform).TechnichalConfiguration.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&media_ids={2}&base_url={3}&pic={4}&quality={5}&format={6}&roles={7}",
                        ConfigManager.GetInstance().GetConfig(int.Parse(GroupId.ToString()), platform).TechnichalConfiguration.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        MediaIDStr.ToString(),
                        GetSiteBaseURL(false, GroupId, platform),
                        Pic.ToString(), fileQuality, fileFormat, MediaTagStr);
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path to the relevent GroupID  and ChannelID by user name
        /// </summary>

        public static string GetRssPathByUser(object tvmUser, object ChannelId, object Pic, int groupID, PlatformType platform)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty( ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.TVM.TVMRssURL))
            {
                TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByUser((string)tvmUser);
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}",
                        ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.TVM.TVMRssURL,
                        account.GroupID.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false, groupID, platform),
                        Pic.ToString());
            }
            return RssPath;
        }
    }


}
