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

        public DistributedLock()
        {
            _KeyValueStore = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
        }

        /// <summary>
        /// Using a key-value store to create a Lock docuemnt that while exists will not allow 
        /// any other process to request same lock
        /// </summary>
        /// <returns>Ture if successfully locked, flase otherwise</returns>
        public bool Lock(IEnumerable<string> keys, int numOfRetries, int retryIntervalMs, int ttlSeconds, string lockInitiator)
        {
            var lockObj = new LockObjectDocument() { LockInitiator = lockInitiator };
            _Logger.Debug($"DistributedLock > Acquiring lock on keys:[{string.Join(",", keys)}]");
            foreach (var key in keys)
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
            }

            _Logger.Debug($"DistributedLock > Acquired lock on key:[{string.Join(",", keys)}]...");
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
    }
}
