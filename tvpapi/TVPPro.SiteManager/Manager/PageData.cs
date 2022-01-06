using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using System.Web;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Helper;
using System.Data;
using System.Collections;
using TVPPro.Configuration.Site;
using Phx.Lib.Log;
using System.Reflection;

namespace TVPPro.SiteManager.Manager
{
    public class PageData
    {
        //static PageData()
        //{
        //    m_Instance = new PageData();
        //}

        private PageData()
        {
            Init();
        }

        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        //private const int m_initCounter = 3;
        private Dictionary<string, Dictionary<long, PageContext>> m_LanguageIDPages;
        private Dictionary<string, Dictionary<string, PageContext>> m_LanguageTokenPages;

        private static PageData m_Instance;
        static object instanceLock = new object();

        private dsPageData DataOnPage;
        //public dsPageData SiteGallries
        //{
        //    get
        //    {
        //        return DataOnPage;
        //    }
        //}

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

        //public List<BreadCrumbItem> PageHierarchy
        //{
        //    get
        //    {
        //        return HttpContext.Current.Items[REQUEST_PAGE_HIERARCHY_KEY] as List<BreadCrumbItem>;
        //    }
        //    set
        //    {
        //        HttpContext.Current.Items[REQUEST_PAGE_HIERARCHY_KEY] = value;
        //    }
        //}


        #endregion

        #region Constants
        private const string REQUEST_PAGE_KEY = "REQUESTPAGEKEY";
        private const string REQUEST_PAGE_HIERARCHY_KEY = "REQUESTPAGEHIERARCHYKEY";
        #endregion

