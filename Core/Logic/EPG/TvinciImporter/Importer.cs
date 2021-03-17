using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ScheduledTasks;
using Uploader;

namespace TvinciImporter
{
    public class Importer : BaseTask
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static object m_locker = new object();
        public Importer(int nTaskID, int nIntervalInSec, string sParameters)
            : base(nTaskID, nIntervalInSec, sParameters)
        {
        }

        public static BaseTask GetInstance(int nTaskID, int nIntervalInSec, string sParameters)
        {
            return new Importer(nTaskID, nIntervalInSec, sParameters);
        }

        static protected int GetSyncStatus(int nGroupID)
        {
            int nStatus = 0;
            bool bCreate = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select SYNC_STATUS from importer_status where ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int nCount = selectQuery.Table("query").DefaultView.Count;
                if (nCount > 0)
                    nStatus = int.Parse(selectQuery.Table("query").DefaultView[0].Row["SYNC_STATUS"].ToString());
                else
                    bCreate = true;
            }
            selectQuery.Finish();
            selectQuery = null;

            if (bCreate == false)
            {
                ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("importer_status");
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("SYNC_STATUS", "=", 0);
                insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
                insertQuery.Execute();
                insertQuery.Finish();
                insertQuery = null;
            }
            return nStatus;
        }

        static protected void SetSyncStatus(int nGroupID, int nStatus)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("importer_status");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("SYNC_STATUS", "=", 0);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATE_DATE", "=", DateTime.UtcNow);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        protected override bool DoTheTaskInner()
        {
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select * from importer_alerts where RUN_STATUS=0";
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nGroupID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["GROUP_ID"].ToString());
                        int nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        int nSyncStatus = 0;
                        lock (m_locker)
                        {
                            nSyncStatus = GetSyncStatus(nGroupID);
                            if (nSyncStatus == 0)
                            {
                                SetSyncStatus(nGroupID, 1);
                            }
                        }
                        if (nSyncStatus == 0)
                        {
                            string sXMLUrl = selectQuery.Table("query").DefaultView[i].Row["IMPORT_URL"].ToString();
                            string sNotifyURL = selectQuery.Table("query").DefaultView[i].Row["NOTIFY_URL"].ToString();
                            bool bOK = ImporterImpl.DoTheWork(nGroupID, sXMLUrl, sNotifyURL, nID);
                            int nCounter = 1;
                            while (BaseUploader.m_nNumberOfRuningUploads != 0)
                            {
                                System.Threading.Thread.Sleep(1000);
                                log.Debug("message - IMPORTER Sync process finished, but uploads are still in progress - waiting message number: " + nCounter.ToString());
                                nCounter++;
                            }
                            lock (m_locker)
                            {
                                SetSyncStatus(nGroupID, 0);
                                UpdateSyncStatus(nID, 1);
                            }
                            log.Debug("message - IMPORTER process finished successfully");
                            return true;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                log.Error("Exception - On function: DoTheTaskInner", ex);
                int nCounter = 1;
                while (BaseUploader.m_nNumberOfRuningUploads != 0)
                {
                    System.Threading.Thread.Sleep(1000);
                    log.Error("message - IMPORTER process finished with exceptions, but uploads are still in progress - waiting message number: " + nCounter.ToString(), ex);
                    nCounter++;
                }
                return false;
            }
            return false;
        }

        protected void UpdateSyncStatus(int nImportID, int nStatus)
        {
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("importer_alerts");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("RUN_STATUS", "=", nStatus);
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("update_date", "=", DateTime.UtcNow);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nImportID);
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }
    }
}
