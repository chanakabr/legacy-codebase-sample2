using System;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for DirectQuery.
	/// </summary>
	public class DirectQuery : Query
	{
		public DirectQuery()
		{				
		}

		public override bool Execute()
		{
			return Execute(m_sOraStr);
		}

		~DirectQuery(){}

		protected virtual bool Execute(string oraStr)
		{
            m_sErrorMsg = "";
			m_sOraStr = oraStr;
			int_Execute();
			oraStr = m_sOraStr;
			try 
			{
				command.ExecuteNonQuery();
			}
			catch(Exception ex) 
			{
                logger.Error("Failed to execute sql statment",ex);
                m_sErrorMsg = ex.Message;
				string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception accured: "+ex.Message;
				Logger.Logger.Log(this.GetType().ToString(), sMes , "ODBC_Net");
				return false;
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
				p.m_sOraStr += " " +sOraStr;
			return p;
		}
	}
}
