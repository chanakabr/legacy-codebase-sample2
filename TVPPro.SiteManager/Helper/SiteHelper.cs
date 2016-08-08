using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Helpers;
using System.Data;
using System.Configuration;
using Tvinci.Data.TVMDataLoader.Protocols;
using TVPPro.Configuration.Technical;
using System.Drawing;
using System.IO;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using System.Runtime.InteropServices;
using TVPPro.SiteManager.Context;
using Tvinci.Web.Controls.Gallery.Part;
using System.Web;
using TVPPro.SiteManager.Manager;
using TVPPro.Configuration.Site;
using System.Web.UI.WebControls;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using System.Web.UI;
using System.Xml;
using TVPPro.Configuration.Online;
using System.Net;
using Tvinci.Localization;
using System.Text.RegularExpressions;

namespace TVPPro.SiteManager.Helper
{
    public class SiteHelper
    {
        private const int FBMINSIZEX = 200;
        private const int FBMINSIZEY = 200;

        /// <summary>
        /// Returns the base URL.
        /// </summary>
        public static string GetSiteBaseURL(bool isSecure)
        {
            if (TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.UsePermanentURL)
            {
                return string.Concat(((isSecure) ? TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.SecureBaseUri : TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.BaseUri).TrimEnd('/'), '/');
            }
            else
            {
                return string.Concat((isSecure) ? "https://" : "http://", HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.ApplicationPath.TrimEnd('/'), '/');
            }
        }

        public static string SecureResource(string resourceUrl)
        {
            if (!string.IsNullOrEmpty(resourceUrl))
            {
                string isSecureVal = System.Configuration.ConfigurationManager.AppSettings["ISSecure"];
                bool isSecure = (string.IsNullOrEmpty(isSecureVal)) ? false : bool.Parse(isSecureVal);
                bool hasHttps = (resourceUrl.IndexOf("https", StringComparison.OrdinalIgnoreCase) >= 0);

                if (isSecure)
                {
                    if (!hasHttps)
                    {
                        resourceUrl = resourceUrl.Replace("http", "https");
                    }
                }
                else
                {
                    if (hasHttps)
                    {
                        resourceUrl = resourceUrl.Replace("https", "http");
                    }
                }
            }

            return resourceUrl;
        }

        /// <summary>
        /// Returns the absolute path to the specific item.
        /// </summary>
        public static string GetAbsolutePath(string RelativeFilePath)
        {
            string Identifier = ConfigurationManager.AppSettings["ClientIdentifier"];

            return Tvinci.Helpers.LinkHelper.ParseURL(string.Concat("Clients/", Identifier, RelativeFilePath));
        }

        /// <summary>
        /// Returns  the RSS Path to the relevent GroupID  and ChannelID
        /// </summary>

