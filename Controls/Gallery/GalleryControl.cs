using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tvinci.Data.DataLoader;
using System.Collections;
using Tvinci.Web.Controls.Gallery;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Tvinci.Web.Controls.Gallery.Part;
using System.Runtime.Serialization;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Web.Controls.Gallery
{

    public class PreDataHandlingEventArgs : EventArgs             
    {
            public GalleryTab Tab{get;internal set;}
            public ILoaderAdapter Loader{get;internal set;}
            public object Data{get;set;}
    }
    [Flags]
    public enum eBehaivor
    {
        None = 0,
        AcceptEmptyTab = 2,
        SuspendTabExistsCheck = 4
    }

    public class GalleryControl : GalleryBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public class GallerySource
        {
            public IEnumerable Source { get; private set; }
            public int ActivePageIndex { get; private set; }
            public int PageSize { get; private set; }
            public long ItemsInSource { get; private set; }
            public int ItemsInActivePage { get; private set; }
            
            public GallerySource(IEnumerable source, int activePageIndex, int pageSize, int itemsInActivePage, long itemsInSource)
            {
                Source = source;
                ActivePageIndex = activePageIndex;
                PageSize = pageSize;
                ItemsInActivePage = itemsInActivePage;
                ItemsInSource = itemsInSource;
            }

            public long GetPagesCount()
            {
                if (PageSize == 0)
                {
                    return 1;
                }
                else
                {
                    if ((ItemsInSource % PageSize) != 0)
                    {
                        return (ItemsInSource / PageSize) + 1;
                    }
                    else
                    {
                        return (ItemsInSource / PageSize);
                    }

                }
            }
        }

        
        public event EventHandler ActiveTabChanged;


        #region Fields & Properties
        public eBehaivor Behaivor { get; set; }
        List<ILoaderAdapter> m_loaders = new List<ILoaderAdapter>();
        List<GalleryTab> m_tabs = new List<GalleryTab>();
        GallerySource m_gallerySource = null;
        Dictionary<string, int> m_tabIndex = new Dictionary<string, int>();
        public eGalleryBehaivor GalleryBehaivor { get; set; }
        #endregion

        #region Properties


        public int ActiveTabPageIndex
        {
            get
            {
                return ActiveTab.PageIndex;
            }
            set
            {
                ActiveTab.PageIndex = value;
            }

        }
        
        public ILoaderAdapter ActiveLoader
        {
            get
            {
                if (ActiveTabIndex >= 0 && ActiveTabIndex < m_loaders.Count)
                {
                    return m_loaders[ActiveTabIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        //bool firstTimeRequestingActiveTab = true;
        public GalleryTab ActiveTab
        {
            get
            {
                if (ActiveTabIndex != int.MinValue)
                {
                    //firstTimeRequestingActiveTab = false;
                    return m_tabs[ActiveTabIndex];
                }
                else
                {
                    //if (firstTimeRequestingActiveTab)
                    //{
                        //firstTimeRequestingActiveTab = false;

                    //}
                    return null;
                }                
            }
        }

        public int ActiveTabIndex
        {
            get
            {
                object result = ViewState["ActiveTabIndex"];

                if (result == null)
                {
                    ViewState["ActiveTabIndex"] = result = int.MinValue;
                }

                return (int)result;
            }
            set
            {
                object currentValue = ViewState["ActiveTabIndex"];
                if (currentValue != null && (int)currentValue != value)
                {
                    ViewState["ActiveTabIndex"] = value;
                    OnActiveTabChanged(EventArgs.Empty);
                }
                else if (currentValue == null)
                {
                    ViewState["ActiveTabIndex"] = value;
                }
            }
        }

        #endregion


        #region Override methods

        
        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }
        
        protected override void LoadControlState(object savedState)
        {
            if (savedState is Triplet)
            {
                Triplet data = (Triplet)savedState;

                if (data.Second != null)
                {
                    if (data.Second is GalleryTab)
                    {
                        m_tabs.Add((GalleryTab)data.Second);
                        m_loaders.Add((ILoaderAdapter)data.Third);
                        m_tabIndex.Add(m_tabs[0].Identifier, 0);
                    }
                    else
                    {
                        int i = 0;
                        foreach (GalleryTab tab in (GalleryTab[])data.Second)
                        {
                            m_tabIndex.Add(tab.Identifier, i);
                            i++;
                            m_tabs.Add(tab);
                        }

                        if (data.Third != null)
                        {
                            m_loaders.AddRange((ILoaderAdapter[])data.Third);
                        }
                    }
                }

                // restore the databound state. 
                base.LoadControlState(data.First);
            }
            else
            {
                base.LoadControlState(savedState);
            }
        }

        protected override object SaveControlState()
        {
            object data = base.SaveControlState();

			if ((GalleryBehaivor & eGalleryBehaivor.DontPersistTab) != eGalleryBehaivor.DontPersistTab)
			{
				// optimize viewstate length by sending array only when needed
				if (m_tabs.Count == 1)
				{
					data = new Triplet(data, m_tabs[0], m_loaders.Count == 0 ? null : m_loaders[0]);
				}
				else if (m_tabs.Count > 1)
				{
					data = new Triplet(data, m_tabs.ToArray(), m_loaders.Count == 0 ? null : m_loaders.ToArray());
				}
			}
			
            return data;
        }
        #endregion

        #region Public methods
        public ILoaderAdapter FindLoader(string tabIdentifier, bool allowInvalidIdentifier)
        {
            int index;
            if (m_tabIndex.TryGetValue(tabIdentifier, out index))
            {
                return m_loaders[index];
            }
            else
            {
                if (allowInvalidIdentifier)
                {
                    return null;
                }
                else
                {
                    throw new Exception(string.Format("Cannot find tab with identifier '{0}'", tabIdentifier));
                }
            }
        }
        public void AssignTabLoader(string tabIdentifier, ILoaderAdapter dataAdapter)
        {
            if (m_tabIndex.ContainsKey(tabIdentifier))
            {
                // replace tab loader
                int index = m_tabIndex[tabIdentifier];

                if (index > m_loaders.Count)
                {
                    ILoaderAdapter[] nullArray = new ILoaderAdapter[index - m_loaders.Count];
                    m_loaders.AddRange(nullArray);
                    m_loaders.Insert(index, dataAdapter);
                }
                else if (index == m_loaders.Count)
                {
                    m_loaders.Add(dataAdapter);
                }else
                {
                    m_loaders[index] = dataAdapter;
                }
            }
            else
            {
                throw new Exception(string.Format("Cannot find tab with identifier '{0}' (Did you tried to perform this action on 'OnInit' method? this action should be done on 'PageLoad' only) ", tabIdentifier));
            }
        }
        
        
        [Flags]
        public enum eAddTabOption
        {
            None = 0,
            AllowOnPostBack = 2,
            OverrideIfExist = 4
        }

        public void AddTab(string identifier, GalleryTab tabInfo)
        {
            AddTab(identifier, tabInfo, eAddTabOption.None);
        }

        protected override bool shouldSearchForParts(Control control)
        {
            if (control is MultipleDataPartContainer)
            {
                ((MultipleDataPartContainer)control).SyncActiveTab(ActiveTab.Identifier);
                return true;
            }

            return base.shouldSearchForParts(control);
        }

        public bool IsTabExists(string identifier)
        {
            return m_tabIndex.ContainsKey(identifier);
        }

        public void AddTab(string identifier, GalleryTab tabInfo, eAddTabOption addOptions)
        {
            if (tabInfo == null)
            {
                throw new ArgumentNullException("tabInfo");
            }

            if ((Behaivor & eBehaivor.SuspendTabExistsCheck) != eBehaivor.SuspendTabExistsCheck)
            {
                if (this.Page.IsPostBack && ((addOptions & eAddTabOption.AllowOnPostBack) != eAddTabOption.AllowOnPostBack))
                {
                    throw new Exception(string.Format(@"Cannot add\override tab '{0}' during postback (To baypass this attidute - set the option 'AllowOnPostBack', If the container control cannot be assigned with page. use 'Behaivor.SuspendTabExistsCheck)", identifier));
                }
            }
            
            tabInfo.Identifier = identifier;

            if (m_tabIndex.ContainsKey(identifier))
            {
                if ((addOptions & eAddTabOption.OverrideIfExist) == eAddTabOption.OverrideIfExist)
                {
                    int index = m_tabIndex[identifier];
                    m_tabs[index] = tabInfo;
                }
                else
                {
                    throw new Exception(string.Format("Tab collection already has item with identifier '{0}' (To baypass this attidute - set the option 'OverrideIfExist')", identifier));
                }
            }
            else
            {
                m_tabIndex.Add(identifier, m_tabs.Count);
                m_tabs.Add(tabInfo);
            }

        }

        #endregion

        #region Private & Protected methods
        protected virtual void OnActiveTabChanged(EventArgs e)
        {
            if (ActiveTabChanged != null)
            {
                ActiveTabChanged(this, e);
            }
        }

        private GallerySource InitializeContent(IEnumerable source, int activePageIndex, int pageSize, long itemsInSource)
        {
            PagedDataSource pagedDataSource = new PagedDataSource();
            pagedDataSource.DataSource = source;
            return new GallerySource(pagedDataSource, activePageIndex, pageSize, pagedDataSource.Count, itemsInSource);
        }

        private GallerySource InitializeContent(IEnumerable source, int activePageIndex, int pageSize)
        {

            PagedDataSource pagedDataSource = new PagedDataSource();
            pagedDataSource.DataSource = source;

            int itemsInSource = pagedDataSource.Count;

            int itemsInActivePage;
            if (pageSize != 0)
            {
                pagedDataSource.AllowPaging = true;
                pagedDataSource.PageSize = pageSize;
                pagedDataSource.CurrentPageIndex = activePageIndex;
                itemsInActivePage = pagedDataSource.Count;
            }
            else
            {
                itemsInActivePage = pagedDataSource.Count;
            }

            return new GallerySource(pagedDataSource, activePageIndex, pageSize, itemsInActivePage, itemsInSource);
        }

        //public class PreAdapterResult
        public delegate object PreAdapterResultHandlingDelegate(GalleryTab tab, ILoaderAdapter loader , object adapterResult);

        

        [Obsolete("Use 'PreDataHandlingEventArgs' instead")]
        public PreAdapterResultHandlingDelegate PreAdapterResultHandlingMethod { get; set; }
        public event EventHandler<PreDataHandlingEventArgs> PreDataHandling;

        private GallerySource createGallerySource(int tabIndex)
        {
            GallerySource result;

            ILoaderAdapter loader = m_loaders[tabIndex];
            GalleryTab tab = m_tabs[tabIndex];

            if (loader == null)
            {
                return null;
            }

            object dataSource = loader.Execute();

            if (dataSource is DataSet)
            {
                if (!string.IsNullOrEmpty(tab.LoaderTableName) && ((DataSet)dataSource).Tables.Contains(tab.LoaderTableName))
                {
                    dataSource = ((DataSet)dataSource).Tables[tab.LoaderTableName];
                }
                else
                {
                    if (string.IsNullOrEmpty(tab.LoaderTableName))
                    {
                        throw new Exception("When using adapter which result a 'Dataset', the property 'LoaderTableName' must be assigned on the 'GalleryTab' instance");
                    }
                    else
                    {
                        throw new Exception(string.Format("Failed to extract the relevent table from adpater result. Cannot find table name '{0}' in dataset of type '{1}'", tab.LoaderTableName, ((DataSet)dataSource).GetType().Name));
                    }

                }
            }

            if (PreDataHandling != null) 
            {
                PreDataHandlingEventArgs e = new PreDataHandlingEventArgs() { Tab = tab, Loader = loader,  Data = dataSource};
                PreDataHandling(this,e);
                if (dataSource != e.Data)
                {
                    dataSource = e.Data;
                }
            }
            else
            {
                if (PreAdapterResultHandlingMethod != null)
                {
                    dataSource = PreAdapterResultHandlingMethod(tab, loader, dataSource);
                }
            }

            if (dataSource is DataTable)
            {
                dataSource = ((DataTable)dataSource).DefaultView;
            }            
            else if (dataSource is IEnumerable)
            {
                dataSource = (IEnumerable)dataSource;
            }
            else
            {
                if (dataSource == null)
                {
                    // TODO log
                    return null;
                }
                else
                {
                    throw new Exception(string.Format("Gallery control not support adapter result of type '{0}'", dataSource.GetType().Name));
                }
            }
         
            switch (tab.PagingMethod)
            {
                case ePagingMethod.Default:
                    result = InitializeContent((IEnumerable)dataSource, tab.PageIndex, tab.PageSize);
                    break;
                case ePagingMethod.FromLoader:
                    ISupportPaging paging = loader as ISupportPaging;

                    if (paging != null)
                    {
                        long itemsInSource;

                        if (!paging.TryGetItemsCount(out itemsInSource))
                        {
                            return null;
                        }

                        result = InitializeContent((IEnumerable)dataSource, paging.PageIndex, paging.PageSize, itemsInSource);
                    }
                    else
                    {
                        return null;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;


        }
               
        protected override void PreSync()
        {
            if (m_gallerySource == null)
            {
                m_gallerySource = createGallerySource(ActiveTabIndex);
            }
        }
        
        protected override bool CanUpdateGallery()
        {            
            if (m_tabs.Count != 0)
            {
                if (!isActiveTabValid() && ActiveTabIndex == int.MinValue)
                {
                    ActiveTabIndex = findFirstValidTab();

                    if (ActiveTabIndex != int.MinValue)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            
            return false;
        }

        
        private int findFirstValidTab()
        {
            int count = 0;

            if (m_tabs.Count == m_loaders.Count)
            {            
                foreach (GalleryTab tab in m_tabs)
                {
                    if (m_gallerySource == null)
                    {
                        m_gallerySource = createGallerySource(count);
                    }

                    if (m_gallerySource != null)
                    {
                        if (m_gallerySource.ItemsInActivePage != 0 || (Behaivor & eBehaivor.AcceptEmptyTab) == eBehaivor.AcceptEmptyTab)
                        {
                            return count;
                        }
                        else
                        {
                            m_gallerySource = null;
                        }
                    }
                    count++;
                }
            }

            return int.MinValue;

            
        }

        private bool isActiveTabValid()
        {
            if (ActiveTabIndex != int.MinValue)
            {
                if (m_tabs.Count > ActiveTabIndex)
                {

                    if (m_gallerySource == null)
                    {
                        m_gallerySource = createGallerySource(ActiveTabIndex);
                    }
                    if (m_gallerySource == null)
                    {
                        m_gallerySource = new GallerySource(new List<object>(), 0, 0, 0, 0);
                        if (m_gallerySource.ItemsInActivePage != 0 || (Behaivor & eBehaivor.AcceptEmptyTab) == eBehaivor.AcceptEmptyTab)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

            //if (ActiveTabIndex != int.MinValue)
            //{
            //    if (m_tabs.Count > ActiveTabIndex)
            //    {
            //        GallerySource source = createGallerySource(ActiveTabIndex);

            //        if (source != null)                    
            //        {
            //            if (source.ItemsInActivePage != 0 || (Behaivor & eBehaivor.AcceptEmptyTab) == eBehaivor.AcceptEmptyTab)
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}

            //return false;
        }
        
        protected override void PostSync()
        {                        
            m_gallerySource = null;
            base.PostSync();
        }

        public enum eTabVisibleMode
        {
            ShowIfMultiple,
            ShowAlways,
            ShowIfHasItems
        }

        [Flags]
        public enum eGalleryBehaivor
        {
            Default = 0,
            IncludeCountInTab = 2,
            DontPersistTab = 4
        }

        
        
        public GalleryControl()
        {
            GalleryBehaivor = eGalleryBehaivor.Default;            
        }

        
        protected override void HandlePart(IGalleryPart part)
        {
            switch (part.HandlerID)
            {
                case ClientPagingHandler.Identifier:
                    ClientPagingHandler cpHhandler = new ClientPagingHandler();
                    cpHhandler.Process((ClientPagingPart)part, m_gallerySource.ItemsInSource);
                    break;
                case ContentPartHandler.Identifier:
                    ContentPartHandler handler = new ContentPartHandler();
                    handler.HandleItems((ContentPart)part, m_gallerySource.Source, m_gallerySource.ItemsInActivePage);
                    break;
                case TabPartHandler.Identifier:
                    TabPartHandler tabHandler = new TabPartHandler();
                    List<TabPartHandler.TabItem> tabToShowList = buildList();
                    tabHandler.HandleTab((TabPart)part, tabToShowList,ActiveTabIndex);
                    break;
                case PagingPartHandler.Identifier:
                    PagingPartHandler pagingHandler = new PagingPartHandler();
                    pagingHandler.HandlePaging((PagingPart)part, ActiveTab.PageIndex, m_gallerySource.GetPagesCount(), m_gallerySource.ItemsInSource, m_gallerySource.PageSize);
                    break;
                case SortPartHandler.Identifier:
                    SortPartHandler sortHandler = new SortPartHandler();
                    sortHandler.Process((SortPart)part);
                    break;
                default:
                    break;
            }
        }
        
        private List<TabPartHandler.TabItem> buildList()
        {
            List<TabPartHandler.TabItem> result = new List<TabPartHandler.TabItem>();

            if (m_tabs.Count > 0)
            {
                int count = 0;
                foreach (GalleryTab tab in m_tabs)
                {
                    //if (m_gallerySource == null)
                    //{
                    //    m_gallerySource = createGallerySource(count);
                    //}
                    //result.Add(new TabPartHandler.TabItem(count, tab.Title) { ItemsCount = m_gallerySource.ItemsInSource, TabIdentifier = tab.Identifier });

                    result.Add(new TabPartHandler.TabItem(count, tab.Title) { ItemsCount = count == ActiveTabIndex ? m_gallerySource != null ? m_gallerySource.ItemsInSource : 0 : 1, TabIdentifier = tab.Identifier });
                    count++;
                }
            }
          
            return result;
        }

        #endregion

        protected override bool IsValidGalleryPart(IGalleryPart part)
        {
            return true;
        }

        public bool TryGetActiveTabIndex(out int tabIndex)
        {
            tabIndex = ActiveTabIndex;
            return (tabIndex != int.MinValue);
        }

        public void FocusOnFirstValidTab()
        {
            ActiveTabIndex = findFirstValidTab();            
        }
    }
}

///*
//private string serializeData(object data)
//        {
//            // WARNING - use this for viewstate debugging only
//            LosFormatter formatter = new LosFormatter();
//            System.IO.MemoryStream ms = new System.IO.MemoryStream();

//            formatter.Serialize(ms, data);

//            //return ms.ToArray();            
//            string viewstate = Convert.ToBase64String(ms.ToArray());

//            return viewstate;
//        }

//*/