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
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Data Members

        private CouchbaseManager.CouchbaseManager couchbaseClient;
        private object locker;
        private int maximumTries;
        private int secondsInCache;

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
            couchbaseClient = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CACHE);
            secondsInCache = -1;
        }

        public CouchbaseSynchronizer(int maximumTries, int secondsInCache = -1)
            : this()
        {
            this.maximumTries = maximumTries;
            this.secondsInCache = secondsInCache;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Do an action with a queue-like order. First thread/machine will lock and start action, 
        /// second/others one will start action only after the previous finish
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool DoAction(string key, Dictionary<string, object> parameters = null)
        {
            bool result = false;

            ulong version;

            // Check if this key is already locked
            var getResult = couchbaseClient.GetWithVersion<int>(key, out version);

            // If get was succesful
            int isLocked = getResult;

            // If not locked
            if (isLocked == 0)
            {
                ulong newVersion;

                // Try to lock
                bool setResult = couchbaseClient.SetWithVersion(key, 1, version, out newVersion);

                // If succesfully locked
                if (setResult)
                {
                    if (this.SynchronizedAct != null)
                    {
                        result = this.SynchronizedAct(parameters);
                    }

                    // Try to unlock
                    bool secondSetResult = couchbaseClient.SetWithVersion(key, 0, newVersion);

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


            return result;
        }

        /// <summary>
        /// Performs an action ONLY ONCE. Other machines/threads will be denied of the action!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool SingleDoAction(string key, out bool actionResult, Dictionary<string, object> parameters = null)
        {
            bool result = false;
            actionResult = false;

            ulong version;

            // Check if this key is already locked
            var getResult = couchbaseClient.GetWithVersion<int>(key, out version);

            // If get was succesful
            int isLocked = getResult;

            // If not locked
            if (isLocked == 0)
            {
                bool setResult;
                ulong newVersion;

                if (this.secondsInCache > 0)
                {
                    // Try to lock temporarily/permenantly 
                    setResult = couchbaseClient.SetWithVersion(key, 1, version, out newVersion, (uint)this.secondsInCache);
                }
                else
                {
                    // Try to lock permenantly 
                    setResult = couchbaseClient.SetWithVersion(key, 1, version, out newVersion);
                }

                // If succesfully locked
                if (setResult)
                {
                    try
                    {
                        if (this.SynchronizedAct != null)
                        {
                            actionResult = this.SynchronizedAct(parameters);
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("Syncrhnoizer failed performing action. key = {0}, message = {1}, st = {2}, target site = {3}",
                            key, ex.Message, ex.StackTrace, ex.TargetSite), ex);
                        throw ex;
                    }
                    // Always unlock, even if exception is thrown - to avoid infinite lock
                    finally
                    {
                        // Try to unlock
                        bool secondSetResult = couchbaseClient.SetWithVersion(key, 0, newVersion);
                    }

                    isLocked = 0;
                }
            }

            return result;
        }

        #endregion
    }
}