        #region Public Properties
        public static PageData Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (instanceLock)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new PageData();
                        }
                    }
                }
                return m_Instance;
            }
        }

        #endregion

        #region Private Methods
        public void Init()
        {
            logger.Info("Started intializing page information");

            // Load pages dataset]
            try
            {
                PageDataLoader pd = new PageDataLoader();
                DataOnPage = pd.Execute();

                if (DataOnPage == null)
                {
                    logger.Error("Page data returned null");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed retrieving page data information", ex);
                return;
            }

            // Run on pages and create dictionary
            m_LanguageIDPages = new Dictionary<string, Dictionary<long, PageContext>>();
            m_LanguageTokenPages = new Dictionary<string, Dictionary<string, PageContext>>();
            foreach (var page in DataOnPage.Pages)
            {
                if (page.PageToken.Equals(Enums.ePages.Article.ToString())) continue;

                PageContext pg = CreatePageItem(page);

                // Add to language by id dictionary
                if (m_LanguageIDPages.ContainsKey(page.LanguageCulture))
                {
                    // Check if page id exists for language
                    if (m_LanguageIDPages[page.LanguageCulture].ContainsKey(page.ID))
                    {
                        logger.ErrorFormat("Page ID:{0} already exists for langauge:{1}", page.ID, page.LanguageCulture);
                        throw new Exception(string.Format("Page ID:{0} already exists for langauge:{1}", page.ID, page.LanguageCulture));
                    }

                    m_LanguageIDPages[page.LanguageCulture].Add(page.ID, pg);
                }
                else
                {
                    Dictionary<long, PageContext> langDict = new Dictionary<long, PageContext>();
                    m_LanguageIDPages.Add(page.LanguageCulture, langDict);

                    langDict.Add(page.ID, pg);
                }

                // Add to language by page token dictionary
                if (pg.PageToken != Enums.ePages.UnKnown && pg.PageToken != Enums.ePages.Dynamic
                    && pg.PageToken != Enums.ePages.Article && pg.IsActive)
                {
                    if (m_LanguageTokenPages.ContainsKey(page.LanguageCulture))
                    {
                        // Check if page id exists for language
                        if (m_LanguageTokenPages[page.LanguageCulture].ContainsKey(page.PageToken))
                        {
                            logger.ErrorFormat("Page Token:{0} already exists for langauge:{1}", page.PageToken, page.LanguageCulture);
                            //throw new Exception(string.Format("Page Token:{0} already exists for langauge:{1}", page.PageToken, page.LanguageCulture));
                        }
                        else
                            m_LanguageTokenPages[page.LanguageCulture].Add(pg.PageToken.ToString(), pg);
                    }
                    else
                    {
                        Dictionary<string, PageContext> langDict = new Dictionary<string, PageContext>();
                        m_LanguageTokenPages.Add(page.LanguageCulture, langDict);

                        langDict.Add(pg.PageToken.ToString(), pg);
                    }
                }
            }

            // Run on all page and set page's children and parent
            foreach (var page in DataOnPage.Pages)
            {
                if (page.PageToken.Equals(Enums.ePages.Article.ToString())) continue;

                PageContext pg;
                if (m_LanguageIDPages[page.LanguageCulture].TryGetValue(page.ID, out pg))
                {
                    if (!page.IsParentPageIDNull())
                    {
                        // Try to get parent page
                        PageContext parentPage;
                        if (m_LanguageIDPages[page.LanguageCulture].TryGetValue(page.ParentPageID, out parentPage))
                        {
                            // Set page's parent
                            pg.Parent = parentPage;

                            // Add page to parent's children
                            parentPage.Children.Add(pg);
                        }
                    }
                }
            }

            logger.Info("Finished intializing page information, found: " + DataOnPage.Pages.Count + " pages");

        }

        private PageContext CreatePageItem(dsPageData.PagesRow page)
        {
            PageContext pg = new PageContext();

            pg.ID = page.ID;
            pg.PageMetadataID = page.SitePageMetadataID;

            if (!page.IsPageTokenNull())
            {
                //Check if page exists
                if (Enum.IsDefined(typeof(Enums.ePages), page.PageToken))
                    pg.PageToken = (Enums.ePages)Enum.Parse(typeof(Enums.ePages), page.PageToken, true);
                else
                {
                    logger.ErrorFormat("Page Token:{0} doesn't exists in enums list", page.PageToken);
                    pg.PageToken = Enums.ePages.UnKnown;
                }
            }
            else
            {
                pg.PageToken = Enums.ePages.UnKnown;
            }

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

            if (!page.IsDescriptionNull())
                pg.Description = page.Description;

            if (!page.IsKeywordsNull())
                pg.Keywords = page.Keywords;

            if (!page.IsPlayerChannelNull())
                pg.PlayerChannel = page.PlayerChannel;

            if (!page.IsPlayerTreeCategoryNull())
                pg.PlayerTreeCategory = page.PlayerTreeCategory;

            if (!page.IsCarouselChannelNull())
                pg.CarouselChannel = page.CarouselChannel;

            if (!page.IsMenuIDNull())
                pg.MenuID = page.MenuID;

            if (!page.IsFooterIDNull())
                pg.FooterID = page.FooterID;

            if (!page.IsMiddleFooterIDNull())
                pg.MiddleFooterID = page.MiddleFooterID;

            if (!page.IsProfileIDNull())
                pg.ProfileID = page.ProfileID;

            if (!page.IsIsProtectedNull())
                pg.IsProtected = (page.IsProtected == 1);

            if (!page.IsIsActiveNull())
                pg.IsActive = (page.IsActive == 1);

            if (!page.IsBrandingBigPicIDNull())
                pg.BrandingBigImageID = page.BrandingBigPicID;

            if (!page.IsBrandingSmallPicIDNull())
                pg.BrandingSmallImageID = page.BrandingSmallPicID;

            if (!page.IsBrandingPixelHeightNull())
                pg.BrandingPixelHeigt = page.BrandingPixelHeight;

            if (!page.IsBrandingRecurringHorizonalNull())
                pg.BrandingRecurringHorizonal = page.BrandingRecurringHorizonal;

            if (!page.IsBrandingRecurringVerticalNull())
                pg.BrandingRecurringVertical = page.BrandingRecurringVertical;

            if (!page.IsHasSideProfileNull())
            {
                pg.HaseSideProfile = page.HasSideProfile;
            }
            else
            {
                pg.HaseSideProfile = 1;
            }

            // Add page's galleries
            IEnumerable GalleryList =
                (from galleries in DataOnPage.PageGalleries
                 where galleries.SitePageID == pg.ID
                 select galleries) as IEnumerable;

            AddGalleriesToPage(pg, page, GalleryList);

            //Add the inactive gallery list (for editorial mode)
            IEnumerable InActiveGalleryList =
                (from galleries in DataOnPage.InActivePageGalleries
                 where galleries.SitePageID == pg.ID
                 select galleries) as IEnumerable;

            AddGalleriesToPage(pg, page, InActiveGalleryList);
            return pg;
        }

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


        private void AddGalleriesToPage(PageContext page, dsPageData.PagesRow pageRow, IEnumerable GalleryList)
        {
            //dsPageData.PageGalleriesRow[] galleries = pageRow.GetPageGalleriesRows();

            if (GalleryList == null)
                return;

            Dictionary<string, Dictionary<long, PageGallery>> galleryDict = new Dictionary<string, Dictionary<long, PageGallery>>();

            bool isActiveGallery = true;
            if (GalleryList is IEnumerable<dsPageData.InActivePageGalleriesRow>)
            {
                isActiveGallery = false;
            }

            foreach (DataRow dr in GalleryList)
            {
                dsPageData.PageGalleriesRow galleryRow = null;
                dsPageData.InActivePageGalleriesRow inActiveGalleryRow = null;
                if (isActiveGallery)
                {
                    galleryRow = dr as dsPageData.PageGalleriesRow;
                }
                else
                {
                    inActiveGalleryRow = dr as dsPageData.InActivePageGalleriesRow;
                }

                bool isChildGallery = false;
                PageGallery pgallery = new PageGallery();

                pgallery.GalleryID = (isActiveGallery ? galleryRow.ID : inActiveGalleryRow.ID);

                pgallery.GalleryType = (isActiveGallery ? (TVPPro.SiteManager.Context.Enums.eGalleryType)galleryRow.GalleryType : (TVPPro.SiteManager.Context.Enums.eGalleryType)inActiveGalleryRow.GalleryType);
                pgallery.UIGalleryType = (isActiveGallery ? (TVPPro.SiteManager.Context.Enums.eUIGalleryType)galleryRow.UiComponentType : (TVPPro.SiteManager.Context.Enums.eUIGalleryType)inActiveGalleryRow.UiComponentType);
                pgallery.ViewType = (isActiveGallery ? galleryRow.ViewType : inActiveGalleryRow.ViewType);
                pgallery.NumberOfItemsPerPage = (isActiveGallery ? galleryRow.NumberOfItemsPerPage : inActiveGalleryRow.NumberOfItemsPerPage);

                if ((isActiveGallery && !galleryRow.IsTitleNull()) || (!isActiveGallery && !inActiveGalleryRow.IsTitleNull()))
                    pgallery.Title = (isActiveGallery ? galleryRow.Title : inActiveGalleryRow.Title);

                if ((isActiveGallery && !galleryRow.IsTVMChannelIDNull()) || (!isActiveGallery && !inActiveGalleryRow.IsTVMChannelIDNull()))
                    pgallery.TVMChannelID = (isActiveGallery ? galleryRow.TVMChannelID : inActiveGalleryRow.TVMChannelID);

                if ((isActiveGallery && !galleryRow.IsMainPlayerUNNull() && !string.IsNullOrEmpty(galleryRow.MainPlayerUN)) || (!isActiveGallery && !inActiveGalleryRow.IsMainPlayerUNNull() && !string.IsNullOrEmpty(inActiveGalleryRow.MainPlayerUN)))
                {
                    pgallery.TVMUser = (isActiveGallery ? galleryRow.MainPlayerUN : inActiveGalleryRow.MainPlayerUN);
                    pgallery.TVMPass = (isActiveGallery ? galleryRow.MainPlayerPass : inActiveGalleryRow.MainPlayerPass);
                }

                else if ((isActiveGallery && !galleryRow.IsTvmAccountUNNull()) || (!isActiveGallery && !inActiveGalleryRow.IsTvmAccountUNNull()))
                {
                    pgallery.TVMUser = (isActiveGallery ? galleryRow.TvmAccountUN : inActiveGalleryRow.TvmAccountUN);
                    pgallery.TVMPass = (isActiveGallery ? galleryRow.TvmAccountPass : inActiveGalleryRow.TvmAccountPass);
                }

                if ((isActiveGallery && !galleryRow.IslocationNull()) || (!isActiveGallery && !inActiveGalleryRow.IslocationNull()))
                    pgallery.GalleryLocation = (isActiveGallery ? galleryRow.location : inActiveGalleryRow.location);

                if ((isActiveGallery && !galleryRow.Isorder_numNull()) || (!isActiveGallery && !inActiveGalleryRow.Isorder_numNull()))
                    pgallery.GalleryOrder = (isActiveGallery ? galleryRow.order_num : inActiveGalleryRow.order_num);

                if ((isActiveGallery && !galleryRow.IsPIC_MAINNull()) || (!isActiveGallery && !inActiveGalleryRow.IsPIC_MAINNull()))
                    pgallery.MainPic = (isActiveGallery ? galleryRow.PIC_MAIN : inActiveGalleryRow.PIC_MAIN);

                if ((isActiveGallery && !galleryRow.IsMAIN_CULTURENull()) || (!isActiveGallery && !inActiveGalleryRow.IsMAIN_CULTURENull()))
                    pgallery.MainCulture = (isActiveGallery ? galleryRow.MAIN_CULTURE : inActiveGalleryRow.MAIN_CULTURE);

                if ((isActiveGallery && !galleryRow.IsCULTURENull()) || (!isActiveGallery && !inActiveGalleryRow.IsCULTURENull()))
                    pgallery.Culture = (isActiveGallery ? galleryRow.CULTURE : inActiveGalleryRow.CULTURE);

                if ((isActiveGallery && !galleryRow.IsPictureSizeNull()) || (!isActiveGallery && !inActiveGalleryRow.IsPictureSizeNull()))
                    pgallery.PictureSize = (isActiveGallery ? galleryRow.PictureSize : inActiveGalleryRow.PictureSize);

                if ((isActiveGallery && !galleryRow.IsBooleanParamNull()) || (!isActiveGallery && !inActiveGalleryRow.IsBooleanParamNull()))
                    pgallery.BooleanParam = (isActiveGallery ? Convert.ToBoolean(galleryRow.BooleanParam) : Convert.ToBoolean(inActiveGalleryRow.BooleanParam));

                if ((isActiveGallery && !galleryRow.IsFamily_NumNull()) || (!isActiveGallery && !inActiveGalleryRow.IsFamily_NumNull()))
                    pgallery.FamilyID = (isActiveGallery ? galleryRow.Family_Num : inActiveGalleryRow.Family_Num);

                if ((isActiveGallery && !galleryRow.IsNumericParamNull()) || (!isActiveGallery && !inActiveGalleryRow.IsNumericParamNull()))
                    pgallery.NumericParam = (isActiveGallery ? galleryRow.NumericParam : inActiveGalleryRow.NumericParam);

                //pgallery.IsPoster = galleryRow.IsPoster;

                if ((isActiveGallery && !galleryRow.IsNumOfItemsNull()) || (!isActiveGallery && !inActiveGalleryRow.IsNumOfItemsNull()))
                    pgallery.NumOfItems = (isActiveGallery ? galleryRow.NumOfItems : inActiveGalleryRow.NumOfItems);//this propery is the numbers of items show s when the gallery open (not the total number of items)

                if ((isActiveGallery && !galleryRow.IsLinkHeaderNull()) || (!isActiveGallery && !inActiveGalleryRow.IsLinkHeaderNull()))
                    pgallery.LinksHeader = (isActiveGallery ? galleryRow.LinkHeader : inActiveGalleryRow.LinkHeader);

                if ((isActiveGallery && !galleryRow.Isitem_linkNull()) || (!isActiveGallery && !inActiveGalleryRow.Isitem_linkNull()))
                    pgallery.Link = (isActiveGallery ? galleryRow.item_link : inActiveGalleryRow.item_link);

                if ((isActiveGallery && !galleryRow.IsGroupTitleNull()) || (!isActiveGallery && !inActiveGalleryRow.IsGroupTitleNull()))
                    pgallery.GroupTitle = (isActiveGallery ? galleryRow.GroupTitle : inActiveGalleryRow.GroupTitle);

                if ((isActiveGallery && !galleryRow.IsMAIN_DESCRIPTIONNull()) || (!isActiveGallery && !inActiveGalleryRow.IsMAIN_DESCRIPTIONNull()))
                    pgallery.MainDescription = (isActiveGallery ? galleryRow.MAIN_DESCRIPTION : inActiveGalleryRow.MAIN_DESCRIPTION);

                if ((isActiveGallery && !galleryRow.IsSUB_DESCRIPTIONNull()) || (!isActiveGallery && !inActiveGalleryRow.IsSUB_DESCRIPTIONNull()))
                    pgallery.SubDescription = (isActiveGallery ? galleryRow.SUB_DESCRIPTION : inActiveGalleryRow.SUB_DESCRIPTION);

                if ((isActiveGallery && !galleryRow.IsSWFNull()) || (!isActiveGallery && !inActiveGalleryRow.IsSWFNull()))
                    pgallery.SWFFile = (isActiveGallery ? galleryRow.SWF : inActiveGalleryRow.SWF);

                if (isActiveGallery)
                {
                    AddLocalesToGallery(pgallery, galleryRow.GetGalleryLocalesRows());
                }
                else
                {
                    AddLocalesToGallery(pgallery, inActiveGalleryRow.GetGalleryLocalesRows());
                }

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
                    if (isActiveGallery)
                    {
                        page.Galleries.Add(pgallery);
                    }
                    else
                    {
                        page.InActiveGalleries.Add(pgallery);
                    }
                }
                else
                {
                    //Child gallery - add it to the parent's children list
                    if (pgallery.MainCulture != null)
                    {
                        galleryDict[pgallery.MainCulture][pgallery.GalleryID].AddChildGallery(pgallery);
                    }
                }
            }
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Extract the page galleries by location and culture.
        /// </summary>
        public IEnumerable<PageGallery> GetCurrentPageGalleries(Enums.eGalleryLocation Location)
        {
            PageContext currentPage = GetCurrentPage();

            if (currentPage == null)
                return null;

            IEnumerable<PageGallery> PageGalleryList =
                from galleries in getGelleries(currentPage.Galleries, currentPage.InActiveGalleries)
                where galleries.GalleryLocation != null && galleries.GalleryLocation.Equals(Location.ToString()) &&
                (galleries.MainCulture == null || galleries.MainCulture.Equals(TextLocalization.Instance.UserContext.Culture))
                select galleries;

            return PageGalleryList;
        }

        private IEnumerable<PageGallery> getGelleries(IEnumerable<PageGallery> ActiveGalleries, IEnumerable<PageGallery> InActiveGalleries)
        {
            if (SessionHelper.LocaleInfo != null && SessionHelper.LocaleInfo.IsAdminLocale)
                return ActiveGalleries.Union(InActiveGalleries);
            else
                return ActiveGalleries;
        }
        public IEnumerable<PageGallery> GetCurrentPageGalleries(Enums.eGalleryLocation Location, Locale locale)
        {
            PageContext currentPage = GetCurrentPage();

            if (currentPage == null)
                return null;

            //Get all galleries from page according to user language
            IEnumerable<PageGallery> PageGalleryList =
                from galleries in currentPage.Galleries
                where galleries.GalleryLocation.Equals(Location.ToString()) &&
                (galleries.MainCulture == null || galleries.MainCulture.Equals(TextLocalization.Instance.UserContext.Culture))
                select galleries;

            if (locale != null && locale.IsAdminLocale)
            {
                PageGalleryList = PageGalleryList.Union(from galleries in currentPage.InActiveGalleries
                                                        where galleries.GalleryLocation.Equals(Location.ToString()) &&
                                                        (galleries.MainCulture == null || galleries.MainCulture.Equals(TextLocalization.Instance.UserContext.Culture))
                                                        select galleries);
            }
            //Group the galleries by family num
            Dictionary<long, List<PageGallery>> galleriesByFamilies =
                (from p in PageGalleryList
                 group p by p.FamilyID).ToDictionary(gr => gr.Key, gr => gr.ToList());

            List<PageGallery> galleryList = new List<PageGallery>();
            Enums.eLocaleUserState localeUserState = locale.LocaleUserState;

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

            IEnumerable<PageGallery> retVal = from galleries in galleryList
                                              orderby galleries.GalleryOrder ascending
                                              select galleries;

            return retVal;
        }

        //Check if a gallery fits the locale
        private bool IsGalleryInLocale(PageGallery pg, Locale locale, Enums.eLocaleUserState localeUserState)
        {
            if (pg.Locale_Langs != null && !pg.Locale_Langs.Contains(locale.LocaleLanguage))
                return false;

            if (pg.Locale_Devices != null && !pg.Locale_Devices.Contains(locale.LocaleDevice))
                return false;

            if (pg.Locale_Countrys != null && !pg.Locale_Countrys.Contains(locale.LocaleCountry))
                return false;

            if (pg.Locale_UserStates != null && !pg.Locale_UserStates.Contains((long)localeUserState))
                return false;

            return true;
        }

        //Get the gallery that most matches the user's locale. The locale attributes are prioreterized 
        //From high to low : Device -> Country -> Language -> User State
        private PageGallery GetPageGalleryByLocaleScore(List<PageGallery> galleries, Locale locale, TVPPro.SiteManager.Context.Enums.eLocaleUserState localeUserState)
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

        public PageContext GetCurrentPage()
        {
            if (HttpContext.Current == null || HttpContext.Current.Items == null)
                return null;

            object pageObject = pageContext;

            if (pageObject == null)
                return null;

            return pageObject as PageContext;
        }

        public void SetCurrentPage(Enums.ePages pageToken)
        {
            SetCurrentPage(pageToken.ToString());
        }

        public void SetCurrentPage(string pageToken)
        {
            Dictionary<string, PageContext> dict;
            if (m_LanguageTokenPages.TryGetValue(TextLocalization.Instance.UserContext.Culture, out dict))
            {
                PageContext page;
                if (dict.TryGetValue(pageToken, out page))
                {
                    pageContext = page;
                }
                else
                {
                    string msg = string.Format("Page token '{0}' in language {1} not found", pageToken, TextLocalization.Instance.UserContext.Culture);
                    logger.Error(msg);
                    //throw new Exception(msg);
                }
            }
            else
            {
                string msg = string.Format("No pages found in language {0}", TextLocalization.Instance.UserContext.Culture);
                logger.Error(msg);
                throw new Exception(msg);
            }
        }

        public void SetCurrentPage(long pageID)
        {
            Dictionary<long, PageContext> dict;

            if (m_LanguageIDPages.TryGetValue(TextLocalization.Instance.UserContext.Culture, out dict))
            {
                PageContext page;
                if (dict.TryGetValue(pageID, out page) && (page.IsActive || (SessionHelper.LocaleInfo != null && SessionHelper.LocaleInfo.IsAdminLocale)))
                {
                    pageContext = page;
                }
                else
                {
                    dsPageData.PagesRow drPage = (from p in DataOnPage.Pages where p.ID.Equals(pageID) select p).FirstOrDefault();
                    PageContext pg = CreatePageItem(drPage);

                    if (pg != null)
                    {
                        pageContext = pg;
                    }
                    else
                    {
                        string msg = string.Format("Page id '{0}' in language {1} not found", pageID, TextLocalization.Instance.UserContext.Culture);
                        logger.Error(msg);
                        throw new Exception(msg);
                    }
                }
            }
            else
            {
                string msg = string.Format($"No pages found in language {TextLocalization.Instance.UserContext.Culture}");
                logger.Error(msg);
                throw new Exception(msg);
            }
        }

        //public void SetCurrentPage(iucon.web.Controls.ParameterCollection IuconParams)
        //{
        //    long pageID = 0;
        //    long.TryParse(IuconParams["PageID"], out pageID);
        //    TVPPro.SiteManager.Context.Enums.ePages PageToken = (TVPPro.SiteManager.Context.Enums.ePages)Enum.Parse(typeof(TVPPro.SiteManager.Context.Enums.ePages), IuconParams["PageToken"], true);

        //    if (pageID != 0)
        //        SetCurrentPage(pageID);
        //    else if (PageToken != TVPPro.SiteManager.Context.Enums.ePages.UnKnown && PageToken != Enums.ePages.Dynamic)
        //        SetCurrentPage(PageToken);
        //}

        //Get TVM Account params by MediaType
        public TVMAccountType GetTVMAccountByMediaType(int tvmMediaID)
        {
            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where ((!accounts.IsTvmTypeIDNull()) && accounts.TvmTypeID == tvmMediaID)
                                                select accounts).FirstOrDefault();

            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        //Get TVM Accounts by Group ID
        public TVMAccountType GetTVMAccountByGroupID(int groupID)
        {
            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where (accounts.Base_Group_ID == groupID && !string.IsNullOrEmpty(accounts.Player_UN))
                                                select accounts).FirstOrDefault();

            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        public TVMAccountType GetTVMAccountByUserName(string userName)
        {
            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where (accounts.Player_UN == userName)
                                                select accounts).FirstOrDefault();

            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }


        //Get TVM Accounts by Account ID - there can be more than one per ID so the media type is also necessary
        public TVMAccountType GetTVMAccountByAccountType(Context.Enums.eAccountType accountType)
        {
            string AccountName = accountType.ToString();
            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where accounts.Name == AccountName
                                                select accounts).FirstOrDefault();

            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        public TVMAccountType GetTVMAccountByID(int id, string mediaType)
        {

            dsPageData.TVMAccountsRow tvmRow = (from accounts in DataOnPage.TVMAccounts
                                                where accounts.ID == id && !accounts.IsMediaTypeNull() && accounts.MediaType.ToLower().Equals(mediaType.ToLower())
                                                select accounts).FirstOrDefault();


            TVMAccountType retVal = new TVMAccountType(tvmRow.Player_UN, tvmRow.Player_Pass, tvmRow.Name, tvmRow.Group_ID, tvmRow.Group_ID, tvmRow.Api_Ws_User, tvmRow.Api_Ws_Password);
            return retVal;
        }

        public void SetBreadCrumb(string BreadCrumbText)
        {
            pageContext.BreadCrumbText = BreadCrumbText;
        }

        public long? GetProfileIDFromPageID(long? pageID)
        {
            if (pageID.HasValue)
                return m_LanguageIDPages[TextLocalization.Instance.UserContext.Culture][(long)pageID].ProfileID;
            else return null;
        }

        internal long? GetPageIDFromURL(string sPageUrl)
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
                iRet = (from entry in m_LanguageIDPages[TextLocalization.Instance.UserContext.Culture]
                        where entry.Value.URL != null && sPageUrl.ToLower().Contains(entry.Value.URL.ToLower()) && entry.Value.IsActive
                        select (long?)entry.Key).FirstOrDefault();
            }

            return iRet;
        }

        internal string GetPageNameFromID(long? iPadeID)
        {
            if (iPadeID != null)
            {
                return m_LanguageIDPages[TextLocalization.Instance.UserContext.Culture][(long)iPadeID].Name;
            }

            return "NoTitle";
        }

        //public void SetPageHierarchy(ref dsItemInfo ItemInfo)
        //{
        //    PageHierarchy = new List<BreadCrumbItem>();

        //    if (ItemInfo != null && ItemInfo.Item.Rows.Count > 0)
        //    {
        //        string sMediaType = ItemInfo.Item[0].MediaType;
        //        string sMediaTitle = ItemInfo.Item[0].Title;

        //        for (var i = 0; i < SiteConfiguration.Instance.Data.Pages.Heirarchies.Count; i++)
        //        {
        //            if (ItemInfo.Item[0].MediaType.Equals(SiteConfiguration.Instance.Data.Pages.Heirarchies[i].MediaType, StringComparison.OrdinalIgnoreCase))
        //            {

        //                for (var j = 0; j < SiteConfiguration.Instance.Data.Pages.Heirarchies[i].Count; j++)
        //                {
        //                    var sBreadCrumb = string.Empty;
        //                    var sLink = string.Empty;

        //                    if (SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Table.Equals("Tags", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        if (ItemInfo.Item.Rows.Count > 0 && ItemInfo.Item[0].GetChildRows("Item_Tags").Length > 0 && ItemInfo.Item[0].GetChildRows("Item_Tags")[0].Table.Columns.Contains(SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value))
        //                        {
        //                            sBreadCrumb = ItemInfo.Item[0].GetChildRows("Item_Tags")[0][SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value].ToString();
        //                        }
        //                        TVPPro.SiteManager.DataLoaders.SearchMediaIDLoader.MediaIDTypePair showInfoPair = new SearchMediaIDLoader() { ExactSearch = false, MediaName = sBreadCrumb }.Execute();
        //                        if (showInfoPair.ID != 0) sLink = SiteHelper.GetPageURL(showInfoPair.Type, showInfoPair.ID, showInfoPair.SeriesName);
        //                    }
        //                    else if (SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Table.Equals("Metas", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        if (ItemInfo.Item.Rows.Count > 0 && ItemInfo.Item[0].GetChildRows("Item_Metas").Length > 0 && ItemInfo.Item[0].GetChildRows("Item_Tags")[0].Table.Columns.Contains(SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value))
        //                        {
        //                            sBreadCrumb = ItemInfo.Item[0].GetChildRows("Item_Metas")[0][SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value].ToString();
        //                        }
        //                        TVPPro.SiteManager.DataLoaders.SearchMediaIDLoader.MediaIDTypePair showInfoPair = new SearchMediaIDLoader() { ExactSearch = false, MediaName = sBreadCrumb }.Execute();
        //                        if (showInfoPair.ID != 0) sLink = SiteHelper.GetPageURL(showInfoPair.Type, showInfoPair.ID, showInfoPair.SeriesName);
        //                    }
        //                    else if (SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Table.Equals("Item", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        sBreadCrumb = ItemInfo.Item[0][SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value].ToString();
        //                    }
        //                    else
        //                    {
        //                        sBreadCrumb = string.Concat(SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].Value);
        //                        sLink = SiteConfiguration.Instance.Data.Pages.Heirarchies[i][j].URL;
        //                    }

        //                    PageHierarchy.Add(new BreadCrumbItem(sBreadCrumb, sLink));
        //                }

        //                break;
        //            }
        //        }
        //        //pageHierarchy = 
        //    }
        //}
       
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
        public TVPPro.SiteManager.Context.Enums.ePages PageToken { get; set; }
        public TVPPro.SiteManager.DataLoaders.CustomLayoutLoader.CustomLayout CustomLayout { get; internal set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public long PlayerChannel { get; set; }
        public long PlayerTreeCategory { get; set; }
        public long CarouselChannel { get; set; }

        public long MenuID { get; set; }
        public long FooterID { get; set; }
        public long MiddleFooterID { get; set; }
        public long ProfileID { get; set; }
        public bool IsProtected { get; set; }
        public bool IsActive { get; set; }
        public long BrandingBigImageID { get; set; }
        public long BrandingSmallImageID { get; set; }
        public long BrandingPixelHeigt { get; set; }
        public int BrandingRecurringHorizonal { get; set; }
        public int BrandingRecurringVertical { get; set; }
        public int HaseSideProfile { get; set; }
        public string BodyClass { get; set; }

        private List<PageContext> m_Children = new List<PageContext>();
        public List<PageContext> Children
        {
            get
            {
                return m_Children;
            }
        }


        public PageContext Parent { get; set; }

        private List<PageGallery> m_Galleries = new List<PageGallery>();
        public List<PageGallery> Galleries
        {
            get
            {
                return m_Galleries;
            }
        }

        private List<PageGallery> m_InActiveGalleries = new List<PageGallery>();
        public List<PageGallery> InActiveGalleries
        {
            get
            {
                return m_InActiveGalleries;
            }
        }
        public void InitCustomLayout(TVPPro.SiteManager.Context.Enums.eCustomLayoutItemType layoutItemType, long? itemID)
        {
            CustomLayoutLoader layoutItem = new CustomLayoutLoader();
            layoutItem.ItemType = layoutItemType;

            if (itemID > 0)
            {
                layoutItem.ItemID = (long)itemID;
            }

            TVPPro.SiteManager.DataLoaders.CustomLayoutLoader.CustomLayout layout = layoutItem.Execute();

            if (layout != null)
                CustomLayout = layout;
        }

        public void ResetCustomLayout()
        {
            CustomLayout = null;
        }
    }
    #endregion

    #region PageGallery
    public class PageGallery
    {
        public long GalleryID { get; set; }
        public TVPPro.SiteManager.Context.Enums.eGalleryType GalleryType { get; set; }
        public TVPPro.SiteManager.Context.Enums.eUIGalleryType UIGalleryType { get; set; }
        public string ViewType { get; set; }
        public string Title { get; set; }
        public long TVMChannelID { get; set; }
        public int NumberOfItemsPerPage { get; set; }
        public string PictureSize { get; set; }
        public bool IsPoster { get; set; }
        public int NumOfItems { get; set; }
        public bool BooleanParam { get; set; }
        public long NumericParam { get; set; }

        public string TVMUser { get; set; }
        public string TVMPass { get; set; }
        public string GalleryLocation { get; set; }
        public string MainCulture { get; set; }
        public string Culture { get; set; }
        public string LinksHeader { get; set; }
        public string Link { get; set; }
        public string GroupTitle { get; set; }
        public string MainDescription { get; set; }
        public string SubDescription { get; set; }
        public string SWFFile { get; set; }
        public long GalleryOrder { get; set; }
        public long MainPic { get; set; }
        public List<string> Locale_Langs { get; set; }
        public List<string> Locale_Countrys { get; set; }
        public List<long> Locale_UserStates { get; set; }
        public List<string> Locale_Devices { get; set; }

        public long FamilyID { get; set; }
        public bool IsActive { get; set; }

        protected List<PageGallery> m_childGalleries = new List<PageGallery>();

        public void AddChildGallery(PageGallery pg)
        {
            m_childGalleries.Add(pg);
        }

        public List<PageGallery> ChildGalleries
        {
            get
            {
                //Filter only the related galleries with the user's context culture
                IEnumerable<PageGallery> relatedGalleries = from galleries in this.m_childGalleries
                                                            where (galleries.Culture == null || galleries.Culture.Equals(TextLocalization.Instance.UserContext.Culture))
                                                            select galleries;

                return relatedGalleries.ToList<PageGallery>();
            }
        }
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
