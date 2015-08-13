using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CouchbaseManager;
using Couchbase;
using Enyim.Caching.Memcached;
using System.Threading;

namespace Synchronizer
{
    public class CouchbaseSynchronizer
    {
        #region Delegates

        public delegate bool SynchrnoizedActHandler(Dictionary<string, object> parameters);

        #endregion

        #region Events

        public event SynchrnoizedActHandler SynchronizedAct;

        #endregion

        #region Static Data Members

        private static readonly Random random;

        #endregion

        #region Data Members

        private CouchbaseClient couchbaseClient;
        private object locker;
        private int maximumTries;

        #endregion

        #region Ctors

        static CouchbaseSynchronizer()
        {
            random = new Random();
        }

        public CouchbaseSynchronizer()
        {
            locker = new object();
            maximumTries = 100;
            couchbaseClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.CACHE);
        }

        public CouchbaseSynchronizer(int maximumTries)
            : this()
        {
            this.maximumTries = maximumTries;
        }

        #endregion

        public bool DoAction(string key, Dictionary<string, object> parameters = null)
        {
            bool result = false;

            // Check if this adapter is already locked
            var getResult = couchbaseClient.GetWithCas<int>(key);

            // If get was succesful
            if (getResult.StatusCode == 0)
            {
                int isLocked = getResult.Result;

                // If not locked
                if (isLocked == 0)
                {
                    // Try to lock
                    CasResult<bool> setResult = couchbaseClient.Cas(StoreMode.Set, key, 1, getResult.Cas);

                    // If succesfully locked
                    if (setResult.StatusCode == 0 && setResult.Result)
                    {
                        if (this.SynchronizedAct != null)
                        {
                            result = this.SynchronizedAct(parameters);
                        }

                        // Try to unlock
                        CasResult<bool> secondSetResult = couchbaseClient.Cas(StoreMode.Set, key, 0, setResult.Cas);

                        isLocked = 0;
                    }
                    // If the set failed, it is probably because another machine set the value before us
                    else
                    {
                        isLocked = 1;
                    }
                }

                // If CB is locked already - whether if Get returns 1 or CAS versioning failed
                if (isLocked == 1)
                {
                    // Make sure only one thread checks CB. The rest will wait and check once after lock is released
                    lock (locker)
                    {
                        // Don't try forever
                        int triesLeft = this.maximumTries;

                        // While adapter configuration is locked
                        while (isLocked == 1 && triesLeft > 0)
                        {
                            Thread.Sleep(random.Next(50));

                            // Upadte locked status
                            isLocked = couchbaseClient.Get<int>(key);

                            triesLeft--;
                        }
                    }
                }
            }

            return result;
        }
    }
}
