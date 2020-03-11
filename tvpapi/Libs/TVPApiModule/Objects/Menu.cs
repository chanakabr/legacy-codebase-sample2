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
        public long ID { get; set; }
        public List<MenuItem> MenuItems { get; set; }
        public TVPApi.MenuBuilder.MenuType Type { get; set; }

        public Menu()
        {

        }

        public MenuItem GetMenuItem(long ID)
        {
            MenuItem retVal = null;

            IEnumerable<MenuItem> menuItems = from items in MenuItems
                                              where items.ID == ID
                                              select items;

            if (menuItems != null && menuItems.Count() == 1)
            {
                retVal = menuItems.FirstOrDefault();
            }
            return retVal;
        }
    }
}
