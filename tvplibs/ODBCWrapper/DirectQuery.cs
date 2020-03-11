using System;
using System.Reflection;
using KLogMonitor;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for DirectQuery.
    /// </summary>
    public class DirectQuery : Query
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public DirectQuery()
        {
        }

        public override bool Execute()
        {
            return Execute(m_sOraStr);
        }

        ~DirectQuery() { }

        protected virtual bool Execute(string oraStr)
        {
            m_sErrorMsg = "";
            m_sOraStr = oraStr;
            int_Execute();
            oraStr = m_sOraStr;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_DATABASE, null, null, null, null) { Database = m_sOraStr })
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                m_sErrorMsg = ex.Message;
                string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                logger.Error(sMes, ex);
                return false;
            }
            return true;
        }

        public static DirectQuery operator +(DirectQuery p, object sOraStr)
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
    }
}
