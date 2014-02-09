//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Tvinci.Web.Controls.Gallery.Part;
//using TVPPro.SiteManager.DataLoaders;
//using System.Web.UI;
//using Tvinci.Web.Controls.ContainerControl;
//using Tvinci.Data.DataLoader.PredefinedAdapters;

//namespace TVPPro.SiteManager.Controls
//{
//    [Serializable]
//    public class MainMenuDataLoader : CustomAdapter<List<IMenuItem>>
//    {

//        protected override List<IMenuItem> CreateSourceResult()
//        {
//            return new TVPPagesLoader().Execute().MainMenu;
//        }

//        protected override Guid UniqueIdentifier
//        {
//            get { return new Guid("{2E8E735A-6BA8-4b51-A861-81E803545D56}"); }
//        }
//    }
//    public class MainMenuContentPart : ContentPart<IMenuItem>
//    {
//        [TemplateContainer(typeof(ContentPartItem<IMenuItem>))]        
//        public override System.Web.UI.ITemplate Template {get;set;}        
//    }

//    public class MainMenuInnerItemTemplate : TemplatedContainer
//    {
//        [TemplateContainer(typeof(ContentPartItem<IMenuItem>))]
//        public override System.Web.UI.ITemplate Template { get; set; }        
//    }

//    public class MainMenuItem : Control, IMenuItem
//    {
//        IMenuItem m_item;

//        public MainMenuItem(IMenuItem item)
//        {
//            m_item = item;
//        }

//        #region IMenuItem Members

//        public List<IMenuItem> GetChilds()
//        {
//            return m_item.GetChilds();
//        }

//        public int MenuOrder
//        {
//            get
//            {
//                return m_item.MenuOrder;
//            }
//            set
//            {
//                m_item.MenuOrder = value;
//            }
//        }

//        public string MenuText
//        {
//            get
//            {
//                return m_item.MenuText;
//            }
//            set
//            {
//                m_item.MenuText = value;
//            }            
//        }

//        public string MenuLink
//        {
//            get
//            {
//                return m_item.MenuLink;
//            }            
//        }

//        #endregion

//        #region IMenuItem Members


//        public bool IsInMenu
//        {
//            get
//            {
//                return m_item.IsInMenu;
//            }
//            set
//            {
//                m_item.IsInMenu = value;
//            }
//        }

//        #endregion

//        #region IMenuItem Members


//        public bool IsChildOf(IMenuItem item)
//        {
//            return m_item.IsChildOf(item) ;
//        }

//        #endregion
//    }
//}
