using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CachingProvider;
using CouchbaseManager;
using KLogMonitor;

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

        public DistributedLock(int groupId)
        {
            _KeyValueStore = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            _GroupId = groupId;
        }

        /// <summary>
        /// Using a key-value store to create a Lock docuemnt that while exists will not allow 
        /// any other process to request same lock
        /// </summary>
        /// <returns>Ture if successfully locked, flase otherwise</returns>
        public bool Lock(IEnumerable<string> keys, int numOfRetries, int retryIntervalMs, int ttlSeconds, string lockInitiator)
        {
            var globalLockKey = GetGlobalLockKey(_GroupId);
            // global lock object is used when aquiring lock on multiple keys.
            // it is here to make sure tow processes will not aquires partial lock form each list which will cause a deadlock
            var globalLockObj = new LockObjectDocument() { LockInitiator = $"{lockInitiator}_{string.Join("_", keys)}" };
            try
            {
                var isGlobalLockedSucess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, globalLockObj, globalLockKey);
                if (!isGlobalLockedSucess) { return false; }

                var lockObj = new LockObjectDocument() { LockInitiator = lockInitiator };
                _Logger.Debug($"DistributedLock > Acquiring lock on keys:[{string.Join(",", keys)}]");


                foreach (var key in keys)
                {
                    var isLockedSucess = LockSingleKey(numOfRetries, retryIntervalMs, ttlSeconds, lockObj, key);
                    if (!isLockedSucess)
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                _Logger.Error($"error while trying to lock initiator:[{lockInitiator}] keys:[{string.Join(",", keys)}]", e);
            }
            finally
            {
                Unlock(new[] { globalLockKey });
            }

            _Logger.Debug($"DistributedLock > Acquired lock on key:[{string.Join(",", keys)}]...");
            return true;
        }

        private bool LockSingleKey(int numOfRetries, int retryIntervalMs, int ttlSeconds, LockObjectDocument lockObj, string key)
        {
            var lockAttempt = 0;
            _Logger.Debug($"DistributedLock > Acquiring lock on key:[{key}]");

            var isLockSuccessful = _KeyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
            while (!isLockSuccessful && lockAttempt < numOfRetries)
            {
                isLockSuccessful = _KeyValueStore.Add(key, lockObj, (uint)ttlSeconds, asJson: false, suppressErrors: true);
                if (!isLockSuccessful)
                {
                    lockAttempt++;
                    _Logger.Debug($"DistributedLock > Lock attempt [{lockAttempt}/{numOfRetries}]: Could not acquire lock on key:[{key}], trying again in:[{retryIntervalMs}]");
                    Thread.Sleep(retryIntervalMs);
                }
            }

            if (lockAttempt >= numOfRetries)
            {
                _Logger.Error($"DistributedLock > Could not acquired lock on key:[{key}], all retry attempts exhausted.");
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
            foreach (var key in keys)
            {
                if (_KeyValueStore.Remove(key))
                {
                    _Logger.Debug($"DistributedLock > Lock on key was removed:[{key}]");

                }
                else
                {
                    _Logger.Error($"DistributedLock > Could not remove lock on key:[{key}]");
                }
            }
        }

        public static string GetGlobalLockKey(int groupId)
        {
            return $"OTT_DISTRIBUTED_GLOBAL_LOCK_{groupId}";
        }
    }
}
