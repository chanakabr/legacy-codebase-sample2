using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using KLogMonitor;

namespace Scheduler
{
    class Runner
    {
        static public object o = new object();
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public Runner()
        {
            log.Debug("Scheduled Runner started to run");
        }

        protected void RunDll(string sDllName, Int32 nID, Int32 nIntervalInSec, string sParameters)
        {
            try
            {
                string sBaseLoc = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                Assembly a = Assembly.LoadFrom(sBaseLoc + "/" + sDllName);
                Type[] ts = a.GetTypes();
                foreach (Type t in ts)
                {
                    try
                    {
                        string sType = t.ToString();
                        object[] arguments = new object[3];
                        arguments[0] = nID;
                        arguments[1] = nIntervalInSec;
                        arguments[2] = sParameters;
                        if (t.GetMethod("GetInstance") != null)
                        {
                            object theTask = t.InvokeMember("GetInstance", BindingFlags.Default | BindingFlags.InvokeMethod, null, "ScheduledTasks.BaseTask", arguments);
                            if (theTask != null)
                                ((ScheduledTasks.BaseTask)(theTask)).DoTheTask();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Exception on {0}, exception: {1}", sDllName, ex);
                        UpdateTaskStatus(nID, 0, nIntervalInSec);
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Exception on {0}, exception: {1}", sDllName, e);
                UpdateTaskStatus(nID, 0, nIntervalInSec);
            }
        }

        public void DoTheJob()
        {
            using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_API_START, null, "Scheduler", null, null))
            {
                DoTheJobOnes();
            }
        }

        virtual protected void UpdateTaskStatus(Int32 nTaskID, Int32 nStatus)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", nStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        virtual protected void UpdateTaskStatus(Int32 nTaskID, Int32 nStatus, Int32 nInterval)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", nStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.Now);
            if (nInterval > 0)
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_DATE", "=", DateTime.UtcNow.AddMinutes(5));
            updateQuery += "where";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nTaskID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        virtual public void DoTheJobOnes()
        {
            Int32 nID = 0;
            string sDLL = "";
            Int32 nInterval = 0;
            try
            {
                //string sServer = ConfigurationManager.AppSettings["SERVER"].ToString();
                string sServer = string.Empty;
                try
                {
                    sServer = TCMClient.Settings.Instance.GetValue<string>("SERVER");
                }
                catch (Exception ex)
                {
                    sServer = string.Empty;
                    log.Error("Scheduler Runner - key=SERVER", ex);
                }

                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from scheduled_tasks where RUN_STATUS=0 and RUN_DATE<getdate() and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("server", "=", sServer);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        sDLL = selectQuery.Table("query").DefaultView[i].Row["DLL"].ToString();
                        nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        nInterval = int.Parse(selectQuery.Table("query").DefaultView[i].Row["INTERVAL"].ToString());
                        string sParameters = "";
                        object oParams = selectQuery.Table("query").DefaultView[i].Row["PARAMETERS"];
                        if (oParams == null || oParams == DBNull.Value)
                            sParameters = "";
                        else
                            sParameters = oParams.ToString();
                        lock (o)
                        {
                            UpdateTaskStatus(nID, 1);
                        }
                        RunDll(sDLL, nID, nInterval, sParameters);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("message - exception DoTheJobOnes TVM_Tasker. Exception on {0}, exception: {1}", sDLL, ex);
                UpdateTaskStatus(nID, 0, nInterval);
            }
        }
    }
}
