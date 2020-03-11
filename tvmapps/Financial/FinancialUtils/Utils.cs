using System;

namespace FinancialUtils
{
    public class FinancialPayment
    {
        public Int32 m_nContractID;
        public Int32 m_nEntityID;
        public double m_dFullAmount;
        public double m_dFinalAmount;
        public Int32 m_nCurrencyID;
        public string m_sDiscountReason;
        public Int32 m_nGroupID;
        public double m_dBillingTransactionAmount;
        public Int32 m_nBillingProcessorID;
        public Int32 m_nBillingMethodID;

        public void Initialize(Int32 nContractID, Int32 nEntityID, double dFullAmount,
            double dFinalAmount, Int32 nCurrencyID, string sDiscountReason , Int32 nGroupID ,
            double dBillingTransactionAmount, Int32 nBillingProcessorID, Int32 nBillingMethodID)
        {
            m_nBillingProcessorID = nBillingProcessorID;
            m_nBillingMethodID = nBillingMethodID;
            m_nContractID = nContractID;
            m_nEntityID = nEntityID;
            m_dFullAmount = dFullAmount;
            m_dFinalAmount = dFinalAmount;
            m_nCurrencyID = nCurrencyID;
            m_sDiscountReason = sDiscountReason;
            m_nGroupID = nGroupID;
            m_dBillingTransactionAmount = dBillingTransactionAmount;
        }

        public FinancialPayment()
        {
            m_nBillingProcessorID = 0;
            m_nBillingMethodID = 0;
            m_dBillingTransactionAmount = 0.0;
            m_nGroupID = 0;
            m_nContractID = 0;
            m_nEntityID = 0;
            m_dFullAmount = 0;
            m_dFinalAmount = 0;
            m_nCurrencyID = 0;
            m_sDiscountReason = "";
        }
    }

    public class Utils
    {
        public static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        static public Int32 GetCurrencyIDByCode(string sCode3)
        {
            Int32 nCurrencyID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from lu_currency where";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code3", "=", sCode3);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nCurrencyID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return nCurrencyID;
        }
        static public Int32 GetSyncStatus(Int32 nRowID)
        {
            Int32 nRet = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("tables_sync", "sync_status", nRowID, 0).ToString());
            return nRet;
        }

        static public void UpdateSyncStatus(Int32 nStatus, Int32 nRow)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("tables_sync");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("sync_status", "=", nStatus);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nRow);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static public double GetDoubleSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
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

        static public string GetStrSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
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

        static public Int32 GetIntSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
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

        static public DateTime GetDateSafeVal(ref ODBCWrapper.DataSetSelectQuery selectQuery, string sField, Int32 nIndex)
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

        static public Int32 GetCountryID(string sFullName)
        {
            Int32 nRet = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from countries where  ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LTRIM(RTRIM(LOWER(COUNTRY_NAME)))", "=", sFullName.Trim().ToLower());
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nRet = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
            }
            selectQuery.Finish();
            selectQuery = null;
            return nRet;
        }
    }
}
