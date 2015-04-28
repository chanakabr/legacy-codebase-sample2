using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Clients.Exceptions;
using WebAPI.Clients.Utils;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models;
using WebAPI.Users;

namespace WebAPI.Clients
{
    public class UsersClient : BaseClient
    {
        //private readonly ILog logger = LogManager.GetLogger(typeof(UsersClient));

        public UsersClient()
        {
            // TODO: Complete member initialization
        }
       
        protected WebAPI.Users.UsersService Users
        {
            get
            {
                return (Module as WebAPI.Users.UsersService);
            }
        }

        public WebAPI.Models.User SignIn(int groupId, string userName, string password)
        {
            WebAPI.Models.User user = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                //TODO: add parameers
                UserResponseObject response = Users.SignIn(group.UsersCredentials.Username, group.UsersCredentials.Password, userName, password, string.Empty, string.Empty, string.Empty, false);
                user = Mapper.Map<WebAPI.Models.User>(response);
            }
            catch (Exception)
            {
                throw new ClientException((int)StatusCode.InternalConnectionIssue);
            }
            return user;
        }
    }
}