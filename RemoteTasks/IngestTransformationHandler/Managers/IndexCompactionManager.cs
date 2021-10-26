using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiLogic.EPG;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using ConfigurationManager;
using Core.Catalog;
using CouchbaseManager;
using IngestHandler.Common;
using KLogMonitor;
using Synchronizer;

namespace IngestTransformationHandler.Managers
{
    public interface IIndexCompactionManager
    {
        void RunEpgIndexCompactionIfRequired(int partnerId);
    }
    
    public class IndexCompactionManager : IIndexCompactionManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ICouchbaseManager _couchbaseManager;
        private readonly EPGIngestV2Configuration _epgV2Config;

        public IndexCompactionManager()
        {
            _couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            _epgV2Config = ApplicationConfiguration.Current.EPGIngestV2Configuration;
        }

        /// <summary>
        /// This method will run at the beginning of the Transformation handler.
        /// It will check if index compaction is required and if it is one of the ingests will begin compaction.
        /// All other ingests will stay locked because of the double-check-lock pattern, and will be released
        /// once the compaction was completed and the lock was released they will find out that the last runn date
        /// has updated and they dont need to run so they can continue with ingest
        /// </summary>
        public void RunEpgIndexCompactionIfRequired(int partnerId)
        {
            var locker = new DistributedLock(partnerId);
            try
            {
                if (IsRunIntervalForCompactionPassed(partnerId))
                {
                    _logger.Info($"index compaction required for partner:[{partnerId}]! locking and verifying check");
                    // Lock and check again in case someone already started by now (simple double check lock pattern)
                    LockEpgV2IndexCompaction(partnerId, locker);
                    if (IsRunIntervalForCompactionPassed(partnerId))
                    {
                        _logger.Info($"index compaction starting for partner:[{partnerId}]");
                        CompactEpgIndices(partnerId);
                        _couchbaseManager.Set<DateTime>(GetLastRunDateKey(partnerId), DateTime.UtcNow, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"error while trying to compact epg v2 indices, partner:[{partnerId}]", ex);
                throw;
            }
            finally
            {
                locker.Unlock(GetGlobalEpgIngestLockKey(partnerId));
            }
            
        }

        private void CompactEpgIndices(int partnerId)
        {
            var idxManager = IndexManagerFactory.Instance.GetIndexManager(partnerId);
            var epgV2PartnerConfig = EpgV2PartnerConfigurationManager.Instance.GetConfiguration(partnerId);
            var isSuccess = idxManager.CompactEpgV2Indices(epgV2PartnerConfig.FutureIndexCompactionStart, epgV2PartnerConfig.PastIndexCompactionStart);
            if (!isSuccess) { throw new Exception($"failed to compact EPG indices for partner:[{partnerId}]"); }
        }

        private void LockEpgV2IndexCompaction(int partnerId, DistributedLock locker)
        {
            var isLocked = locker.Lock(GetGlobalEpgIngestLockKey(partnerId),
                _epgV2Config.LockNumOfRetries.Value,
                _epgV2Config.LockRetryIntervalMS.Value,
                _epgV2Config.LockTTLSeconds.Value,
                nameof(IndexCompactionManager));
            if (!isLocked) { throw new Exception("Failed to acquire lock on ingest dates"); }
        }

        private bool IsRunIntervalForCompactionPassed(int partnerId)
        {
            var lastRunDate = _couchbaseManager.Get<DateTime>(GetLastRunDateKey(partnerId));
            return DateTime.UtcNow.Subtract(lastRunDate).TotalMinutes >= _epgV2Config.IndexCompactionIntervalMinutes.Value;
        }

        private static string GetLastRunDateKey(int partnerId)
        {
            return $"EpgIngestV2IndexCompactionLastRunTime_{partnerId}";
        }

        private IEnumerable<string> GetGlobalEpgIngestLockKey(int partnerId)
        {
            return new[] { $"epg_v2_index_compaction_lock_{partnerId}" };
        }
    }
}