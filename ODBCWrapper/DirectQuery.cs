using System;
using System.Data.SqlClient;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for DirectQuery.
	/// </summary>
	public class DirectQuery : Query
	{
		public DirectQuery()
		{
            m_bIsWritable = true;
		}

		public override bool Execute()
		{
			return Execute(m_sOraStr.ToString());
		}

		~DirectQuery(){}

        public bool SetLockTimeOut(ref SqlConnection con)
        {
            if (m_nLockTimeOut == -1)
                return true;
            m_sErrorMsg = "";
            m_sOraStr = new System.Text.StringBuilder("SET LOCK_TIMEOUT ").Append(m_nLockTimeOut.ToString());
            int_Execute();

            try
            {
                if (con.State != System.Data.ConnectionState.Open)
                    con.Open();
                command.Connection = con;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to execute sql statment", ex);
                m_sErrorMsg = ex.Message;
                string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception accured: " + ex.Message;
                Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net");
                return false;
            }
            return true;
        }

		protected virtual bool Execute(string oraStr)
		{
            m_sErrorMsg = "";
			m_sOraStr = new System.Text.StringBuilder(oraStr);
			int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return false;
            using (SqlConnection con = new SqlConnection(sConn))
            {
                oraStr = m_sOraStr.ToString();
                try
                {
                    con.Open();
                    SetLockTimeOut(con);
                    command.Connection = con;
                    DateTime dStart = DateTime.Now;
                    command.ExecuteNonQuery();
                    TimeSpan t = DateTime.Now - dStart;
                    if (t.TotalMilliseconds > m_nLongQueryTime)
                    {
                        string sMes = t.TotalMilliseconds.ToString() + "ms: " + m_sLastExecutedOraStr;
                        Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net_Long");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Failed to execute sql statment", ex);
                    m_sErrorMsg = ex.Message;
                    string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
                    return false;
                }
            }
			return true;
		}

		public static DirectQuery operator +(DirectQuery p, object sOraStr)
		{
			if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
			{
				p.AddParameter(((Parameter)sOraStr).m_sParName , 
					((Parameter)sOraStr).m_sType , 
					((Parameter)sOraStr).m_sParVal);
			}
			else
				p.m_sOraStr.Append(" ").Append(sOraStr);
			return p;
		}
	}
}
