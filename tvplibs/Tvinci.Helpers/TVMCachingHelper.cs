using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ODBCWrapper;
using System.Web;
using System.Threading;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using System.Data;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Helpers
{
    public class TVMCachingHelper
    {

        private TVMCachingHelper()
        {
            // Register to timer
            m_SyncTimer = new System.Timers.Timer(60000);
            m_NoSignalTimer = new System.Timers.Timer(900000);
            m_SyncTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_SyncTimer_Elapsed);
            m_NoSignalTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_NoSignalTimer_Elapsed);
            m_NoSignalTimer.AutoReset = false;
        }

        public const string TVMONLINE_OFFLINE_STATUS = "OFFLINE";
        public const string TVMOSTATUS_OFFLINE_STATUS = "OFFLINE";

        private static TVMCachingHelper m_instance = new TVMCachingHelper();
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string m_LastTVMOnline;
        private string m_LastTVMStatus;
        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
        private System.Timers.Timer m_SyncTimer;
        private System.Timers.Timer m_NoSignalTimer;


        public static TVMCachingHelper Instance
        {
            get
            {
                return m_instance;
            }
            set
            {

                m_instance = value;
            }
        }

        public string TVMOnline
        {
            get
            {
                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        return m_LastTVMOnline;
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return null;
            }
            set
            {
                // Stop no signal timer
                m_NoSignalTimer.Stop();

                UpdateTVMOnline(value);

                // Start no signal timer
                m_NoSignalTimer.Start();
            }
        }

        public string TVMStatus
        {
            get
            {
                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        return m_LastTVMStatus;
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return null;
            }
        }

        private void sync()
        {
            logger.Debug("Started sync process.");

            // Get values from database
            string tOnline = "";
            string tStatus = "";

            try
            {
                DatabaseDirectAdapter<DataTable> q = new DatabaseDirectAdapter<DataTable>(delegate(ODBCWrapper.DataSetSelectQuery query)
                {
                    query += "select * from TVMStatus";
                });

                DataRow row;

                row = q.ExtractRow(true);
                bool success = true;

                // Extract TVMOnline
                if (row.IsNull("TVMOnline"))
                {
                    tOnline = TVMONLINE_OFFLINE_STATUS;
                    success = false;
                    logger.Error("Failed to extract TVMOnline from database");
                }
                else
                {
                    tOnline = row["TVMOnline"].ToString();
                }

                // Extract TVMStatus
                if (row.IsNull("TVMStatus"))
                {
                    tStatus = TVMOSTATUS_OFFLINE_STATUS;
                    success = false;
                    logger.Error("Failed to extract TVMStatus from database");
                }
                else
                {
                    tStatus = row["TVMStatus"].ToString();
                }

                // Extract last database update time
                if (!row.IsNull("UPDATE_DATE") && row["UPDATE_DATE"] is DateTime)
                {
                    DateTime lastUpdate = (DateTime)row["UPDATE_DATE"];

                    // If no update in last 15 minutes - mark TVM as offline
                    if ((DateTime.Now - lastUpdate).Minutes >= 15)
                    {
                        tOnline = TVMONLINE_OFFLINE_STATUS;
                        tStatus = TVMOSTATUS_OFFLINE_STATUS;
                        logger.Warn("TVM status was not updated in database for more than 15 minutes.");
                    }
                }
                else
                {
                    logger.Error("Failed to extract UPDATE_DATE from database");
                    success = false;
                }

                if (success)
                {
                    logger.Debug(string.Format("Successfully extracted values from database, Online:{0}, Status:{1}.", tOnline, tStatus));
                }
            }
            catch (Exception ex)
            {
                logger.Error("TVMCachingHelper failed to extract values from database.", ex);
                tOnline = TVMONLINE_OFFLINE_STATUS;
                tStatus = TVMOSTATUS_OFFLINE_STATUS;
            }

            // Set values
            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    m_LastTVMOnline = tOnline;
                    m_LastTVMStatus = tStatus;

                    logger.Debug("Sync finished successfully.");
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }
        }

        private void UpdateTVMOnline(string status)
        {
            logger.Info(string.Format("Updating TVMOnline in database to value: {0}", status));

            // Update database
            UpdateQuery uQuery = new UpdateQuery("TVMStatus");
            try
            {
                uQuery += ODBCWrapper.Parameter.NEW_PARAM("TVMOnline", status);
                uQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", DateTime.Now);

                uQuery.Execute();

                sync();
            }
            catch (Exception e)
            {
                logger.Error(string.Format("Failed updating TVMOnline in database to value: {0}", status), e);
            }
            finally
            {
                uQuery.Finish();
                uQuery = null;
            }
        }

        #region Public Methods
        public void Start()
        {
            logger.Info("Starting automatic sync of tvm status from database.");

            try
            {
                m_SyncTimer.Start();

                // Force sync
                sync();

            }
            catch (Exception ex)
            {
                logger.Error("Failed to start automatic sync of tvm status from database", ex);
            }
        }

        public void Stop()
        {
            logger.Info("Stopping automatic sync of tvm status from database.");

            try
            {
                m_SyncTimer.Stop();
                m_NoSignalTimer.Stop();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to stop automatic sync of tvm status from database", ex);
            }
        }
        #endregion

        #region Event Handlers
        void m_SyncTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            sync();
        }

        void m_NoSignalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // The no signal timer has elapsed
            logger.Info("No signal from TVM monitor for 15 minutes - setting status to offline");

            UpdateTVMOnline(TVMONLINE_OFFLINE_STATUS);
        }
        #endregion
    }
}
