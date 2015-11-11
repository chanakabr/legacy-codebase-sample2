using System;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for UpdateQuery.
	/// </summary>
	public class UpdateQuery: DirectQuery
	{
		public UpdateQuery(string sTableName)
		{
			m_sOraStr = "update " + sTableName + " set ";
		}

		~UpdateQuery(){}

		public static UpdateQuery operator +(UpdateQuery p, object sOraStr)
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

		protected override bool AddParameter(string sParName , string sType , object sParVal)
		{
			if (m_sOraStr.ToUpper().IndexOf("WHERE") > 0)
				return base.AddParameter(sParName , sType , sParVal);
			if (sType == "")
				sType = "=";
			if (table_ind > 0)
			{
				m_sOraStr += ",";
				return base.AddParameter(sParName , sType , sParVal);
			}
			else
				return base.AddParameter(sParName , sType , sParVal);
		}
	}
}
