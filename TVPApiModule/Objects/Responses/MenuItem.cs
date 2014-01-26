using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

/// <summary>
/// Summary description for Menu
/// </summary>
/// 

namespace TVPApi
{
    public class MenuItem
    {
        public long id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public MenuBuilder.MenuType menu_type { get; set; }
        public long page_id { get; set; }
        public string culture { get; set; }
        public List<MenuItem> children { get; set; }
        public int rule_id { get; set; }

        public MenuItem()
        {

        }
    }
}
