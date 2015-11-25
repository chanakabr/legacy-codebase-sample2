using System;
using System.Data.Odbc;
using System.Reflection;
using KLogMonitor;
using System.Data.SqlClient;

namespace ODBCWrapper
{
    /// <summary>
    /// Summary description for SelectQuery.
    /// </summary>
    public class SelectQuery : Query
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public SelectQuery()
        {
            m_myReader = null;
            command = null;
        }

        ~SelectQuery() { }

        public override void Finish()
        {
            base.Finish();
            if (m_myReader != null)
                m_myReader.Close();

            m_myReader = null;

        }

        public SelectQuery(ref Connection conn)
            : base(ref conn)
        {
            m_myReader = null;
            command = null;
        }

        public bool NextRow()
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
                return m_myReader.Read();
            else
                return false;
            //}
        }

        public string GetValue(string sName)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                for (int i = 0; i < m_myReader.FieldCount; i++)
                {
                    if (m_myReader.GetName(i).ToString().ToUpper() == sName.ToUpper())
                        return m_myReader.GetValue(i).ToString();
                }
                return "";
            }
            else
            {
                return "";
            }
            //}
        }

        public object GetObjValue(string sName)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                for (int i = 0; i < m_myReader.FieldCount; i++)
                {
                    if (m_myReader.GetName(i).ToString().ToUpper() == sName.ToUpper())
                        return m_myReader.GetValue(i);
                }
                return null;
            }
            else
            {
                return null;
            }
            //}
        }

        public bool GetValue(Int32 ind,
            ref string sName,
            ref object sValue)
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                sName = m_myReader.GetName(ind).ToString().ToUpper();
                sValue = m_myReader.GetValue(ind);
                return true;
            }
            else
                return false;
            //}
        }

        public Int32 GetFieldCount()
        {
            //lock(m_crit_sec)
            //{
            if (m_myReader != null)
            {
                return m_myReader.FieldCount;
            }
            else
                return 0;
            //}
        }
        public override bool Execute()
        {
            return Execute(m_sOraStr);
        }

        private bool Execute(string oraStr)
        {
            m_sOraStr = oraStr;
            int_Execute();
            try
            {
                m_myReader = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                string message = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception occurred: " + ex.Message;
                logger.Error(message, ex);
                return false;
            }
            return true;
        }

        public static SelectQuery operator +(SelectQuery p, object sOraStr)
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

        public static implicit operator SqlDataReader(SelectQuery m)
        {
            return m.m_myReader;
        }

        protected SqlDataReader m_myReader;
    }
}
