using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using KLogMonitor;
using System.Reflection;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Linq;

namespace ODBCWrapper
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string DB_SP_ROUTING_KEY = "db_sp_routing";
        private const int CB_DB_SP_ROUTING_EXPIRY_MIN = 60;
        private const string REGEX_TABLE_NAME = @"\bjoin\s+(?<Retrieve>[a-zA-Z\._\d]+)\b|\bfrom\s+(?<Retrieve>[a-zA-Z\._\d]+)\b|\bupdate\s+(?<Update>[a-zA-Z\._\d]+)\b|\binsert\s+(?:\binto\b)?\s+(?<Insert>[a-zA-Z\._\d]+)\b|\btruncate\s+table\s+(?<Delete>[a-zA-Z\._\d]+)\b|\bdelete\s+(?:\bfrom\b)?\s+(?<Delete>[a-zA-Z\._\d]+)\b";
        public static readonly DateTime FICTIVE_DATE = new DateTime(2000, 1, 1);
        static List<string> dbWriteLockParams = (!string.IsNullOrEmpty(TCMClient.Settings.Instance.GetValue<string>("DB_WriteLock_Params"))) ? TCMClient.Settings.Instance.GetValue<string>("DB_WriteLock_Params").Split(';').ToList<string>() : null;
        static public string dBVersionPrefix = (!string.IsNullOrEmpty(TCMClient.Settings.Instance.GetValue<string>("DB_Settings.prefix"))) ? string.Concat("__", TCMClient.Settings.Instance.GetValue<string>("DB_Settings.prefix"), "__") : string.Empty;
        static bool UseReadWriteLockMechanism = TCMClient.Settings.Instance.GetValue<bool>("DB_WriteLock_Use");
        [ThreadStatic]
        public static bool UseWritable;

        protected const int RETRY_LIMIT = 5;

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

        public static Int64 GetLongSafeVal(DataRow dr, string sField, Int64 returnThisInCaseOfFail)
        {
            Int64 res = returnThisInCaseOfFail;
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    res = Int64.Parse(dr[sField].ToString());
                return res;
            }
            catch
            {
                return res;
            }
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, Int32 nCachSec)
        {
            return GetTableSingleVal(sTable, sFieldName, nID, nCachSec, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, Int32 nCachSec, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, nCachSec, sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, long nID, Int32 nCachSec, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, nCachSec, sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID)
        {
            return GetTableSingleVal(sTable, sFieldName, nID, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, Int32 nID, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, "id", "=", nID, sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, string sConnectionKey)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, -1, sConnectionKey);
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, Int32 nCachSec)
        {
            return GetTableSingleVal(sTable, sFieldName, sWhereField, sWhereSign, sWhereVal, nCachSec, "");
        }

        static public object GetTableSingleVal(string sTable, string sFieldName, string sWhereField, string sWhereSign, object sWhereVal, Int32 nCachSec, string sConnectionKey)
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

        static public DateTime? GetNullableDateSafeVal(DataRow dr, string sField)
        {
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                {
                    return (DateTime)(dr[sField]);
                }
                return null;
            }
            catch
            {
                return null;
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

        static public DateTime GetDateSafeVal(object o, string format = "M/dd/yyyy h:mm:ss tt")
        {
            try
            {
                if (o != null && o != DBNull.Value)
                {
                    DateTime dt = new DateTime();
                    //string format = "M/dd/yyyy h:mm:ss tt";
                    //string format = "dd/MM/yyyy HH:mm:ss";

                    if (DateTime.TryParseExact(o.ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dt))
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
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("missing key");
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error("Key=" + sKey, ex);
            }
            return result;
        }

        public static int GetIntSafeVal(object o, int returnThisInCaseOfFail)
        {
            int temp = 0;
            int res = returnThisInCaseOfFail;
            if (o != null)
            {
                string s = o.ToString();
                if (s.Length > 0 && Int32.TryParse(s, out temp))
                {
                    res = temp;
                }
            }

            return res;
        }

        public static int GetIntSafeVal(DataRow dr, string sField, int returnThisInCaseOfFail)
        {
            int res = returnThisInCaseOfFail;
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    res = int.Parse(dr[sField].ToString());
                return res;
            }
            catch
            {
                return res;
            }
        }

        /// <summary>
        /// Extracts an integer value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public int ExtractInteger(DataRow p_drSource, string p_sFieldName)
        {
            int nResult = 0;

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        nResult = Convert.ToInt32(objValue);
                    }
                }
            }
            catch
            {
            }

            return (nResult);
        }

        /// <summary>
        /// Extracts a boolean value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public bool ExtractBoolean(DataRow p_drSource, string p_sFieldName)
        {
            bool bResult = false;

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        bResult = Convert.ToBoolean(objValue);
                    }
                }
            }
            catch
            {
            }

            return (bResult);
        }

        /// <summary>
        /// Extracts a date time value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public DateTime ExtractDateTime(DataRow p_drSource, string p_sFieldName)
        {
            DateTime dtResult = DateTime.MinValue;

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        dtResult = Convert.ToDateTime(objValue);
                    }
                }
            }
            catch
            {
            }

            return (dtResult);
        }

        /// <summary>
        /// Extracts a nullable date time value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public DateTime? ExtractNullableDateTime(DataRow p_drSource, string p_sFieldName)
        {
            DateTime? dtResult = null;

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        dtResult = Convert.ToDateTime(objValue);
                    }
                }
            }
            catch
            {
            }

            return (dtResult);
        }

        /// <summary>
        /// Extracts a dynamic type value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public string ExtractString(DataRow p_drSource, string p_sFieldName)
        {
            string sResult = string.Empty;

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        sResult = Convert.ToString(objValue);
                    }
                }
            }
            catch
            {
            }

            return (sResult);
        }

        /// <summary>
        /// Extracts a dynamic type value from a data row in the most efficient way
        /// </summary>
        /// <param name="p_drSource"></param>
        /// <param name="p_sFieldName"></param>
        /// <returns></returns>
        static public T ExtractValue<T>(DataRow p_drSource, string p_sFieldName)
        {
            T oResult = default(T);

            try
            {
                if (p_drSource != null)
                {
                    object objValue = p_drSource[p_sFieldName];

                    if (objValue != null && objValue != DBNull.Value)
                    {
                        oResult = (T)Convert.ChangeType(objValue, typeof(T));
                    }
                }
            }
            catch
            {
            }

            return (oResult);
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

        public static long DateTimeToUnixTimestampMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalMilliseconds;
        }

        public static DateTime UnixTimestampToDateTime(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long DateTimeToUnixTimestampUtc(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static DateTime ConvertToUtc(DateTime time, string timezone)
        {
            DateTime unspecifiedKindTime = new DateTime(time.Year, time.Month, time.Day, time.Hour,
                                             time.Minute, time.Second, DateTimeKind.Unspecified);

            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedKindTime, tst);
        }

        public static DateTime ConvertFromUtc(DateTime time, string timezone)
        {
            DateTime unspecifiedKindTime = new DateTime(time.Year, time.Month, time.Day, time.Hour,
                                             time.Minute, time.Second, DateTimeKind.Utc);

            TimeZoneInfo tst = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            return TimeZoneInfo.ConvertTimeFromUtc(unspecifiedKindTime, tst);
        }

        public static SqlQueryInfo GetSqlDataMonitor(SqlCommand command)
        {
            SqlQueryInfo sqlInfo = new SqlQueryInfo();

            if (command != null)
            {
                // update db name
                sqlInfo.Database = command.Connection != null ? command.Connection.Database : "unknown";

                if (command.CommandType == System.Data.CommandType.StoredProcedure)
                {
                    // stored procedure
                    sqlInfo.QueryType = KLogEnums.eDBQueryType.EXECUTE;
                    sqlInfo.Table = command.CommandText != null ? command.CommandText : "stored_procedure_unknown";
                }
                else
                {
                    string query = string.Empty;
                    if (!string.IsNullOrEmpty(command.CommandText))
                    {
                        query = command.CommandText.Trim().ToLower();

                        // update query type
                        if (query.StartsWith("select"))
                            sqlInfo.QueryType = KLogEnums.eDBQueryType.SELECT;
                        else if (query.StartsWith("delete"))
                            sqlInfo.QueryType = KLogEnums.eDBQueryType.DELETE;
                        else if (query.StartsWith("insert"))
                            sqlInfo.QueryType = KLogEnums.eDBQueryType.INSERT;
                        else if (query.StartsWith("update"))
                            sqlInfo.QueryType = KLogEnums.eDBQueryType.UPDATE;
                        else if (query.StartsWith("set"))
                        {
                            sqlInfo.QueryType = KLogEnums.eDBQueryType.COMMAND;
                            sqlInfo.Table = command.CommandText != null ? command.CommandText : "command unknown";
                        }

                        // get table name
                        Regex tableNameReegx = new Regex(REGEX_TABLE_NAME, RegexOptions.Singleline);
                        var allMatches = tableNameReegx.Matches(query);
                        if (allMatches != null && allMatches.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < allMatches.Count; i++)
                            {
                                if (i == 0)
                                    sb.Append(allMatches[i].ToString().ToLower().Replace("from", string.Empty).Trim());
                                else
                                    sb.Append(" " + allMatches[i].ToString().ToLower().Replace("from", string.Empty).Trim());
                            }

                            sqlInfo.Table = sb.ToString();
                        }
                    }
                }
            }
            return sqlInfo;
        }

        public static DataRow GetTableSingleRow(string tableName, long id, string connectionKey = "", int timeInCache = -1, bool checkStatusAndIsActive = false)
        {
            return GetTableSingleRow(tableName, id.ToString(), connectionKey, timeInCache, checkStatusAndIsActive);
        }

        public static DataRow GetTableSingleRow(string tableName, string id, string connectionKey = "", int timeInCache = -1, bool checkStatusAndIsActive = false)
        {
            return GetTableSingleRowByValue(tableName, "ID", id, checkStatusAndIsActive, connectionKey, timeInCache);
        }

        public static DataRow GetTableSingleRowByValue(string tableName, string columnName, object value,
            bool checkStatusAndIsActive = false, string connectionKey = "", int timeInCache = -1)
        {
            List<KeyValuePair<string, object>> values = new List<KeyValuePair<string, object>>();

            values.Add(new KeyValuePair<string, object>(columnName, value));

            if (checkStatusAndIsActive)
            {
                values.Add(new KeyValuePair<string, object>("STATUS", 1));
                values.Add(new KeyValuePair<string, object>("IS_ACTIVE", 1));
            }

            return GetTableSingleRowByValues(tableName,
                values,
                connectionKey,
                timeInCache);
        }

        public static DataRow GetTableSingleRowByValues(string tableName, List<KeyValuePair<string, object>> values, string connectionKey = "", int timeInCache = -1)
        {
            DataRow result = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            if (timeInCache != -1)
            {
                selectQuery.SetCachedSec(timeInCache);
            }

            if (connectionKey != "")
            {
                selectQuery.SetConnectionKey(connectionKey);
            }

            //selectQuery += "select " + sFieldName + " from " + sTable + " where ";
            selectQuery += "SELECT * FROM " + tableName + " WHERE ";

            for (int i = 0; i < values.Count; i++)
            {
                var value = values[i];
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(value.Key, "=", value.Value);

                if (i < values.Count - 1)
                {
                    selectQuery += " AND ";
                }
            }

            if (selectQuery.Execute("query", true) != null)
            {
                var table = selectQuery.Table("query");

                if (table != null && table.DefaultView.Count > 0 && table.Rows != null && table.Rows.Count > 0)
                {
                    result = table.Rows[0];
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return result;
        }

        public static DataRow GetTableSingleRowColumnsByParamValue(string tableName, string paramName, string paramID, List<string> columnsToFetch, string connectionKey = "", int timeInCache = -1)
        {
            DataRow result = null;
            if (columnsToFetch != null && columnsToFetch.Count > 0)
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

                if (timeInCache != -1)
                {
                    selectQuery.SetCachedSec(timeInCache);
                }

                if (!string.IsNullOrEmpty(connectionKey))
                {
                    selectQuery.SetConnectionKey(connectionKey);
                }
                selectQuery += string.Format("SELECT {0} FROM " + tableName + " WHERE ", string.Join(",", columnsToFetch));
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM(paramName, "=", paramID);

                if (selectQuery.Execute("query", true) != null)
                {
                    var table = selectQuery.Table("query");

                    if (table != null && table.DefaultView.Count > 0 && table.Rows != null && table.Rows.Count > 0)
                    {
                        result = table.Rows[0];
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }

            return result;
        }

        public static DataTable GetCompleteTable(string tableName, string connectionKey = "", int timeInCache = -1)
        {
            DataTable result = null;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            if (timeInCache != -1)
            {
                selectQuery.SetCachedSec(timeInCache);
            }

            if (connectionKey != "")
            {
                selectQuery.SetConnectionKey(connectionKey);
            }

            selectQuery += "SELECT * FROM " + tableName;

            if (selectQuery.Execute("query", true) != null)
            {
                var table = selectQuery.Table("query");

                if (table != null && table.DefaultView.Count > 0 && table.Rows != null && table.Rows.Count > 0)
                {
                    result = table;
                }
            }

            selectQuery.Finish();
            selectQuery = null;

            return result;
        }

        public static long? GetLongSafeVal(DataRow dr, string sField, long? returnThisInCaseOfFail)
        {
            long? res = returnThisInCaseOfFail;
            try
            {
                if (dr != null && dr[sField] != DBNull.Value)
                    res = Int64.Parse(dr[sField].ToString());
                return res;
            }
            catch
            {
                return res;
            }
        }

        public static void CheckDBReadWrite(string sKey, object oValue, object executer, bool isWritable, ref bool useWriteable)
        {
            //Logger.Logger.Log("DBLock ", "m_bUseWritable is '" + useWriteable + "',For: " + executer, "ODBC_DBLock");
            //m_bIsWritable || m_bUseWritable
            if (!useWriteable)
            {
                Utils.UseWritable = ReadWriteLock(sKey, oValue, executer, isWritable);

                //if (useWriteable) Logger.Logger.Log("DBLock ", "m_bUseWritable changed to '" + Utils.UseWritable + "', for: " + executer, "ODBC_DBLock");
            }
        }

        public static bool ReadWriteLock(string sKey, object oValue, object executer, bool isWritable)
        {
            if (!UseReadWriteLockMechanism)
            {
                return true;
            }

            bool bRet = false;
            try
            {
                string cbKeyPrefix = "DB_WriteLock_";

                //Logger.Logger.Log("DBLock; Executer=" + executer + ", isWritable=" + isWritable, "none", "ODBC_Net");

                if (dbWriteLockParams != null)
                {

                    if (dbWriteLockParams.Any(x => x.ToLower().Equals(sKey.ToLower().TrimStart('@'))))
                    {
                        //Couchbase.CouchbaseClient cbClient = CouchbaseManager.CouchbaseManager.GetInstance(CouchbaseManager.eCouchbaseBucket.CACHE);
                        CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);


                        // ReadOnly request
                        if (!isWritable)
                        {
                            try
                            {
                                if (oValue is System.Collections.IList)
                                {
                                    foreach (var val in (oValue as System.Collections.IList))
                                    {
                                        //Logger.Logger.Log("DBLock ", "Check lock for " + executer + ", Key: " + cbKeyPrefix + val, "ODBC_DBLock");
                                        if (!string.IsNullOrEmpty(cbManager.Get<string>(cbKeyPrefix + val)))
                                        {
                                            //Logger.Logger.Log("DBLock ", "Key exist for " + executer + ", Key: " + cbKeyPrefix + val, "ODBC_DBLock");
                                            bRet = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //Logger.Logger.Log("DBLock ", "Check lock for " + executer + ", Key: " + cbKeyPrefix + oValue, "ODBC_DBLock");
                                    bool b = !string.IsNullOrEmpty(cbManager.Get<string>(cbKeyPrefix + oValue.ToString()));
                                    if (b)
                                    {
                                        //Logger.Logger.Log("DBLock ", "Key exist for " + executer + ", Key: " + cbKeyPrefix + oValue, "ODBC_DBLock");
                                        bRet = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //Logger.Logger.Log("DBLock; Executer=" + executer + ", isWritable=" + isWritable, ex.ToString(), "ODBC_DBLock");
                            }
                        }

                        // Write request
                        else
                        {
                            try
                            {
                                if (oValue is System.Collections.IList)
                                {
                                    foreach (string val in oValue as IList<string>)
                                    {
                                        bool res = cbManager.Set(cbKeyPrefix + val, executer, (uint)TCMClient.Settings.Instance.GetValue<int>("DB_WriteLock_TTL"));
                                        //Logger.Logger.Log("DBLock ", "Created (" + res + ") for " + executer + ", with Key: " + cbKeyPrefix + val, "ODBC_DBLock");
                                    }
                                }
                                else
                                {
                                    bool res = cbManager.Set(cbKeyPrefix + oValue, executer, (uint)TCMClient.Settings.Instance.GetValue<int>("DB_WriteLock_TTL"));
                                    //Logger.Logger.Log("DBLock ", "Created (" + res + ") for " + executer + ", with Key: " + cbKeyPrefix + oValue, "ODBC_DBLock");
                                }
                            }
                            catch (Exception ex)
                            {
                                //Logger.Logger.Log("DBLock; Executer=" + executer + ", isWritable=" + isWritable, ex.ToString(), "ODBC_DBLock");
                            }

                            bRet = true; // created a session lock for a write lock
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Logger.Log("DBLock", ex.ToString(), "ODBC_DBLock");
            }
            return bRet;
        }

        public static DatabaseStoredProceduresMapping GetDatabaseStoredProceduresRouting()
        {
            DatabaseStoredProceduresMapping response = null;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            Couchbase.IO.ResponseStatus getResult = new Couchbase.IO.ResponseStatus();
            string dbSpRoutingKey = GetDbSpRoutingKey();
            if (string.IsNullOrEmpty(dbSpRoutingKey))
            {
                log.ErrorFormat("Failed GetDbSpRoutingKey");
                return response;
            }
            try
            {
                int numOfRetries = 0;
                while (numOfRetries < limitRetries)
                {
                    response = cbClient.Get<DatabaseStoredProceduresMapping>(dbSpRoutingKey, out getResult);
                    if (getResult == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        log.ErrorFormat("Error while trying to get database stored procedure routing, KeyNotFound. key: {0}", dbSpRoutingKey);
                        break;
                    }
                    else if (getResult == Couchbase.IO.ResponseStatus.Success)
                    {
                        log.DebugFormat("DatabaseStoredProceduresMapping with key {0} was found", dbSpRoutingKey);
                        break;
                    }
                    else
                    {
                        log.ErrorFormat("Retrieving DatabaseStoredProceduresMapping with key: {0} failed with status: {1}, retryAttempt: {2}, maxRetries: {3}", dbSpRoutingKey, getResult, numOfRetries, limitRetries);
                        numOfRetries++;
                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to get DatabaseStoredProceduresMapping", ex);
            }

            return response;
        }

        public static bool SetDatabaseStoredProceduresRouting(DatabaseStoredProceduresMapping dbSpRouting)
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string dbSpRoutingKey = GetDbSpRoutingKey();
            if (string.IsNullOrEmpty(dbSpRoutingKey))
            {
                log.ErrorFormat("Failed GetDbSpRoutingKey");
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    ulong version;
                    Couchbase.IO.ResponseStatus status;
                    DatabaseStoredProceduresMapping currentDbSpRouting = cbClient.GetWithVersion<DatabaseStoredProceduresMapping>(dbSpRoutingKey, out version, out status);
                    if (status == Couchbase.IO.ResponseStatus.Success || status == Couchbase.IO.ResponseStatus.KeyNotFound)
                    {
                        result = cbClient.SetWithVersion<DatabaseStoredProceduresMapping>(dbSpRoutingKey, dbSpRouting, version, (CB_DB_SP_ROUTING_EXPIRY_MIN * 60));
                    }

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while updating database stored procedure routing. key: {0}, number of tries: {1}/{2}", dbSpRoutingKey, numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to update DatabaseStoredProceduresMapping", ex);                
            }

            return result;
        }

        public static bool RemoveDatabaseStoredProcedureRouting()
        {
            bool result = false;
            CouchbaseManager.CouchbaseManager cbClient = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.CACHE);
            int limitRetries = RETRY_LIMIT;
            Random r = new Random();
            string dbSpRoutingKey = GetDbSpRoutingKey();
            if (string.IsNullOrEmpty(dbSpRoutingKey))
            {
                log.ErrorFormat("Failed GetDbSpRoutingKey");
                return false;
            }
            try
            {
                int numOfRetries = 0;
                while (!result && numOfRetries < limitRetries)
                {
                    result = cbClient.Remove(dbSpRoutingKey);

                    if (!result)
                    {
                        numOfRetries++;
                        log.ErrorFormat("Error while trying to remove database stored procedure routing. key: {0}, number of tries: {1}/{2}", dbSpRoutingKey, numOfRetries, limitRetries);

                        System.Threading.Thread.Sleep(r.Next(50));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error while trying to remove DatabaseStoredProceduresMapping", ex);
            }

            return result;
        }

        private static string GetDbSpRoutingKey()
        {
            string key = string.Empty;
            string version = GetTcmConfigValue("Version");
            if (!string.IsNullOrEmpty(version))
            {
                key = string.Format("{0}_{1}", version, DB_SP_ROUTING_KEY);
            }
            else
            {
                log.Error("Can't get version value from TCM");
            }

            return key;
        }

        internal static bool GetProcedureDbMappingByName(string procedureName)
        {
            bool result = false;
            ODBCWrapper.StoredProcedure spGetDomainRecordingsByDomainSeriesId = new ODBCWrapper.StoredProcedure("GetDatabaseStoredProcedureRouting");
            spGetDomainRecordingsByDomainSeriesId.AddParameter("@Name", procedureName);
            DataTable dt = spGetDomainRecordingsByDomainSeriesId.Execute();
            if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
            {
                int route = GetIntSafeVal(dt.Rows[0], "route", 0);
                result = route == 1;
            }

            return result;
        }
    }
}
