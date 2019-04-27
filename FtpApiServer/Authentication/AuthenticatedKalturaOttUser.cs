using FubarDev.FtpServer.AccountManagement;
using System;

namespace FtpApiServer.Authentication
{
    public class AuthenticatedKalturaOttUser : IFtpUser
    {

        public string Ks { get; set; }
        public int GroupId { get; set; }
        public string Username { get; set; }

        string IFtpUser.Name => Username;

        public AuthenticatedKalturaOttUser(string userName, string ks, int groupId)
        {
            Ks = ks;
            GroupId = groupId;
            Username = userName;
        }


        bool IFtpUser.IsInGroup(string groupName)
        {
            return groupName.Equals(GroupId.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}