using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Phx.Lib.Log;
using System.Reflection;

namespace Uploader
{
    public abstract class BaseUploader
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected string m_sBasePath;

        protected string m_sAddress;
        protected string m_sUserName;
        protected string m_sPass;
        protected string m_sPrefix;

        public static Int32 m_nNumberOfRuningUploads = 0;
        public static string m_currentGroupDirUpload = string.Empty;
        public static List<string> m_currentlyUploadedGroups = new List<string>();

        protected int m_nGroupID = 0;

        protected static ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();

        public static void SetRunningProcesses(Int32 n)
        {
            m_nNumberOfRuningUploads = n;
        }

        public abstract bool Upload(string fileToUpload, bool deleteFileAfterUpload);

        public abstract void UploadDirectory(string directoryToUpload);

        protected abstract void ProccessJob(UploadJob job, ref int nFailCount);

        private bool IsGroupUploading()
        {
            bool retVal = false;
            try
            {
                if (m_locker.TryEnterReadLock(200))
                {
                    if (m_currentlyUploadedGroups.Contains(m_sAddress))
                    {
                        retVal = true;
                    }
                }
            }
            finally
            {
                m_locker.ExitReadLock();
            }
            return retVal;
        }

        protected void AddUploadGroup()
        {
            try
            {
                if (m_locker.TryEnterWriteLock(200))
                {
                    if (m_currentlyUploadedGroups == null)
                    {
                        m_currentlyUploadedGroups = new List<string>();
                    }
                    if (!m_currentlyUploadedGroups.Contains(m_sAddress))
                    {
                        m_currentlyUploadedGroups.Add(m_sAddress);
                    }
                }
            }
            finally
            {
                m_locker.ExitWriteLock();
            }

        }

        protected void RemoveUploadGroup()
        {
            try
            {
                if (m_locker.TryEnterWriteLock(200))
                {
                    if (m_currentlyUploadedGroups != null && m_currentlyUploadedGroups.Contains(m_sAddress))
                    {
                        m_currentlyUploadedGroups.Remove(m_sAddress);
                    }
                }
            }
            finally
            {
                m_locker.ExitWriteLock();
            }
        }

        protected void ProcessScope(List<UploadJob> jobs, int startIndex, int endIndex, ref int nFailCount)
        {
            for (int mediaRowIndex = startIndex; mediaRowIndex <= endIndex; mediaRowIndex++)
            {
                ProccessJob(jobs[mediaRowIndex], ref nFailCount);
            }
        }

        public void UploadQueue(string sBasePath, int nMaxThreads, int nMaxRows)
        {
            if (m_sAddress.Trim() == "")
                return;

            if (!Directory.Exists(sBasePath))
            {
                log.Debug("Directory does not exist: " + sBasePath + " Directory does not exist: " + sBasePath);
                return;
            }

            m_sBasePath = sBasePath;

            int nID = 0;
            int nTotalJobs = 0;
            int nFailJobs = 0;

            log.Debug("Start Queue - group : " + m_nGroupID + ", " + sBasePath + ", max connections : " + nMaxThreads + ", rows : " + nMaxRows);

            List<UploadJob> pendingJobs = UploadHelper.GetGroupPendingJobs(m_nGroupID, nID, nMaxRows);

            while (pendingJobs != null && pendingJobs.Count > 0)
            {
                nTotalJobs += pendingJobs.Count;
                nID = pendingJobs[pendingJobs.Count - 1].id;

                if (nMaxThreads <= 1 || pendingJobs.Count < nMaxThreads)
                {
                    foreach (UploadJob job in pendingJobs)
                    {
                        ProccessJob(job, ref nFailJobs);
                    }
                }
                else
                {
                    int nNumOfCycles = pendingJobs.Count / nMaxThreads;

                    if (pendingJobs.Count % nMaxThreads > 0)
                        nNumOfCycles++;

                    for (int i = 0; i < nNumOfCycles; i++)
                    {
                        List<UploadJob> jobsInCycle = pendingJobs.Skip(i * nMaxThreads).Take(nMaxThreads).ToList();

                        int nNumOfJobsInCycle = Math.Min(jobsInCycle.Count, nMaxThreads);

                        ManualResetEvent[] manualResetEvents = new ManualResetEvent[nNumOfJobsInCycle];

                        for (int j = 0; j < nNumOfJobsInCycle; j++)
                        {
                            int index = j;

                            manualResetEvents[index] = new ManualResetEvent(false);

                            ThreadPool.QueueUserWorkItem(
                                arg =>
                                {
                                    ProccessJob(jobsInCycle[index], ref nFailJobs);
                                    manualResetEvents[index].Set();
                                });
                        }

                        WaitHandle.WaitAll(manualResetEvents);
                    }

                    RemoveUploadGroup();
                    Thread.Sleep(1000);
                }

                pendingJobs = UploadHelper.GetGroupPendingJobs(m_nGroupID, nID, nMaxRows);
            }

            log.Debug("Finish Queue - " + string.Format("group : {0}, jobs : {1}, fail : {2}", m_nGroupID.ToString(), nTotalJobs, nFailJobs));
        }
    }
}
