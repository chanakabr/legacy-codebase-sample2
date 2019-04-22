using FubarDev.FtpServer.AccountManagement;
using System;

namespace FtpApiServer
{
    public class KalturaOttAuthenticationProvider : IMembershipProvider
    {
        public MemberValidationResult ValidateUser(string username, string password)
        {
            // TODO: Authenticate user with group/userId and password.
            var usernameParams = username.Split('/');
            var user = usernameParams[0];
            var groupId = usernameParams[1];
            var authenticatedUser = new KalturaOttUser(username, groupId);
            Console.WriteLine($"Authentication user:[{username}] and group:[{groupId}]");
            var validationResult = new MemberValidationResult(MemberValidationStatus.AuthenticatedUser, authenticatedUser);
            return validationResult;
        }
    }

    public class KalturaOttUser : IFtpUser
    {
        private readonly string _GroupId;
        public string Name { get; set; }


        public KalturaOttUser(string userName, string groupId)
        {
            _GroupId = groupId;
            Name = userName;
        }


        public bool IsInGroup(string groupName)
        {
            return groupName.Equals(_GroupId, StringComparison.OrdinalIgnoreCase);
        }
    }
}