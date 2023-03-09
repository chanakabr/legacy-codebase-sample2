using System;
using System.Collections.Generic;
using System.Text;
using ApiObjects;
using ApiObjects.Roles;
using Core.Users;

namespace ApiLogic.Users.Managers
{
    public interface IUserManager
    {
        int AddAdminUser(int partnerId, string username, string password);
    }

    public class UserManager : IUserManager
    {

        private static readonly Lazy<UserManager> LazyInstance = new Lazy<UserManager>(() => new UserManager(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public static UserManager Instance => LazyInstance.Value;

        private static readonly List<long> AdminRole = new List<long> { PredefinedRoleId.ADMINISTRATOR };

        public int AddAdminUser(int partnerId, string username, string password)
        {
            var user = new User
            {
                m_oBasicData = new UserBasicData
                {
                    m_sUserName = username,
                    RoleIds = AdminRole
                }
            };

            user.m_oBasicData.SetPassword(password, partnerId);
            return user.SaveForInsert(partnerId, true, false);
        }
    }
}
