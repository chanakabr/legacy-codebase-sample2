using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Clients;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using AutoMapper;
using WebAPI.Exceptions;
using WebAPI.Utils;
using WebAPI.Models.General;
using System.Reflection;
using KLogMonitor;
using WebAPI.Managers.Models;
using WebAPI.Managers;
using WebAPI.Models.API;

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
                groupKeyFormat = TCMClient.Settings.Instance.GetValue<string>("group_key_format");
                groupCacheTtlSeconds = TCMClient.Settings.Instance.GetValue<int>("group_cache_ttl_seconds");
                syncObj = new object();
                syncLock = new ReaderWriterLockSlim();
            }
            catch (Exception ex)
            {
                log.Error("Error while initiating groups manager", ex);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Groups cache");
            }
        }

        public static Group GetGroup(int groupId, HttpContext context = null)
        {
            if (instance == null)
                instance = new GroupsManager();

            string groupKey = string.Format(groupKeyFormat, groupId);
            Group tempGroup = null;

            if ((context == null && HttpContext.Current.Cache.Get(groupKey) == null) || (context != null && context.Cache.Get(groupKey) == null))
            {
                if (syncLock.TryEnterWriteLock(10000))
                {
                    try
                    {
                        if ((context == null && HttpContext.Current.Cache.Get(groupKey) == null) || (context != null && context.Cache.Get(groupKey) == null))
                        {
                            Group group = createNewInstance(groupId);

                            if (group != null)
                            {
                                if (context == null)
                                {
                                    HttpContext.Current.Cache.Add(groupKey, group, null, DateTime.UtcNow.AddSeconds(groupCacheTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                                }
                                else
                                {
                                    context.Cache.Add(groupKey, group, null, DateTime.UtcNow.AddSeconds(groupCacheTtlSeconds), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception: {2}", groupKey, groupId, ex);
                        throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
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

                    if (context == null)
                    {
                        res = HttpContext.Current.Cache.Get(groupKey);
                    }
                    else
                    {
                        res = context.Cache.Get(groupKey);
                    }

                    if (res != null && res is Group)
                    {
                        tempGroup = res as Group;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error while trying to get group from cache. group key: {0}, group ID: {1}, exception {2}", groupKey, groupId, ex);
                    throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
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
            List<KalturaUserRole> roles = null;

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
                    log.Warn("failed to get group cache from Couchbase");
                    throw new Exception();
                }

                // get group languages
                var languages = ClientsManager.ApiClient().GetGroupLanguages(groupId);
                if (languages != null)
                    group.Languages = Mapper.Map<List<Language>>(languages);


                // get group roles
                roles = ClientsManager.ApiClient().GetRoles(groupId);
            }
            else
            {
                group = new Group();

                // get default roles scheme for default group
                roles = ClientsManager.ApiClient().GetRoles();
            }

            if (roles != null)
            {
                // build dictionary permission items - roles with groups dictionary, for easy access
                group.PermissionItemsRolesMapping = RolesManager.BuildPermissionItemsDictionary(roles);
                group.RolesIdsNamesMapping = roles.ToDictionary(dr => dr.getId(), dr => dr.Name);
            }

            return group;
        }


    }

}