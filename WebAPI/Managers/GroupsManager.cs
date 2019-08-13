using System;
using System.Collections.Generic;
using System.Threading;
using WebAPI.ClientManagers.Client;
using AutoMapper;
using WebAPI.Exceptions;
using System.Reflection;
using KLogMonitor;
using WebAPI.Managers.Models;
using ConfigurationManager;
using CachingProvider.LayeredCache;

namespace WebAPI.ClientManagers
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
            try
            {
                groupKeyFormat = ApplicationConfiguration.GroupsManagerConfiguration.KeyFormat.Value;
                groupCacheTtlSeconds = ApplicationConfiguration.GroupsManagerConfiguration.CacheTTLSeconds.IntValue;
                syncObj = new object();
                syncLock = new ReaderWriterLockSlim();
            }
            catch (Exception ex)
            {
                log.Error("Error while initiating groups manager", ex);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Groups cache");
            }
        }

        public static Group GetGroup(int groupId)
        {
            if (instance == null)
            {
                instance = new GroupsManager();
            }

            Group group = null;
            var groupKey = string.Format(groupKeyFormat, groupId);
            var invalidationKey = LayeredCacheKeys.PhoenixGroupsManagerInvalidationKey(groupId);
            
            if (!LayeredCache.Instance.Get(groupKey, 
                                           ref group, 
                                           BuildGroup,
                                           new Dictionary<string, object>() { { "groupId", groupId } }, 
                                           groupId, 
                                           LayeredCacheConfigNames.PHOENIX_GROUPS_MANAGER_CACHE_CONFIG_NAME,
                                           new List<string>() { invalidationKey }))
            {
                log.ErrorFormat("Failed building Phoenix group object for groupId: {0}", groupId);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }
            
            return group;
        }

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
                    CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);
                    group = cbManager.Get<Group>(string.Format(groupKeyFormat, groupId), true);
                }

                if (group == null)
                {
                    log.Warn("failed to get group cache on createNewInstance method");
                    throw new Exception();
                }

                // get group languages
                var languages = ClientsManager.ApiClient().GetGroupLanguages(groupId);
                if (languages != null)
                    group.Languages = Mapper.Map<List<Language>>(languages);
            }
            else
            {
                group = new Group();
            }

            return group;
        }

        //public static Group GetGroup(int groupId, HttpContext context = null)
        //{
        //    if (instance == null)
        //        instance = new GroupsManager();

        //    string groupKey = string.Format(groupKeyFormat, groupId);
        //    Group tempGroup = null;

        //    if ((context == null && HttpContext.Current.Cache.Get(groupKey) == null) || (context != null && context.Cache.Get(groupKey) == null))
        //    {
        //        if (syncLock.TryEnterWriteLock(10000))
        //        {
        //            try
        //            {
        //                if ((context == null && HttpContext.Current.Cache.Get(groupKey) == null) || (context != null && context.Cache.Get(groupKey) == null))
        //                {
        //                    Group group = createNewInstance(groupId);

        //                    if (group != null)
        //                    {
        //                        if (context == null)
        //                        {
        //                            HttpContext.Current.Cache.Add(groupKey, group, null, DateTime.UtcNow.AddSeconds(groupCacheTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
        //                        }
        //                        else
        //                        {
        //                            context.Cache.Add(groupKey, group, null, DateTime.UtcNow.AddSeconds(groupCacheTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception: {2}", groupKey, groupId, ex);
        //                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
        //            }
        //            finally
        //            {
        //                syncLock.ExitWriteLock();
        //            }
        //        }
        //    }

        //    // If item already exist
        //    if (syncLock.TryEnterReadLock(10000))
        //    {
        //        try
        //        {
        //            object res = null;

        //            if (context == null)
        //            {
        //                res = HttpContext.Current.Cache.Get(groupKey);
        //            }
        //            else
        //            {
        //                res = context.Cache.Get(groupKey);
        //            }

        //            if (res != null && res is Group)
        //            {
        //                tempGroup = res as Group;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception {2}", groupKey, groupId, ex);
        //            throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
        //        }
        //        finally
        //        {
        //            syncLock.ExitReadLock();
        //        }
        //    }

        //    return tempGroup;
        //}        
    }
}