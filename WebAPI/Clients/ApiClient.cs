using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Api;
using WebAPI.Exceptions;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Utils;
using KLogMonitor;
using System.Reflection;


namespace WebAPI.Clients
{
    public class ApiClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ApiClient()
        {
        }

        protected WebAPI.Api.API Api
        {
            get
            {
                return (Module as WebAPI.Api.API);
            }
        }

        public LanguageObj[] GetGroupLanguages(string username, string password)
        {
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    return Api.GetGroupLanguages(username, password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to get languages. username: {0}, exception: {1}", username, ex);
                throw new ClientException((int)StatusCode.InternalConnectionIssue, "Error while calling API web service");
            }
        }

        public LanguageObj[] GetGroupLanguages(int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);
            return GetGroupLanguages(group.ApiCredentials.Username, group.ApiCredentials.Password);
        }
    }
}