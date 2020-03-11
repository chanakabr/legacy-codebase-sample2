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
        public long ID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public MenuBuilder.MenuType MenuType { get; set; }
        public long PageID { get; set; }
        public string Culture { get; set; }
        public List<MenuItem> Children { get; set; }
        public int RuleID { get; set; }

        public MenuItem()
        {

        }
    }
}
