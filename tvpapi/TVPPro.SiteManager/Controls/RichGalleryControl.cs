//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Collections.ObjectModel;

//namespace TVPPro.SiteManager.Controls
//{

//    public class TabManager : List<RichGalleryTab>
//    {
//        public int ValidTabsCount(bool stopIfMoreThenOne)
//        {
//            int count = 0;

//            foreach (RichGalleryTab tab in this)
//            {
//                if (IsValidTab(tab))
//                {
//                    count++;

//                    if (stopIfMoreThenOne && count>1)
//                    {
//                        return count;
//                    }
//                }                
//            }

//            return count;
//        }

//        public int FindFirstValidTabIndex()
//        {
//            for(int i=0;i<base.Count;i++)
//            {
//                if (IsValidTab(i))
//                {
//                    return i;
//                }

//            }

//            return -1;            
//        }

//        public bool IsValidTab(int tabIndex)
//        {
//            return IsValidTab( base[tabIndex]);
//        }

//        public bool IsValidTab(RichGalleryTab tab)
//        {
//            return tab.Visible;
//        }

//        public RichGalleryTab FindTab(string identifier)
//        {
//            RichGalleryTab tab = this.Find(delegate(RichGalleryTab obj)
//            {
//                return (obj.Identifier == identifier);
//            });

//            if (tab == null)
//            {
//                throw new Exception(string.Format("Cannot find tab with name '{0}' (did you forget to assign name to the GalleryTab in its' constructor?)", identifier));
//            }

//            return tab;
//        }

//    }
    
//    [ParseChildren(true)]
//    [PersistChildren(false)]
//    public class RichGalleryTab : Control
//    {        
//        public RichGalleryTab()
//        {
//            Identifier = Guid.NewGuid().ToString();
//        }
        
//        [PersistenceMode(PersistenceMode.InnerProperty)]
//        public virtual string Identifier { get; set; }
//        [PersistenceMode(PersistenceMode.InnerProperty)]
//        public string Title { get; set; }
//        [PersistenceMode(PersistenceMode.InnerProperty)]
//        public string SortValues { get; set; }
        
//        [PersistenceMode(PersistenceMode.InnerProperty)]
//        public PlaceHolder DataTemplate { get; set; }

//		[PersistenceMode(PersistenceMode.InnerProperty)]
//		public string LocalizedTitleIdentifier {get;set;}		
//    }
//}
