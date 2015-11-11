using System;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for Parameter.
	/// </summary>
	public class Parameter
	{
		private Parameter(string sParName , string sType , object sParVal)
		{
			m_sParName = sParName;
			m_sType = sType;
			m_sParVal = sParVal;
		}

		~Parameter(){}

		public string m_sParName = "";
		public string m_sType = "";
		public object m_sParVal = null;

		public static Parameter NEW_PARAM(string sParName , string sType , object sParVal)
		{
			return new Parameter(sParName , sType , sParVal);
		}

		public static Parameter NEW_PARAM(string sParName , object sParVal)
		{
			return NEW_PARAM(sParName , "" , sParVal);
		}
	}
}
