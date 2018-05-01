using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using System.Collections.Generic;
using TVPPro.SiteManager.Manager;
using KLogMonitor;
using System.Reflection;


namespace TVPApi
{
    public class PageData
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PageData(int groupID, PlatformType platform)
        {
            Init(groupID, platform);
        }

        private PageData()
        {
        }

        #region Fields

        private const string REQUEST_PAGE_KEY = "REQUESTPAGEKEY";

        private Dictionary<string, Dictionary<long, PageContext>> m_LanguageIDPages;
        private Dictionary<string, Dictionary<string, PageContext>> m_LanguageTokenPages;

        private dsPageData DataOnPage;

        public dsPageData SiteGallries
        {
            get
            {
                return DataOnPage;
            }
        }

        private PageContext pageContext
        {
            get
            {
                return HttpContext.Current.Items[REQUEST_PAGE_KEY] as PageContext;
            }
            set
            {
                HttpContext.Current.Items[REQUEST_PAGE_KEY] = value;
            }
        }
        #endregion

        #region Public Properties

        private void AddLocalesToGallery(PageGallery pg, dsPageData.GalleryLocalesRow[] localesArr)
        {
            if (localesArr != null && localesArr.Count() > 0)
            {
                foreach (dsPageData.GalleryLocalesRow localeRow in localesArr)
                {
                    if (!localeRow.IsCountryNull())
                    {
                        if (pg.Locale_Countrys == null)
                        {
                            pg.Locale_Countrys = new List<string>();
                        }
                        pg.Locale_Countrys.Add(localeRow.Country);
                    }
                    if (!localeRow.IsDeviceNull())
                    {
                        if (pg.Locale_Devices == null)
                        {
                            pg.Locale_Devices = new List<string>();
                        }

                        pg.Locale_Devices.Add(localeRow.Device);
                    }
                    if (!localeRow.IsLanguageNull())
                    {
                        if (pg.Locale_Langs == null)
                        {
                            pg.Locale_Langs = new List<string>();
                        }
                        pg.Locale_Langs.Add(localeRow.Language);
                    }
                    if (!localeRow.IsUserStateNull())
                    {
                        if (pg.Locale_UserStates == null)
                        {
                            pg.Locale_UserStates = new List<long>();
                        }
                        pg.Locale_UserStates.Add(localeRow.UserState);
                    }
                }
            }
        }

        public List<PageGallery> GetLocaleGalleries(List<PageGallery> PageGalleryList, Locale locale)
        {

            //Group the galleries by family num
            Dictionary<long, List<PageGallery>> galleriesByFamilies =
                (from p in PageGalleryList
                 group p by p.FamilyID).ToDictionary(gr => gr.Key, gr => gr.ToList());

            List<PageGallery> galleryList = new List<PageGallery>();
            LocaleUserState localeUserState = locale.LocaleUserState;

            //All the galleries with default family id
            if (galleriesByFamilies.ContainsKey(0))
            {
                foreach (PageGallery pg in galleriesByFamilies[0])
                {
                    if (IsGalleryInLocale(pg, locale, localeUserState))
                    {
                        galleryList.Add(pg);
                    }
                }
            }

            //for each family group get the highest scoring gallery (except family id 0 which we already handled)
            foreach (KeyValuePair<long, List<PageGallery>> pair in galleriesByFamilies)
            {
                if (pair.Key != 0)
                {
                    PageGallery hsGallery = GetPageGalleryByLocaleScore(pair.Value, locale, localeUserState);
                    if (hsGallery != null)
                    {
                        galleryList.Add(hsGallery);
                    }
                }
            }

            return galleryList;
        }

        //Check if a gallery fits the locale
        private bool IsGalleryInLocale(PageGallery pg, Locale locale, LocaleUserState localeUserState)
        {
            if (pg.Locale_Langs != null && !string.IsNullOrEmpty(locale.LocaleLanguage) && !pg.Locale_Langs.Contains(locale.LocaleLanguage))
                return false;

            if (pg.Locale_Devices != null && !string.IsNullOrEmpty(locale.LocaleDevice) && !pg.Locale_Devices.Contains(locale.LocaleDevice))
                return false;

            if (pg.Locale_Countrys != null && !string.IsNullOrEmpty(locale.LocaleCountry) && !pg.Locale_Countrys.Contains(locale.LocaleCountry))
                return false;

            if (pg.Locale_UserStates != null && localeUserState != LocaleUserState.Unknown && !pg.Locale_UserStates.Contains((long)localeUserState))
                return false;

            return true;
        }

