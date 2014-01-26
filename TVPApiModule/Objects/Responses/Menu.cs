using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Menu
/// </summary>
/// 

namespace TVPApi
{
    public class Menu
    {
        public long id { get; set; }
        public List<MenuItem> menuItems { get; set; }
        public TVPApi.MenuBuilder.MenuType type { get; set; }

        public Menu()
        {

        }

        public MenuItem GetMenuItem(long ID)
        {
            MenuItem retVal = null;

            IEnumerable<MenuItem> menuItemList = from items in menuItems
                                              where items.ID == ID
                                              select items;

            if (menuItemList != null && menuItemList.Count() == 1)
            {
                retVal = menuItemList.FirstOrDefault();
            }
            return retVal;
        }
    }
}
