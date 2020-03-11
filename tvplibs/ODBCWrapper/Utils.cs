using System;
using System.Collections.Generic;
using System.Text;

namespace ODBCWrapper
{
    public class Utils
    {
        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, Int32 nCachSec)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, nCachSec);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, -1);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, Int32 nCachSec)
        {
            object oRet = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (nCachSec != -1)
                selectQuery.SetCachedSec(nCachSec);
            selectQuery += "select " + sFieldName + " from " + sTable + " where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM(sWhereField, sWhereSign, sWhereVal);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    oRet = selectQuery.Table("query").DefaultView[0].Row[sFieldName];
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return oRet;
        }
        
        static public string ReWriteTableValue(string sVal)
        {
            double number;
            if (double.TryParse(sVal, out number))
            {
                return String.Format("{0:0.##}", number);
            }
            else
            {
                return sVal;
            }             
        }

        static public DateTime GetCurrentDBTime()
        {
            object t = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new DataSetSelectQuery();
            selectQuery += "select getdate() as t from accounts";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    t = selectQuery.Table("query").DefaultView[0].Row["t"];
            }
            selectQuery.Finish();
            selectQuery = null;
            if (t != null && t != DBNull.Value)
                return (DateTime)t;
            return new DateTime();
        }
    }

    
}
