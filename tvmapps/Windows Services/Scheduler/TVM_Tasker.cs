using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using KLogMonitor;

namespace Scheduler
{
    partial class TVM_Tasker : ServiceBase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TVM_Tasker()
        {
            InitTcmConfig();
            InitializeComponent();
        }

        private void InitTcmConfig()
        {
            try
            {
                TCMClient.Settings.Instance.Init();
            }
            catch (Exception ex)
            {
                log.Error("Scheduler.TVM_Tasker - InitTcmConfig", ex);
            }
        }


        private void InitializeDLLs()
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("scheduled_tasks_b");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", 0);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_DEPRICATED", "=", 0);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;

        }

        protected override void OnStart(string[] args)
        {
            log.Debug("message OnStart - TVM_Tasker");
            InitializeDLLs();
            timer1 = new System.Timers.Timer();
            timer1.Interval = 60000;
            timer1.Enabled = true;
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Start();
        }

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Runner t = new Runner();
            ThreadStart job = new ThreadStart(t.DoTheJob);
            Thread thread = new Thread(job);
            thread.Start();
        }

        protected override void OnStop()
        {
            log.Debug("message - OnStop, TVM_Tasker");
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
