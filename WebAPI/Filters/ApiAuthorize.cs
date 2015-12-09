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
using Couchbase.Extensions;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers;

namespace WebAPI.Controllers
{
    internal class ApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        [Flags]
        public enum eRole { /* Placeholder */ }
        public eRole Role { get; set; }
        public bool allowAnonymous { get; private set; }
        
        private bool silent;
        
        // TODO: get from configuration
        private const long ANONYMOUS_ROLE_ID = 0;
        private const string PARTNER_WILDCARD = "partner*";
        private const string HOUSEHOLD_WILDCARD = "household*";

        public ApiAuthorizeAttribute(bool AllowAnonymous = false, bool Silent = false)
            : base()
        {
            allowAnonymous = AllowAnonymous;
            silent = Silent;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            KS ks = KS.GetFromRequest();

            if (ks == null)
                throw new UnauthorizedException((int)StatusCode.ServiceForbidden, "Service Forbidden");
            else if (!ks.IsValid && !silent)
                throw new UnauthorizedException((int)StatusCode.ExpiredKS, "Expired KS");

            string service = (string)actionContext.ActionArguments["service_name"];
            string action = (string)actionContext.ActionArguments["action_name"];

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
            var extraUserId = HttpContext.Current.Items["user_id"];
            string userId = extraUserId != null ? extraUserId.ToString() : null;

            // if exists and is in the allowed group users list - override the user id in ks (HOUSEHOLD_WILDCARD = everyone in the domain is allowed, PARTNER_WILDCARD = everyone in the group is allowed)
            if ((!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(allowedUsersGroup)) && (
                allowedUsersGroup.Contains(userId)||
                allowedUsersGroup.Contains(PARTNER_WILDCARD) ||
                (allowedUsersGroup.Contains(HOUSEHOLD_WILDCARD) && AuthorizationManager.IsUserInHousehold(userId, ks.GroupId))))
            {
                ks.UserId = userId;
                KS.SaveOnRequest(ks);
            }
            
            return true;
        }
    }
}
