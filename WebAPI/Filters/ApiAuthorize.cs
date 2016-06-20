using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers;
using WebAPI.Filters;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : Attribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        
        private bool silent;
        
        private const long ANONYMOUS_ROLE_ID = 0;
        private const string PARTNER_WILDCARD = "partner*";
        private const string HOUSEHOLD_WILDCARD = "household*";

        public ApiAuthorizeAttribute(bool Silent = false)
            : base()
        {
            silent = Silent;
        }

        public bool IsAuthorized(string service, string action)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null)
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            
            if (!ks.IsValid && !silent)
                throw new UnauthorizedException((int)StatusCode.ExpiredKS, "Expired KS");

            string allowedUsersGroup = null;
            List<long> roleIds = new List<long>() { ANONYMOUS_ROLE_ID };

            if (ks.UserId != "0")
            {
                // not anonymous user - get user's roles
                var userRoleIds = ClientsManager.UsersClient().GetUserRoleIds(ks.GroupId, ks.UserId);
                if (userRoleIds != null && userRoleIds.Count > 0)
                {
                    roleIds.AddRange(userRoleIds);
                }
            }

            // no roles found for the user
            if (roleIds == null || roleIds.Count == 0)
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            
            // user not permitted
            if (!RolesManager.IsActionPermitedForRoles(ks.GroupId, service, action, roleIds, out allowedUsersGroup))
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            
            // allowed group users (additional user_id) handling:
            // get user_id additional parameter
            string userId = null;
            if (HttpContext.Current.Items.Contains(RequestParser.REQUEST_USER_ID))
            {
                var extraUserId = HttpContext.Current.Items[RequestParser.REQUEST_USER_ID];
                userId = extraUserId != null ? extraUserId.ToString() : null;
            }
            // if exists and is in the allowed group users list - override the user id in ks (HOUSEHOLD_WILDCARD = everyone in the domain is allowed, PARTNER_WILDCARD = everyone in the group is allowed)
            if (!string.IsNullOrEmpty(userId))
            {
                if (!string.IsNullOrEmpty(allowedUsersGroup) && (
                    allowedUsersGroup.Contains(userId) ||
                    (allowedUsersGroup.Contains(PARTNER_WILDCARD) && AuthorizationManager.IsUserInGroup(userId, ks.GroupId)) ||
                    (allowedUsersGroup.Contains(HOUSEHOLD_WILDCARD) && AuthorizationManager.IsUserInHousehold(userId, ks.GroupId))))
                {
                    ks.UserId = userId;
                    KS.SaveOnRequest(ks);
                }
                else
                {
                    throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service forbidden for additional user");
                }
            }
            return true;
        }
    }
}
