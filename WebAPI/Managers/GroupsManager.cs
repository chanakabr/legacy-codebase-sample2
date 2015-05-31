using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Clients;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using AutoMapper;
using Couchbase.Extensions;
using WebAPI.Exceptions;
using WebAPI.Utils;
using WebAPI.Models.General;
using System.Reflection;
using KLogMonitor;

namespace WebAPI.ClientManagers
{
    public class GroupsManager
    {
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
                groupKeyFormat = TCMClient.Settings.Instance.GetValue<string>("group_key_format");
                groupCacheTtlSeconds = TCMClient.Settings.Instance.GetValue<int>("group_cache_ttl_seconds");
                syncObj = new object();
                syncLock = new ReaderWriterLockSlim();
            }
            catch (Exception ex)
            {
                log.Error("Error while initiating groups manager", true, ex);
                throw new InternalServerErrorException((int)StatusCode.MissingConfiguration, "Groups cache configuration missing");
            }
        }

        public static Group GetGroup(int groupId)
        {
            if (instance == null)
                instance = new GroupsManager();

            string groupKey = string.Format(groupKeyFormat, groupId);
            Group tempGroup = null;

            if (HttpContext.Current.Cache.Get(groupKey) == null)
            {
                if (syncLock.TryEnterWriteLock(10000))
                {
                    try
                    {
                        if (HttpContext.Current.Cache.Get(groupKey) == null)
                        {
                            Group group = createNewInstance(groupId);

                            if (group != null)
                            {
                                HttpContext.Current.Cache.Add(groupKey, group, null, DateTime.UtcNow.AddSeconds(groupCacheTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}", true, ex, groupKey, groupId);
                        throw new InternalServerErrorException((int)StatusCode.MissingConfiguration, "Group configuration not found");
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
                    var res = HttpContext.Current.Cache.Get(groupKey);
                    if (res != null && res is Group)
                    {
                        tempGroup = res as Group;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}", true, ex, groupKey, groupId);
                    throw new InternalServerErrorException((int)StatusCode.MissingConfiguration, "Group configuration not found");
                }
                finally
                {
                    syncLock.ExitReadLock();
                }
            }

            return tempGroup;
        }

        private static Group createNewInstance(int groupId)
        {
            Group group = null;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE) { Database = CouchbaseBucket.Groups.ToString(), QueryType = Events.eDBQueryType.SELECT })
            {
                group = CouchbaseManager.GetInstance(CouchbaseBucket.Groups).GetJson<Group>(string.Format(groupKeyFormat, groupId));
            }

            if (group == null)
            {
                log.Warning("failed to get group cache from Couchbase");
                throw new Exception();
            }

            var languages = ClientsManager.ApiClient().GetGroupLanguages(group.ApiCredentials.Username, group.ApiCredentials.Password);
            if (languages != null)
                group.Languages = Mapper.Map<List<Language>>(languages);

            return group;
        }
    }
}