        //Get the gallery that most matches the user's locale. The locale attributes are prioreterized 
        //From high to low : Device -> Country -> Language -> User State
        private PageGallery GetPageGalleryByLocaleScore(List<PageGallery> galleries, Locale locale, LocaleUserState localeUserState)
        {
            PageGallery retVal = null;
            double prevScore = 0;
            string localeLanguage = locale.LocaleLanguage;
            string localeCountry = locale.LocaleCountry;
            string localeDevice = locale.LocaleDevice;

            //Country userCountry = TVPPro.SiteManager.Services.UsersService.Instance.GetIPToCountry(ip);    
            foreach (PageGallery gallery in galleries)
            {
                double currentScore = 0;
                if (gallery.Locale_UserStates != null)
                {
                    if (gallery.Locale_UserStates.Contains((int)localeUserState))
                        currentScore += Math.Pow(2, 0);
                    else
                        continue;
                }

                if (gallery.Locale_Langs != null)
                {
                    if (gallery.Locale_Langs.Contains(localeLanguage))
                        currentScore += Math.Pow(2, 1);
                    else
                        continue;
                }

                if (gallery.Locale_Countrys != null)
                {
                    if (gallery.Locale_Countrys.Contains(localeCountry))
                        currentScore += Math.Pow(2, 2);
                    else
                        continue;
                }

                if (gallery.Locale_Devices != null)
                {
                    if (gallery.Locale_Devices.Contains(localeDevice))
                        currentScore += Math.Pow(2, 3);
                    else
                        continue;
                }

                //This means that this is a default gallery and we havnt found a precise match yet
                if (currentScore == 0 && retVal == null)
                {
                    retVal = gallery;
                }
                else if (currentScore > prevScore)
                {
                    retVal = gallery;
                    prevScore = currentScore;
                }
            }

            return retVal;
        }

        public List<PageGallery> GetPageGalleriesByLocation(PageContext page, GalleryLocation location, string languageCode)
        {
            List<PageGallery> retVal = null;

            IEnumerable<PageGallery> PageGalleryList =
                from galleries in page.GetGalleries()
                where galleries.GalleryLocation != null && galleries.GalleryLocation.Equals(location.ToString()) &&
                (galleries.MainCulture == null || galleries.MainCulture.Equals(languageCode))
                select galleries;

            if (PageGalleryList != null && PageGalleryList.Count() > 0)
            {
                retVal = PageGalleryList.ToList<PageGallery>();
            }
            return retVal;

        }
        #endregion

