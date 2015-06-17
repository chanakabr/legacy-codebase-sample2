using System;
using System.Reflection;
using KLogMonitor;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for InsertQuery.
    /// </summary>
    public class InsertQuery : DirectQuery
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public InsertQuery(string sTableName)
        {
            SetTable(sTableName);
        }

        ~InsertQuery() { }

        public void SetTable(string sTableName)
        {
            m_sOraStr = "insert into " + sTableName + " ";
            sInsertStructure = "(";
            sInsertValues = "(";
        }

        protected override bool AddParameter(string sParName, string sType, object sParVal)
        {
            if (sInsertStructure != "(")
                sInsertStructure += ",";
            sInsertStructure += sParName;

            if (sInsertValues != "(")
                sInsertValues += ",";
            sInsertValues += "?";
            m_hashTable[table_ind] = sParVal;
            table_ind++;
            return true;
        }

        protected override bool Execute(string oraStr)
        {
            m_sOraStr = oraStr;
            m_sOraStr += sInsertStructure;
            m_sOraStr += ") ";
            m_sOraStr += "VALUES ";
            m_sOraStr += sInsertValues;
            m_sOraStr += ")";
            oraStr = m_sOraStr;
            int_Execute();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = m_sOraStr, QueryType = KLogEnums.eDBQueryType.INSERT })
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                m_sErrorMsg = ex.Message;
                string message = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                logger.Error(message, ex);
                return false;
            }
            return true;
        }

        public static InsertQuery operator +(InsertQuery p, object sOraStr)
        {
            if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
            {
                p.AddParameter(((Parameter)sOraStr).m_sParName,
                    ((Parameter)sOraStr).m_sType,
                    ((Parameter)sOraStr).m_sParVal);
            }
            else
                p.m_sOraStr += " " + sOraStr;
            return p;
        }

        protected string sInsertStructure = "(";
        protected string sInsertValues = "(";
    }
}
