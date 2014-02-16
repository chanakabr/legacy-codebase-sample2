using System;
using System.Data.SqlClient;
using System.Data;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for InsertQuery.
	/// </summary>
	public class InsertQuery: DirectQuery
	{
        public InsertQuery():base()
        {
        }

        
        public InsertQuery(string sTableName)
		{
			SetTable(sTableName);
            m_bIsWritable = true;
		}

		~InsertQuery(){}

		public void SetTable(string sTableName)
		{
			m_sOraStr = new System.Text.StringBuilder("insert into ").Append(sTableName).Append(" ");
			sInsertStructure = "(";
			sInsertValues = "(";
		}

        /// <summary>
        /// Inserts bulk of rows to a specific table(which its name represent by sTableName param) at the db
        /// at one time, the bulk of rows is taken from dtData param.
        /// the dtData datatable structure must match the structure of the destination table at the db.
        /// </summary>
        /// <param name="sTableName"></param>
        /// <param name="dtData"></param>
        public void InsertBulk(string sTableName, DataTable dtData)
        {
            if (dtData != null && dtData.Rows.Count > 0)
            {
                int numRows = dtData.Rows.Count;
                string connString = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connString)
                {
                    DestinationTableName = sTableName,
                    BatchSize = numRows,
                    BulkCopyTimeout = 360
                };

                foreach (DataColumn col in dtData.Columns)
                {
                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                }
                bulkCopy.WriteToServer(dtData);
            }
        }

		protected override bool AddParameter(string sParName , string sType , object sParVal)
		{
			if (sInsertStructure != "(")
				sInsertStructure += ",";
			sInsertStructure += sParName;

			if (sInsertValues != "(")
				sInsertValues += ",";
            sInsertValues += "@P" + table_ind.ToString();
			m_hashTable[table_ind] = sParVal;
			table_ind++;
			return true;
		}

		protected override bool Execute(string oraStr)
		{
			m_sOraStr = new System.Text.StringBuilder(oraStr);
			m_sOraStr.Append(sInsertStructure);
			m_sOraStr.Append(") ");
			m_sOraStr.Append("VALUES "); 
			m_sOraStr.Append(sInsertValues);
			m_sOraStr.Append(")");
			oraStr = m_sOraStr.ToString();
			int_Execute();
            string sConn = ODBCWrapper.Connection.GetConnectionString(m_sConnectionKey, m_bIsWritable);
            if (sConn == "")
                return false;
            using (SqlConnection con = new SqlConnection(sConn))
            {
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
                    string sMes = "While running : '" + m_sLastExecutedOraStr + "'\r\n Exception accured: " + ex.Message;
                    Logger.Logger.Log(this.GetType().ToString(), sMes, "ODBC_Net", ex.Message);
                    return false;
                }
            }
			return true;
		}

		public static InsertQuery operator +(InsertQuery p, object sOraStr)
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

		protected string sInsertStructure = "(";
		protected string sInsertValues = "(";
	}
}
