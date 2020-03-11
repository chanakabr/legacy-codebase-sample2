using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using System.Data;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Helpers;
using Tvinci.Data.DataLoader;

namespace TVPPro.SiteManager.DataLoaders
{
	[Serializable]
    public class MenuLoader : DatabaseAdapter<DataTable,  dsMenu.MenuDataTable>
    {
        #region Properties

       

        #endregion

        protected override void InitializeQuery(ODBCWrapper.DataSetSelectQuery query)   
        {
            query += "SELECT q.MENU_ID as MenuID, q.ID as ItemID, q.PARENT_MENU_ITEM_ID as ParentItemID, q.MENU_TYPE as MenuType, q.LINK as url,q.HAS_NO_FOLLOW as HasNoFollow, tmit.TITLE, ll.CULTURE, q.ORDER_NUM as [index] ";
            query += "FROM (SELECT tmi.ID, tmi.ORDER_NUM, tmi.MENU_ID, tmi.PARENT_MENU_ITEM_ID, tmi.LINK,tmi.HAS_NO_FOLLOW, tm.MENU_TYPE ";
            query += "FROM tvp_menu_items tmi, tvp_menu tm ";
            query += "WHERE (tmi.STATUS = 1) AND (tmi.IS_ACTIVE = 1) and tm.ID = tmi.MENU_ID) AS q LEFT OUTER JOIN ";
            query += "tvp_menu_items_texts AS tmit ON q.ID = tmit.MENU_ITEM_ID AND tmit.IS_ACTIVE = 1 AND tmit.STATUS = 1 LEFT OUTER JOIN ";
			query += "lu_languages AS ll ON ll.ID = tmit.LANGUAGE_ID ORDER BY q.MENU_ID, q.ORDER_NUM";

			//query += "select mm.ID, mm.ParentID, mm.URL, mm.DefaultItem, mm.SitePageID, lg.Culture, mmd.Title from MainMenu mm left join MainMenuMetaData mmd on mm.ID=mmd.MainMenuID left join lu_Languages lg on mmd.LanguageID=lg.ID where ";
			//query += DatabaseHelper.AddCommonFields("mm.Status", "mm.Is_active", eExecuteLocation.Application, false);
			//query += "order by mm.ItemOrder";
        }

		protected override dsMenu.MenuDataTable FormatResults(DataTable originalObject)
        {
			dsMenu.MenuDataTable res = new dsMenu.MenuDataTable();

            if (originalObject == null)
                return null;

            res.Merge(originalObject);

            return res;
        }

		protected override Guid UniqueIdentifier
		{
			get { return new Guid("{7758707D-E665-4adb-8A08-7EA3CEFDCF6E}"); }
		}
    }
}
