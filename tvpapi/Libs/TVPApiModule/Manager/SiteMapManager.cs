using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using Tvinci.Data.DataLoader;
using System.Threading;
using System.Reflection;
using KLogMonitor;
using TVinciShared;

/// <summary>
/// Summary description for SiteMapManager
/// </summary>
/// 

namespace TVPApi
{
    public class SiteMapManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private Dictionary<string, SiteMap> m_siteMapInstances = null;
        private Dictionary<long, Profile> m_Profiles = new Dictionary<long, Profile>();
        private Dictionary<string, PageData> m_pageData = new Dictionary<string, PageData>();

        private static ReaderWriterLockSlim m_SiteMapsLocker = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim m_ProfilesLocker = new ReaderWriterLockSlim();
        private static ReaderWriterLockSlim m_PageDataLocker = new ReaderWriterLockSlim();

        private static string m_currentGroupID = null;

        private SiteMap m_UnifiedSiteMap = null;
        private static SiteMapManager m_siteMapManager = null;

        static ILoaderCache m_dataCaching = LoaderCacheLite.Current;

        //Get instance by group ID and platform
        public static SiteMapManager GetInstance
        {
            get
            {
                if (m_siteMapManager == null)
                    m_siteMapManager = new SiteMapManager();

                return m_siteMapManager;
            }
        }

        #region C'tor
        private SiteMapManager()
        {

        }
        #endregion