        public static string GetRssPath(object GroupId, object ChannelId, object Pic)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}",
                        TechnicalConfiguration.Instance.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false),
                        Pic.ToString());
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path full media path (not the item page url) to the relevent GroupID  and ChannelID
        /// </summary>

        public static string GetRssPath(object GroupId, object ChannelId, object Pic, string fileQuality, string fileFormat)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}&quality={5}&format={6}",
                        TechnicalConfiguration.Instance.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false),
                        Pic.ToString(), fileQuality, fileFormat);
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path full media path (not the item page url) to the relevent GroupID  and ChannelID
        /// </summary>

        public static string GetRssPathWithMediaIDS(object GroupId, object MediaIDStr, object MediaTagStr, object Pic, string fileQuality, string fileFormat)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.TVMRssURL))
            {
                RssPath = string.Format("{0}?group_id={1}&type=rss&media_ids={2}&base_url={3}&pic={4}&quality={5}&format={6}&roles={7}",
                        TechnicalConfiguration.Instance.Data.TVM.TVMRssURL,
                        GroupId.ToString(),
                        MediaIDStr.ToString(),
                        GetSiteBaseURL(false),
                        Pic.ToString(), fileQuality, fileFormat, MediaTagStr);
            }
            return RssPath;
        }

        /// <summary>
        /// Returns  the RSS Path to the relevent GroupID  and ChannelID by user name
        /// </summary>

        public static string GetRssPathByUser(object tvmUser, object ChannelId, object Pic)
        {
            string RssPath = string.Empty;

            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.TVM.TVMRssURL))
            {
                TVMAccountType account = PageData.Instance.GetTVMAccountByUserName((string)tvmUser);
                RssPath = string.Format("{0}?group_id={1}&type=rss&channel_id={2}&base_url={3}&pic={4}",
                        TechnicalConfiguration.Instance.Data.TVM.TVMRssURL,
                        account.GroupID.ToString(),
                        ChannelId.ToString(),
                        GetSiteBaseURL(false),
                        Pic.ToString());
            }
            return RssPath;
        }

        /// <summary>
        /// Returns the relative path to the specific item.
        /// </summary>
        public static string GetRelativePath(string RelativeFilePath)
        {
            string Identifier = ConfigurationManager.AppSettings["ClientIdentifier"];

            return string.Concat(@"~/Clients/", Identifier, RelativeFilePath);
        }

        /// <summary>
        /// Returns the relative path to the page.
        /// </summary>
        public static string GetPageRelativePath()
        {
            string Identifier = ConfigurationManager.AppSettings["ClientIdentifier"];
            return string.Concat("Clients/", Identifier);
        }


        public static void CheckAge()
        {
            if (!SessionHelper.IsLegalAge)
            {
                TVPPro.SiteManager.Helper.SessionHelper.NextPage = HttpContext.Current.Request.RawUrl;
                HttpContext.Current.Response.Redirect("~/Static.aspx?desc=Warning&page=WarningPage");

            }
        }

        #region Page URL Builders

        /// <summary>
        /// Get friendly URLs for all media types (if supported by site configuration) 
        /// </summary>
        /// <param name="MediaType"></param>
        /// <param name="MediaID"></param>
        /// <param name="MediaName"></param>
        /// <returns></returns>
        public static string GetPageURL(object MediaType, object MediaID, object MediaName)
        {
            //string mediaName = "NoTitle";
            //if (MediaName != null)
            //{
            //    mediaName = MediaName.ToString();
            //    mediaName = mediaName.Replace("/", "");
            //    mediaName = mediaName.Replace(".", "");
            //    mediaName = mediaName.Replace(",", "");
            //    mediaName = mediaName.Replace(":", "");
            //    mediaName = mediaName.Replace("&", "");
            //    mediaName = mediaName.Replace("'", "");
            //    mediaName = mediaName.Replace(" ", "-");
            //    mediaName = mediaName.Replace("+", "");
            //    mediaName = mediaName.Replace("%", "");
            //    mediaName = mediaName.Replace("*", "");
            //}

            //RegexOptions options = RegexOptions.None;
            //Regex regex = new Regex(@"[-]{2,}", options);
            //mediaName = regex.Replace(mediaName, @"-");

            string mediaName = ReplaceSpecialChars(MediaName.ToString());

            MediaTypes.MediaTypeInfo mediaTypeInfo = MediaTypes.Instance.GetMediaTypeInfo(MediaType.ToString());

            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
            {
                string sEscapedName = HttpUtility.UrlEncode((mediaName).ToLower(), Encoding.UTF8);
                if (TechnicalConfiguration.Instance.Data.Translation.CharacterReplace != null)
                {
                    foreach (Character replaceChar in TechnicalConfiguration.Instance.Data.Translation.CharacterReplace.CharacterCollection)
                    {
                        sEscapedName = sEscapedName.Replace(replaceChar.OldChar.ToLower(), replaceChar.NewChar);
                    }
                }

                string sMediaType = (TechnicalConfiguration.Instance.Data.Translation.UseTranslatedMediaType) ? TextLocalization.Instance[mediaTypeInfo.TypeName] : mediaTypeInfo.TypeName;

                return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}", sMediaType, sEscapedName, MediaID.ToString()));
            }
            else
            {
                return LinkHelper.ParseURL(string.Format(@"~/{0}?MediaID={1}&MediaType={2}", mediaTypeInfo.PageFilename, MediaID.ToString(), MediaType.ToString()));
            }
        }


        /// <summary>
        /// Get friendly URLs for article page
        /// </summary>
        /// <param name="PageID"></param>
        /// <param name="MediaName"></param>
        /// <returns></returns>
        public static string GetArticlePageURL(object PageID, object MediaName)
        {
            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
            {
                //string mediaName = MediaName.ToString().Replace("/", "%2f").Replace(".", "");
                string mediaName = ReplaceSpecialChars(MediaName.ToString());

                // not allow empty media name is friendlyURL
                if (string.IsNullOrEmpty(mediaName)) mediaName = "NoTitle";
                string sEscapedName = Uri.EscapeDataString(mediaName);

                return LinkHelper.ParseURL(string.Format(@"~/Article/{0}/{1}", sEscapedName, PageID.ToString())).Replace("%20", "-");
            }
            else
            {
                return LinkHelper.ParseURL(string.Format(@"~/Article.aspx?PageID={0}", PageID.ToString()));
            }
        }

        /// <summary>
        /// Get friendly URLs for non-media pages
        /// </summary>
        /// <param name="sOriginalURL"></param>
        /// <returns></returns>
        public static string GetPageURL(string sOriginalURL)
        {
            string sRet = sOriginalURL;
            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
            {
                if (sOriginalURL.ToLower().Contains("dynamicpage.aspx"))
                {
                    long? iPageID = PageData.Instance.GetPageIDFromURL(sOriginalURL);
                    string sPageName = PageData.Instance.GetPageNameFromID(iPageID);
                    string sPageToken = (string.IsNullOrEmpty(TextLocalization.Instance["Dynamic"])) ? "Page" : TextLocalization.Instance["Dynamic"];

                    if (TechnicalConfiguration.Instance.Data.Translation.CharacterReplace != null)
                    {
                        string sEscapedName = Uri.EscapeDataString(sPageName).ToLower();

                        foreach (Character replaceChar in TechnicalConfiguration.Instance.Data.Translation.CharacterReplace.CharacterCollection)
                        {
                            sEscapedName = sEscapedName.Replace(replaceChar.OldChar.ToLower(), replaceChar.NewChar);
                        }

                        return LinkHelper.ParseURL(string.Format("~/{0}/{1}/{2}", sPageToken, sEscapedName, iPageID));

                    }
                    else
                    {
                        return LinkHelper.ParseURL(string.Format("~/{0}/{1}/{2}", sPageToken, sPageName.Replace(' ', '-'), iPageID));

                    }
                }
            }

            return LinkHelper.ParseURL(sRet.ToLower().StartsWith("/") ? "~" + sRet : sRet);
        }


        public static string GetEscapePageUri(string originalURL, bool useCharacterReplace)
        {
            string encodedUrl = Uri.EscapeUriString(originalURL);

            if (useCharacterReplace && TechnicalConfiguration.Instance.Data.Translation.CharacterReplace != null)
            {
                foreach (Character replaceChar in TechnicalConfiguration.Instance.Data.Translation.CharacterReplace.CharacterCollection)
                {
                    encodedUrl = encodedUrl.Replace(replaceChar.OldChar.ToLower(), replaceChar.NewChar);
                }
            }
            return encodedUrl;
        }


        public static bool IsVirtualDirectory()
        {
            return HttpRuntime.AppDomainAppVirtualPath != "/";
        }


        public static bool IsMediaEndDateValid(dsItemInfo.ItemRow itemRow)
        {
            bool result = true;

            if (itemRow["EndPurchaseDate"] != null)
            {
                object mediaEndDate = itemRow["EndPurchaseDate"];
                DateTime endPurchaseDate;
                bool parseResult = DateTime.TryParse(mediaEndDate.ToString(), out endPurchaseDate);
                if (parseResult == true)
                {
                    result = (endPurchaseDate > DateTime.UtcNow || endPurchaseDate == DateTime.MinValue);
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static string ConvertFriendlyURLToNewLang(string url, string locale, string pageToken)
        {
            int iAddSlashes = IsVirtualDirectory() ? 1 : 0;

            string[] urlParts = url.Split('/');
            if (pageToken == TVPPro.SiteManager.Context.Enums.ePages.MediaPage.ToString())
            {
                pageToken = urlParts[3 + iAddSlashes];
                List<LanguageContext> langs = TextLocalization.Instance.GetLanguages();
                foreach (LanguageContext ctx in langs)
                {
                    TextLocalization.Instance.SetActiveLanguageByCulture(ctx.Culture, true);

                    //Try to find in the mediaType                    
                    KeyValuePair<string, MediaTypes.MediaTypeInfo> mType = MediaTypes.Instance.MediaTypesByID.Where(x =>
                        TextLocalization.Instance[x.Value.TypeName].ToLower() == pageToken.ToLower()).FirstOrDefault();
                    if (mType.Key != null)
                    {
                        pageToken = mType.Value.TypeName;
                        break;
                    }
                }
            }

            TextLocalization.Instance.SetActiveLanguageByCulture(locale, true);
            if (pageToken != string.Empty)
                urlParts[3 + iAddSlashes] = TextLocalization.Instance[string.Concat(pageToken, "_page")];

            return string.Join("/", urlParts);
        }

        public static string GetPageTokenByURL(string url)
        {
            string currentCulture = TextLocalization.Instance.UserContext.Culture;
            string pageToken = url.Split('/')[3 + (SiteHelper.IsVirtualDirectory() ? 1 : 0)];
            string foundPage = null;

            if (pageToken == string.Empty)
                return string.Empty;

            foreach (LanguageContext ctx in TextLocalization.Instance.GetLanguages())
            {
                TextLocalization.Instance.SetActiveLanguageByCulture(ctx.Culture, true);

                foreach (string pageName in System.Enum.GetNames(typeof(TVPPro.SiteManager.Context.Enums.ePages)))
                {
                    if (pageToken.ToLower() == TextLocalization.Instance[string.Concat(pageName, "_page")].ToLower())
                    {
                        foundPage = pageName;
                        break;
                    }
                }

                if (foundPage != null)
                    break;
            }

            if (foundPage == null)
                return null;

            return foundPage;
        }

        public static string GetMediaPageUrl(object MediaType, object ID, object MediaName)
        {
            //string mediaName = MediaName.ToString();
            //mediaName = mediaName.Replace("/", "%2f");
            //mediaName = mediaName.Replace(".", "");

            string mediaName = ReplaceSpecialChars(MediaName.ToString());


            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
            {
                return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}", MediaType.ToString(), ID.ToString(), Uri.EscapeDataString(mediaName))).Replace("%20", "+");
            }
            else
            {
                return LinkHelper.ParseURL(string.Format(@"~/MediaPage.aspx?MediaID={0}&MediaType={1}", ID.ToString(), MediaType.ToString()));
            }
        }


        //Linear Channel URL - ToDo : add supoort for friendly url
        public static string GetLinearChannelPageUrl(object groupID, object channelID)
        {
            string retVal = string.Empty;
            retVal = LinkHelper.ParseURL(string.Format(@"~/LinearChannel.aspx?ChannelID={0}&GroupID={1}", channelID.ToString(), groupID.ToString()));
            return retVal;
        }

        public enum eTagType { Tag, Meta, Info };


        //This function is in order to add server side controls for tag links
        public static void AddServerTagLinks(PlaceHolder panelObj, DataTable dtItem, string sName, eTagType eTagType, string Seperator, bool BreakLine, string className)
        {
            string[] arrLinks;
            if (dtItem.Rows.Count > 0 && dtItem.Columns.Contains(sName))
            {
                arrLinks = dtItem.Rows[0][sName].ToString().Split('|');
                for (int i = 0; i < arrLinks.Length; i++)
                {
                    LinkButton linkBtn = new LinkButton();
                    linkBtn.Command += new CommandEventHandler(OnTagClick_Command);

                    if (!Seperator.Equals("<dd>"))
                    {
                        linkBtn.Text = arrLinks[i];
                        linkBtn.CommandArgument = string.Format("{0}={1}", sName, arrLinks[i]);
                        panelObj.Controls.Add(linkBtn);
                    }
                    else
                    {
                        linkBtn.Text = string.Format("<span>{0}</span>", arrLinks[i]);
                        linkBtn.CssClass = className;
                        linkBtn.CommandArgument = string.Format("{0}={1}", sName, arrLinks[i]);
                        Literal startLit = new Literal();
                        startLit.Text = "<dd>";
                        Literal endLit = new Literal();
                        endLit.Text = "</dd>";
                        panelObj.Controls.Add(startLit);
                        panelObj.Controls.Add(linkBtn);
                        panelObj.Controls.Add(endLit);
                    }
                }
            }
            if (BreakLine)
            {
                Literal breakLit = new Literal();
                breakLit.Text = "<br />";
                panelObj.Controls.Add(breakLit);
            }

        }

        protected static void OnTagClick_Command(object sender, CommandEventArgs e)
        {
            string[] strArr = ((string)(e.CommandArgument)).Split('=');
            string value = strArr[1];
            string name = strArr[0];
            string url;
            Dictionary<string, string> dictMetasVar = new Dictionary<string, string>();
            dictMetasVar.Add("Original Name", value.ToString());
            dsItemInfo itemInfo = (new SearchMediaLoader() { Name = value.ToString(), dictMetas = dictMetasVar, CutType = SearchMediaLoader.eCutType.Or, PageSize = 1, WithInfo = true }).Execute();
            if (itemInfo.Item != null && itemInfo.Item.Rows.Count > 0)
            {
                url = SiteHelper.GetMetaPageUrl(itemInfo.Item[0].ID, itemInfo.Item[0].MediaTypeID, itemInfo.Item[0].Title);
            }
            else
            {
                url = SiteHelper.GetLink(value, SiteHelper.eTagType.Tag, name);
            }
            ScriptManager.RegisterClientScriptBlock((Control)sender, sender.GetType(), "RedirectPage", string.Format("RedirectPage('{0}');", url), true);
        }

        public static string GetMetaLinks(DataTable dtItem, string sName, eTagType eTagType, string Seperator)
        {
            string sRet = string.Empty;
            string sLinkTemplate = (!Seperator.Equals("<dd>")) ? "{2}<a href='{0}' target='_blank'>{1}</a>" : "<dd><a href='{0}' target='_blank' class='ubber grey22'><span>{1}</span></a></dd>";

            string[] arrLinks;

            if (dtItem.Rows.Count > 0 && dtItem.Columns.Contains(sName))
            {
                arrLinks = dtItem.Rows[0][sName].ToString().Split('|');

                int i = 0;
                for (i = 0; i < arrLinks.Length; i++)
                {
                    sRet += string.Format(sLinkTemplate, arrLinks[i], arrLinks[i], ((!string.IsNullOrEmpty(sRet)) ? Seperator : string.Empty));
                }
            }

            return sRet;

        }

        public static string GetTagsAndMetasLinks(DataTable dtItem, string sName, eTagType eTagType, string Seperator, bool BreakLine, bool addMoreButton, int tagCount)
        {
            string sRet = string.Empty;
            string sLinkTemplate = (!Seperator.Equals("<dd>")) ? "{2}<a href='{0}'>{1}</a>" : "<dd><a href='{0}' class='ubber grey22'><span>{1}</span></a></dd>";

            string[] arrLinks;

            if (dtItem.Rows.Count > 0 && dtItem.Columns.Contains(sName))
            {
                arrLinks = dtItem.Rows[0][sName].ToString().Split('|');

                int i = 0;
                for (i = 0; i < arrLinks.Length; i++)
                {
                    if (!Seperator.Equals("<dd>"))
                    {
                        sRet += string.Format(sLinkTemplate, GetLink(arrLinks[i], eTagType, sName), arrLinks[i], ((!string.IsNullOrEmpty(sRet)) ? Seperator : string.Empty));
                    }
                    else
                    {
                        if (i == (tagCount + 1))
                        {
                            if (addMoreButton)
                            {
                                sRet += string.Format(@"<span id=""{0}"" style=""display:none;"">", string.Format("{0}span", sName.Replace(' ', '_')));
                            }
                            else
                            {
                                break;
                            }
                        }
                        sRet += string.Format(sLinkTemplate, GetLink(arrLinks[i], eTagType, sName), arrLinks[i]);
                    }
                }

                if (i > tagCount + 1 && Seperator.Equals("<dd>"))
                {
                    if (addMoreButton)
                    {
                        sRet += string.Format(@"</span><dd id=""{0}""><a href=""javascript:void(0)"" style=""color:#666"" class=""s11"" onclick=""ShowHideDivClick('{1}','{0}');document.getElementById('{0}Less').style.display = 'block';"">", string.Format("{0}toggler", sName.Replace(' ', '_')), string.Format("{0}span", sName.Replace(' ', '_'))) + String.Format("<span style=\"color:#ACACAC !important;\" >{0}</span>", TextLocalization.Instance["More"]) + "...</a></dd>";
                        sRet += string.Format(@"<dd id=""{0}Less"" style=""display:none""><a href=""javascript:void(0)"" style=""color:#666"" class=""s11"" onclick=""ShowHideDivClick('{0}','{1}');document.getElementById('{0}Less').style.display='none';"">", string.Format("{0}toggler", sName.Replace(' ', '_')), string.Format("{0}span", sName.Replace(' ', '_'))) + String.Format("<span style=\"color:#ACACAC !important;\" >{0}</span>", TextLocalization.Instance["Less"]) + "...</a></dd>";
                    }
                }
                if (BreakLine)
                    sRet += "<br />";
            }

            return sRet;
        }
        public static string GetTagsAndMetasLinks(DataTable dtItem, string sName, eTagType eTagType, string Seperator, bool BreakLine)
        {
            return GetTagsAndMetasLinks(dtItem, sName, eTagType, Seperator, BreakLine, true, 3);
        }

        public static string GetTagsServerLinks(DataTable dtItem, string sName, string Seperator, bool BreakLine, int mediaType, bool addMoreButton, int tagCount)
        {
            string sRet = string.Empty;

            string sLinkTemplate = (!Seperator.Equals("<dd>")) ? "{2}<a href='{0}' onclick='{2}'>{1}</a>" : "<dd><a href='{0}' class='ubber grey22' onclick='{2}'><span>{1}</span></a></dd>";

            string[] arrLinks;

            if (dtItem.Rows.Count > 0 && dtItem.Columns.Contains(sName))
            {
                arrLinks = dtItem.Rows[0][sName].ToString().Split('|');

                int i = 0;
                for (i = 0; i < arrLinks.Length; i++)
                {
                    string href = "javascript:void(0)";
                    string onclick = string.Format("GetTagURL(\"{0}\", \"{1}\", {2})", sName, arrLinks[i], mediaType);
                    if (!Seperator.Equals("<dd>"))
                    {
                        sRet += string.Format(sLinkTemplate, href, arrLinks[i], ((!string.IsNullOrEmpty(sRet)) ? Seperator : string.Empty), onclick);
                    }
                    else
                    {
                        if (i == (tagCount + 1))
                        {
                            if (addMoreButton)
                            {
                                sRet += string.Format(@"<span id=""{0}"" style=""display:none;"">", string.Format("{0}span", sName.Replace(' ', '_')));
                            }
                            else
                            {
                                break;
                            }
                        }
                        sRet += string.Format(sLinkTemplate, href, arrLinks[i], onclick);
                    }
                }

                if (i > (tagCount + 1) && Seperator.Equals("<dd>"))
                {
                    if (addMoreButton)
                    {
                        sRet += string.Format(@"</span><dd id=""{0}""><a href=""javascript:void(0)"" class=""c6 s11 undr"" onclick=""ShowHideDivClick('{1}','{0}')"">", string.Format("{0}toggler", sName.Replace(' ', '_')), string.Format("{0}span", sName.Replace(' ', '_'))) + TextLocalization.Instance["More"] + "...</a></dd>";
                    }

                }
            }

            if (BreakLine)
                sRet += "<br />";
            return sRet;
        }

        public static string GetTagsServerLinks(DataTable dtItem, string sName, string Seperator, bool BreakLine, int mediaType)
        {
            return GetTagsServerLinks(dtItem, sName, Seperator, BreakLine, mediaType, true, 3);
        }

        public static string GetTagsAndMetasLinks(DataRow dr, string sName, eTagType eTagType, string Seperator, bool BreakLine)
        {
            string sRet = string.Empty;
            string sLinkTemplate = (!Seperator.Equals("<dd>")) ? "{2}<a href='{0}'>{1}</a>" : "<dd><a href='{0}' class='ubber grey22'><span>{1}</span></a></dd>";

            string[] arrLinks;

            arrLinks = dr[sName].ToString().Split('|');
            for (int i = 0; i < arrLinks.Length; i++)
            {
                if (!Seperator.Equals("<dd>"))
                {
                    sRet += string.Format(sLinkTemplate, GetLink(arrLinks[i], eTagType, sName), arrLinks[i], ((!string.IsNullOrEmpty(sRet)) ? Seperator : string.Empty));
                }
                else
                {
                    sRet += string.Format(sLinkTemplate, GetLink(arrLinks[i], eTagType, sName), arrLinks[i]);
                }
            }

            if (BreakLine)
                sRet += "<br />";
            return sRet;
        }

        public static string GetTagsAndMetas(DataTable dtItem, string sName, bool BreakLine)
        {
            StringBuilder sbRet = new StringBuilder();

            if (dtItem.Columns.Contains(sName))
            {
                sbRet.Append(dtItem.Rows[0][sName].ToString());

                if (BreakLine)
                {
                    sbRet.Append("<br />");
                }
            }
            return sbRet.ToString();
        }

        public static string GetLink(object value, eTagType linkType, string sName)
        {
            return GetLink(value, linkType, sName, false);
        }

        public static string GetLink(object value, eTagType linkType, string sName, bool fromService)
        {
            value = value.ToString().Replace("'", "%27");

            if (!fromService)
            {
                string redirect = MediaMappingHelper.Instance[sName];
                if (!String.IsNullOrEmpty(redirect))
                {
                    string redirectLink = string.Format(@"javascript:GetTagURL(""{0}"", ""{1}"", ""{2}"")", sName, value.ToString(), redirect);
                    return redirectLink;
                }
            }

            if (linkType == eTagType.Meta)
                return SiteHelper.GetSearchPageUrl(HttpUtility.UrlEncode(value.ToString()), Enums.eSearchType.ByKeyword);
            else
            {
                string url = string.Format("{0}={1}", sName, value.ToString());
                //string url = string.Format("{0}={1}", sName, HttpEncodeEverythingButThoseStrings(value.ToString(), new string[] { " " }));
                return SiteHelper.GetSearchPageUrl(url, Enums.eSearchType.ByTag, true);
            }
        }

        public static string GetTagURL(string tagType, string value, int mediaType)
        {
            Dictionary<string, string> dictMetasVar = new Dictionary<string, string>();
            dictMetasVar.Add(tagType, value.ToString());
            TVMAccountType account = PageData.Instance.GetTVMAccountByMediaType(mediaType);

            if (tagType == "author")
            {
                dsItemInfo itemInfo = (new SearchMediaLoader(account.TVMUser, account.TVMPass) { Name = value.ToString(), CutType = SearchMediaLoader.eCutType.Or, PageSize = 1, WithInfo = true }).Execute();
                if (itemInfo.Item != null && itemInfo.Item.Rows.Count > 0)
                {
                    return SiteHelper.GetPageURL(itemInfo.Item[0].MediaTypeID, itemInfo.Item[0].ID, itemInfo.Item[0].Title);
                }
            }
            return SiteHelper.GetLink(value, SiteHelper.eTagType.Tag, tagType);
        }


        //ToDo - remove this function and use only the overloaded function once implementing DB changes in Projector
        public static string GetShowPageUrl(object ShowNameItem, object SeasonNumberItem)
        {
            if (SeasonNumberItem != null)
            {
                if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
                {
                    return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}", "Show", SeasonNumberItem.ToString(), Uri.EscapeDataString(ShowNameItem.ToString()))).Replace("%20", "+");
                }
                else
                {
                    return LinkHelper.ParseURL(string.Format(@"~/ShowPage.aspx?ShowName={0}&Season={1}", ShowNameItem.ToString(), SeasonNumberItem.ToString()));
                }
            }
            else
            {
                if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
                {
                    return LinkHelper.ParseURL(string.Format(@"~/{0}/0/{1}", "Show", Uri.EscapeDataString(ShowNameItem.ToString()))).Replace("%20", "+");
                }
                else
                {
                    return LinkHelper.ParseURL(string.Format(@"~/ShowPage.aspx?ShowName={0}", ShowNameItem.ToString()));
                }

            }
        }

        public static string GetShowPageUrl(object ChannelID, object GroupID, object MediaID, object TvmMediaID)
        {
            if (GroupID != null)
            {
                string groupID = GroupID.ToString();

                TVMAccountType account = PageData.Instance.GetTVMAccountByMediaType(int.Parse(TvmMediaID.ToString()));
                groupID = account.BaseGroupID.ToString();

                if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
                {
                    return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}/{3}", "Show", ChannelID.ToString(), groupID.ToString(), MediaID.ToString())).Replace("%20", "+");
                }
                else
                {
                    return LinkHelper.ParseURL(string.Format(@"~/ShowPage.aspx?ChannelID={0}&GroupID={1}&MediaID={2}&MediaType={3}", ChannelID.ToString(), groupID.ToString(), MediaID.ToString(), TvmMediaID.ToString()));
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetShowPageUrl(object ChannelID, object GroupID, object MediaID, object TvmMediaID, object seasonNum)
        {
            if (GroupID != null)
            {
                string groupID = GroupID.ToString();

                TVMAccountType account = PageData.Instance.GetTVMAccountByMediaType(int.Parse(TvmMediaID.ToString()));
                groupID = account.BaseGroupID.ToString();

                if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
                {
                    return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}/{3}/{4}", "Show", ChannelID.ToString(), groupID.ToString(), seasonNum.ToString(), MediaID.ToString())).Replace("%20", "+");
                }
                else
                {
                    return LinkHelper.ParseURL(string.Format(@"~/ShowPage.aspx?ChannelID={0}&GroupID={1}&MediaID={2}&Season={3}", ChannelID.ToString(), groupID.ToString(), MediaID.ToString(), seasonNum.ToString()));
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetShowPageUrl(object Type, object ID, object MediaName)
        {
            //string mediaName = MediaName.ToString();
            //mediaName = mediaName.Replace("/", "%2f");
            //mediaName = mediaName.Replace(".", "");

            string mediaName = ReplaceSpecialChars(MediaName.ToString());

            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature)
            {
                return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}", Type.ToString(), ID.ToString(), Uri.EscapeDataString(mediaName))).Replace("%20", "+");
            }
            else
            {
                return LinkHelper.ParseURL(string.Format(@"~/ShowPage.aspx?MediaID={0}&MediaType={1}", ID.ToString(), Type.ToString()));
            }
        }

        public static string GetDynamicPageURL(object pageID)
        {
            return QueryStringHelper.CreateQueryString("~/DynamicPage.aspx", new QueryStringPair("PageID", pageID.ToString()));
        }

        public static string GetMediaListPageUrl()
        {
            return LinkHelper.ParseURL(@"~/MediaList.aspx");
        }

        public static string GetMyZonePageUrl()
        {
            return LinkHelper.ParseURL(@"~/MyZone.aspx");
        }

        public static string GetSearchPageUrl(object Item, Enums.eSearchType SearchType)
        {
            return GetSearchPageUrl(Item, SearchType, false);
        }

        public static string GetSearchPageUrl(object Item, Enums.eSearchType SearchType, bool urlEncoded = false, bool withFrindlyURL = true)
        {
            if (TVPPro.Configuration.Site.SiteConfiguration.Instance.Data.Features.FriendlyURL.SupportFeature && withFrindlyURL)
            {
                string sPageToken = (TextLocalization.Instance["SearchPage"].Contains("{")) ? "Search" : TextLocalization.Instance["SearchPage"];
                string sEscapedName = urlEncoded ? Item.ToString() : Uri.EscapeDataString(Item.ToString());

                if (SearchType.Equals(Enums.eSearchType.ByTag))
                {
                    return LinkHelper.ParseURL(string.Concat(@"~/", sPageToken, "/", sEscapedName.Replace(".", "||~~").Replace("*", "|||~~").Replace("&", "||||~~").Replace("%", "|||||~~")));
                }
                else if (SearchType.Equals(Enums.eSearchType.ByType))
                {
                    return string.Concat(LinkHelper.ParseURL(string.Concat(@"~/", sPageToken, "/Type=")), sEscapedName.Replace(".", "||~~").Replace("*", "|||~~").Replace("&", "||||~~").Replace("%", "|||||~~"));
                }
                else
                {
                    return string.Concat(LinkHelper.ParseURL(string.Concat(@"~/", sPageToken, "/")), sEscapedName.Replace(".", "||~~").Replace("*", "|||~~").Replace("&", "||||~~").Replace("%", "|||||~~"));
                }
            }
            else
            {
                if (SearchType.Equals(Enums.eSearchType.ByTag))
                {
                    return string.Concat(LinkHelper.ParseURL(@"~/Search.aspx"), "?SearchType=ByTag&TagPairs=", Item.ToString());
                }
                else if (SearchType.Equals(Enums.eSearchType.ByType))
                {
                    return string.Concat(LinkHelper.ParseURL(@"~/Search.aspx"), "?Type=", Item.ToString());
                }
                else
                {
                    String retURL = String.Empty;

                    retURL += LinkHelper.ParseURL(@"~/Search.aspx");

                    return string.Concat(LinkHelper.ParseURL(@"~/Search.aspx"), String.Format("{0}Search=", retURL.LastIndexOf("?") != -1 ? "&" : "?"), Item.ToString());
                }
            }
        }

        public static string GetPackageUrl(object PackageItem)
        {
            return string.Concat(LinkHelper.ParseURL(@"~/Package.aspx"), "?name=", PackageItem.ToString(), "&type=Package");
        }

        public static string GetPackageUrl(object MediaID, object GroupID, object BaseID)
        {
            return LinkHelper.ParseURL(string.Format(@"~/Package.aspx?MediaID={0}&GroupID={1}&BaseID={2}", MediaID.ToString(), GroupID.ToString(), BaseID.ToString()));
        }

        public static string GetMetaPageUrl(object MediaID, object MediaTypeID, object MediaTitle)
        {
            return LinkHelper.ParseURL(string.Format(@"~/MetaPage.aspx?MediaID={0}&MediaType={1}", MediaID.ToString(), MediaTypeID.ToString()));
        }
        public static string GetMetaPageUrl(object MediaID, string tag, string value)
        {
            return LinkHelper.ParseURL(string.Format(@"~/{0}/{1}/{2}", tag.ToString(), value.ToString(), MediaID.ToString()));
        }

        public static string GetFriendlyUrl(string sMediaType, string sID, string sName)
        {
            int iTest;
            // in case sMediaType is a string name
            if (!int.TryParse(sMediaType, out iTest))
            {
                return (!String.IsNullOrEmpty(sMediaType) && sMediaType.Equals("Show")) ?
                    String.Format("~/ShowPage.aspx?ShowName={0}&Season={1}", sName, sID) :
                    String.Concat("~/MediaPage.aspx?MediaID=", sID);
            }
            // in case sMediaType is a string number
            else
            {
                MediasContentTypeLoader MediasContentType = new MediasContentTypeLoader();
                TVPPro.SiteManager.DataEntities.dsMediaTypes m_Types = MediasContentType.Execute();

                string sTypeName = string.Empty;
                try
                {
                    sTypeName = (from r in m_Types.MediaTypes where r["TvmTypeID"].ToString() == sMediaType select r).First()["TvmTypeID"].ToString();
                }
                catch (Exception)
                {

                }

                //switch (sTypeName)
                //{

                //}
                return (sTypeName.ToLower().Equals("show")) ? String.Format("~/ShowPage.aspx?MediaID={0}&MediaType={1}", sID, sMediaType) :
                    (sTypeName.ToLower().Equals("stars")) ? String.Format("~/MetaPage.aspx?MediaID={0}&MediaType={1}", sID, sMediaType) :
                    (sTypeName.ToLower().Equals("package")) ? String.Format("~/Package.aspx?MediaID={0}&MediaType={1}", sID, sMediaType) :
                     String.Format("~/MediaPage.aspx?MediaID={0}&MediaType={1}", sID, sMediaType);
            }
        }

        #endregion

        public static string GerMediaPrice(object Item)
        {
            return "-----";
        }

        public static string GetMediaRating(object Item)
        {
            double Rate = 0;
            double.TryParse(Item.ToString(), out Rate);

            Rate = Math.Round(Rate);

            switch (int.Parse(Rate.ToString()))
            {
                case 0:
                    return "star5";
                case 1:
                    return "star4";
                case 2:
                    return "star3";
                case 3:
                    return "star2";
                case 4:
                    return "star1";
                case 5:
                    return "star0";
                default:
                    return "star0";
            }
        }

        public static string GetMediaDuration(object Item)
        {
            string Duration = Item.ToString();

            return Duration;
        }

        public static string GetSimpleCutString(string theOriginalString, string subForExtraText, int maxLength)
        {
            if (subForExtraText.Length > maxLength)
            {
                return theOriginalString;
            }

            if (theOriginalString.Length > maxLength)
            {
                if (theOriginalString.Length + subForExtraText.Length > maxLength)
                {
                    return string.Format("{0}{1}", theOriginalString.Substring(0, maxLength - subForExtraText.Length), subForExtraText);
                }
            }

            return theOriginalString;
        }

        public static string GetCutString(object theOriginalString, float theFontSize, int theMaxSize)
        {
            return GetCutString(theOriginalString, SiteConfiguration.Instance.Data.Global.DefaultSiteFont, theFontSize, theMaxSize);
        }

        public static string GetCutString(object theOriginalString, string theFontName, float theFontSize, int theMaxSize)
        {
            if (theOriginalString == null || string.IsNullOrEmpty(theOriginalString.ToString()))
                return string.Empty;

            string orig = theOriginalString.ToString();
            string dots = "...";

            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.PageUnit = GraphicsUnit.Pixel;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    using (Font fnt = new Font(theFontName, theFontSize, GraphicsUnit.Pixel))
                    {


                        StringFormat format = new StringFormat(StringFormat.GenericTypographic);
                        format.Trimming = StringTrimming.Word;

                        SizeF dotsSize = g.MeasureString(dots, fnt, 1000, format);

                        if (g.MeasureString(orig, fnt, 10000, format).Width <= theMaxSize)
                            return orig;

                        for (int i = 0; i < orig.Length + 1; i++)
                        {
                            if (g.MeasureString(orig.Substring(0, i), fnt, 10000, format).Width >= theMaxSize)
                            {
                                for (int j = i - 1; j >= 0; j--)
                                {
                                    if (g.MeasureString(orig.Substring(0, j), fnt, 10000, format).Width + dotsSize.Width <= theMaxSize)
                                    {
                                        return orig.Substring(0, j).Trim() + dots;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return orig;
        }

        public static string GetItemRowNumber(int itemNumber)
        {
            return itemNumber >= 10 ? itemNumber.ToString() : string.Concat("0", itemNumber.ToString());
        }

        public static string GetImageLink(object baseLink, int width, int height)
        {
            return GetImageLink(baseLink, width, height, false);
        }

        public static string GetImageLink(object baseLink, int width, int height, bool RelativeToClient)
        {
            if (baseLink == null || !(baseLink is string))
                return string.Empty;

            string baseURL = "~";
            string ImageToLoad = string.Format("{0}_{1}x{2}{3}", Path.GetFileNameWithoutExtension((string)baseLink), width, height, Path.GetExtension((string)baseLink));

            if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.Site.BasePictureUrl))
            {
                return LinkHelper.ParseURL(string.Format("{0}/{1}", TechnicalConfiguration.Instance.Data.Site.BasePictureUrl, ImageToLoad));
            }
            else if (RelativeToClient)
            {
                string Identifier = ConfigurationManager.AppSettings["ClientIdentifier"];
                return LinkHelper.ParseURL(string.Format("{0}/Clients/{1}/Pics/{2}", baseURL, Identifier, ImageToLoad));
            }
            else
            {
                return LinkHelper.ParseURL(string.Format("{0}/pics/{1}", baseURL, ImageToLoad));
            }
        }

        public static DataTable GetSeperatedDataTable(Object dtSource, String sByColumnName, char cSeperator)
        {
            DataTable dtRet = new DataTable(String.Concat("dt", sByColumnName));

            if (dtSource is DataTable)
            {
                dtRet.Columns.Add(sByColumnName, typeof(String));

                if ((dtSource as DataTable).Columns.Contains(sByColumnName) && (dtSource as DataTable).Rows.Count > 0)
                {
                    String sColumnValue = (dtSource as DataTable).Rows[0][sByColumnName].ToString();
                    if (sColumnValue.Contains(cSeperator))
                    {
                        String[] arrValues = sColumnValue.Split(cSeperator);
                        foreach (String sValue in arrValues)
                        {
                            DataRow rowNew = dtRet.NewRow();
                            rowNew[sByColumnName] = sValue;
                            dtRet.Rows.Add(rowNew);
                        }
                    }
                    else
                    {
                        DataRow rowNew = dtRet.NewRow();
                        rowNew[sByColumnName] = sColumnValue;
                        dtRet.Rows.Add(rowNew);
                    }
                }
            }
            return dtRet;
        }

        public static string FormatButtomTitleByType(ref ContentPartItem<DataRow> Container, Enums.eGalleryType GalleryType, string EpisodeNumberTag, string SeasonNumberTag, string ShowNameTag)
        {
            StringBuilder FormatedString = new StringBuilder();
            switch (GalleryType)
            {
                case Enums.eGalleryType.Episode:
                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)) > 0)
                    {
                        FormatedString.Append(string.Format("{1}{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag), TextLocalization.Instance["SeriesShort"]));
                        if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) > 0)
                        {
                            FormatedString.Append(", ");
                        }
                    }
                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) > 0)
                    {
                        FormatedString.Append(string.Format("{1}{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag), TextLocalization.Instance["EpisodeShort"]));
                    }
                    break;
                case Enums.eGalleryType.Show:
                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", ShowNameTag)))
                    {
                        FormatedString.Append(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", ShowNameTag));
                    }

                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)))
                    {
                        if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", ShowNameTag)))
                        {
                            FormatedString.Append("/");
                        }

                        FormatedString.Append(string.Format("{1}{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag), TextLocalization.Instance["SeriesShort"]));
                    }

                    break;
            }

            return FormatedString.ToString();
        }

        public static string GetTitleByTypeListGallery(ref ContentPartItem<DataRow> Container, Enums.eGalleryType GalleryType, string EpisodeNumberTag, string SeasonNumberTag, string ShowNameTag)
        {
            StringBuilder sb = new StringBuilder();
            switch (GalleryType)
            {
                case Enums.eGalleryType.Episode:
                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Tags", ShowNameTag)))
                    {
                        sb.Append(DataHelper.GetValueFromRelation(ref Container, "Item_Tags", ShowNameTag));
                    }
                    if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)) || !string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)))
                    {
                        sb.Append("/ ");
                        if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)))
                        {
                            sb.Append(string.Format("SE.{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)));
                        }
                        if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)))
                        {
                            sb.Append(string.Format(" EP.{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)));
                        }
                    }
                    break;
            }

            return sb.ToString();
        }

        public static string GetFriendlyURLCacheKey(String RequestParts)
        {
            return "FRIENDLY_URL_404_" + RequestParts;
        }

        public static string GetAffiliateValue(string CookieName, string CookieValue)
        {
            string AffiliateVal = string.Empty;

            //if the affiliate value exist on the session take it from session otherwise take it from cookie
            if (!string.IsNullOrEmpty(SessionHelper.Affiliate))
            {
                AffiliateVal = SessionHelper.Affiliate;
            }
            else if (CookiesHelper.Enabled())
            {
                CookiesHelper cookie = new CookiesHelper(CookieName);
                if (!string.IsNullOrEmpty(cookie.GetValue(CookieValue)))
                {
                    AffiliateVal = cookie.GetValue(CookieValue);
                }
            }
            return AffiliateVal;
        }

        public static bool MediaIsNew(string sMediaTime, double iDays)
        {
            bool bRet = false;

            try
            {
                if (!string.IsNullOrEmpty(sMediaTime))
                {
                    bRet = DateTime.Now.CompareTo(DateTime.Parse(sMediaTime).AddDays(iDays)) < 0;
                }
            }
            catch (Exception)
            {

            }

            return bRet;
        }

        public static bool TryLoadXML(string FileURL, out XmlDocument XMLDoc)
        {
            bool bRet = false;
            XMLDoc = new XmlDocument();

            try
            {
                XMLDoc.Load(HttpContext.Current.Server.MapPath(FileURL));

                bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }

            return bRet;
        }

        static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader)
        {
            //Create the HTTP POST request and the authentication headers
            HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
            oWebRequest.Method = "post";
            oWebRequest.ContentType = "text/xml; charset=utf-8";
            //oWebRequest.Headers["SOAPAction"] = sSoapHeader;

            byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
            //oWebRequest.ContentLength = encodedBytes.Length;
            //oWebRequest.AllowWriteStreamBuffering = true;

            //Send the request.
            Stream requestStream = oWebRequest.GetRequestStream();
            requestStream.Write(encodedBytes, 0, encodedBytes.Length);
            requestStream.Close();

            try
            {
                HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                HttpStatusCode sCode = oWebResponse.StatusCode;
                Stream receiveStream = oWebResponse.GetResponseStream();

                StreamReader sr = new StreamReader(receiveStream);
                string resultString = sr.ReadToEnd();

                sr.Close();
                oWebRequest = null;
                oWebResponse = null;
                return resultString;
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                WebResponse errRsp = ex.Response;
                using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
                {
                    return rdr.ReadToEnd();
                }
            }
        }

        //public static string GetFlashLink(object baseLink)
        //{
        //    // Check if a custom pictures path is given
        //    string flashPath = "~/upload/flash/";
        //    if (!string.IsNullOrEmpty(TechnicalConfiguration.Instance.Data.ManualCustomization.CustomFlashPath))
        //    {
        //        flashPath = TechnicalConfiguration.Instance.Data.ManualCustomization.CustomFlashPath;
        //        if (!flashPath.EndsWith("/"))
        //            flashPath += "/";
        //    }

        //    return LinkHelper.ParseURL(string.Format("{0}{1}", flashPath, Path.GetFileNameWithoutExtension(baseLink.ToString())));
        //}

        public static string FormatEpisodeButtomTitle(ref ContentPartItem<DataRow> Container, string EpisodeNumberTag, string SeasonNumberTag, string ShowNameTag)
        {
            StringBuilder FormatedString = new StringBuilder();

            if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Tags", ShowNameTag)))
            {
                FormatedString.Append(DataHelper.GetValueFromRelation(ref Container, "Item_Tags", ShowNameTag));
                FormatedString.Append(", ");
            }

            if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag)) > 0)
            {
                FormatedString.Append(string.Format("{1}{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", SeasonNumberTag), TextLocalization.Instance["SeriesShort"]));
                if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) > 0)
                {
                    FormatedString.Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) && int.Parse(DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag)) > 0)
            {
                FormatedString.Append(string.Format("{1}{0}", DataHelper.GetValueFromRelation(ref Container, "Item_Metas", EpisodeNumberTag), TextLocalization.Instance["EpisodeShort"]));
            }

            return FormatedString.ToString();
        }

        public static string HtmlEncode(string Text)
        {
            Text = Text.Replace("'", "%27");

            return Text;
        }

        public static string GetPageUrlWithAncor(object MediaType, object MediaID, object MediaName, string AncorName)
        {
            if (!string.IsNullOrEmpty(AncorName))
                return string.Format("{0}{1}{2}", GetPageURL(MediaType, MediaID, MediaName), "#", AncorName);
            else
                return GetPageURL(MediaType, MediaID, MediaName);
        }

        public static string GetTagReturnHtml(ContentPartItem<DataRow> Container, string tagName, int valueCount, string OpeningThemeName, bool ShowOpeningThemeNameIfStringEmpty, string Seperator, string SeperatorStyle, Enums.eLinkType linkType, bool WithName)
        {
            StringBuilder sb = new StringBuilder();
            string fullStr = string.Empty;
            if (linkType == Enums.eLinkType.Meta)
            {
                fullStr = DataHelper.GetValueFromRelation(ref Container, "Item_Metas", tagName);
            }
            else
            {
                fullStr = DataHelper.GetValueFromRelation(ref Container, "Item_Tags", tagName);
            }

            if (!string.IsNullOrEmpty(fullStr))
            {
                if (WithName)
                {
                    sb = sb.AppendFormat("{0}{1}", OpeningThemeName, ": ");
                }
                string[] strArr = fullStr.Split('|');
                int itemCount = 0;

                foreach (string str in strArr)
                {
                    if (itemCount < valueCount)
                    {
                        switch (Seperator)
                        {
                            case "a":
                                string SeperatorLink;
                                if (linkType == Enums.eLinkType.Meta)
                                {
                                    SeperatorLink = SiteHelper.GetSearchPageUrl(HttpUtility.UrlEncode(str), Enums.eSearchType.ByKeyword);
                                }
                                else
                                {
                                    string url = string.Format("{0}={1}", tagName.Replace(' ', '+'), HttpUtility.UrlEncode(str.ToString()));
                                    SeperatorLink = SiteHelper.GetSearchPageUrl(url, Enums.eSearchType.ByTag, false, false);
                                }

                                sb.Append(string.Format("<a href=\"{0}\" class=\"{1}\"><span>{2}</span></a>", SeperatorLink, SeperatorStyle, str));

                                break;
                            default:
                                break;
                        }
                        itemCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (ShowOpeningThemeNameIfStringEmpty)
                {
                    sb = sb.Append(OpeningThemeName);
                }
                sb = sb.Append(fullStr);
            }
            return sb.ToString();
        }

        public static string GetToolTipTagReturnHtml(ContentPartItem<DataRow> Container, string tagName, int valueCount, string OpeningThemeName, bool ShowOpeningThemeNameIfStringEmpty, string Seperator, string SeperatorStyle, Enums.eLinkType linkType, bool WithName)
        {
            StringBuilder sb = new StringBuilder();
            string fullStr = string.Empty;
            string strHelper;
            if (linkType == Enums.eLinkType.Meta)
            {
                fullStr = DataHelper.GetValueFromRelation(ref Container, "Item_Metas", tagName);
            }
            else
            {
                fullStr = DataHelper.GetValueFromRelation(ref Container, "Item_Tags", tagName);
            }

            if (!string.IsNullOrEmpty(fullStr))
            {
                if (WithName)
                {
                    sb = sb.AppendFormat("{0}{1}", OpeningThemeName, ": ");
                }
                string[] strArr = fullStr.Split('|');
                int itemCount = 0;
                foreach (string str in strArr)
                {
                    if (itemCount < valueCount)
                    {
                        switch (Seperator)
                        {
                            case "a":
                                string SeperatorLink;
                                if (linkType == Enums.eLinkType.Meta)
                                {
                                    SeperatorLink = SiteHelper.GetSearchPageUrl(HttpUtility.UrlEncode(str), Enums.eSearchType.ByKeyword);
                                }
                                else
                                {
                                    string url = string.Format("{0}={1}", tagName.Replace(' ', '+'), HttpUtility.UrlEncode(str.ToString()));
                                    SeperatorLink = SiteHelper.GetSearchPageUrl(url, Enums.eSearchType.ByTag);
                                }

                                if (itemCount != 0)
                                    strHelper = string.Concat(", ", str);
                                else
                                    strHelper = str;

                                sb.Append(string.Format("<a href=\"{0}\" class=\"{1}\"><span>{2}</span></a>", SeperatorLink, SeperatorStyle, strHelper));

                                break;
                            default:
                                break;
                        }
                        itemCount++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (ShowOpeningThemeNameIfStringEmpty)
                {
                    sb = sb.Append(OpeningThemeName);
                }
                sb = sb.Append(fullStr);
            }
            return sb.ToString();
        }

        public static string GetClientIP()
        {
            string ip = string.Empty;
            string retIp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string[] ipRange;
            if (!string.IsNullOrEmpty(retIp) && (ipRange = retIp.Split(',')) != null && ipRange.Length > 0)
            {
                ip = ipRange[0];
            }
            else
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            if (ip.Equals("127.0.0.1") || ip.Equals("::1") || ip.StartsWith("192.168.")) ip = "81.218.199.175";

            if (ip.Contains(':'))
            {
                ip.Substring(0, ip.IndexOf(':'));
            }

            return ip.Trim();
        }

        public static string FormatApostrophe(string str)
        {
            if (str.Contains("'"))
                return str.Replace("'", "\\'");
            return str;

        }

        public static string SubstringReplacer(string str, Dictionary<string, string> dictSubstringsToBeRepalced)
        {
            // key in dictionary = the substring that needs to be replaced with its corresponding value in dict.
            foreach (KeyValuePair<string, string> pair in dictSubstringsToBeRepalced)
                if (str.Contains(pair.Key))
                    str = str.Replace(pair.Key, pair.Value);
            return str;
        }

        public static string GetFacebookOgImage(dsItemInfo mediaInfo)
        {
            return GetFacebookOgImage(mediaInfo, "16:9");
        }

        public static string GetFacebookOgImage(dsItemInfo mediaInfo, string dim)
        {
            string result = string.Empty;

            //Find the smallest picture size that is still larger than 200X200
            try
            {
                var picRows = mediaInfo.Item[0].GetPicturesRows();
                if (picRows != null && picRows.Length > 0)
                {
                    var picRow = (from p in picRows
                                  let dimentions = p.PicSize.Split('X', 'x')
                                  let width = int.Parse(dimentions[0])
                                  let height = int.Parse(dimentions[1])
                                  where width >= FBMINSIZEX && height >= FBMINSIZEY
                                  orderby dim == "2:3" ? width : height
                                  select p).FirstOrDefault();

                    if (picRow != null)
                    {
                        result = picRow.URL;
                    }
                }
            }
            catch (Exception)
            {
                // TODO : log !!
            }

            return result;
        }

        public static string DecodeUrlSpecialChars(string sStrToDecode, IEnumerable<string> oCollectionOfSpecialChars)
        {
            string res = sStrToDecode;
            foreach (string specialChar in oCollectionOfSpecialChars)
            {
                string specialCharEncoded = HttpUtility.UrlEncode(specialChar);
                if (!string.IsNullOrEmpty(specialCharEncoded))
                    res = res.Replace(specialCharEncoded, specialChar);
            }
            return res;
        }

        public static string ReplaceFirstCharOccurenceBeforeChar(string sStr, char cOldChar, char cNewChar, char cBeforeChar)
        {
            int i = -1;
            int indexOfBeforeChar = sStr.IndexOf(cBeforeChar);
            if(indexOfBeforeChar > -1)
                for (i = 0; i < indexOfBeforeChar; i++)
                    if (sStr[i] == cOldChar)
                        break;
            if (i > -1 && i < indexOfBeforeChar)
                return sStr.Substring(0, i) + cNewChar.ToString() + sStr.Substring(i + 1);
            return sStr;
        }

        public static string HttpEncodeEverythingButThoseStrings(string sStr, IEnumerable<string> oCollectionOfSpecialChars)
        {
            string res = HttpUtility.UrlEncode(sStr);
            foreach (string s in oCollectionOfSpecialChars)
            {
                string encodedStr = HttpUtility.UrlEncode(s);
                res = res.Replace(encodedStr, s);
            }
            return res;
        }


        public static string ReplaceSpecialChars(string str)
        {
            StringBuilder unicodeChars = new StringBuilder();

            foreach (char c in UnicodeChars(str))
            {
                unicodeChars.Append(c);
            }
            
            string sWithoutSpecialCharacters = Regex.Replace(str.Replace(" ","-"), string.Format(@"[^a-zA-Z0-9-{0}]", unicodeChars.ToString()), string.Empty);
            string replaceAllDashSequenceWithASingleDash = Regex.Replace(sWithoutSpecialCharacters, @"[-]+(?=[^-])?", "-");

            string result = replaceAllDashSequenceWithASingleDash.Normalize(NormalizationForm.FormD);

            return result;
        }

        private static IEnumerable<char> UnicodeChars(string str)
        {
            foreach (char c in str)
            {
                if (c > 127)
                {
                    yield return c;
                }
            }
         }
    }
}