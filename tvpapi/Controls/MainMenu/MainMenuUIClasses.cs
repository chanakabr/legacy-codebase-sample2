using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Data.DataLoader;
using Tvinci.Web.Controls.Gallery.Part;
using System.Web.UI;
using Tvinci.Web.Controls.ContainerControl;
using Tvinci.Localization;


namespace Tvinci.Web.Controls.MainMenu
{
    public class MainMenuContentPart : ContentPart<MainMenuControl>
    {
        [System.Web.UI.TemplateContainer(typeof(ContentPartItem<MainMenuControl>))]
        public override System.Web.UI.ITemplate Template { get; set; }
    }

    public class MainMenuInnerItemTemplate : TemplatedContainer
    {
        [TemplateContainer(typeof(MainMenuControl))]
        public override System.Web.UI.ITemplate Template { get; set; }
    }


    [Serializable]
    public class MainMenuDataLoader : CustomAdapter<MainMenuManager, List<MainMenuControl>>
    {
        private int Level
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Filter, "Level", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Filter, "Level", value);

            }
        }

        public MainMenuDataLoader(int level)
        {
            Level = level;
        }

        protected override List<MainMenuControl> FormatResults(MainMenuManager originalObject)
        {
            if (originalObject == null)
            {
                return new List<MainMenuControl>();
            }

            List<MenuItem> items;
            if (originalObject.TryGetMenu(Level, out items))
            {
                List<MainMenuControl> result = new List<MainMenuControl>();
                foreach (MenuItem item in items)
                {
                    short langID;
                    short.TryParse(LanguageManager.Instance.UserContext.ValueInDB.ToString(), out langID);
                    if (langID == item.LanguageID)
                    {
                        string dummyValue;
                        if (item.Link.LocalizedTitle.Count == 0 || item.Link.LocalizedTitle.TryGetValue(LanguageManager.Instance.UserContext.CultureInfo, out dummyValue))
                        {
                            if (item.IsVisible == true)
                            {
                                result.Add(new MainMenuControl(item, originalObject));
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                return new List<MainMenuControl>();
            }
        }

        protected override MainMenuManager CreateSourceResult()
        {
            MainMenuManager result = MainMenuManager.GetManager();
            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{2EAD1BE9-818F-498b-B859-DCCCE16050C3}"); }
        }
    }

    public class MainMenuControl : System.Web.UI.Control, INamingContainer
    {

        public MenuItem MenuItem { get; private set; }
        private MainMenuManager m_itemManager;

        public MainMenuControl(MenuItem item, MainMenuManager itemManager)
        {
            m_itemManager = itemManager;
            MenuItem = item;
        }

        public bool IsActive()
        {
            return m_itemManager.IsInActivePath(m_itemManager, MenuItem);
        }

        public List<MainMenuControl> GetChildren()
        {
            List<MainMenuControl> result = new List<MainMenuControl>();
            foreach (MenuItem item in MenuItem.Children)
            {
                result.Add(new MainMenuControl(item, m_itemManager));
            }

            return result;
        }
    }
}