using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using Phx.Lib.Log;
using System.Reflection;


namespace Tvinci.Data.Loaders
{
    public class FailOverManager
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private int m_nMaxTotalRequests;
        private int m_nMinTotalrequests;
        private int m_nThresholdPercent; //(percent) 
        private int m_nSafeModeDuration; //(milliseconds)

        private int m_nTotalRequests;
        private int m_nFailedRequests;

        private bool m_bSafeMode;
        public event Action SafeModeStarted;
        public event Action SafeModeEnded;
        private System.Timers.Timer m_tSafeModeTimer;

        private ReaderWriterLockSlim m_lock;

        private static FailOverManager m_Instance;
        private static object instanceLock = new object();

        public static FailOverManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (instanceLock)
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = new FailOverManager();
                        }
                    }
                }
                return m_Instance;
            }
        }

        private FailOverManager()
        {
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["FailOverManager.MaxTotalRequests"], out m_nMaxTotalRequests);
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["FailOverManager.MinTotalRequests"], out m_nMinTotalrequests);
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["FailOverManager.ThresholdPercent"], out m_nThresholdPercent);
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["FailOverManager.SafeModeDuration"], out m_nSafeModeDuration);
            m_nTotalRequests = 0;
            m_nFailedRequests = 0;
            m_bSafeMode = false;
            if (m_nSafeModeDuration != 0)
            {
                m_tSafeModeTimer = new System.Timers.Timer(m_nSafeModeDuration);
                m_tSafeModeTimer.Elapsed += new ElapsedEventHandler(ExitSafeMode);
                m_tSafeModeTimer.Stop();
            }
            m_lock = new ReaderWriterLockSlim();
        }

        public int TotalRequests
        {
            get { return m_nTotalRequests; }
        }

        public int FailedRequests
        {
            get { return m_nFailedRequests; }
        }

        public bool SafeMode
        {
            get
            {
                m_lock.EnterReadLock();
                try
                {
                    return m_bSafeMode;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        public void AddRequest(bool success)
        {
            if (m_tSafeModeTimer != null)
            {
                m_nTotalRequests++;
                if (!success)
                    m_nFailedRequests++;
                if (m_nTotalRequests >= m_nMaxTotalRequests)
                    Restart();
                float failPercentage = 0;
                if (m_nTotalRequests != 0)
                    failPercentage = (float)m_nFailedRequests / (float)m_nTotalRequests * 100;
                if (m_nTotalRequests > m_nMinTotalrequests && failPercentage > m_nThresholdPercent)
                {
                    m_lock.EnterWriteLock();
                    try
                    {
                        logger.Debug("Entering Safe Mode");
                        m_bSafeMode = true;
                        m_tSafeModeTimer.Start();
                        if (SafeModeStarted != null)
                            SafeModeStarted();
                    }
                    finally
                    {
                        m_lock.ExitWriteLock();
                    }
                }
            }
        }

        private void ExitSafeMode(object sender, ElapsedEventArgs e)
        {
            logger.Debug("Stoping Safe Mode");
            m_tSafeModeTimer.Stop();

            if (SafeModeEnded != null)
            {
                SafeModeEnded();
            }
            Restart();
        }

        private void Restart()
        {
            m_bSafeMode = false;
            m_nTotalRequests = 0;
            m_nFailedRequests = 0;
        }
    }
}
