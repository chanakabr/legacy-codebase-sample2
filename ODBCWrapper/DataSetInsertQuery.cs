using System;
using System.Data.SqlClient;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for DataSetInsertQuery.
    /// </summary>
    public class DataSetInsertQuery : DataSetQuery
    {
        public DataSetInsertQuery(string sTableName)
        {
            SetTable(sTableName);
            m_bIsWritable = true;
        }

        ~DataSetInsertQuery() { }

        public void SetTable(string sTableName)
        {
            m_sTableName = sTableName;
            m_sOraStr = new System.Text.StringBuilder("insert into ").Append(sTableName).Append(" ");
        }

        protected override bool AddParameter(string parameterName, string type, object value)
        {
            if (m_sInsertStructure != "(")
                m_sInsertStructure += ",";
            m_sInsertStructure += parameterName;

            if (m_sInsertValues != "(")
                m_sInsertValues += ",";
            m_sInsertValues += "@P" + table_ind.ToString();

            if (value == null)
                value = DBNull.Value;

            m_hashTable[table_ind] = value;
            table_ind++;

            Utils.CheckDBReadWrite(parameterName, value, "DataSetInsertQuery", m_bIsWritable, ref Utils.UseWritable);

            return true;
        }

        protected override void FillQueryString(string oraStr)
        {
            m_sOraStr = new System.Text.StringBuilder(oraStr);
            m_sOraStr.Append(m_sInsertStructure).Append(") ");
            m_sOraStr.Append("VALUES ").Append(m_sInsertValues).Append(")");
        }

        public static DataSetInsertQuery operator +(DataSetInsertQuery p, object sOraStr)
        {
            if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
            {
                p.AddParameter(((Parameter)sOraStr).m_sParName,
                    ((Parameter)sOraStr).m_sType,
                    ((Parameter)sOraStr).m_sParVal);
            }

            return p;
        }

        public static implicit operator System.Data.DataSet(DataSetInsertQuery m)
        {
            return m.m_myDataSet;
        }

        protected override void Clean()
        {
            base.Clean();
            SetTable(m_sTableName);
        }

        private string m_sTableName = "";
    }
}
