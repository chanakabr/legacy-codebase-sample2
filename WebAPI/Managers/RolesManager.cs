using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace WebAPI.Managers
{
    public class RolesManager
    {
        /// <summary>
        /// Builds a dictionary representing the schema of roles, permissions and action permission items for the group.
        /// </summary>
        /// <param name="roles">List of roles</param>
        /// <returns>Dictionary of dictionaries, where the key of the first dictionary is a string representing a service action pair (format: {service}_{action}) 
        /// and the value is a dictionary representing all the role IDs containing the permission item of the service action pair, and the users group list that is relevant for the action, 
        /// the second's dictionary key is the role ID and the value is a ';' separated list of users allowed in a group permission</returns>
        internal static Dictionary<string, Dictionary<long, string>> BuildPermissionItemsDictionary(List<KalturaUserRole> roles)
        {
            Dictionary<string, Dictionary<long, string>> dictionary = new Dictionary<string, Dictionary<long, string>>();

            string serviceActionPair;
            KalturaApiActionPermissionItem apiActionPermissionItem;
            string usersGroup;

            foreach (var role in roles)
            {
                foreach (var permission in role.Permissions)
                {
                    // if the permission is group permission, get the users group list to append later
                    if (permission is KalturaGroupPermission)
                    {
                        usersGroup = ((KalturaGroupPermission)permission).Group;
                    }
                    else
                    {
                        usersGroup = string.Empty;
                    }

                    foreach (var permissionItem in permission.PermissionItems)
                    {
                        // the dictionary is relevant only for action permission items
                        if (permissionItem is KalturaApiActionPermissionItem)
                        {
                            apiActionPermissionItem = (KalturaApiActionPermissionItem)permissionItem;
                            
                            // build the service action key
                            serviceActionPair = string.Format("{0}_{1}", apiActionPermissionItem.Service, apiActionPermissionItem.Action).ToLower();

                            // if the dictionary already contains the action, try to append the role and /or the users group
                            if (dictionary.ContainsKey(serviceActionPair))
                            {
                                if (!dictionary[serviceActionPair].ContainsKey(role.Id))
                                {
                                    dictionary[serviceActionPair].Add(role.Id, usersGroup);
                                }
                                else
                                {
                                    dictionary[serviceActionPair][role.Id] = string.Format("{0};{1}", usersGroup, dictionary[serviceActionPair][role.Id]);
                                }
                            }
                            // add the action to the dictionary
                            else
                            {
                                dictionary.Add(serviceActionPair, new Dictionary<long, string>() { { role.Id, usersGroup } });
                            }
                        }
                    }
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Checks if an action from a service is allowed for a user role under the group's role schema
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="service">Service name</param>
        /// <param name="action">Action name</param>
        /// <param name="roleIds">Role IDs</param>
        /// <param name="usersGroup">list of users separated by ';'</param>
        /// <returns>True if the action is permitted, false otherwise, and a users group list (if exists) for this action</returns>
        internal static bool IsActionPermitedForRoles(int groupId, string service, string action, List<long> roleIds, out string usersGroup)
        {
            usersGroup = null;
            StringBuilder usersGroupStringBuilder = new StringBuilder(); 

            // build the key for the service action key for roles schema (permission items - roles dictionary)
            string serviceActionKey = string.Format("{0}_{1}", service, action).ToLower();

            // get group's roles schema
            var actionPermissionItemsDictionary = GroupsManager.GetGroup(groupId).ActionPermissionItemsDictionary; 

            // if the permission for the action is not defined in the schema - return false
            if (!actionPermissionItemsDictionary.ContainsKey(serviceActionKey))
            {
                return false;
            }

            var roles = actionPermissionItemsDictionary[serviceActionKey];
            bool isPermitted = false;


            foreach (var roleId in roleIds)
            {
                // if the permission item for the action is part of one of the supplied roles - return true
                if (roles.ContainsKey(roleId))
                {
                    isPermitted = true;

                    // the action is permitted for the role, append the users group of the permission if defined
                    if (usersGroupStringBuilder.Length == 0)
                    {
                        usersGroupStringBuilder.Append(roles[roleId]);
                    }
                    else
                    {
                        usersGroupStringBuilder.AppendFormat(";{0}", roles[roleId]);
                    }
                }
            }

            usersGroup = usersGroupStringBuilder.ToString();
            return isPermitted;
        }
    }
}