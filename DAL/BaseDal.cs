using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Tvinci.Core.DAL
{
    public abstract class BaseDal
    {
        internal static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1); // must match the fictive end date
        // which ODBCWrapper.Utils.GetDateSafeVal returns.

        public static DataTable GetGroupsTree(long groupID)
        {
            return GetGroupsTree(groupID, "MAIN_CONNECTION_STRING");
        }

        public static DataTable GetGroupsTree(long groupID, string sConnectionKey)
        {     
            DataTable dtGroups = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (!string.IsNullOrEmpty(sConnectionKey))
                selectQuery.SetConnectionKey(sConnectionKey);

            selectQuery += "select * from dbo.F_Get_GroupsTree(" + groupID.ToString() + ")";

            if (selectQuery.Execute("query", true) != null)
            {
               dtGroups = selectQuery.Table("query");
            }
            selectQuery.Finish();
            selectQuery = null;

            return dtGroups;
        }
     
    }
}
