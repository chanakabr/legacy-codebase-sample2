using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScheduledTasks;
using System.Data;
using DAL;
using KLogMonitor;
using System.Reflection;
using Core.Notification;

namespace NotificationFeeder
{
    /// <summary>
    /// Feeder for notifications requests,
    /// Get 2 parameters: 1)number of requests 2)group id.
    /// In each iteration get specific number requests from db (according to number of requests param) filtered
    /// by group id and process these requests by calling NotificationManager.HanldeRequests method.
    /// </summary>
    public class Feeder : BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private int m_NumOfRequests = 0;
        private long m_GroupID = 0;


        #region Constructor
        public Feeder(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
            InitParams(sParameters);
        }
        #endregion

        #region private Methods
        private void InitParams(string sParameters)
        {
            log.Debug("InitParams Begin - sParameters=" + sParameters + " NotificationFeeder");

            string[] arrParams = sParameters.Split(new char[] { '|' });
            m_NumOfRequests = int.Parse(arrParams[0]);
            m_GroupID = long.Parse(arrParams[1]);

            log.Debug("InitParams Finish - Num Of Requests=" + m_NumOfRequests.ToString() + "GroupID=" + m_GroupID.ToString() + " NotificationFeeder");
        }

        private void ProcessNotificationRequests()
        {
            log.Debug("ProcessNotificationRequests - Num Of Requests=" + m_NumOfRequests.ToString() + ",GroupID=" + m_GroupID.ToString() + " NotificationFeeder");
            NotificationManager.Instance.HandleRequests(m_NumOfRequests, m_GroupID);
        }

        #endregion

        #region protected Methods
        protected override bool DoTheTaskInner()
        {
            bool result = true;

            try
            {
                ProcessNotificationRequests();
            }
            catch
            {
                //TBD: Write to log
                result = false;
            }
            return result;
        }
        #endregion

        #region public Methods
        public static ScheduledTasks.BaseTask GetInstance(Int32 nTaskID, Int32 nIntervalInSec, string sParameters)
        {
            return new Feeder(nTaskID, nIntervalInSec, sParameters);
        }
        #endregion

    }
}
