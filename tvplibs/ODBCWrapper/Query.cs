using System;
using System.Data.Odbc;
using System.Collections;
using KLogMonitor;
using System.Reflection;
using System.Data.SqlClient;

namespace ODBCWrapper
{
	/// <summary>
	/// Summary description for Query.
	/// </summary>
	public abstract class Query
	{
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

		private int m_nTimeout;
        protected string m_sErrorMsg;
        private Int32 m_nTop;
		static public Int32 GetSequence(string sSeqName)
		{
			Int32 nRet = -1;
			ODBCWrapper.DataSetSelectQuery selectQuery = 
				new ODBCWrapper.DataSetSelectQuery();
			selectQuery += "select ";
			selectQuery += sSeqName;
			selectQuery += ".nextval from dual";
			selectQuery.Execute("seq" , true);
			if (selectQuery.Table("seq").DefaultView.Count > 0)
			{
				nRet = int.Parse(selectQuery.Table("seq").DefaultView[0].Row[0].ToString());
			}
			selectQuery.Finish();
			selectQuery = null;
			return nRet;
		}

        public string GetErrorMsg()
        {
            return m_sErrorMsg;
        }

		protected Query()
		{
			//ODBCWrapper.Connection.AddUser();
            m_sErrorMsg = "";
			m_conn = new Connection();
			m_hashTable = new object[255];
			m_nTimeout = 0;
            m_nTop = 0;
		}

        protected string CustomConnectionString
        {
            set
            {
                m_conn.CustomConnectionString = value;
            }
        }

        public void SetTop(Int32 nTop)
        {
            m_nTop = nTop;
        }

		protected Query(ref Connection conn)
		{
			isOwnConnection = false;
		}

		//public static implicit operator Connection(Query m) 
		//{
			//return m.m_conn;
		//}

		public virtual void Finish()
		{
			//lock(m_crit_sec)
			//{
				m_hashTable = null;
				m_conn.Finish();
				if (command != null)
					command = null;
			//}
		}

        protected string GetCachStr()
        {
            string sCachStr = "";
            string sOraStr = m_sOraStr;
            string sToReplace = "select top " + m_nTop.ToString() + " ";
            string sDistinctToReplace = "selecttmp distinct top " + m_nTop.ToString() + " ";
            if (m_nTop != 0)
            {
                sOraStr = sOraStr.ToLower().Replace("select distinct", sDistinctToReplace);
                sOraStr = sOraStr.ToLower().Replace("select ", sToReplace);
                sOraStr = sOraStr.ToLower().Replace("selecttmp ", "select ");

            }
            sCachStr += sOraStr + "(";
            if (table_ind > 0)
            {
                for (int i = 0; i < table_ind; i++)
                {
                    if (i > 0)
                        sCachStr += ",";
                    sCachStr += m_hashTable[i].ToString();
                }
            }
            sCachStr += ")";
            return sCachStr;
        }
		
		protected bool int_Execute()
		{
			command = null;
            string sToReplace = "select top " + m_nTop.ToString() + " ";
            string sDistinctToReplace = "selecttmp distinct top " + m_nTop.ToString() + " ";
            if (m_nTop != 0)
            {
                m_sOraStr = m_sOraStr.ToLower().Replace("select distinct", sDistinctToReplace);
                m_sOraStr = m_sOraStr.ToLower().Replace("select ", sToReplace);
                m_sOraStr = m_sOraStr.ToLower().Replace("selecttmp ", "select ");
                
            }
			command = new SqlCommand(m_sOraStr);
            command.CommandType = m_CommandType;

			if (m_nTimeout != 0)
				command.CommandTimeout = m_nTimeout;
			if (table_ind > 0)
			{
				for (int i=0; i < table_ind; i++)
				{
					SqlParameter par = new SqlParameter("@P" + i.ToString() , m_hashTable[i] );
					command.Parameters.Add(par);
					m_hashTable[i] = null;
				}
				table_ind = 0;
			}
            m_conn.GetConnection(ref command);
			m_sLastExecutedOraStr = m_sOraStr;
			Clean();
			return true;
		}

		public void SetTimeout(Int32 nTimeout)
		{
			m_nTimeout = nTimeout;
		}

		public virtual bool Execute()
		{
			return int_Execute();
		}

		protected virtual bool AddParameter(string sParName , string sType , object sParVal)
		{
            if (m_CommandType == System.Data.CommandType.Text)
            {
                m_sOraStr += " " + sParName;
                m_sOraStr += sType;
                m_sOraStr += "@P" + table_ind;
            }

			m_hashTable[table_ind] = sParVal;
			table_ind++;
			return true;
		}

		public static Query operator +(Query p, object sOraStr)
		{
			if (sOraStr.GetType() == System.Type.GetType("ODBCWrapper.Parameter"))
			{
				p.AddParameter(((Parameter)sOraStr).m_sParName , 
					((Parameter)sOraStr).m_sType , 
					((Parameter)sOraStr).m_sParVal);
				sOraStr = null;
			}
			else
				p.m_sOraStr += " " +sOraStr;
			return p;
		}

		protected virtual void Clean()
		{
			m_sOraStr = "";
		}

		~Query()
		{
			m_crit_sec = null;
			m_hashTable = null;
		}

		//protected ODBCWrapper.Connection m_conn = null;
		protected bool isOwnConnection = true;

		protected SqlCommand command;
		protected ODBCWrapper.Connection m_conn;
		protected object m_crit_sec = new object();
		protected string m_sOraStr = "";
		protected string m_sLastExecutedOraStr = "";
		protected object[] m_hashTable = null;
		protected Int32 table_ind = 0;

        public System.Data.CommandType m_CommandType = System.Data.CommandType.Text;
        public System.Data.CommandType CommandType 
        {
            set
            {
                m_CommandType = value;
            }
        }
	}
}
