using System;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for DataSetSelectQuery.
    /// </summary>
    public class DataSetSelectQuery : DataSetQuery
    {
        public DataSetSelectQuery()
        {
            m_bIsWritable = false;
        }

        ~DataSetSelectQuery() { }

        public static DataSetSelectQuery operator +(DataSetSelectQuery p, object sOraStr)
        {
            if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
            {
                p.AddParameter(((Parameter)sOraStr).m_sParName,
                    ((Parameter)sOraStr).m_sType,
                    ((Parameter)sOraStr).m_sParVal);
            }
            else
                p.m_sOraStr.Append(" ").Append(sOraStr);

            return p;
        }

        public static implicit operator System.Data.DataSet(DataSetSelectQuery m)
        {
            return m.m_myDataSet;
        }
    }
}
