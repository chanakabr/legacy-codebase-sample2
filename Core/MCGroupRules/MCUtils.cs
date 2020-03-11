using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using TVinciShared;

namespace MCGroupRules
{
    public static class MCUtils
    {
        public static List<string> GetVarTableNames(DataTable ruleVarsDt)
        {
            string tableName = "Table_Name";
            DataView view = new DataView(ruleVarsDt);
            DataTable tmpDt = view.ToTable(true, tableName);
            List<string> retList = new List<string>();
            foreach (DataRow dt in tmpDt.Rows)
            {
                retList.Add(dt[tableName].ToString());
            }

            return retList;
        }

        public static Dictionary<string, string> GetVarsForTable(DataTable ruleVarsDt, string tableName)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            DataRow[] drArr = ruleVarsDt.Select("Table_Name ='" + tableName + "'");
            foreach (DataRow dr in drArr)
            {
                retDict.Add(dr["Col_Name"].ToString(), dr["Var_Name"].ToString());
            }
            return retDict;
        }

        public static string IntListToCsvString(List<int> intList)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            if (intList.Count > 0)
            {
                foreach (int guid in intList)
                {
                    i++;
                    sb.Append(guid);
                    if (i < intList.Count)
                    {
                        sb.Append(",");
                    }
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        public static string GetGuidColNameByTableName(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "users_media_mark":
                    return "site_user_guid";
                case "users":
                    return "ID";
                default:
                    return "ID";
            }

        }

        public static void SetConnectionStringByVarTable(string tableName, ref ODBCWrapper.DataSetSelectQuery selectQuery)
        {
            switch (tableName.ToLower())
            {
                case "users":
                    selectQuery.SetConnectionKey("users_connection");
                    break;
                case "subscriptions_purchases":
                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                    break;
                default:
                    break;
            }
        }

        public static string GetAllGroupIdsToStr(int groupId)
        {
            string groupIdsStr = groupId.ToString();
            string tmpStr = "";
            PageUtils.GetAllGroupsStr(groupId, ref tmpStr);
            groupIdsStr += tmpStr;
            return groupIdsStr;
        }

        public static int GetDaysFromMinPeriodId(int minPeriodId)
        {
            if (minPeriodId < 1111111)
            {
                return minPeriodId / 1440;
            }
            else if (minPeriodId >= 1111111 && minPeriodId < 11111111)
            {
                int tmpInt = int.Parse(minPeriodId.ToString()[0].ToString());
                TimeSpan ts = (DateTime.Today - DateTime.Today.AddMonths(-tmpInt));
                return ts.Days;
            }
            else if (minPeriodId >= 11111111)
            {
                int tmpInt = int.Parse(minPeriodId.ToString()[0].ToString());
                TimeSpan ts = (DateTime.Today - DateTime.Today.AddYears(-tmpInt));
                return ts.Days;
            }
            else return int.MaxValue;
        }


        

        
    }
}
