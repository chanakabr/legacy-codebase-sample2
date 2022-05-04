using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CouchbaseManager;
using FeatureFlag;
using Ott.Lib.FeatureToggle;
using Phx.Lib.Log;

[assembly: InternalsVisibleTo("Synchronizer.Tests")]

namespace Synchronizer
{
    public class LockObjectDocument
    {
        public string LockInitiator { get; set; }
    }

    public class DistributedLock
    {
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;

        private static readonly KLogger _Logger = new KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ICouchbaseManager _keyValueStore;
        private readonly LockContext _context;

        private readonly string _additionalInfo = string.Empty;

        public DistributedLock(LockContext context, IPhoenixFeatureFlag phoenixFeatureFlag)
        {
            _keyValueStore = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            _context = context;
            _phoenixFeatureFlag = phoenixFeatureFlag;
        }

        public DistributedLock(LockContext context, IPhoenixFeatureFlag phoenixFeatureFlag, IReadOnlyDictionary<string, string> additionalInfoDic) : this(context, phoenixFeatureFlag)
        {
            _additionalInfo = FlatAdditionalInfo(additionalInfoDic);
        }

        internal DistributedLock(LockContext context, ICouchbaseManager keyValueStore, IPhoenixFeatureFlag phoenixFeatureFlag, IReadOnlyDictionary<string, string> additionalInfoDic)
        {
            _keyValueStore = keyValueStore;
            _phoenixFeatureFlag = phoenixFeatureFlag;
            _context = context;
            _additionalInfo = FlatAdditionalInfo(additionalInfoDic);
        }

        /// <summary>
        /// Using a key-value store to create a Lock docuemnt that while exists will not allow 
        /// any other process to request same lock
        /// </summary>
        /// <returns>Ture if successfully locked, flase otherwise</returns>
        public bool Lock(IEnumerable<string> keys, int numOfRetries, int retryIntervalMs, int ttlSeconds, string lockInitiator, string globalLockKeyNameInitiator = "")
        {
            var globalLockKey = GetGlobalLockKey(_context.GroupId)+ globalLockKeyNameInitiator ;
            // global lock object is used when aquiring lock on multiple keys.
            // it is here to make sure tow processes will not aquires partial lock form each list which will cause a deadlock
            var globalLockInitiator = $"{lockInitiator}_{string.Join("_", keys)}";
            var globalLockObj = new LockObjectDocument() { LockInitiator = globalLockInitiator };
            try
            {
                var isGlobalLockedSuccess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, globalLockObj, globalLockKey);
                if (!isGlobalLockedSuccess) { return false; }

                var lockObj = new LockObjectDocument() { LockInitiator = lockInitiator };
                _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquiring lock on keys:[{string.Join(",", keys)}]"));

                // Add distinct to save the users from themselves in case they are trying to lock the same key twice (should have same effect)
                foreach (var key in keys.Distinct())
                {
                    var isLockedSuccess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, lockObj, key);
                    if (!isLockedSuccess)
                    {
                        _Logger.Error(WrapLogMessageWithMetadata($"DistributedLock > Could not acquired lock on key:[{key}], all retry attempts exhausted."));
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _Logger.Error(WrapLogMessageWithMetadata($"error while trying to lock initiator:[{lockInitiator}] keys:[{string.Join(",", keys)}]"), e);
            }
            finally
            {
                Unlock(new[] { globalLockKey }, globalLockInitiator);
            }

            _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquired lock on key:[{string.Join(",", keys)}]..."));
            return true;
        }

        private bool LockSingleKey(int numOfRetries, int retryIntervalMs, int ttlSeconds, LockObjectDocument lockObj, string key)
        {
            var lockAttempt = 0;
            _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquiring lock on key:[{key}]"));

            var isLockSuccessful = _keyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
            while (!isLockSuccessful && lockAttempt < numOfRetries)
            {
                isLockSuccessful = _keyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
                if (!isLockSuccessful)
                {
                    lockAttempt++;
                    _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Lock attempt [{lockAttempt}/{numOfRetries}]: Could not acquire lock on key:[{key}], trying again in:[{retryIntervalMs}]"));
                    Thread.Sleep(retryIntervalMs);
                }
            }

            if (lockAttempt >= numOfRetries)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unlocks the give list of keys
        /// </summary>
        /// <param name="keys"></param>
        public IReadOnlyDictionary<string, bool> Unlock(IEnumerable<string> keys, string lockInitiator)
        {
            if (keys == null || !keys.Any())
            {
                return new Dictionary<string, bool>();
            }
            
            var isStrictUnlockingDisabled = _phoenixFeatureFlag.IsStrictUnlockDisabled();
            return keys.Distinct()
                .Select(key => UnlockInternal(key, lockInitiator, isStrictUnlockingDisabled))
                .ToDictionary(unlockResult => unlockResult.key, unlockResult => unlockResult.result);
        }

        public static string GetGlobalLockKey(int groupId)
        {
            return $"OTT_DISTRIBUTED_GLOBAL_LOCK_{groupId}";
        }

        private (string key, bool result) UnlockInternal(string key, string lockInitiator, bool isStrictUnlockingDisabled)
        {
            var lockObjectDocument = _keyValueStore.GetWithVersion<LockObjectDocument>(key, out var version, out var status);
            var tempResult = false;
            switch (status)
            {
                case eResultStatus.SUCCESS:
                    if (!isStrictUnlockingDisabled)
                    {
                        if (string.Compare(lockObjectDocument.LockInitiator, lockInitiator, StringComparison.InvariantCultureIgnoreCase) != 0)
                        {
                            _Logger.Error($"The lock key is trying to be deleted by non-initiator. Key - {key}, LockInitiator - {lockInitiator}");
                            tempResult = false;
                            break;
                        }
                    }

                    tempResult = _keyValueStore.Remove(key, version);
                    _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Lock on key was removed:[{key}]"));
                    break;
                case eResultStatus.KEY_NOT_EXIST:
                    tempResult = true;
                    _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Lock on key was removed:[{key}]"));
                    break;
                case eResultStatus.ERROR:
                    tempResult = false;
                    _Logger.Error($"There was an error during retrieval of lock document. Key - {key}, LockInitiator - {lockInitiator}");
                    break;
            }

            if (tempResult)
            {
                _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Lock on key was removed:[{key}]"));
                return (key, true);
            }

            _Logger.Error(WrapLogMessageWithMetadata($"DistributedLock > Could not remove lock on key:[{key}]"));
            return (key, false);
        }

        private static string FlatAdditionalInfo(IReadOnlyDictionary<string, string> additionalInfoDic)
        {
            if (additionalInfoDic == null)
            {
                return string.Empty;
            }

            var result = new StringBuilder(" Metadata: ");
            foreach (var additionalInfoPair in additionalInfoDic)
            {
                result.Append($"{additionalInfoPair.Key} - {additionalInfoPair.Value},");
            }
            
            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        private string WrapLogMessageWithMetadata(string logMessage)
        {
            return logMessage + _additionalInfo;
        }
    }
}
