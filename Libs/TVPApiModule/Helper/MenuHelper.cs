using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
/// <summary>
/// Summary description for MenuHelper
/// </summary>
/// 

namespace TVPApi
{
    public class MenuHelper
    {
        public MenuHelper()
        {
        }

        
       

        //Get specific menu
        public static Menu GetMenuByID(InitializationObject initObj, long ID, int groupID)
        {
            Menu retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                List<Menu> menues = siteMap.Menues;
                retVal = (from menu in menues
                          where menu.ID == ID
                          select menu).FirstOrDefault();
            }
            return retVal;
        }

        //Get specific footer
        public static Menu GetFooterByID(InitializationObject initObj, long ID, int groupID)
        {
            Menu retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                List<Menu> footers = siteMap.Footers;
                retVal = (from menu in footers
                          where menu.ID == ID
                          select menu).FirstOrDefault();
            }
            return retVal;
        }



    }
}
