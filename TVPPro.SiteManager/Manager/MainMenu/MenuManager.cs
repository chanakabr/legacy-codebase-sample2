using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using KLogMonitor;
using System.Reflection;
using TVinciShared;

namespace TVPPro.SiteManager.Manager
{
    public class MenuBuilder
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //      Dictionary<iMenuID, Dictionary<iMenuID, List<MenueItems>>>
        private Dictionary<string, Dictionary<long, List<MenuPartControl>>> m_dictMenuItems;


        #region Constructor
        private MenuBuilder()
        {
            Init();
        }
        #endregion

        #region Public Properties
        private static MenuBuilder m_Instance;
        public static MenuBuilder Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new MenuBuilder();

                return m_Instance;
            }
        }
        #endregion

        #region Public Methods
        public void Init()
        {
            logger.Info("Starting initialization of main menu manager");

            try
            {
                // Run loader and get data
                dsMenu.MenuDataTable data = new MenuLoader().Execute();

                if (data == null)
                {
                    logger.Error("MainMenuLoader returned null data");
                    return;
                }

                m_dictMenuItems = new Dictionary<string, Dictionary<long, List<MenuPartControl>>>();

                // collect and build ALL menus hierarchy
                CreateMenuItem(data, null, string.Empty);
            }
            catch (Exception ex)
            {
                logger.Error("Failed initialzing main menu manager", ex);
                return;
            }

            logger.Info("Finished initialization of main menu manager");
        }

        //TODO: tvp_new changes: (to delete) override for other websites because of the new DB structure
        public List<MenuPartControl> GetLevelItems(int level) { return new List<MenuPartControl>(); }

        public List<MenuPartControl> GetMenuItems(long iMenuID)
        {
            try
            {
                return m_dictMenuItems[TextLocalization.Instance.UserContext.Culture][iMenuID];

            }
            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null) ex = ex.InnerException;
                if (ex == null) ex = new Exception("Unknown Exception");
            }

            return null;
        }
        #endregion

        #region Private Methods
        private void CreateMenuItem(dsMenu.MenuDataTable sourceData, MenuPartItem parentMenuItem, string culture)
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
                rows = sourceData.Where(row => !row.IsParentItemIDNull() && row.ParentItemID != 0 && row.ParentItemID == parentMenuItem.ID);
            }

            // Run on items
            foreach (dsMenu.MenuRow row in rows)
            {
                // Create menu item
                MenuPartItem item = new MenuPartItem();
                item.ID = row.ItemID;

                if (!row.IsMenuIDNull())
                    item.MenuID = row.MenuID;

                if (!row.IsTitleNull())
                    item.Title = row.Title;

                if (!row.IsIndexNull())
                    item.Index = row.Index;

                if (!row.IsCultureNull())
                    item.Calture = row.Culture;

                if (!row.IsURLNull())
                {
                    item.URL = row.URL;

                    // set SitePageID from URL
                    item.SitePageID = PageData.Instance.GetPageIDFromURL(row.URL);
                }

                if (!row.IsHasNoFollowNull())
                {
                    item.HasNoFollow = row.HasNoFollow;
                }

                // Add menu item to list
                if (parentMenuItem == null)
                {
                    Dictionary<long, List<MenuPartControl>> dictList = new Dictionary<long, List<MenuPartControl>>();
                    // add culture key if not exsist 
                    if (!m_dictMenuItems.ContainsKey(row.Culture))
                    {
                        m_dictMenuItems.Add(row.Culture, new Dictionary<long, List<MenuPartControl>>());
                    }
                    // add MenuID key if not exsist 
                    if (!m_dictMenuItems[row.Culture].ContainsKey(row.MenuID))
                    {
                        m_dictMenuItems[row.Culture].Add(row.MenuID, new List<MenuPartControl>());
                    }

                    m_dictMenuItems[row.Culture][item.MenuID].Add(new MenuPartControl(item));
                }
                else
                {
                    if (item.Calture.Equals(parentMenuItem.Calture))
                    {
                        item.Parent = parentMenuItem;
                        parentMenuItem.Children.Add(item);
                    }
                }

                // Add all of item's children recursively
                CreateMenuItem(sourceData, item, row.Culture);
            }
        }
        #endregion
    }

    #region MainMenuItem
    public class MenuPartItem
    {
        public long ID { get; set; }
        public string URL { get; set; }
        public bool DefaultItem { get; set; }
        public long? SitePageID { get; set; }
        public long MenuID { get; set; }
        public string Title { get; set; }
        public int Index { get; set; }
        public int Total { get; set; }
        public string Calture { get; set; }
        public int HasNoFollow { get; set; }

        public List<MenuPartItem> Children = new List<MenuPartItem>();
        public MenuPartItem Parent { get; set; }

        public bool IsActive
        {
            get
            {
                return CheckItemSelected(this);
            }
        }

        private bool CheckItemSelected(MenuPartItem item)
        {
            // Check if the current item is selected
            if (IsSelectedItem(item))
                return true;

            // Check children
            bool res;
            foreach (MenuPartItem child in item.Children)
            {
                res = CheckItemSelected(child);

                if (res)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSelectedItem(MenuPartItem item)
        {
            PageContext curPage = PageData.Instance.GetCurrentPage();
            if (curPage == null)
                return false;

            if ((item.SitePageID.HasValue && item.SitePageID.Value == curPage.ID) ||
                (!string.IsNullOrEmpty(item.URL) &&
                System.Web.HttpContext.Current.Request.GetUrl().AbsoluteUri.ToLower().EndsWith(item.URL.ToLower())))
            {
                return true;
            }

            return false;
        }
    }
    #endregion

    //public class MenuContentPart : ContentPart<MenuPartControl>
    //{
    //    [System.Web.UI.TemplateContainer(typeof(ContentPartItem<MenuPartControl>))]
    //    public override System.Web.UI.ITemplate Template { get; set; }
    //}

    //public class MenuInnerItemTemplate : TemplatedContainer
    //{
    //    [TemplateContainer(typeof(MenuPartControl))]
    //    public override System.Web.UI.ITemplate Template { get; set; }
    //}

    public class MenuPartControl
    {
        public MenuPartItem MenuItem { get; private set; }

        public MenuPartControl(MenuPartItem item)
        {
            MenuItem = item;
        }

        public bool IsActive()
        {
            return MenuItem.IsActive;
        }

        public string GetLink()
        {
            return MenuItem.URL;
        }
    }
}
