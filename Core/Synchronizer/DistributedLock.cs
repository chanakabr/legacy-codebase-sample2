using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CouchbaseManager;
using Phx.Lib.Log;

namespace Synchronizer
{
    public class LockObjectDocument
    {
        public string LockInitiator { get; set; }
    }

    public class DistributedLock
    {
        private static readonly KLogger _Logger = new KLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly CouchbaseManager.CouchbaseManager _KeyValueStore;
        private readonly int _GroupId;

        private readonly string _additionalInfo = string.Empty;

        public DistributedLock(int groupId)
        {
            _KeyValueStore = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            _GroupId = groupId;
        }
        
        public DistributedLock(int groupId, IReadOnlyDictionary<string, string> additionalInfoDic)
        {
            _KeyValueStore = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            _GroupId = groupId;
            _additionalInfo = FlatAdditionalInfo(additionalInfoDic);
        }

        /// <summary>
        /// Using a key-value store to create a Lock docuemnt that while exists will not allow 
        /// any other process to request same lock
        /// </summary>
        /// <returns>Ture if successfully locked, flase otherwise</returns>
        public bool Lock(IEnumerable<string> keys, int numOfRetries, int retryIntervalMs, int ttlSeconds, string lockInitiator,string globalLockKeyNameInitiator = "")
        {
            var globalLockKey = GetGlobalLockKey(_GroupId)+ globalLockKeyNameInitiator ;
            // global lock object is used when aquiring lock on multiple keys.
            // it is here to make sure tow processes will not aquires partial lock form each list which will cause a deadlock
            var globalLockObj = new LockObjectDocument() { LockInitiator = $"{lockInitiator}_{string.Join("_", keys)}" };
            try
            {
                var isGlobalLockedSuccess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, globalLockObj, globalLockKey);
                if (!isGlobalLockedSuccess) { return false; }

                var lockObj = new LockObjectDocument() { LockInitiator = lockInitiator };
                _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquiring lock on keys:[{string.Join(",", keys)}]"));

                // Add distinct to save the users from themselves in case they are trying to lock the same key twice (should have same effect)
                foreach (var key in keys.Distinct())
                {
                    var isLockedSucess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, lockObj, key);
                    if (!isLockedSucess)
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
                Unlock(new[] { globalLockKey });
            }

            _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquired lock on key:[{string.Join(",", keys)}]..."));
            return true;
        }

        private bool LockSingleKey(int numOfRetries, int retryIntervalMs, int ttlSeconds, LockObjectDocument lockObj, string key)
        {
            var lockAttempt = 0;
            _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Acquiring lock on key:[{key}]"));

            var isLockSuccessful = _KeyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
            while (!isLockSuccessful && lockAttempt < numOfRetries)
            {
                isLockSuccessful = _KeyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
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
        public void Unlock(IEnumerable<string> keys)
        {
            if (keys == null || !keys.Any())
                return;

            foreach (var key in keys.Distinct())
            {
                if (_KeyValueStore.Remove(key))
                {
                    _Logger.Debug(WrapLogMessageWithMetadata($"DistributedLock > Lock on key was removed:[{key}]"));

                }
                else
                {
                    _Logger.Error(WrapLogMessageWithMetadata($"DistributedLock > Could not remove lock on key:[{key}]"));
                }
            }
        }

        public static string GetGlobalLockKey(int groupId)
        {
            return $"OTT_DISTRIBUTED_GLOBAL_LOCK_{groupId}";
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
