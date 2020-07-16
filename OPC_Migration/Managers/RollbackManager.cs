using Core.Catalog;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace OPC_Migration
{
    public class RollbackManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private long sequenceId;
        private int groupId;

        public RollbackManager(int groupId, long sequenceId)
        {
            this.groupId = groupId;
            this.sequenceId = sequenceId;
        }

        public eMigrationResultStatus PerformRollback()
        {
            eMigrationResultStatus result = eMigrationResultStatus.OK;
            if (!RollbackOfNewOpcTables())
            {
                log.Error("RollBackOfNewOpcTables failed");
                Console.WriteLine("RollBackOfNewOpcTables failed");
                return eMigrationResultStatus.FailedRollBackOfNewOpcTables;
            }

            log.Debug("RollBackOfNewOpcTables succeeded");
            Console.WriteLine("RollBackOfNewOpcTables succeeded");
            Dictionary<string, List<string>> rollbackTablesInfo = GetRollbackTablesInfo();
            if (rollbackTablesInfo == null || rollbackTablesInfo.Count == 0)
            {
                log.Error("GetRollbackTablesInfo failed");
                Console.WriteLine("GetRollbackTablesInfo failed");
                return eMigrationResultStatus.FailedGettingRollbackTablesInfo;
            }

            log.Debug("GetRollbackTablesInfo succeeded");
            Console.WriteLine("GetRollbackTablesInfo succeeded");
            if (!RollbackOfAllTables(rollbackTablesInfo))
            {
                log.Error("RollbackOfAllTables failed");
                Console.WriteLine("RollbackOfAllTables failed");
                return eMigrationResultStatus.FailedRollbackOfAllTables;
            }

            log.Debug("RollbackOfAllTables succeeded");
            Console.WriteLine("RollbackOfAllTables succeeded");
            if (!Utils.ClearAllCaches(groupId))
            {
                log.Error("ClearAllCaches failed");
                Console.WriteLine("ClearAllCaches failed");
                return eMigrationResultStatus.ClearAllCachesFailed;
            }

            log.Debug("ClearAllCaches succeeded");
            Console.WriteLine("ClearAllCaches succeeded");
            return result;
        }

        private Dictionary<string, List<string>> GetRollbackTablesInfo()
        {
            Dictionary<string, List<string>> rollbackTablesInfo = new Dictionary<string, List<string>>();
            try
            {
                DataTable dt = OPCMigrationDAL.GetRollbackTables(sequenceId);
                if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                {
                    log.ErrorFormat("didn't get any tables from DB on GetRollbackTables proceudre, sequenceID: {0}", sequenceId);
                    return rollbackTablesInfo;
                }

                foreach (DataRow dr in dt.Rows)
                {
                    string tableName = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                    string columnName = ODBCWrapper.Utils.GetSafeStr(dr, "column_name");
                    if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(columnName))
                    {
                        if (!rollbackTablesInfo.ContainsKey(tableName))
                        {
                            rollbackTablesInfo.Add(tableName, new List<string>());
                        }

                        rollbackTablesInfo[tableName].Add(columnName);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("failed GetRollbackTablesInfo for sequenceID: {0}", sequenceId), ex);
                return null;
            }

            return rollbackTablesInfo;
        }

        private bool RollbackOfAllTables(Dictionary<string, List<string>> rollbackTablesInfo)
        {
            bool res = true;
            try
            {
                if (rollbackTablesInfo == null || rollbackTablesInfo.Count == 0)
                {
                    return false;
                }

                ConcurrentDictionary<string, List<string>> failedRollbacks = new ConcurrentDictionary<string, List<string>>(); 
                Parallel.ForEach(rollbackTablesInfo, (rollbackTable) =>
                {
                    if (rollbackTable.Value == null || rollbackTable.Value.Count == 0)
                    {
                        log.ErrorFormat("no columns to rollback for table {0}", rollbackTable.Key);
                        res = false;
                    }
                    else
                    {
                        log.DebugFormat("starting to rollback table {0}, columns: {1}", rollbackTable.Key, string.Join(",", rollbackTable.Value));
                        if (OPCMigrationDAL.RollbackPerTable(rollbackTable.Key, rollbackTable.Value, sequenceId))
                        {
                            log.DebugFormat("table {0} rollback is done", rollbackTable.Key);
                        }
                        else
                        {
                            if (failedRollbacks.TryAdd(rollbackTable.Key, rollbackTable.Value))
                            {
                                log.WarnFormat("table {0} rollback has failed, retry will be tried once", rollbackTable.Key);
                            }
                            else
                            {
                                log.ErrorFormat("table {0} rollback has failed, retry will not be applied", rollbackTable.Key);
                                res = false;
                            }
                        }
                    }
                });

                if (failedRollbacks.Count > 0)
                {
                    log.DebugFormat("starting rollback retry for all failed tables");
                    foreach (KeyValuePair<string, List<string>> rollbackTable in failedRollbacks)
                    {
                        if (rollbackTable.Value == null || rollbackTable.Value.Count == 0)
                        {
                            log.ErrorFormat("no columns to rollback for table {0}", rollbackTable.Key);
                            res = false;
                        }
                        else
                        {
                            log.DebugFormat("retrying rollback for table {0}, columns: {1}", rollbackTable.Key, string.Join(",", rollbackTable.Value));
                            if (OPCMigrationDAL.RollbackPerTable(rollbackTable.Key, rollbackTable.Value, sequenceId))
                            {
                                log.DebugFormat("table {0} rollback is done", rollbackTable.Key);
                            }
                            else
                            {
                                log.ErrorFormat("table {0} rollback retry has failed", rollbackTable.Key);
                                res = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Failed RollbackOfAllTables", ex);
                res = false;
            }

            return res;
        }

        private bool RollbackOfNewOpcTables()
        {
            try
            {
                return OPCMigrationDAL.RollbackNewOpcTables(groupId, Utils.UPDATING_USER_ID);
            }
            catch (Exception ex)
            {
                log.Error("Failed RollbackOfNewOpcTables", ex);
                return false;
            }
        }

    }
}
