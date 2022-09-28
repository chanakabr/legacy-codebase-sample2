using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiLogic.EPG;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Epg;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Catalog;
using CouchbaseManager;
using FeatureFlag;
using IngestHandler.Common;
using IngestHandler.Common.Locking;
using Ott.Lib.FeatureToggle;
using Ott.Lib.FeatureToggle.Managers;
using Phx.Lib.Appconfig.Types;
using Phx.Lib.Log;
using Synchronizer;

namespace IngestHandler.Common.Managers
{
    public interface IIndexCompactionManager
    {
        void RunEpgIndexCompactionIfRequired(int partnerId, long bulkUploadId = 0);
    }
    
    public class IndexCompactionManager : IIndexCompactionManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ICouchbaseManager _couchbaseManager;
        private readonly EPGIngestV2Configuration _epgV2Config;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;

        public IndexCompactionManager(IPhoenixFeatureFlag phoenixFeatureFlag)
        {
            _phoenixFeatureFlag = phoenixFeatureFlag;
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
        public void RunEpgIndexCompactionIfRequired(int partnerId, long bulkUploadId = 0)
        {
            var epgV2PartnerConfig = EpgPartnerConfigurationManager.Instance.GetEpgV2Configuration(partnerId);
            if (!epgV2PartnerConfig.IsIndexCompactionEnabled)
            {
                _logger.Debug($"skipping index compaction as it is not enabled for partner:[{partnerId}]");
                return;
            }
            
            var lockerMetadata = GenerateLockerMetadata(bulkUploadId);
            var locker = new DistributedLock(new LockContext(partnerId), _phoenixFeatureFlag, lockerMetadata);
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
                        CompactEpgIndices(partnerId, epgV2PartnerConfig.FutureIndexCompactionStart, epgV2PartnerConfig.PastIndexCompactionStart);
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
                locker.Unlock(GetGlobalEpgIngestLockKey(partnerId), nameof(IndexCompactionManager));
            }
        }

        private void CompactEpgIndices(int partnerId, int futureCompactionStart, int pastCompactioStart)
        {
            var idxManager = IndexManagerFactory.Instance.GetIndexManager(partnerId);
            var isSuccess = idxManager.CompactEpgV2Indices(futureCompactionStart, pastCompactioStart);
            if (!isSuccess) { throw new Exception($"failed to compact EPG indices for partner:[{partnerId}]"); }
        }

        private void LockEpgV2IndexCompaction(int partnerId, DistributedLock locker)
        {
            var isLocked = locker.Lock(GetGlobalEpgIngestLockKey(partnerId),
                _epgV2Config.LockNumOfRetries.Value,
                _epgV2Config.LockRetryIntervalMS.Value,
                _epgV2Config.LockTTLSeconds.Value,
                nameof(IndexCompactionManager),
                LockInitiator.EpgIngestGlobalLockKeyInitiator);
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

        private static IReadOnlyDictionary<string, string> GenerateLockerMetadata(long bulkUploadId)
        {
            return bulkUploadId == 0
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>
                {
                    { "BulkUploadId", bulkUploadId.ToString() }
                };
        }
    }
}
