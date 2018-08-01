using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CachingProvider;
using KLogMonitor;
using CachingProvider.LayeredCache;
using ConfigurationManager;

namespace Core.Users
{
    public class UsersCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();

        private static bool Add(string key, object obj)
        {
            return TvinciCache.WSCache.Instance.Add(key, obj);
        }

        private static T Get<T>(string key)
        {
            return TvinciCache.WSCache.Instance.Get<T>(key);
        }

        public static bool AddItem(string key, object obj)
        {
            return (!string.IsNullOrEmpty(key)) && Add(key, obj);
        }

        public static bool GetItem<T>(string key, out T oValue)
        {
            return TvinciCache.WSCache.Instance.TryGet(key, out oValue);
        }


        #region ExternalCache

        #region OutOfProcessCache           
        private ICachingService cache = null;
        #endregion

        private static UsersCache instance = null;

        private static double GetDocTTLSettings()
        {
            double nResult = ApplicationConfiguration.UsersCacheConfiguration.TTLSeconds.DoubleValue;
            if (nResult == 0)
            {
                nResult = 1440.0;
            }

            return nResult;
        }

        private UsersCache()
        {
            // create an instance of user to external cache (by CouchBase)
            cache = CouchBaseCache<User>.GetInstance("CACHE");
        }

        #endregion

        public static UsersCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new UsersCache();
                    }
                }
            }

            return instance;
        }

        #region Public methods

        // try getting the user from the cache
        /*internal User GetUser(int userID, int groupID)
        {
            User userObject = null;
            try
            {
                if (shouldUseCache)
                {
                    string sKey = string.Format("group_{0}_{1}_{2}", groupID, userKeyCache, userID);
                    // try getting the userID from the cache, the result is not relevant since we return null if no user is found
                    bool isSuccess = this.cache.GetJsonAsT<User>(sKey, out userObject);
                }

                if (userObject != null)
                {
                    userObject.SetReadingInvalidationKeys();
                }

                return userObject;
            }
            catch (Exception ex)
            {
                log.Debug("GetUser - " + string.Format("Couldn't get user {0}, ex = {1}", userID, ex.Message), ex);
                return null;
            }
        }
        */

        internal User GetUser(int userId, int groupId)
        {
            User user = null;
            try
            {
                string key = LayeredCacheKeys.GetUserKey(userId, groupId);
                User userToGet = null;
                if (!LayeredCache.Instance.Get<User>(key, ref userToGet, GetUser, new Dictionary<string, object>() { { "groupId", groupId }, { "userId", userId } }, groupId,
                                                    LayeredCacheConfigNames.USER_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetUserInvalidationKey(userId.ToString()) }))
                {
                    log.DebugFormat("GetUser - Couldn't get userId {0}", userId);
                }
                else
                {
                    user = TVinciShared.ObjectCopier.Clone<User>(userToGet);
                    if (user != null)
                    {
                        user.SetReadingInvalidationKeys();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUser for groupId: {0}, userId: {1}", groupId, userId), ex);
            }

            return user;
        }

        /*internal bool InsertUser(User user, int groupID)
        {
            bool isInsertSuccess = false;
            Random r = new Random();
            int limitRetries = RETRY_LIMIT;
            try
            {
                if (shouldUseCache)
                {
                    if (user == null)
                    {
                        return false;
                    }

                    string key = string.Format("group_{0}_{1}_{2}", groupID, userKeyCache, user.m_sSiteGUID);

                    //insert user to cache
                    while (limitRetries > 0)
                    {
                        isInsertSuccess = this.cache.SetJson<User>(key, user, cacheTTL);
                        if (!isInsertSuccess)
                        {
                            Thread.Sleep(r.Next(50));
                            limitRetries--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!isInsertSuccess)
                    {
                        log.Error(string.Format("Failed inserting user {0} to cache", user.m_sSiteGUID));
                    }

                    return isInsertSuccess;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed inserting user {0} to cache, ex = {1}", user != null ? user.m_sSiteGUID : "0", ex.Message), ex);
                return false;
            }
        }
        */

        /*internal bool RemoveUser(int userID, int groupID)
        {
            bool isRemoveSuccess = false;
            Random r = new Random();
            int limitRetries = RETRY_LIMIT;
            try
            {
                if (shouldUseCache)
                {
                    string key = string.Format("group_{0}_{1}_{2}", groupID, userKeyCache, userID);

                    //remove user from cache
                    while (limitRetries > 0)
                    {
                        BaseModuleCache cacheModule = cache.Remove(key);
                        if (cacheModule != null && cacheModule.result != null)
                        {
                            isRemoveSuccess = (bool)cacheModule.result;
                        }
                        if (!isRemoveSuccess)
                        {
                            Thread.Sleep(r.Next(50));
                            limitRetries--;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!isRemoveSuccess)
                    {
                        log.Error(string.Format("Failed removing user {0} from cache", userID.ToString()));
                    }
                    else
                    {
                        User.InvalidateUser(userID.ToString());
                    }

                    return isRemoveSuccess;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed removing user {0} from cache, ex = {1}", userID, ex.Message), ex);
                return false;
            }
        }
        */

        internal bool RemoveUser(int userId, int groupId)
        {
            bool res = false;
            try
            {
                string invalidationKey = LayeredCacheKeys.GetUserInvalidationKey(userId.ToString());
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed removing user {0} from cache", userId);
                    return res;
                }

                res = true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed removing user {0} from cache, ex = {1}", userId, ex.Message), ex);
            }

            return res;
        }

        #endregion

        #region Private Methods

        private static Tuple<User, bool> GetUser(Dictionary<string, object> funcParams)
        {
            bool res = false;
            User user = null;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("userId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    int? userId = funcParams["userId"] as int?;
                    user = new User();
                    res = groupId.HasValue && userId.HasValue && user.Initialize(userId.Value, groupId.Value, true);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUser failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<User, bool>(user, res);
        }

        #endregion

    }

}