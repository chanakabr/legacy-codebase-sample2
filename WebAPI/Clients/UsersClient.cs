using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models;
using WebAPI.Users;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        public UsersClient()
        {
        }
       
        protected WebAPI.Users.UsersService Users
        {
            get
            {
                return (Module as WebAPI.Users.UsersService);
            }
        }

        public WebAPI.Models.User.ClientUser SignIn(int groupId, string userName, string password)
        {
            WebAPI.Models.User.ClientUser user = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                //TODO: add parameters
                UserResponseObject response = Users.SignIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, string.Empty, string.Empty, false);
                user = Mapper.Map<WebAPI.Models.User.ClientUser>(response);
            }
            catch (Exception)
            {
                throw new ClientException((int)StatusCode.InternalConnectionIssue);
            }
            return user;
        }
    }
}