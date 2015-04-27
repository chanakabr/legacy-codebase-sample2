using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Api;
using WebAPI.Clients.Utils;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models;


namespace WebAPI.Clients
{
    public class ApiClient : BaseClient
    {
        public ApiClient()
        {
            // TODO: Complete member initialization
        }

        protected WebAPI.Api.API Api
        {
            get
            {
                return (Module as WebAPI.Api.API);
            }
        }


        public LanguageObj[] GetGroupLanguages(int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                return Api.GetGroupLanguages(group.ApiCredentials.Username, group.ApiCredentials.Password);
            }
            catch (Exception)
            {
                throw new InternalServerErrorException((int)StatusCode.InternalConnectionIssue, "Error while calling API web service");
                
            }
        }
    }
}