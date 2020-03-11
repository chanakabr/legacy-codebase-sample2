using System;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for DataSetInsertQuery.
	/// </summary>
	public class DataSetInsertQuery: DataSetQuery
	{
		public DataSetInsertQuery(string sTableName)
		{
			SetTable(sTableName);
		}

		~DataSetInsertQuery(){}

		public void SetTable(string sTableName)
		{
			m_sTableName = sTableName;
			m_sOraStr = "insert into " + sTableName + " ";
		}

		protected override bool AddParameter(string sParName , string sType , object sParVal)
		{
			if (m_sInsertStructure != "(")
				m_sInsertStructure += ",";
			m_sInsertStructure += sParName;

			if (m_sInsertValues != "(")
				m_sInsertValues += ",";
			m_sInsertValues += "?";
			m_hashTable[table_ind] = sParVal;
			table_ind++;
			return true;
		}

		protected override void FillQueryString(string oraStr)
		{
			m_sOraStr = oraStr;
			m_sOraStr += m_sInsertStructure + ") ";
			m_sOraStr += "VALUES " + m_sInsertValues + ")";
		}

		public static DataSetInsertQuery operator +(DataSetInsertQuery p, object sOraStr)
		{
			if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
			{
				p.AddParameter(((Parameter)sOraStr).m_sParName , 
					((Parameter)sOraStr).m_sType , 
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
