using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.ServiceProcess;


namespace Scheduler
{
    partial class TVM_Tasker : ServiceBase
    {
        public TVM_Tasker()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1 = new System.Timers.Timer();
            timer1.Interval = 20000;
            timer1.Enabled = true;
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Start();
        }

        protected void UpdateServiceStatus(Int32 nID, Int32 nRequestStatus, Int32 nStatus)
        {
            Logger.Logger.Log("Stop", "Updateing DB", "SERVICE_HANDLER");
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("windows_services");
            if (nStatus >= 0)
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("STATUS" , "=" , nStatus);
            if (nRequestStatus >= 0)
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("REQUEST_TYPE", "=", nRequestStatus);
            updateQuery += "where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        static protected ServiceController GetServiceByName(string sName , ref Int32 nServiceStatus)
        {
            nServiceStatus = -1;
            ServiceController[] services = ServiceController.GetServices();
            {
                foreach (ServiceController service in services)
                {
                    if (service.DisplayName.ToUpper().Trim() == sName.ToUpper().Trim())
                    {
                        try
                        {
                            if (service.Status == ServiceControllerStatus.Running)
                                nServiceStatus = 1;
                            if (service.Status == ServiceControllerStatus.ContinuePending)
                                nServiceStatus = 2;
                            if (service.Status == ServiceControllerStatus.Paused)
                                nServiceStatus = 3;
                            if (service.Status == ServiceControllerStatus.PausePending)
                                nServiceStatus = 4;
                            if (service.Status == ServiceControllerStatus.StartPending)
                                nServiceStatus = 5;
                            if (service.Status == ServiceControllerStatus.Stopped)
                                nServiceStatus = 6;
                            if (service.Status == ServiceControllerStatus.StopPending)
                                nServiceStatus = 7;
                            return service;
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Exception", ex.Message + "\r\n" + ex.StackTrace, "SERVICE_HANDLER");
                        }
                    }
                }
            }
            return null;
        }

        void  timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select * from windows_services ";
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                for (int i = 0; i < nCount; i++)
                {
                    string sServiceName = selectQuery.Table("query").DefaultView[i].Row["SERVICE_NAME"].ToString();
                    Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    Int32 nServiceRequestID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["REQUEST_TYPE"].ToString());
                    Int32 nStatus = -1;
                    ServiceController service = GetServiceByName(sServiceName , ref nStatus);
                    if (service == null)
                        continue;
                    if (nServiceRequestID == 0)
                    {
                    }
                    //Stop
                    if (nServiceRequestID == 1)
                    {
                        Logger.Logger.Log("Stop", "Service: " + sServiceName + " - is being stoped", "SERVICE_HANDLER");
                        service.Stop();
                    }
                    //Start
                    if (nServiceRequestID == 2)
                    {
                        Logger.Logger.Log("Stop", "Service: " + sServiceName + " - is being started", "SERVICE_HANDLER");
                        service.Start();
                    }
                    UpdateServiceStatus(nID, 0, nStatus);    
                }
                
            }
            selectQuery.Finish();
            selectQuery = null;
        }

        protected override void OnStop()
        {
            Logger.Logger.Log("message", "OnStop", "TVM_Tasker");
        }

        protected override void OnPause()
        {
            base.OnPause();
            timer1.Stop();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
            timer1.Start();
        }

        protected System.Timers.Timer timer1 = null;
    }
}
