using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;


/// <summary>
/// Summary description for MenuBuilder
/// </summary>
namespace TVPApi
{
    public class MenuBuilder
    {
        public enum MenuType
        {
            Menu = 1,
            Footer = 2
        }

        #region Private Fields
        private Dictionary<string, Dictionary<long, List<MenuItem>>> m_dictMenuItems;
        private Dictionary<string, Dictionary<long, List<MenuItem>>> m_dictFooterItems;
        private static Dictionary<string, MenuBuilder> m_Instances;
        #endregion

        #region Constructor
        private MenuBuilder(int groupID, PlatformType platform)
        {
            Init(groupID, platform);
        }
        #endregion



        #region Public Methods

        public static MenuBuilder GetInstance(int groupID, PlatformType platform)
        {
            string keyStr = groupID.ToString();
            if (platform != PlatformType.Unknown)
            {
                keyStr = string.Concat(keyStr, platform.ToString());
            }
            if (m_Instances == null)
            {
                m_Instances = new Dictionary<string, MenuBuilder>();
            }
            if (!m_Instances.ContainsKey(keyStr))
            {
                m_Instances.Add(keyStr, new MenuBuilder(groupID, platform));
            }
            return m_Instances[keyStr];
        }

        public static void Clear()
        {
            if (m_Instances != null)
            {
                foreach (KeyValuePair<string, MenuBuilder> menuPair in m_Instances)
                {
                    menuPair.Value.ClearMenues();
                }
                m_Instances.Clear();
            }
        }





        public void Init(int groupID, PlatformType platform)
        {
           // m_Logger.Info("Starting initialization of main menu manager");

            try
            {
                // Run loader and get data
                dsMenu.MenuDataTable data = (new APIMenuLoader() { GroupID = groupID, Platform = platform }).Execute();

                if (data == null)
                {
                   // m_Logger.Error("MainMenuLoader returned null data");
                    return;
                }

                m_dictMenuItems = new Dictionary<string, Dictionary<long, List<MenuItem>>>();

                // collect and build ALL menus hierarchy
                CreateMenuItem(data, null, string.Empty, groupID, platform);
            }
            catch (Exception ex)
            {
               // m_Logger.Error("Failed initialzing main menu manager", ex);
                return;
            }

           // m_Logger.Info("Finished initialization of main menu manager");
        }


        public List<MenuItem> GetMenuItems(string languageID, long iMenuID)
        {
            try
            {
                return m_dictMenuItems[languageID][iMenuID];

            }
            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null) ex = ex.InnerException;
                if (ex == null) ex = new Exception("Unknown Exception");
            }

            return null;
        }

        public Dictionary<long, List<MenuItem>> GetMenues(string languageCode)
        {
            try
            {
                return m_dictMenuItems[languageCode];

            }
            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null) ex = ex.InnerException;
                if (ex == null) ex = new Exception("Unknown Exception");
            }

            return null;
        }

        public Dictionary<long, List<MenuItem>> GetFooters(string languageCode)
        {
            try
            {
                return m_dictFooterItems[languageCode];

            }
            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null) ex = ex.InnerException;
                if (ex == null) ex = new Exception("Unknown Exception");
            }

            return null;
        }

        public Dictionary<string, Dictionary<long, List<MenuItem>>> GetMenuLangDict()
        {
            return m_dictMenuItems;
        }

        public Dictionary<string, Dictionary<long, List<MenuItem>>> GetFooterLangDict()
        {
            return m_dictFooterItems;
        }



        #endregion

        #region Private Methods

        private void ClearMenues()
        {
            if (m_dictFooterItems != null)
            {
                m_dictFooterItems.Clear();
            }
            if (m_dictMenuItems != null)
            {
                m_dictMenuItems.Clear();
            }
        }


        private void CreateMenuItem(dsMenu.MenuDataTable sourceData, MenuItem parentMenuItem, string culture, int groupID, PlatformType platform)
        {
            // Get current items
            IEnumerable<dsMenu.MenuRow> rows;
            if (parentMenuItem == null)
            {
                // Get root items
                rows = sourceData.Where(row => row.IsParentItemIDNull() || row.ParentItemID == 0);
            }
            else
            {
                // Get parent's children
                rows = sourceData.Where(row => !row.IsParentItemIDNull() && row.ParentItemID != 0 && row.ParentItemID == parentMenuItem.id);
            }

            // Run on items
            foreach (dsMenu.MenuRow row in rows)
            {
                // Create menu item
                MenuItem item = new MenuItem();
                item.id = row.ItemID;

                if (!row.IsTitleNull())
                    item.name = row.Title;

                if (!row.IsCultureNull())
                {
                    Tvinci.Localization.LanguageContext lc;
                    if (TVPPro.SiteManager.Manager.TextLocalization.Instance.TryGetLanguageByCulture(row.Culture, out lc))
                        item.culture = lc.CultureInfo.DisplayName;
                    else
                        item.culture = row.Culture;
                }
                if (!row.IsURLNull())
                {
                    item.url = row.URL;

                    // set SitePageID from URL
                    long? pageID = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetPageIDFromURL(row.URL, item.culture);
                    if (pageID.HasValue)
                    {
                        item.page_id = pageID.Value;
                    }
                }
                if (!row.IsMenuTypeNull())
                {
                    item.menu_type = (MenuType)(row.MenuType);
                }

                // Add menu item to list
                if (parentMenuItem == null)
                {
                    // add culture key if not exsist 

                    if ((MenuType)(row.MenuType) == MenuType.Menu)
                    {
                        AddItemToDict(ref m_dictMenuItems, row, item);
                    }
                    else
                    {
                        AddItemToDict(ref m_dictFooterItems, row, item);
                    }
                }
                else
                {
                    if (item.culture.Equals(parentMenuItem.culture))
                    {
                        if (parentMenuItem.children == null)
                        {
                            parentMenuItem.children = new List<MenuItem>();
                        }

                        parentMenuItem.children.Add(item);
                    }
                }

                // Add all of item's children recursively
                CreateMenuItem(sourceData, item, row.Culture, groupID, platform);
            }
        }

        private void AddItemToDict(ref Dictionary<string, Dictionary<long, List<MenuItem>>> dict, dsMenu.MenuRow row, MenuItem item)
        {
            if (dict == null)
            {
                dict = new Dictionary<string, Dictionary<long, List<MenuItem>>>();
            }

            if (!dict.ContainsKey(item.culture))
            {
                dict.Add(item.culture, new Dictionary<long, List<MenuItem>>());
            }
            // add MenuID key if not exsist 
            if (!dict[item.culture].ContainsKey(row.MenuID))
            {
                dict[item.culture].Add(row.MenuID, new List<MenuItem>());
            }

            dict[item.culture][row.MenuID].Add(item);
        }

        #endregion
    }


}
