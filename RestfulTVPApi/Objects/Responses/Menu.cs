using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Manager;

/// <summary>
/// Summary description for Menu
/// </summary>
/// 

namespace RestfulTVPApi.Objects.Responses
{
    public class Menu
    {
        public long id { get; set; }
        public List<MenuItem> menu_items { get; set; }
        public MenuBuilder.MenuType type { get; set; }

        public Menu()
        {

        }

        public MenuItem GetMenuItem(long ID)
        {
            MenuItem retVal = null;

            IEnumerable<MenuItem> menuItemList = from items in menu_items
                                              where items.id == ID
                                              select items;

            if (menuItemList != null && menuItemList.Count() == 1)
            {
                retVal = menuItemList.FirstOrDefault();
            }
            return retVal;
        }
    }
}
