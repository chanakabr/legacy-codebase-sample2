using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Helpers;
using System.Web;
using System.Configuration;
using System.Globalization;
using Tvinci.Localization;
using System.Collections.Specialized;
using Tvinci.Web.HttpModules.Configuration;
using Tvinci.Helpers.Link.Configuration;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Web.Controls.MainMenu
{
    public class MenuItem
    {
        public class LinkInformation
        {
            public string GlobalTitle { get; set; }
            public Dictionary<CultureInfo, string> LocalizedTitle { get; private set; }
            public string URL { get; set; }
            public string ImageTitlePathURL { get; set; }
            public string ImageTitlePathOnURL { get; set; }

            public LinkInformation()
            {
                LocalizedTitle = new Dictionary<CultureInfo, string>();
            }

            public string Title
            {
                get
                {
                    string title = string.Empty;
                    if (HttpContext.Current != null)
                    {
                        if (!LocalizedTitle.TryGetValue(LanguageManager.Instance.UserContext.CultureInfo, out title))
                        {
                            title = string.Empty;
                        }
                    }

                    if (string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(GlobalTitle))
                    {
                        title = GlobalTitle;
                    }

                    return title;
                }
            }

            public string GetAbsoluteURL()
            {
                return LinkHelper.ParseURL(URL);
            }
        }

        public bool ShowChildren { get; set; }
        public int Level { get; set; }
        public string SitePageToken { get; set; }
        public long ID { get; set; }
        public short LanguageID { get; set; }
        public bool IsVisible { get; set; }
        public int Location { get; set; }
        public MenuItem Parent { get; set; }
        public List<MenuItem> Children { get; private set; }
        public LinkInformation Link { get; set; }


        public MenuItem()
        {
            Children = new List<MenuItem>();
            Link = new LinkInformation();
        }
    }

    public class MainMenuManager
    {
        public static class MainMenuProvider
        {
            enum eMode
            {
                New,
                Singleton,
            }

            private static eMode defaultMode = eMode.New;
            static MainMenuProvider()
            {
                string configMode = ConfigurationManager.AppSettings["Tvinci.Web.Controls.MainMenu.Mode"];

                if (Enum.IsDefined(typeof(eMode), configMode))
                    defaultMode = (eMode)Enum.Parse(typeof(eMode), configMode);
            }

            static MainMenuManager singletonInstance = new MainMenuManager();

            public static MainMenuManager GetManager()
            {

                switch (defaultMode)
                {
                    case eMode.New:
                        return new MainMenuManager();
                    case eMode.Singleton:
                        return singletonInstance;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private class RequestContext
        {
            public List<MenuItem> ItemsOnActivePath { get; set; }
            public string ActiveItemToken { get; set; }
            public MenuItem ActiveItem { get; set; }

            public RequestContext()
            {
                ItemsOnActivePath = new List<MenuItem>();
            }
        }
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private Dictionary<short, MenuItem> m_defaultItem = new Dictionary<short, MenuItem>();
        public MenuItem DefaultItem
        {
            get
            {
                if (m_defaultItem.ContainsKey((short)LanguageManager.Instance.UserContext.ValueInDB))
                    return m_defaultItem[(short)LanguageManager.Instance.UserContext.ValueInDB];
                else
                    return null;
            }
            set
            {
                m_defaultItem[(short)LanguageManager.Instance.UserContext.ValueInDB] = value;
            }
        }
        List<MenuItem> m_upperLevel;
        Dictionary<string, MenuItem> m_mapping;
        Dictionary<long, MenuItem> m_items;
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public delegate MenuItem.LinkInformation CustomizeLinkDelegate(MenuItem.LinkInformation link, string SitePageToken);
        public delegate string UniqueItemTokenGeneratorDelegate(string url);

        public static CustomizeLinkDelegate CustomizeLinkMethod { private get; set; }
        public static UniqueItemTokenGeneratorDelegate UniqueItemTokenGeneratorMethod { private get; set; }

        public static string DefaultUniqueItemTokenGenerator(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return string.Empty;
                }

                url = LinkHelper.ParseURL(url);

                if (LinkHelper.IsBaseOfApplication(url))
                    return LinkHelper.RemoveQueryParamterFromURL("Language", url, LanguageManager.Instance.UserContext.Culture);
                else
                    return url;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static MenuItem.LinkInformation DefaultCustomizeLink(MenuItem.LinkInformation link, string SitePageToken)
        {
            return link;
        }

        public static MainMenuManager GetManager()
        {
            return MainMenuProvider.GetManager();
        }

        private MainMenuManager()
        {
            try
            {
                Sync();
            }
            catch
            {
                // no implmentation by design
            }
        }

        static MainMenuManager()
        {
            CustomizeLinkMethod = DefaultCustomizeLink;
            UniqueItemTokenGeneratorMethod = DefaultUniqueItemTokenGenerator;
        }

        public bool IsInActivePath(MainMenuManager menuMgr, MenuItem item)
        {
            //if the menu item has site token its url is absolute so make it relative
            string url;
            url = item.Link.URL;

            if (HttpContext.Current.Items["RequestedUrl"] != null)
            {
                //gets here only when the utl contains movie or episode special pages
                string requsetedURL = HttpContext.Current.Items["RequestedUrl"].ToString();

                if (!string.IsNullOrEmpty(url) && requsetedURL.Contains(LinkHelper.MakeAbsolute(null, url)))
                    return true; //the upper level handles
                else
                {   //one of the children handles
                    foreach (MenuItem childItem in item.Children)
                    {
                        url = childItem.Link.URL;

                        if (requsetedURL.Contains(LinkHelper.MakeAbsolute(null, url)))
                            return true;
                    }
                }
            }
            else
            {
                string ComparedURL = LinkHelper.GetActualPage() + HttpContext.Current.Request.Url.Query;

                if (Compare2URLsWithParams(url, ComparedURL, true))
                {
                    return true;
                }
                else
                {
                    if (item.Children.Count > 0)
                    {
                        foreach (MenuItem childItem in item.Children)
                        {
                            if (childItem.SitePageToken != string.Empty)
                                url = LinkHelper.StripURL(childItem.Link.URL);
                            else
                                url = childItem.Link.URL;

                            if (Compare2URLsWithParams(url, ComparedURL, true))
                            {
                                return true;
                            }
                        }
                    }
                }


                //handle the case that the current item is the default and none of the items should be selected
                if (menuMgr.DefaultItem != null && menuMgr.DefaultItem.ID == item.ID)
                {
                    foreach (MenuItem UpperItem in menuMgr.m_upperLevel)
                    {
                        //there is no need to check the default item - it has been handled above
                        if (UpperItem.LanguageID == (short)LanguageManager.Instance.UserContext.ValueInDB && UpperItem.ID != item.ID)
                        {
                            if (UpperItem.SitePageToken != string.Empty)
                                url = LinkHelper.StripURL(UpperItem.Link.URL);
                            else
                                url = UpperItem.Link.URL;

                            if (Compare2URLsWithParams(url, ComparedURL, true))
                            {
                                return false;
                            }
                            else
                            {
                                if (UpperItem.Children.Count > 0)
                                {
                                    foreach (MenuItem childItem in UpperItem.Children)
                                    {
                                        if (childItem.SitePageToken != string.Empty)
                                            url = LinkHelper.StripURL(childItem.Link.URL);
                                        else
                                            url = childItem.Link.URL;

                                        if (Compare2URLsWithParams(url, ComparedURL, true))
                                        {
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
            }

            return false;

            //RequestContext requestContext = getRequestContext();

            //if (requestContext != null)
            //{
            //    return requestContext.ItemsOnActivePath.Contains(item);
            //}
            //else
            //{
            //    return false;
            //}
            //{

            //}
            //else
            //{
            //    return false;
            //}

            //RequestContext requestContext = getRequestContext();

            //if (requestContext != null)
            //{

            //    return requestContext.ItemsOnActivePath.Contains(item);
            //}
            //else
            //{
            //    return false;
            //}
        }

        private bool Compare2URLsWithParams(string URL1, string URL2, bool IsURL2Base64)
        {
            //the result to be returned
            bool result = false;

            if (URL1.Contains("~/"))
            {
                URL1 = URL1.Substring(2);
            }
            //URL1 and URL2 are in the format "X.aspx?Y=z&W=B.."

            if (LinkHelper.GetLinkWithoutQuerystring(URL1, true).ToLower() == LinkHelper.GetLinkWithoutQuerystring(URL2, true).ToLower())
            {
                string query1 = LinkHelper.GetQuerystring(URL1);
                string query2 = LinkHelper.GetQuerystring(URL2);
                //query1 and query2 are in the format "Y=z&W=B.."

                if (query1 == query2)
                {
                    //nothing to check queries are the same
                    result = true;
                }
                else
                {
                    NameValueCollection col1 = LinkHelper.GetNameValueCollectionFromQueryString(query1);

                    string LanguageStr = string.Empty;

                    if (Tvinci.Web.HttpModules.Configuration.QueryConfigManager.Base64Mode || IsURL2Base64)
                    {
                        //if Language parameter exists the value is in length 2 (i.e. "he" or "ru")
                        if (query2.ToLower().Contains("language"))
                        {
                            int index = query2.ToLower().IndexOf("language");

                            LanguageStr = query2.Substring(index + 9); //he or ru

                            if (query2.Length > index + 11)
                            {//Language is not in the end
                                query2 = query2.Substring(0, index) + query2.Substring(index + 12, query2.Length - index - 12);
                            }
                            else
                            {
                                if (query2.ToLower().Contains("&language"))
                                {
                                    query2 = query2.Substring(0, index - 1);
                                }
                                else
                                {
                                    query2 = query2.Substring(0, index);
                                }
                            }
                        }

                        #region Handles Query Items that are not in base 64
                        string lowerQuery2 = query2.ToLower();
                        Dictionary<string, string> dctKeysValues = new Dictionary<string, string>();
                        foreach (QueryItem item in QueryConfigManager.Instance.Data.Base64.BypassBase64.QueryItems.GlobalScope)
                        {
                            string lowerKey = item.Key.ToLower();
                            if (lowerQuery2.Contains(lowerKey))
                            {
                                int indexOfKey = lowerQuery2.IndexOf(lowerKey);
                                int indexOfValue = indexOfKey + item.Key.Length + 1;

                                string strValue;
                                int indexOfNextAmper = lowerQuery2.IndexOf("&", indexOfValue);
                                if (indexOfNextAmper != -1)
                                {
                                    strValue = lowerQuery2.Substring(indexOfValue, indexOfNextAmper - indexOfValue);
                                    query2 = query2.Substring(0, indexOfKey) + query2.Substring(indexOfNextAmper + 1, lowerQuery2.Length - indexOfNextAmper - 1);
                                }
                                else
                                {
                                    strValue = lowerQuery2.Substring(indexOfValue, lowerQuery2.Length - indexOfValue);
                                    if (lowerQuery2.Contains("&"))
                                        query2 = query2.Substring(0, indexOfKey - 1);
                                    //lowerQuery2 = lowerQuery2.Substring(0, indexOfKey - 1) + lowerQuery2.Substring(indexOfValue + strValue.Length + 1, lowerQuery2.Length - indexOfValue - strValue.Length - 1);
                                    else//the only query
                                        query2 = string.Empty;
                                }

                                dctKeysValues.Add(item.Key, strValue);
                                lowerQuery2 = query2.ToLower();
                            }
                        }
                        #endregion

                        query2 = QueryStringHelper.DecryptQueryString(query2);

                        StringBuilder sb = new StringBuilder();
                        foreach (string item in dctKeysValues.Keys)
                        {
                            sb.Append("&" + item + "=" + dctKeysValues[item]);
                        }

                        if (sb.ToString() != string.Empty)
                        {//sb == "&X=y&..."
                            if (query2 != string.Empty)//query2 == "Z=w&..."
                                query2 = query2 + sb.ToString();
                            else//query2 == ""
                                query2 = sb.ToString().Substring(1); //removes the first '&'
                        }
                    }

                    if (col1["Language"] == null)
                        col1.Add("Language", LanguageManager.Instance.UserContext.Culture);

                    NameValueCollection col2 = LinkHelper.GetNameValueCollectionFromQueryString(query2);

                    if (col2["Language"] == null)
                    {
                        if (LanguageStr == string.Empty || LanguageStr == LanguageManager.Instance.UserContext.Culture)
                            col2.Add("Language", LanguageManager.Instance.UserContext.Culture);
                        else
                            col2.Add("Language", LanguageStr);
                    }

                    if (col1.Keys.Count == col2.Keys.Count)
                    {
                        foreach (string strCurrentKey in col1.Keys)
                        {
                            if (col2[strCurrentKey] != null)
                            {
                                if (col2[strCurrentKey].ToLower() != col1[strCurrentKey].ToLower())
                                {
                                    // found mismatch in strCurrentKey - quit
                                    return false;
                                }
                            }
                            else
                            {
                                //found param in col1 and not in col2 - quit
                                return false;
                            }
                        }

                        //if got here then the page is similar and each param key-value exists in both URLs
                        result = true;
                    }
                }
            }
            return result;
        }

        private RequestContext getRequestContext()
        {
            bool isReadLockHeld = m_lock.IsReadLockHeld;
            try
            {
                if (!isReadLockHeld)
                {
                    if (!m_lock.TryEnterReadLock(4000))
                    {
                        return null;
                    }
                }


                RequestContext requestContext = HttpContext.Current.Items["MainMenu_requestContext"] as RequestContext;

                if (requestContext == null)
                {
                    requestContext = new RequestContext();
                    requestContext.ActiveItemToken = UniqueItemTokenGeneratorMethod(HttpContext.Current.Request.Url.OriginalString);

                    MenuItem item;
                    if (m_mapping.TryGetValue(requestContext.ActiveItemToken, out item))
                    {
                        requestContext.ActiveItem = item;
                    }
                    else
                    {
                        if (HttpContext.Current.Items["RequestedUrl"] != null)
                        {
                            string strRequestedURL = HttpContext.Current.Items["RequestedUrl"].ToString();

                            foreach (string strItemURL in m_mapping.Keys)
                            {
                                if (strRequestedURL.Contains(strItemURL))
                                {
                                    requestContext.ActiveItem = m_mapping[strItemURL];
                                    break;
                                }
                            }
                        }
                        else
                            requestContext.ActiveItem = DefaultItem;
                    }

                    if (requestContext.ActiveItem != null)
                    {
                        // set active items path list
                        MenuItem tempItem = requestContext.ActiveItem;

                        while (tempItem != null)
                        {
                            requestContext.ItemsOnActivePath.Add(tempItem);
                            tempItem = tempItem.Parent;
                        }

                    }
                    HttpContext.Current.Items["MainMenu_requestContext"] = requestContext;
                }

                return requestContext;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (!isReadLockHeld)
                {
                    m_lock.ExitReadLock();
                }
            }
        }
        public bool TryGetMenu(int level, out List<MenuItem> items)
        {
            items = null;

            if (m_lock.TryEnterReadLock(4000))
            {
                try
                {
                    if (m_items == null)
                    {
                        return false;
                    }

                    if (level <= 0)
                    {
                        throw new ArgumentOutOfRangeException("level", "Must be positive");
                    }
                    if (level == 1)
                    {
                        items = m_upperLevel;
                        return true;
                    }
                    else
                    {
                        RequestContext requestContext = getRequestContext();



                        if (requestContext != null && requestContext.ActiveItem != null)
                        {
                            if ((requestContext.ActiveItem.Level + 1) == level)
                            {
                                // requesting next level of active - check if should show according to configuration
                                if (requestContext.ActiveItem.ShowChildren)
                                {
                                    items = requestContext.ActiveItem.Children;
                                    return true;
                                }
                            }
                            else if ((requestContext.ActiveItem.Level) < level)
                            {
                                // there is no such level for active item. return false;
                                return false;
                            }
                            else
                            {
                                // find the relevant children according to requested level in active path
                                MenuItem parent = requestContext.ActiveItem;
                                while (true)
                                {
                                    if (parent == null || parent.Level == (level - 1))
                                    {
                                        break;
                                    }
                                    parent = parent.Parent;
                                }

                                if (parent != null)
                                {
                                    items = parent.Children;
                                    return true;
                                }
                            }

                        }

                        return false;
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }

            return false;
        }

        public void Sync()
        {
            if (m_lock.TryEnterWriteLock(4000))
            {
                try
                {
                    logger.Info("Entering action - syncing main menu items");

                    m_upperLevel = null;
                    m_items = null;
                    m_mapping = null;

                    extractItems();

                    if (m_items != null)
                    {
                        m_upperLevel.Sort((a, b) => a.Location.CompareTo(b.Location));

                        m_mapping = new Dictionary<string, MenuItem>(new Tvinci.Helpers.CompareCaseInSensitive());
                        buildMapping(m_upperLevel, 1);
                    }


                }
                catch (Exception ex)
                {
                    logger.Error("Error occured while trying to sync main menu items", ex);
                    m_upperLevel = null;
                    m_items = null;
                    m_mapping = null;
                }
                finally
                {
                    logger.Info("Exit action - syncing main menu items");
                    m_lock.ExitWriteLock();
                }
            }
        }


        private void buildMapping(IList<MenuItem> items, int level)
        {
            if (items.Count == 0)
            {
                return;
            }

            List<MenuItem> nextLevel = new List<MenuItem>();

            foreach (MenuItem item in items)
            {
                if (item.LanguageID == (short)LanguageManager.Instance.UserContext.ValueInDB)
                {
                    item.Level = level;
                    if (!string.IsNullOrEmpty(item.Link.URL))
                    {
                        string token = UniqueItemTokenGeneratorMethod(item.Link.URL);

                        if (!string.IsNullOrEmpty(token))
                        {
                            m_mapping[token] = item;
                        }
                    }

                    nextLevel.AddRange(item.Children);
                }
            }

            if (nextLevel.Count != 0)
            {
                level++;
                buildMapping(nextLevel, level);
            }
        }

        private void extractItems()
        {
            List<MenuItem> upperLevel = new List<MenuItem>();
            Dictionary<long, MenuItem> items = new Dictionary<long, MenuItem>();
            try
            {
                MainMenuDS.ItemsDataTable table = new MainMenuDS.ItemsDataTable();

                new DatabaseDirectAdapter(delegate(TVPApi.ODBCWrapper.DataSetSelectQuery query)
                {
                    query += "select ID, URL,DefaultItem 'IsDefault', ShowChildren, CustomImagePath 'CustomImage', CustomImagePathOnActive 'CustomImageOn', PredefinedPageToken 'SitePageToken', ParentID, Title, ItemOrder 'Location', LanguageID, IsVisible from UpperMenuItems where ";
                    query += DatabaseHelper.AddCommonFields("Status", "Is_Active", eExecuteLocation.Application, false);
                }, table).Execute(Tvinci.Data.DataLoader.eExecuteBehaivor.ForceRetrieve);

                if (table != null)
                {
                    Dictionary<long, List<long>> childrenList = new Dictionary<long, List<long>>();

                    // extract all items and map items children list
                    foreach (MainMenuDS.ItemsRow row in table)
                    {
                        MenuItem newItem = createItem(row);

                        if (row.IsDefault)
                        {
                            m_defaultItem[newItem.LanguageID] = newItem;
                        }
                        if (!row.IsParentIDNull())
                        {
                            List<long> childrenIDList;

                            if (!childrenList.TryGetValue(row.ParentID, out childrenIDList))
                            {
                                childrenIDList = new List<long>();
                                childrenList.Add(row.ParentID, childrenIDList);
                            }

                            childrenIDList.Add(row.ID);
                        }
                        else
                        {
                            upperLevel.Add(newItem);
                        }

                        items.Add(row.ID, newItem);
                    }


                    foreach (MenuItem item in items.Values)
                    {
                        List<long> childrenIDList;
                        if (childrenList.TryGetValue(item.ID, out childrenIDList))
                        {
                            // set children of item instance
                            foreach (long childID in childrenIDList)
                            {
                                MenuItem child;
                                if (items.TryGetValue(childID, out child))
                                {
                                    // set parent of item instance
                                    child.Parent = item;

                                    item.Children.Add(child);
                                }
                                else
                                {
                                    logger.DebugFormat("Cannot find media '{0}' to assign as child of '{1}", childID, item.ID);
                                }
                            }

                            item.Children.Sort((a, b) => a.Location.CompareTo(b.Location));

                        }
                    }
                }

                m_items = items;
                m_upperLevel = upperLevel;
            }
            catch (Exception ex)
            {
                logger.Error("Failed to extract menu items", ex);
                m_items = null;
                m_upperLevel = null;
            }
        }

        private MenuItem createItem(MainMenuDS.ItemsRow row)
        {
            MenuItem result = new MenuItem();

            result.SitePageToken = row.SitePageToken;
            result.ID = row.ID;
            result.Location = row.Location;
            result.ShowChildren = row.ShowChildren;
            result.LanguageID = row.LanguageID;
            result.IsVisible = row.IsVisible;

            MenuItem.LinkInformation link = new MenuItem.LinkInformation { URL = row.URL };
            handleItemTitle(row.Title, link);
            link.ImageTitlePathURL = row.CustomImage;
            link.ImageTitlePathOnURL = row.CustomImageOn;

            if (string.IsNullOrEmpty(row.SitePageToken))
            {
                result.Link = link;
            }
            else
            {
                result.Link = CustomizeLinkMethod(link, row.SitePageToken);
            }

            return result;
        }

        private void handleItemTitle(string originalTitle, MenuItem.LinkInformation link)
        {
            if (!string.IsNullOrEmpty(originalTitle))
            {
                foreach (string value in originalTitle.Replace('\r', '\n').Replace("\n\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] token = value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    if (token.Length == 2)
                    {
                        CultureInfo ci = CultureInfo.GetCultureInfo(token[0]);

                        if (ci == null)
                        {
                            throw new Exception(string.Format("Language culture '{0}' is not recognized as valid culture", token[0]));
                        }

                        link.LocalizedTitle.Add(ci, token[1]);
                    }
                    else
                    {
                        link.GlobalTitle = value;
                    }
                }
            }
        }
    }
}