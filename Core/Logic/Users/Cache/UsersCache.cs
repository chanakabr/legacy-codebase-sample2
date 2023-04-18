using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CachingProvider;
using Phx.Lib.Log;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using ApiLogic.Users.Security;

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

        public long GetDomainIdByUser(int userId, int groupId)
        {
            long domainId = 0;
            try
            {
                User user = GetUser(userId, groupId);
                if (user != null && user.m_domianID > 0)
                {
                    domainId = (long)user.m_domianID;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDomainIdByUser for groupId: {0}, userId: {1}", groupId, userId), ex);
            }

            return domainId;
        }

        internal User GetUser(int userId, int groupId)
        {
            User user = null;
            try
            {
                string key = LayeredCacheKeys.GetUserKey(userId, groupId);
                if (LayeredCache.Instance.Get(key,
                                               ref user,
                                               GetUser,
                                               new Dictionary<string, object>()
                                               {
                                                   { "groupId", groupId },
                                                   { "userId", userId }
                                               },
                                               groupId,
                                               LayeredCacheConfigNames.USER_LAYERED_CACHE_CONFIG_NAME,
                                               new List<string>()
                                               {
                                                   LayeredCacheKeys.GetUserInvalidationKey(groupId, userId.ToString()),
                                                   LayeredCacheKeys.GetUserRolesInvalidationKey(groupId, userId.ToString()),
                                                   LayeredCacheKeys.GetUserLoginHistoryInvalidationKey(groupId, userId)
                                               }))
                {
                    if (user != null) // copy, in order to prevent mutation of in-memory cache object
                    {
                        var basicData = (UserBasicData)user.m_oBasicData.Clone();
                        basicData.m_sUserName = UserDataEncryptor.Instance().DecryptUsername(groupId, basicData.m_sUserName);
                        var dynamicData = user.m_oDynamicData.Clone(); 
                        
                        user = new User 
                        {
                            m_oBasicData = basicData,
                            m_oDynamicData = dynamicData,
                            m_sSiteGUID = user.m_sSiteGUID,
                            m_domianID = user.m_domianID,
                            m_eUserState = user.m_eUserState,
                            m_eSuspendState = user.m_eSuspendState,
                            m_nSSOOperatorID = user.m_nSSOOperatorID,
                            m_isDomainMaster = user.m_isDomainMaster,
                            GroupId = groupId
                        };
                    }
                }
                else
                {
                    log.DebugFormat("GetUser - Couldn't get userId {0}", userId);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetUser for groupId: {0}, userId: {1}", groupId, userId), ex);
            }

            return user;
        }

        internal bool RemoveUser(int userId, int groupId)
        {
            bool res = false;
            try
            {
                string invalidationKey = LayeredCacheKeys.GetUserInvalidationKey(groupId, userId.ToString());
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed removing user {0} from cache", invalidationKey);
                    return res;
                }

                invalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(groupId, userId.ToString());
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed removing user {0} from cache", invalidationKey);
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

                    if (userId.HasValue && userId.Value == 0)
                    {
                        res = true; //BEO-10474
                    }
                    else
                    {
                        res = groupId.HasValue && userId.HasValue && user.Initialize(userId.Value, groupId.Value, true);
                        if (res) // encrypt username before put to cache
                        {
                            var dataEncryptor = UserDataEncryptor.Instance();
                            var encryptionType = dataEncryptor.GetUsernameEncryptionType(groupId.Value);
                            user.m_oBasicData.m_sUserName = dataEncryptor.EncryptUsername(groupId.Value, encryptionType, user.m_oBasicData.m_sUserName);
                        }
                    }
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