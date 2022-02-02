using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Phx.Lib.Log;

namespace ScheduledTasks
{
    abstract public class BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected Int32 m_nIntervalInSec;
        protected Int32 m_nTaskID;
        protected string m_sParameters;
        public BaseTask(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            m_nIntervalInSec = nIntervalInSec;
            m_nTaskID = nTaskID;
            m_sParameters = sParameters;
        }

        static protected DateTime GetDBDate()
        {
            object t = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select getdate() as t";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    t = selectQuery.Table("query").DefaultView[0].Row["t"];
            }
            selectQuery.Finish();
            selectQuery = null;
            if (t != null && t != DBNull.Value)
                return (DateTime)t;
            else
                return DateTime.UtcNow;
        }

        protected void CreateNewTaskLine()
        {
            DateTime nTaskOrigRunDate = DateTime.UtcNow;
            string sDLL = "";
            Int32 nInterval = 604800;
            object sPARAMETRS = "";
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from scheduled_tasks where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                {
                    nTaskOrigRunDate = (DateTime)(selectQuery.Table("query").DefaultView[0].Row["RUN_DATE"]);
                    sDLL = selectQuery.Table("query").DefaultView[0].Row["DLL"].ToString();
                    sPARAMETRS = selectQuery.Table("query").DefaultView[0].Row["PARAMETERS"];
                    nInterval = int.Parse(selectQuery.Table("query").DefaultView[0].Row["INTERVAL"].ToString());
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            //DateTime nTaskOrigRunDate = (DateTime)(ODBCWrapper.Utils.GetTableSingleVal("scheduled_tasks", "RUN_DATE", m_nTaskID));

            if (m_nIntervalInSec == 11111)
            {
                nTaskOrigRunDate = nTaskOrigRunDate.AddMonths(1);
            }
            else
            {
                nTaskOrigRunDate = nTaskOrigRunDate.AddSeconds(m_nIntervalInSec);
            }
            DateTime tNow = GetDBDate();
            if (nTaskOrigRunDate < tNow)
                nTaskOrigRunDate = tNow;


            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_DATE", "=", nTaskOrigRunDate);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", m_nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            /*
            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("scheduled_tasks");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DLL", "=", sDLL);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", 0);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PARAMETERS", "=", sPARAMETRS);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("INTERVAL", "=", nInterval);
            
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_DATE", "=", nTaskOrigRunDate);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;
            */
        }

        virtual public bool DoTheTask()
        {
            log.Debug("message - " + GetType().ToString() + " started");

            bool b = DoTheTaskInner();
            CreateNewTaskLine();
            log.Debug("message - " + GetType().ToString() + " finished");
            return b;
        }

        abstract protected bool DoTheTaskInner();
    }
}
