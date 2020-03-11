using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{    
    public class TabPartHandler : GalleryPartHandler
    {
        public class TabItem
        {
            public int TabIndex { get; set; }
            public string Title { get; set; }
            public long ItemsCount { get; set; }
            public string TabIdentifier { get; set; }

            public TabItem(int tabIndex, string title)
            {
                TabIndex = tabIndex;
                Title = title;

                ItemsCount = int.MinValue;
            }
        }

        public const string Identifier = "TabPartHandler";

        public void HandleTab(TabPart part, List<TabItem> tabs, int activeTabIndex)
        {
            Control partWrapper = FindPartWrapper(part,part);

            partWrapper.Visible = true;
            
            part.Controls.Clear();

            if (tabs.Count == 0)
            {
                partWrapper.Visible = false;
            }else if (tabs.Count == 1 && part.VisibleMode == eTabVisibleMode.IfMultiple)
            {
                partWrapper.Visible = false;
            }
            else
            {
                foreach (TabItem tab in tabs)
                {
                    part.OnPreviewTab(tab);
                    TabPartItem container = new TabPartItem(tab.TabIndex, tab.Title, (tab.TabIndex == activeTabIndex));

                    if (tab.ItemsCount != int.MinValue)
                    {
                        container.ItemsCount = tab.ItemsCount;
                    }

                    container.ID = tab.TabIndex.ToString();
                    part.HandleItem(container);
                }                
            }
        }
          
        public override string GetIdentifier()
        {
            return Identifier;
        }
    }
}
