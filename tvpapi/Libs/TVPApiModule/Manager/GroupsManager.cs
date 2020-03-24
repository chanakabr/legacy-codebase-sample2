using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using KLogMonitor;
using TVPApiModule.Manager;
using System.Web;
using TVPApiModule.Objects.Authorization;

namespace TVPApiModule.Manager
{
    public class GroupsManager
    {
        private const string CB_SECTION_NAME = "groups";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static string groupKeyFormat;
        private static int groupCacheTtlSeconds;

        private static object syncObj;
        private static ReaderWriterLockSlim syncLock;

        private static GroupsManager instance = null;

        private GroupsManager()
        {
            groupKeyFormat = "group_{0}";
            groupCacheTtlSeconds = 86400; // TODO: take from config
            syncObj = new object();
            syncLock = new ReaderWriterLockSlim();
        }



        //public static Group GetGroup(int groupId)
        //{
        //    if (instance == null)
        //    {
        //        instance = new GroupsManager();
        //    }

        //    Group group = null;

        //    string groupKey = string.Format(groupKeyFormat, groupId);
        //    Dictionary<string, object> funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
        //    if (!LayeredCache.Instance.Get<Group>(groupKey, ref group, BuildGroup, funcParams, groupId, LayeredCacheConfigNames.PHOENIX_GROUPS_MANAGER_CACHE_CONFIG_NAME,
        //                                                    new List<string>() { LayeredCacheKeys.PhoenixGroupsManagerInvalidationKey(groupId) }))
        //    {
        //        log.ErrorFormat("Failed building Phoenix group object for groupId: {0}", groupId);
        //        throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
        //    }

        //    return group;
        //}


        private static Tuple<Group, bool> BuildGroup(Dictionary<string, object> funcParams)
        {
            bool result = false;
            Group group = null;

            if (funcParams != null && funcParams.ContainsKey("groupId"))
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue && groupId.Value > 0)
                {
                    group = createNewInstance(groupId.Value);

                    if (group != null)
                    {
                        result = true;
                    }
                }
            }

            return new Tuple<Group, bool>(group, result);
        }

        private static Group createNewInstance(int groupId)
        {
            Group group = null;

            // if the group is not default group - get configuration from CB and languages
            if (groupId != 0)
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE) { Database = CB_SECTION_NAME, QueryType = KLogEnums.eDBQueryType.SELECT })
                {
                    CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS, false, true);
                    group = cbManager.Get<Group>(string.Format(groupKeyFormat, groupId), true);
                }

                if (group == null)
                {
                    log.Warn("failed to get group cache on createNewInstance method");
                    throw new Exception();
                }

                // get group languages
                //var languages = ClientsManager.ApiClient().GetGroupLanguages(groupId);
                //if (languages != null)
                //    group.Languages = Mapper.Map<List<Language>>(languages);

            }
            else
            {
                group = new Group();
            }

            return group;
        }

        public static Group GetGroup(int groupId)
        {
            if (instance == null)
                instance = new GroupsManager();

            string groupKey = string.Format(groupKeyFormat, groupId);
            Group tempGroup = null;

            var cacheResult = CachingManager.CachingManager.GetCachedDataNull(groupKey);
            if (cacheResult == null || !(cacheResult is Group))
            {
                if (syncLock.TryEnterWriteLock(10000))
                {
                    try
                    {
                        cacheResult = CachingManager.CachingManager.GetCachedDataNull(groupKey);
                        if (cacheResult == null || !(cacheResult is Group))
                        {
                            Group group = createNewInstance(groupId);

                            if (group != null)
                            {
                                CachingManager.CachingManager.SetCachedData(groupKey, group, groupCacheTtlSeconds, System.Runtime.Caching.CacheItemPriority.Default, 0, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception: {2}", groupKey, groupId, ex);
                        throw new Exception(AuthorizationManager.MISSING_CONFIGURATION_ERROR);
                    }
                    finally
                    {
                        syncLock.ExitWriteLock();
                    }
                }
            }

            // If item already exist
            if (syncLock.TryEnterReadLock(10000))
            {
                try
                {
                    object res = null;

                    res = CachingManager.CachingManager.GetCachedDataNull(groupKey);
                    
                    if (res != null && res is Group)
                    {
                        tempGroup = res as Group;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception {2}", groupKey, groupId, ex);
                    throw new Exception(AuthorizationManager.MISSING_CONFIGURATION_ERROR);
                }
                finally
                {
                    syncLock.ExitReadLock();
                }
            }

            return tempGroup;
        }
    }
}