        #region Private Methods
        public void Init(int groupID, PlatformType platfrom)
        {
            logger.InfoFormat("Init-> [{0}, {1}] - Started initializing page information", groupID, platfrom.ToString());

            // Load pages dataset]
            try
            {
                //TODO: Logger.Logger.Log("Start load pages", groupID + "_" + platfrom.ToString(), "TVPApi");
                APIPageDataLoader pd = new APIPageDataLoader() { GroupID = groupID, Platform = platfrom };
                DataOnPage = pd.Execute();

                if (DataOnPage == null)
                {
                    logger.ErrorFormat("Init-> [{0}, {1}] - Page data returned null", groupID, platfrom.ToString());
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Init-> Failed retrieving page data information", ex);
                return;
            }

            // Run on pages and create dictionary
            m_LanguageIDPages = new Dictionary<string, Dictionary<long, PageContext>>();
            m_LanguageTokenPages = new Dictionary<string, Dictionary<string, PageContext>>();
            foreach (var page in DataOnPage.Pages)
            {
                string culture = GetLanguageString(page.LanguageCulture);
                PageContext pg = CreatePageItem(page);

                // Add to language by id dictionary
                if (m_LanguageIDPages.ContainsKey(culture))
                {
                    // Check if page id exists for language
                    if (m_LanguageIDPages[culture].ContainsKey(page.ID))
                    {
                        //m_Logger.ErrorFormat("Page ID:{0} already exists for langauge:{1}", page.ID, page.LanguageCulture);
                        throw new Exception(string.Format("Page ID:{0} already exists for langauge:{1}", page.ID, page.LanguageCulture));
                    }

                    m_LanguageIDPages[culture].Add(page.ID, pg);
                }
                else
                {
                    Dictionary<long, PageContext> langDict = new Dictionary<long, PageContext>();
                    m_LanguageIDPages.Add(culture, langDict);

                    langDict.Add(page.ID, pg);
                }

                // Add to language by page token dictionary
                if (pg.PageToken != Pages.UnKnown && pg.PageToken != Pages.Dynamic && pg.IsActive)
                {

                    if (m_LanguageTokenPages.ContainsKey(culture))
                    {
                        // Check if page id exists for language
                        if (m_LanguageTokenPages[culture].ContainsKey(page.PageToken))
                        {
                            // m_Logger.ErrorFormat("Page Token:{0} already exists for langauge:{1}", page.PageToken, page.LanguageCulture);
                            //throw new Exception(string.Format("Page Token:{0} already exists for langauge:{1}", page.PageToken, page.LanguageCulture));
                        }
                        else
                            m_LanguageTokenPages[culture].Add(pg.PageToken.ToString(), pg);
                    }
                    else
                    {
                        Dictionary<string, PageContext> langDict = new Dictionary<string, PageContext>();
                        m_LanguageTokenPages.Add(culture, langDict);

                        langDict.Add(pg.PageToken.ToString(), pg);
                    }
                }
            }

            // Run on all page and set page's children and parent
            foreach (var page in DataOnPage.Pages)
            {
                string culture = GetLanguageString(page.LanguageCulture);
                PageContext pg;
                if (m_LanguageIDPages[culture].TryGetValue(page.ID, out pg))
                {
                    if (!page.IsParentPageIDNull())
                    {
                        // Try to get parent page
                        PageContext parentPage;
                        if (m_LanguageIDPages[culture].TryGetValue(page.ParentPageID, out parentPage))
                        {
                            // Add page to parent's children
                            parentPage.Children.Add(pg);
                        }
                    }
                }
            }

            // m_Logger.Info("Finished intializing page information, found: " + DataOnPage.Pages.Count + " pages");
        }

        //public PageContext[] GetPagesByLanguage(string langID)
        //{
        //    PageContext[] retVal = null;
        //    if (!string.IsNullOrEmpty(langID))
        //    {
        //        if (m_LanguageIDPages.ContainsKey(langID))
        //        {
        //            retVal = m_LanguageIDPages[langID].Values.ToArray<PageContext>();
        //        }
        //    }
        //    return retVal;
        //}

        public Dictionary<string, Dictionary<long, PageContext>> GetPageContextsIDDict()
        {
            return m_LanguageIDPages;
        }

        public PageGallery GetPageGallery(long ID, long PageID, string culture)
        {
            PageGallery retVal = null;
            if (m_LanguageIDPages.ContainsKey(culture))
            {
                if (m_LanguageIDPages[culture].ContainsKey(PageID))
                {
                    PageContext pageContext = m_LanguageIDPages[culture][PageID];
                    retVal = (from galleries in pageContext.GetGalleries()
                              where galleries.GalleryID == ID
                              select galleries).FirstOrDefault();
                }

            }
            return retVal;
        }


        private PageContext CreatePageItem(dsPageData.PagesRow page)
        {
            PageContext pg = new PageContext();

            pg.ID = page.ID;
            pg.PageMetadataID = page.SitePageMetadataID;

            if (!page.IsPageTokenNull())
            {
                //Check if page exists
                if (Enum.IsDefined(typeof(Pages), page.PageToken))
                    pg.PageToken = (Pages)Enum.Parse(typeof(Pages), page.PageToken, true);
                else
                    pg.PageToken = Pages.UnKnown;
            }
            else
                pg.PageToken = Pages.UnKnown;

            if (!page.IsBreadCrumbTextNull())
                pg.BreadCrumbText = page.BreadCrumbText;

            if (!page.IsURLNull())
                pg.URL = page.URL;

            if (!page.IsHasPlayerNull())
                pg.HasPlayer = page.HasPlayer;

            if (!page.IsHasCarouselNull())
                pg.HasCarousel = page.HasCarousel;

            if (!page.IsHasMiddleFooterNull())
                pg.HasMiddleFooter = page.HasMiddleFooter;

            if (!page.IsPlayerAutoPlayNull())
                pg.PlayerAutoPlay = page.PlayerAutoPlay;

            if (!page.IsNameNull())
                pg.Name = page.Name;

            if (!page.IsIsActiveNull())
                pg.IsActive = (page.IsActive == 1);

            if (!page.IsDescriptionNull())
                pg.Description = page.Description;

            if (!page.IsPlayerChannelNull())
                pg.PlayerChannel = page.PlayerChannel;

            if (!page.IsPlayerTreeCategoryNull())
                pg.PlayerTreeCategory = page.PlayerTreeCategory;

            if (!page.IsCarouselChannelNull())
                pg.CarouselChannel = page.CarouselChannel;

            if (!page.IsSideProfileIDNull())
                pg.SideProfileID = page.SideProfileID;

            if (!page.IsBottomProfileIDNull())
                pg.BottomProfileID = page.BottomProfileID;

            if (!page.IsMenuIDNull())
                pg.MenuID = page.MenuID;

            if (!page.IsFooterIDNull())
                pg.FooterID = page.FooterID;

            if (!page.IsMiddleFooterIDNull())
                pg.MiddleFooterID = page.MiddleFooterID;

            if (!page.IsProfileIDNull())
                pg.ProfileID = page.ProfileID;

            if (!page.IsIsProtectedNull())
                pg.IsProtected = page.IsProtected == 1;

            // Add page's galleries
            AddGalleriesToPage(pg, page);

            return pg;
        }

        private GalleryItem CreateGalleryItem(dsPageData.PageGalleriesRow galleryRow)
        {
            GalleryItem retVal = new GalleryItem();
            retVal.GalleryID = galleryRow.ID;
            retVal.ViewType = galleryRow.ViewType;
            retVal.NumberOfItemsPerPage = galleryRow.NumberOfItemsPerPage;
            if (!galleryRow.IsTitleNull())
                retVal.Title = galleryRow.Title;

            if (!galleryRow.IsTVMChannelIDNull())
                retVal.TVMChannelID = galleryRow.TVMChannelID;

            if (!galleryRow.IsMainPlayerUNNull() && !string.IsNullOrEmpty(galleryRow.MainPlayerUN))
            {
                retVal.TVMUser = galleryRow.MainPlayerUN;
                retVal.TVMPass = galleryRow.MainPlayerPass;
            }
            else if (!galleryRow.IsTvmAccountUNNull())
            {
                retVal.TVMUser = galleryRow.TvmAccountUN;
                retVal.TVMPass = galleryRow.TvmAccountPass;
            }
            if (!galleryRow.IsPIC_MAINNull())
                retVal.MainPic = galleryRow.PIC_MAIN;

            if (!galleryRow.IsCULTURENull())
            {
                string culture = GetLanguageString(galleryRow.CULTURE);
                retVal.Culture = culture;
            }

            if (!galleryRow.IsMAIN_DESCRIPTIONNull())
            {
                retVal.MainDescription = galleryRow.MAIN_DESCRIPTION;
            }

            if (!galleryRow.IsPictureSizeNull())
                retVal.PictureSize = galleryRow.PictureSize;

            if (!galleryRow.IsBooleanParamNull())
                retVal.BooleanParam = Convert.ToBoolean(galleryRow.BooleanParam);

            if (!galleryRow.IsNumericParamNull())
                retVal.NumericParam = galleryRow.NumericParam;

            //pgallery.IsPoster = galleryRow.IsPoster;

            if (!galleryRow.IsNumOfItemsNull())
                retVal.NumOfItems = galleryRow.NumOfItems;

            if (!galleryRow.Isitem_linkNull())
                retVal.Link = galleryRow.item_link;


            return retVal;
        }

        private void AddButtonLinksToGallery(PageGallery pg)
        {
            IEnumerable<dsPageData.GalleryButtonsRow> buttonRows = GetGalleryButtons(pg.GalleryID);
            if (buttonRows != null && buttonRows.Count() > 0)
            {
                foreach (dsPageData.GalleryButtonsRow buttonRow in buttonRows)
                {
                    if (!buttonRow.IsMainCultureNull())
                    {
                        string cultureStr = GetLanguageString(buttonRow.MainCulture);
                        if (cultureStr == pg.MainCulture)
                        {
                            GalleryButtonLink newObj = new GalleryButtonLink();
                            if (!buttonRow.IsTextNull())
                            {
                                newObj.Text = buttonRow.Text;
                            }
                            if (!buttonRow.IsLinkNull())
                            {
                                newObj.Link = buttonRow.Link;
                            }
                            if (!buttonRow.IsTypeNull())
                            {
                                newObj.Type = (GalleryButtonType)buttonRow.Type;
                            }
                            if (newObj.Type == GalleryButtonType.Button)
                            {
                                pg.GalleryButtons.Add(newObj);
                            }
                            else
                            {
                                if (newObj.Type == GalleryButtonType.Link)
                                {
                                    pg.GalleryLinks.Add(newObj);
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetLanguageString(string cultureCode)
        {
            return cultureCode;
            //try
            //{
            //    Tvinci.Localization.LanguageContext lc;
            //    TextLocalization.Instance.TryGetLanguageByCulture(cultureCode, out lc);
            //    string culture = lc.CultureInfo.DisplayName;
            //    return culture;
            //}
            //catch (Exception ex)
            //{
            //    int i = 0;
            //}
            //return string.Empty;
        }

        private void AddGalleriesToPage(PageContext page, dsPageData.PagesRow pageRow)
        {
            IEnumerable<dsPageData.PageGalleriesRow> GalleryList =
                from galleries in DataOnPage.PageGalleries
                where galleries.SitePageID == page.ID
                select galleries;

            //dsPageData.PageGalleriesRow[] galleries = pageRow.GetPageGalleriesRows();

            if (GalleryList == null || GalleryList.Count() == 0)
                return;

            Dictionary<string, Dictionary<long, PageGallery>> galleryDict = new Dictionary<string, Dictionary<long, PageGallery>>();

            foreach (dsPageData.PageGalleriesRow galleryRow in GalleryList)
            {
                bool isChildGallery = false;
                PageGallery pgallery = new PageGallery();

                pgallery.GalleryID = galleryRow.ID;
                pgallery.UIGalleryType = (UIGalleryType)galleryRow.UiComponentType;

                if (!galleryRow.IsMainPlayerUNNull() && !string.IsNullOrEmpty(galleryRow.MainPlayerUN))
                {
                    pgallery.TVMUser = galleryRow.MainPlayerUN;
                    pgallery.TVMPass = galleryRow.MainPlayerPass;
                }
                else if (!galleryRow.IsTvmAccountUNNull())
                {
                    pgallery.TVMUser = galleryRow.TvmAccountUN;
                    pgallery.TVMPass = galleryRow.TvmAccountPass;
                }

                if (!galleryRow.IslocationNull())
                    pgallery.GalleryLocation = galleryRow.location;

                if (!galleryRow.Ismain_locationNull())
                    pgallery.GalleryOrder = galleryRow.main_location;


                if (!galleryRow.IsMAIN_CULTURENull())
                {
                    string culture = GetLanguageString(galleryRow.MAIN_CULTURE);
                    pgallery.MainCulture = culture;
                }


                if (!galleryRow.IsLinkHeaderNull())
                    pgallery.LinksHeader = galleryRow.LinkHeader;


                if (!galleryRow.IsGroupTitleNull())
                    pgallery.GroupTitle = galleryRow.GroupTitle;

                if (!galleryRow.IsGroupDescriptionNull())
                    pgallery.MainDescription = galleryRow.GroupDescription;

                GalleryItem galleryItem = CreateGalleryItem(galleryRow);
                AddLocalesToGallery(pgallery, galleryRow.GetGalleryLocalesRows());
                //AddButtonLinksToGallery(pgallery);
                if (pgallery.MainCulture != null)
                {
                    //Check if gallery language already exists in dictionary
                    if (galleryDict.ContainsKey(pgallery.MainCulture))
                    {
                        //Check if gallery already exists 
                        if (galleryDict[pgallery.MainCulture].ContainsKey(pgallery.GalleryID))
                        {
                            //This means that the new gallery is a child gallery - add it to the gallery children list later on
                            isChildGallery = true;
                        }
                        else
                        {
                            //New gallery - add it to existing galleris dictionary
                            galleryDict[pgallery.MainCulture].Add(pgallery.GalleryID, pgallery);
                        }
                    }
                    else
                    {
                        //New language - add it as a key to the dictionary
                        galleryDict.Add(pgallery.MainCulture, new Dictionary<long, PageGallery>());
                        //Add the new page gallery to the new language distionary
                        galleryDict[pgallery.MainCulture].Add(pgallery.GalleryID, pgallery);
                    }
                }

                if (!isChildGallery)
                {
                    //Regular gallery - add it to the page
                    string newIDStr = string.Concat(pgallery.GalleryID.ToString(), "0");
                    galleryItem.ItemID = Convert.ToInt32(newIDStr);
                    page.AddGallery(pgallery);
                    pgallery.GalleryItems.Add(galleryItem);
                }
                else
                {
                    //Child gallery - add it to the parent's children list
                    if (pgallery.MainCulture != null && pgallery.MainCulture == galleryItem.Culture)
                    {
                        string newIDStr = string.Concat(pgallery.GalleryID.ToString(), galleryDict[pgallery.MainCulture][pgallery.GalleryID].GalleryItems.Count.ToString());
                        galleryItem.ItemID = Convert.ToInt32(newIDStr);
                        galleryDict[pgallery.MainCulture][pgallery.GalleryID].GalleryItems.Add(galleryItem);

                    }
                }
            }
        }
        #endregion

        #region Public Methods


        //Get gallery buttons by gallery id and localization culture
        public IEnumerable<dsPageData.GalleryButtonsRow> GetGalleryButtons(long galleryID)
        {
            IEnumerable<dsPageData.GalleryButtonsRow> ButtonsList =
                from buttons in DataOnPage.GalleryButtons
                where buttons.GalleryID == galleryID &&
                (buttons.MainCulture.Equals(TextLocalization.Instance.UserContext.Culture))
                select buttons;

            return ButtonsList;
        }


        public PageContext GetPageByToken(string langID, string token)
        {
            PageContext retVal = null;
            Dictionary<string, PageContext> dict;
            if (m_LanguageTokenPages.TryGetValue(langID, out dict))
            {
                dict.TryGetValue(token, out retVal);
            }
            return retVal;
        }

        public PageContext GetPageByID(string langID, long ID)
        {
            PageContext retVal = null;
            Dictionary<long, PageContext> dict;
            if (m_LanguageIDPages.TryGetValue(langID, out dict))
            {
                dict.TryGetValue(ID, out retVal);
            }
            return retVal;
        }


        //Get TVM Account params by MediaType
        public TVMAccountType GetTVMAccountByMediaType(int tvmMediaID)
        {

            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where ((!accounts.IsTvmTypeIDNull()) && accounts.TvmTypeID == tvmMediaID)
                                                select accounts).FirstOrDefault();


            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Base_Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        //Get TVM Account params by MediaType
        public TVMAccountType GetTVMAccountByUser(string user)
        {

            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where ((!accounts.IsPlayer_UNNull()) && accounts.Player_UN.Equals(user))
                                                select accounts).FirstOrDefault();


            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Base_Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        //Get TVM Accounts by Account ID - there can be more than one per ID so the media type is also necessary
        public TVMAccountType GetTVMAccountByAccountType(AccountType accountType)
        {
            string AccountName = accountType.ToString();
            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where accounts.Name == AccountName
                                                select accounts).FirstOrDefault();

            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Base_Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        //Get TVM Accounts by Group ID
        public TVMAccountType GetTVMAccountByGroupID(int groupID)
        {
            try
            {
                dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                    where accounts.Base_Group_ID == groupID
                                                    select accounts).FirstOrDefault();


                TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Base_Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
                return retVal;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("GetTVMAccountByGroupID-> Params:[GroupID: {0}]", groupID), ex);

                if (DataOnPage == null)
                {
                    logger.Error(string.Format("GetTVMAccountByGroupID-> Params:[GroupID: {0}] - Data on Page is null", groupID), ex);
                }
                else
                {
                    if (DataOnPage.TVMAccounts == null)
                    {
                        logger.Error(string.Format("GetTVMAccountByGroupID-> Params:[GroupID: {0}] - Accounts is null", groupID), ex);
                    }
                    else
                    {
                        logger.Error(string.Format("GetTVMAccountByGroupID-> Params:[GroupID: {0}] - {1} Accounts", groupID, DataOnPage.TVMAccounts.Count), ex);
                    }
                }
                return new TVMAccountType();
            }
        }


        public long? GetProfileIDFromPageID(long? pageID)
        {
            if (pageID.HasValue)
                return m_LanguageIDPages[TextLocalization.Instance.UserContext.Culture][(long)pageID].ProfileID;
            else return null;
        }

        internal long? GetPageIDFromURL(string sPageUrl, string culture)
        {
            long? iRet = null;

            if (sPageUrl.ToLower().Contains("pageid="))
            {
                string sPageID = sPageUrl.ToLower().Substring(sPageUrl.ToLower().LastIndexOf("pageid=") + 7);
                sPageID = sPageID.Trim('/');
                long iPageID;
                long.TryParse(sPageID, out iPageID);

                iRet = iPageID;
            }
            else
            {
                try
                {
                    iRet = (from entry in m_LanguageIDPages[culture]
                            where entry.Value.URL != null && sPageUrl.ToLower().Contains(entry.Value.URL.ToLower()) && entry.Value.IsActive
                            select (long?)entry.Key).SingleOrDefault();
                }
                catch (Exception ex) { }
            }

            return iRet;
        }

        #endregion
    }

    #region Page
    public class PageContext
    {
        public long ID { get; set; }
        public long PageMetadataID { get; set; }
        public string BreadCrumbText { get; set; }
        public string URL { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasCarousel { get; set; }
        public bool HasMiddleFooter { get; set; }
        public bool PlayerAutoPlay { get; set; }
        public TVPApi.Pages PageToken { get; set; }
        //public TVPPro.SiteManager.DataLoaders.CustomLayoutLoader.CustomLayout CustomLayout { get; internal set; }

        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long PlayerChannel { get; set; }
        public long PlayerTreeCategory { get; set; }
        public long CarouselChannel { get; set; }

        public long SideProfileID { get; set; }
        public long BottomProfileID { get; set; }
        public long MenuID { get; set; }
        public long FooterID { get; set; }
        public long MiddleFooterID { get; set; }
        public long ProfileID { get; set; }
        public bool IsProtected { get; set; }
        public Menu Menu { get; set; }
        public Menu Footer { get; set; }

        private List<PageContext> m_Children = new List<PageContext>();
        public List<PageContext> Children
        {
            get
            {
                return m_Children;
            }
        }

        private List<PageGallery> m_Galleries = new List<PageGallery>();
        private List<PageGallery> m_localeGalleries;

        public List<PageGallery> MainGalleries
        {
            get
            {
                IEnumerable<PageGallery> MainGalleryList =
                from galleries in GetLocaleGalleries()
                where galleries.GalleryLocation.Equals(TVPApi.GalleryLocation.Main.ToString())
                select galleries;

                if (MainGalleryList != null && MainGalleryList.Count() > 0)
                {
                    return MainGalleryList.ToList<PageGallery>();
                }
                return null;
            }
        }

        public List<PageGallery> TopGalleries
        {
            get
            {
                IEnumerable<PageGallery> TopGalleryList =
                from galleries in GetLocaleGalleries()
                where galleries.GalleryLocation.Equals(TVPApi.GalleryLocation.Top.ToString())
                select galleries;

                if (TopGalleryList != null && TopGalleryList.Count() > 0)
                {
                    return TopGalleryList.ToList<PageGallery>();
                }
                return null;
            }
        }

        private List<PageGallery> GetLocaleGalleries()
        {
            if (m_localeGalleries == null)
            {
                return m_Galleries;
            }
            else
            {
                return m_localeGalleries;
            }
        }

        public void SetLocaleGalleries(List<PageGallery> galleryList)
        {
            m_localeGalleries = galleryList;
        }

        public void AddGallery(PageGallery pg)
        {
            m_Galleries.Add(pg);
        }

        public List<PageGallery> GetGalleries()
        {
            return m_Galleries;
        }

        //public void InitCustomLayout(Enums.eCustomLayoutItemType layoutItemType, long? itemID)
        //{
        //    CustomLayoutLoader layoutItem = new CustomLayoutLoader();
        //    layoutItem.ItemType = layoutItemType;

        //    if (itemID > 0)
        //    {
        //        layoutItem.ItemID = (long) itemID;
        //    }

        //    TVPPro.SiteManager.DataLoaders.CustomLayoutLoader.CustomLayout layout = layoutItem.Execute();

        //    if (layout != null)
        //        CustomLayout = layout;
        //}

        //public void ResetCustomLayout()
        //{
        //    CustomLayout = null;
        //}
    }
    #endregion

    #region PageGallery
    public class PageGallery
    {
        public long GalleryID { get; set; }
        public TVPApi.UIGalleryType UIGalleryType { get; set; }
        public string TVMUser { get; set; }
        public string TVMPass { get; set; }
        public string GalleryLocation { get; set; }
        public int GalleryOrder { get; set; }
        public string MainCulture { get; set; }
        public string LinksHeader { get; set; }
        public string GroupTitle { get; set; }
        public string MainDescription { get; set; }
        public long FamilyID { get; set; }

        protected List<GalleryButtonLink> m_galleryButtons;
        protected List<GalleryButtonLink> m_galleryLinks;
        protected List<GalleryItem> m_GalleryItems;


        public List<string> Locale_Langs { get; set; }
        public List<string> Locale_Countrys { get; set; }
        public List<long> Locale_UserStates { get; set; }
        public List<string> Locale_Devices { get; set; }


        public List<GalleryItem> GalleryItems
        {
            get
            {
                if (m_GalleryItems == null)
                {
                    m_GalleryItems = new List<GalleryItem>();
                }
                return m_GalleryItems;
            }
            set
            {
                m_GalleryItems = value;
            }
        }

        public List<GalleryButtonLink> GalleryButtons
        {
            get
            {
                if (m_galleryButtons == null)
                {
                    m_galleryButtons = new List<GalleryButtonLink>();
                }
                return m_galleryButtons;
            }
        }

        public List<GalleryButtonLink> GalleryLinks
        {
            get
            {
                if (m_galleryLinks == null)
                {
                    m_galleryLinks = new List<GalleryButtonLink>();
                }
                return m_galleryLinks;
            }
        }



    }

    #endregion

    #region GalleryButton

    public struct GalleryButtonLink
    {
        public long ButtonID;
        public string Link;
        public string Text;
        public TVPApi.GalleryButtonType Type;
    }

    #endregion

    #region Gallery Item

    public class GalleryItem
    {
        public long GalleryID { get; set; }
        public long ItemID { get; set; }
        public string ViewType { get; set; }
        public string Title { get; set; }
        public long TVMChannelID { get; set; }
        public int NumberOfItemsPerPage { get; set; }
        public string PictureSize { get; set; }
        public bool IsPoster { get; set; }
        public int NumOfItems { get; set; }
        public bool BooleanParam { get; set; }
        public long NumericParam { get; set; }
        public long MainPic { get; set; }
        public string TVMUser { get; set; }
        public string TVMPass { get; set; }
        public string Link { get; set; }
        public string MainDescription { get; set; }
        public string Culture { get; set; }

    }

    #endregion
    #region TVMAccount
    public struct TVMAccountType
    {
        private string m_TVMUser;
        private string m_TVMPass;
        private string m_MediaType;
        private string m_ApiWsUser;
        private string m_ApiWsPassword;
        private int m_BaseGroupID;
        private int m_GroupID;

        public TVMAccountType(string user, string pass, string type, int baseID, int groupID, string apiWsUser, string apiWsPassword)
        {
            m_TVMUser = user;
            m_TVMPass = pass;
            m_MediaType = type;
            m_BaseGroupID = baseID;
            m_GroupID = groupID;
            m_ApiWsUser = apiWsUser;
            m_ApiWsPassword = apiWsPassword;
        }

        public string TVMUser
        {
            get
            {
                return m_TVMUser;
            }
        }

        public string TVMPass
        {
            get
            {
                return m_TVMPass;
            }
        }

        public string MediaType
        {
            get
            {
                return m_MediaType;
            }
        }

        public int BaseGroupID
        {
            get
            {
                return m_BaseGroupID;
            }
        }

        public int GroupID
        {
            get
            {
                return m_GroupID;
            }
        }

        public string APIUserName
        {
            get
            {
                return m_ApiWsUser;
            }
        }

        public string APIPassword
        {
            get
            {
                return m_ApiWsPassword;
            }
        }
    }

    #endregion
}