        public PageData GetPageData(int groupID, PlatformType platform)
        {
            string sKey = m_siteMapManager.GetKey(groupID, platform);

            PageData retPageData = null;

            // read PageData from shared dictionary
            if (m_PageDataLocker.TryEnterReadLock(1000))
            {
                try
                {
                    m_pageData.TryGetValue(sKey, out retPageData);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPageData->", ex);
                }
                finally
                {
                    m_PageDataLocker.ExitReadLock();
                }
            }

            // if not exsist in shared dictionary create one
            if (retPageData == null)
            {
                retPageData = new PageData(groupID, platform);
            }

            // add PageData to shared dictionary if not exsist
            if (m_PageDataLocker.TryEnterWriteLock(1000))
            {
                try
                {
                    if (!m_pageData.Keys.Contains(sKey))
                    {
                        m_pageData.Add(sKey, retPageData);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("GetPageData->", ex);
                }
                finally
                {
                    m_PageDataLocker.ExitWriteLock();
                }
            }

            return retPageData;
        }

        public Dictionary<string, SiteMap> GetSiteMaps()
        {
            return m_siteMapInstances;
        }

        public SiteMap GetSiteMapInstance(int groupID, PlatformType platform, Locale locale)
        {
            SiteMap tempMap = null;

            logger.InfoFormat("Start Site Map : {0} {1}", groupID, platform);

            string keyStr = GetKey(groupID, platform);

            logger.InfoFormat("Site Map Get Instance : {0}", keyStr);

            //SiteMapManager retVal;
            //if (m_dataCaching.TryGetData<SiteMapManager>(GetUniqueCacheKey(groupID.ToString()), out retVal))
            //{
            //    Logger.Logger.Log("Site Map Found in cache :", "Group ID " + keyStr, "TVPApi");
            //    if (retVal != null && retVal is SiteMapManager)
            //    {
            //        return retVal;
            //    }
            //}
            if (m_siteMapInstances == null)
            {
                logger.InfoFormat("New Site Map :", "Key Str {0}", keyStr);

                m_siteMapInstances = new Dictionary<string, SiteMap>();
            }

            //If this is the first time a group ID is used - initialize a new manager and all relevent objects

            if (m_SiteMapsLocker.TryEnterWriteLock(1000))
            {
                try
                {
                    if (!m_siteMapInstances.ContainsKey(keyStr))
                    {
                        logger.InfoFormat("New Key Str : {0}", keyStr);

                        m_siteMapInstances.Add(keyStr, CreateSiteMap(groupID, platform));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("GetSiteMapInstance->", ex);
                }
                finally
                {
                    m_SiteMapsLocker.ExitWriteLock();
                }
            }


            // If item already exist

            if (m_SiteMapsLocker.TryEnterReadLock(1000))
            {
                try
                {
                    m_siteMapInstances.TryGetValue(keyStr, out tempMap);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSiteMapInstance->", ex);
                }
                finally
                {
                    m_SiteMapsLocker.ExitReadLock();
                }
            }


            if (locale != null)
            {
                foreach (PageContext page in tempMap.GetPages())
                {
                    page.SetLocaleGalleries(PageDataHelper.GetLocaleGalleries(page.GetGalleries(), locale));
                }
            }

            return tempMap;
        }

        public string GetKey(int groupID, PlatformType platform)
        {
            string keyStr = groupID.ToString();
            if (platform != PlatformType.Unknown)
            {
                keyStr = string.Concat(keyStr, platform.ToString().ToLower());
            }

            return keyStr;
        }

        //public SiteMapManager GetMember(int groupID, PlatformType platform)
        //{
        //    lock (instanceLock)
        //    {
        //        SiteMapManager tempMng = null;
        //        Logger.Logger.Log("Start Site Map :", groupID.ToString() + " " + platform.ToString(), "TVPApi");
        //        string keyStr = groupID.ToString();
        //        if (platform != PlatformType.Unknown)
        //        {
        //            keyStr = string.Concat(keyStr, platform.ToString());
        //        }

        //        Logger.Logger.Log("Site Map Get Instance :", "Key String is " + keyStr, "TVPApi");
        //        //SiteMapManager retVal;
        //        //if (m_dataCaching.TryGetData<SiteMapManager>(GetUniqueCacheKey(groupID.ToString()), out retVal))
        //        //{
        //        //    Logger.Logger.Log("Site Map Found in cache :", "Group ID " + keyStr, "TVPApi");
        //        //    if (retVal != null && retVal is SiteMapManager)
        //        //    {
        //        //        return retVal;
        //        //    }
        //        //}
        //        if (m_instances == null)
        //        {
        //            Logger.Logger.Log("New Site Map :", "Key Str" + keyStr, "TVPApi");
        //            m_instances = new Dictionary<string, SiteMapManager>();
        //        }
        //        //If this is the first time a group ID is used - initialize a new manager and all relevent objects
        //        if (!m_instances.ContainsKey(keyStr))
        //        {
        //            Logger.Logger.Log("New Key Str :", "Key Str" + keyStr, "TVPApi");
        //            m_instances.Add(keyStr, new SiteMapManager(groupID, platform));
        //        }
        //        //m_dataCaching.AddData(GetUniqueCacheKey(groupID.ToString()), m_instances[keyStr], new string[] { }, 3600);
        //        tempMng = m_instances[keyStr];
        //        return tempMng;
        //    }
        //}

        //Clear all site maps
        //public void Clear()
        //{
        //    if (m_instances != null)
        //    {
        //        foreach (KeyValuePair<string, SiteMapManager> managerPair in m_instances)
        //        {
        //            managerPair.Value.ClearSiteMap();
        //        }

        //        m_instances.Clear();
        //    }

        //}

        private static string GetUniqueCacheKey(string groupID)
        {
            string platform = string.Empty;
            if (HttpContext.Current.Items.ContainsKey("Platform"))
            {
                platform = HttpContext.Current.Items["Platform"].ToString();
            }
            string retVal = string.Format("SiteMap|GroupID={0}|Platform={1}", groupID, platform);
            return retVal;
        }


        private void ClearSiteMap()
        {
            if (m_siteMapInstances != null)
            {
                m_siteMapInstances.Clear();
            }
            m_Profiles.Clear();
        }


        //Init the new Site map manager with site maps
        //private SiteMapManager(int groupID, PlatformType platfrom)
        //{
        //    m_currentGroupID = groupID.ToString();
        //    Init(groupID, platfrom);
        //}


        public static string GetCurrentLanguageID()
        {
            return m_currentGroupID;
        }

        private SiteMap CreateSiteMap(int groupID, PlatformType platform)
        {
            SiteMap siteMap = null;

            PageData pageData = GetPageData(groupID, platform);

            Dictionary<string, Dictionary<long, PageContext>> pages = pageData.GetPageContextsIDDict();
            if (pages != null)
            {
                siteMap = new SiteMap();
                //for each page language - create a site map
                foreach (KeyValuePair<string, Dictionary<long, PageContext>> langPagePair in pages)
                {
                    foreach (KeyValuePair<long, PageContext> pagePair in langPagePair.Value)
                    {
                        siteMap.GetPages().Add(pagePair.Value);

                        if (siteMap.PagesInfo == null)
                        {
                            siteMap.PagesInfo = new List<PageInfo>();
                        }

                        //Create shallow page info object
                        siteMap.PagesInfo.Add(PageDataHelper.ParsePageContextToPageInfo(pagePair.Value));

                        if (!m_Profiles.ContainsKey(pagePair.Value.SideProfileID) && pagePair.Value.SideProfileID > 0)
                        {
                            Profile profile = CreateProfile(pagePair.Value, pagePair.Value.SideProfileID, TVPApi.GalleryLocation.Side, langPagePair.Key, groupID, platform);
                            m_Profiles.Add(profile.ProfileID, profile);

                            siteMap.GetSideProfiles().Add(profile);
                            ProfileInfo profileInfo = ProfileHelper.ParseProfileToProfileInfo(profile);
                            if (siteMap.SideProfilesInfo == null)
                            {
                                siteMap.SideProfilesInfo = new List<ProfileInfo>();
                            }
                            siteMap.SideProfilesInfo.Add(profileInfo);
                        }

                        //Create profile objects
                        if (!m_Profiles.ContainsKey(pagePair.Value.BottomProfileID) && pagePair.Value.BottomProfileID > 0)
                        {
                            Profile profile = CreateProfile(pagePair.Value, pagePair.Value.BottomProfileID, TVPApi.GalleryLocation.Bottom, langPagePair.Key, groupID, platform);
                            m_Profiles.Add(profile.ProfileID, profile);

                            siteMap.GetBottomProfiles().Add(profile);
                            ProfileInfo profileInfo = ProfileHelper.ParseProfileToProfileInfo(profile);
                            if (siteMap.BottomProfileInfo == null)
                            {
                                siteMap.BottomProfileInfo = new List<ProfileInfo>();
                            }
                            siteMap.BottomProfileInfo.Add(profileInfo);

                        }
                    }
                    //LogManager.Instance.Log(groupID, "SiteMapManager", "Added " + m_siteMaps[langPagePair.Key].PagesInfo.Count + " Pages");
                }
            }
            Dictionary<string, Dictionary<long, List<MenuItem>>> menues;
            Dictionary<string, Dictionary<long, List<MenuItem>>> footers;

            // get all relevent menues/footers from Menu manager singleton

            menues = MenuBuilder.GetInstance(groupID, platform).GetMenuLangDict();
            string keyStr = string.Concat(groupID.ToString(), platform.ToString());

            footers = MenuBuilder.GetInstance(groupID, platform).GetFooterLangDict();


            if (menues != null)
            {

                foreach (KeyValuePair<string, Dictionary<long, List<MenuItem>>> menuLangPair in menues)
                {
                    logger.InfoFormat("Add menu to site map->", menuLangPair.Key);

                    //if (!m_siteMapInstances.ContainsKey(keyStr))
                    //{
                    //    continue;
                    //}
                    foreach (KeyValuePair<long, List<MenuItem>> menuPair in menuLangPair.Value)
                    {
                        Menu menu = new Menu();
                        menu.ID = menuPair.Key;
                        menu.MenuItems = menuPair.Value;
                        menu.Type = MenuBuilder.MenuType.Menu;

                        AddMenu(keyStr, menu, ref siteMap);
                    }
                }

            }

            if (footers != null)
            {

                foreach (KeyValuePair<string, Dictionary<long, List<MenuItem>>> menuLangPair in footers)
                {
                    logger.InfoFormat("Add footer menu to site map->", menuLangPair.Key);

                    //if (!m_siteMapInstances.ContainsKey(keyStr))//menuLangPair.Key))
                    //{
                    //    //m_siteMapInstances.Add(menuLangPair.Key, new SiteMap());
                    //    continue;
                    //}
                    foreach (KeyValuePair<long, List<MenuItem>> menuPair in menuLangPair.Value)
                    {
                        Menu menu = new Menu();
                        menu.ID = menuPair.Key;
                        menu.MenuItems = menuPair.Value;

                        menu.Type = MenuBuilder.MenuType.Footer;
                        AddFooter(keyStr, menu, ref siteMap);


                    }
                }

            }
            return siteMap;
        }

        //Get site map by locale
        //public SiteMap GetSiteMap(Locale locale)
        //{
        //    SiteMap retVal = null;

        //    if (locale != null && !string.IsNullOrEmpty(locale.LocaleLanguage))
        //    {
        //        //Get specific language site map
        //        if (m_siteMaps.ContainsKey(locale.LocaleLanguage))
        //        {
        //            retVal = m_siteMaps[locale.LocaleLanguage];

        //        }
        //    }
        //    //If no language specified - get all site maps
        //    else
        //    {
        //        retVal = m_UnifiedSiteMap;
        //    }

        //    //Filter galleries by locale
        //    if (retVal != null && locale != null)
        //    {
        //        foreach (PageContext page in retVal.GetPages())
        //        {
        //            page.SetLocaleGalleries(PageDataHelper.GetLocaleGalleries(page.GetGalleries(), locale));
        //        }
        //    }

        //    return retVal;
        //}

        //private void Init(int groupID, PlatformType platform)
        //{
        //    // If this is a new site map - init the config (languages, DB connection strings)
        //    LogManager.Instance.Log(groupID, "SiteMapManager", "Init Site Map Manager Started");
        //    ConnectionHelper.InitServiceConfigs(groupID, platform);
        //    FillPages(groupID, platform);
        //    FillMenues(groupID, MenuBuilder.MenuType.Menu, platform);
        //    FillMenues(groupID, MenuBuilder.MenuType.Footer, platform);
        //    //Create the unified site map (for use when no locale is specified
        //    foreach (KeyValuePair<string, SiteMap> sitePair in m_siteMaps)
        //    {
        //        if (m_UnifiedSiteMap == null)
        //        {
        //            m_UnifiedSiteMap = new SiteMap();
        //        }
        //        CreateUnifiedSiteMap(sitePair.Value);
        //    }
        //    LogManager.Instance.Log(groupID, "SiteMapManager", "Init Site Map Manager Completed");
        //}


        //A unified site map for all Languages (easier to search with empty locale
        private void CreateUnifiedSiteMap(SiteMap siteMap)
        {
            m_UnifiedSiteMap.GetPages().AddRange(siteMap.GetPages());

            m_UnifiedSiteMap.PagesInfo.AddRange(siteMap.PagesInfo);

            m_UnifiedSiteMap.Menues.AddRange(siteMap.Menues);
            m_UnifiedSiteMap.Footers.AddRange(siteMap.Footers);
            m_UnifiedSiteMap.GetSideProfiles().AddRange(siteMap.GetSideProfiles());
            m_UnifiedSiteMap.GetBottomProfiles().AddRange(siteMap.GetBottomProfiles());
            m_UnifiedSiteMap.SideProfilesInfo.AddRange(siteMap.SideProfilesInfo);
            m_UnifiedSiteMap.BottomProfileInfo.AddRange(siteMap.BottomProfileInfo);
        }

        //private void FillSitePages(int groupID, PlatformType platform)
        //{
        //    Dictionary<string, Dictionary<long, PageContext>> pages = PageData.GetInstance.GetMember(groupID, platfrom).GetPageContextsIDDict();
        //    if (pages != null)
        //    {
        //        Logger.Logger.Log("Fill Pages", "Start Fill Pages", "TVPApi");
        //        foreach (KeyValuePair<string, Dictionary<long, PageContext>> langPagePair in pages)
        //        {

        //        }
        //    }
        //}

        //Fill sitemaps pages
        //private void FillPages(int groupID, PlatformType platfrom)
        //{
        //    Dictionary<string, Dictionary<long, PageContext>> pages = PageData.GetInstance.GetMember(groupID, platfrom).GetPageContextsIDDict();
        //    if (pages != null)
        //    {
        //        //for each page language - create a site map
        //        foreach (KeyValuePair<string, Dictionary<long, PageContext>> langPagePair in pages)
        //        {
        //            LogManager.Instance.Log(groupID, "SiteMapManager", "Adding " + langPagePair.Key + " pages to site map");
        //            if (!m_siteMaps.ContainsKey(langPagePair.Key))
        //            {
        //                m_siteMaps.Add(langPagePair.Key, new SiteMap());
        //            }
        //            foreach (KeyValuePair<long, PageContext> pagePair in langPagePair.Value)
        //            {
        //                m_siteMaps[langPagePair.Key].GetPages().Add(pagePair.Value);

        //                if (m_siteMaps[langPagePair.Key].PagesInfo == null)
        //                {
        //                    m_siteMaps[langPagePair.Key].PagesInfo = new List<PageInfo>();
        //                }
        //                //Create shallow page info object
        //                m_siteMaps[langPagePair.Key].PagesInfo.Add(PageDataHelper.ParsePageContextToPageInfo(pagePair.Value));

        //                if (!m_Profiles.ContainsKey(pagePair.Value.SideProfileID) && pagePair.Value.SideProfileID > 0)
        //                {
        //                    Profile profile = CreateProfile(pagePair.Value, pagePair.Value.SideProfileID, TVPApi.GalleryLocation.Side, langPagePair.Key, groupID, platfrom);
        //                    m_Profiles.Add(profile.ProfileID, profile);

        //                    m_siteMaps[langPagePair.Key].GetSideProfiles().Add(profile);
        //                    ProfileInfo profileInfo = ProfileHelper.ParseProfileToProfileInfo(profile);
        //                    if (m_siteMaps[langPagePair.Key].SideProfilesInfo == null)
        //                    {
        //                        m_siteMaps[langPagePair.Key].SideProfilesInfo = new List<ProfileInfo>();
        //                    }
        //                    m_siteMaps[langPagePair.Key].SideProfilesInfo.Add(profileInfo);
        //                }
        //                //Create profile objects
        //                if (!m_Profiles.ContainsKey(pagePair.Value.BottomProfileID) && pagePair.Value.BottomProfileID > 0)
        //                {
        //                    Profile profile = CreateProfile(pagePair.Value, pagePair.Value.BottomProfileID, TVPApi.GalleryLocation.Bottom, langPagePair.Key, groupID, platfrom);
        //                    m_Profiles.Add(profile.ProfileID, profile);

        //                    m_siteMaps[langPagePair.Key].GetBottomProfiles().Add(profile);
        //                    ProfileInfo profileInfo = ProfileHelper.ParseProfileToProfileInfo(profile);
        //                    if (m_siteMaps[langPagePair.Key].BottomProfileInfo == null)
        //                    {
        //                        m_siteMaps[langPagePair.Key].BottomProfileInfo = new List<ProfileInfo>();
        //                    }
        //                    m_siteMaps[langPagePair.Key].BottomProfileInfo.Add(profileInfo);

        //                }
        //            }
        //            LogManager.Instance.Log(groupID, "SiteMapManager", "Added " + m_siteMaps[langPagePair.Key].PagesInfo.Count + " Pages");
        //        }
        //    }

        //}


        //Fill site maps menues
        //private void FillMenues(int groupID, MenuBuilder.MenuType type, PlatformType platform)
        //{
        //    Dictionary<string, Dictionary<long, List<MenuItem>>> menues;

        //    // get all relevent menues/footers from Menu manager singleton
        //    if (type == MenuBuilder.MenuType.Menu)
        //    {
        //        menues = MenuBuilder.GetInstance(groupID, platform).GetMenuLangDict();
        //    }
        //    else
        //    {
        //        menues = MenuBuilder.GetInstance(groupID, platform).GetFooterLangDict();
        //    }

        //    if (menues != null)
        //    {

        //        foreach (KeyValuePair<string, Dictionary<long, List<MenuItem>>> menuLangPair in menues)
        //        {
        //            LogManager.Instance.Log(groupID, "SiteMapManager", "Adding " + menuLangPair.Key + " menues to site map");
        //            if (!m_siteMaps.ContainsKey(menuLangPair.Key))
        //            {
        //                m_siteMaps.Add(menuLangPair.Key, new SiteMap());
        //            }
        //            foreach (KeyValuePair<long, List<MenuItem>> menuPair in menuLangPair.Value)
        //            {
        //                Menu menu = new Menu();
        //                menu.ID = menuPair.Key;
        //                menu.MenuItems = menuPair.Value;
        //                if (type == MenuBuilder.MenuType.Menu)
        //                {
        //                    menu.Type = MenuBuilder.MenuType.Menu;
        //                    AddMenu(menuLangPair.Key, menu);
        //                }
        //                else
        //                {
        //                    menu.Type = MenuBuilder.MenuType.Footer;
        //                    AddFooter(menuLangPair.Key, menu);
        //                }
        //            }
        //            LogManager.Instance.Log(groupID, "SiteMapManager", "Added " + m_siteMaps[menuLangPair.Key].Menues.Count + " menues to site map");
        //        }

        //    }
        //}

        private void AddMenu(string keyStr, Menu menu, ref SiteMap siteMap)
        {
            if (siteMap.Menues == null)
            {
                siteMap.Menues = new List<Menu>();
            }

            siteMap.Menues.Add(menu);
        }

        private void AddFooter(string keyStr, Menu menu, ref SiteMap siteMap)
        {
            if (siteMap.Footers == null)
            {
                siteMap.Footers = new List<Menu>();
            }

            siteMap.Footers.Add(menu);
        }

        private Profile CreateProfile(PageContext page, long ID, GalleryLocation location, string languageCode, int groupID, PlatformType platform)
        {
            Profile profile = new Profile();
            profile.ProfileID = ID;
            profile.Galleries = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetPageGalleriesByLocation(page, location, languageCode);
            return profile;
        }


    }
}
