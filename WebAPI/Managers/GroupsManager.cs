using WebAPI.Clients.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Models;
using AutoMapper;
using Couchbase.Extensions;
using WebAPI.Filters;

namespace WebAPI.Managers
{
    public class GroupsManager
    {
        private const string GROUP_KEY_FORMAT = "group_{0}";

        private static volatile Dictionary<int, Group> groupsInstances = new Dictionary<int, Group>();
        private static object syncObj = new object();
        private static ReaderWriterLockSlim syncLock = new ReaderWriterLockSlim();

        public static Group GetGroup(int groupId)
        {
            Group tempGroup = null;

            if (!groupsInstances.ContainsKey(groupId))
            {
                if (syncLock.TryEnterWriteLock(10000))
                {
                    try
                    {
                        if (!groupsInstances.ContainsKey(groupId))
                        {
                            Group group = createNewInstance(groupId);

                            if (group != null)
                            {
                                groupsInstances.Add(groupId, group);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InternalServerErrorException();
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
                    groupsInstances.TryGetValue(groupId, out tempGroup);
                }
                catch (Exception ex)
                {
                    throw new InternalServerErrorException();
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

            group = CouchbaseManager.GetInstance(CouchbaseBucket.Groups).GetJson<Group>(string.Format(GROUP_KEY_FORMAT, groupId));

            if (group == null)
            {
                throw new Exception();
            }

            var languages = ClientsManager.ApiClient().GetGroupLanguages(group.ApiCredentials.Username, group.ApiCredentials.Password);
            if (languages != null)
            {
                group.Languages = Mapper.Map<List<Language>>(languages);
            }

            return group;
        }
    }
}