using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace ODBCWrapper
{
    public class Utils
    {

        static public string GetSafeStr(object o)
        {
            if (o == DBNull.Value)
                return "";
            else if (o == null)
                return "";
            else
                return o.ToString();
        }

         public static int GetIntSafeVal(DataRow dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                     return int.Parse(dr[sField].ToString());
                 return 0;
             }
             catch
             {
                 return 0;
             }
         }

         public static int GetIntSafeVal(DataRowView dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                     return int.Parse(dr[sField].ToString());
                 return 0;
             }
             catch
             {
                 return 0;
             }
         }

         static public Byte GetByteSafeVal(DataRow dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                 {
                     return Convert.ToByte(dr[sField]);
                 }
                 return 0;
             }
             catch
             {
                 return 0;
             }
         }

         public static double GetDoubleSafeVal(DataRow dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                     return double.Parse(dr[sField].ToString());
                 return -1.0;
             }
             catch
             {
                 return -1.0;
             }
         }

         public static string GetSafeStr(DataRow dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                     return dr[sField].ToString();
                 return string.Empty;
             }
             catch
             {
                 return string.Empty;
             }
         }

         public static string GetSafeStr(DataRowView dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                     return dr[sField].ToString();
                 return string.Empty;
             }
             catch
             {
                 return string.Empty;
             }
         }

         static public Int64 GetLongSafeVal(DataRow dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                 {
                     return Convert.ToInt64(dr[sField]);
                 }
                 return 0;
             }
             catch
             {
                 return 0;
             }
         }
         static public Int64 GetLongSafeVal(DataRowView dr, string sField)
         {
             try
             {
                 if (dr != null && dr[sField] != DBNull.Value)
                 {
                     return Convert.ToInt64(dr[sField]);
                 }
                 return 0;
             }
             catch
             {
                 return 0;
             }
         }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, Int32 nCachSec)
        {
            return GetTableSingleVal(sTable, sFieldName, nID, nCachSec, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, Int32 nCachSec, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, nCachSec , sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID)
        {
            return GetTableSingleVal(sTable, sFieldName, nID , "");
        }  


        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID , sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, -1 , sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, Int32 nCachSec)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, nCachSec, "");
        }
        

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, Int32 nCachSec  , string sConnectionKey)
        {
            object oRet = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            if (nCachSec != -1)
                selectQuery.SetCachedSec(nCachSec);
            if (sConnectionKey != "") 
                selectQuery.SetConnectionKey(sConnectionKey);            
          
            //selectQuery += "select " + sFieldName + " from " + sTable + " where ";
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

        static public double GetDoubleSafeVal(ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return double.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        static public string GetStrSafeVal(ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString();
                return "";
            }
            catch
            {
                return "";
            }
        }

        static public Byte GetByteSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    return Convert.ToByte(o);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }


        static public Int32 GetIntSafeVal(ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return int.Parse(selectQuery.Table("query").DefaultView[nIndex].Row[sField].ToString());
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public Int32 GetIntSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    return Convert.ToInt32(o);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }


        static public double GetDoubleSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    return Convert.ToDouble(o);
                }
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        static public Int64 GetLongSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    return Convert.ToInt64(o);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public UInt64 GetUnsignedLongSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {

                    return Convert.ToUInt64(o);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        static public DateTime GetDateSafeVal(ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
        {
            try
            {
                if (selectQuery.Table("query").DefaultView[nIndex].Row[sField] != null &&
                    selectQuery.Table("query").DefaultView[nIndex].Row[sField] != DBNull.Value)
                    return (DateTime)(selectQuery.Table("query").DefaultView[nIndex].Row[sField]);
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }

        static public DateTime GetDateSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                {
                    return (DateTime)(dr[sField]);
                }
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }

        static public DateTime GetDateSafeVal(DataRowView dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                {
                    return (DateTime)(dr[sField]);
                }
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }


        static public DateTime GetDateSafeVal(object o)
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    DateTime dt = new DateTime();
                    if (DateTime.TryParseExact(o.ToString(), "M/dd/yyyy h:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
                    {
                        return dt; 
                    }
                    else
                    {
                        return (DateTime)(o);
                    }
                }
                return new DateTime(2000, 1, 1);
            }
            catch
            {
                return new DateTime(2000, 1, 1);
            }
        }


        public static string GetDelimitedStringFromDataTable(DataTable dt, string delimiter, string columnName, string prefix, string suffix)
        {
            string result = GetDelimitedStringFromDataTable(dt, delimiter, columnName);
            if (string.IsNullOrEmpty(result) == false && result.Length > 0)
            {
                result = prefix + result + suffix;
            }
            return result;
        }

        public static string GetDelimitedStringFromDataTable(DataTable dt, string delimiter, string columnName)
        {
            StringBuilder sb = new StringBuilder();

            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(delimiter);
                    }
                    sb.Append(dt.Rows[i][columnName].ToString());
                }
            }
            return sb.ToString();
        }


        public static string GetTcmConfigValue(string sKey)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                Logger.Logger.Log("ODBCWRapper", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }
    }

    
}
