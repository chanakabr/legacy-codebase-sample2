using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class AdminAccountUserResponse
    {

        public AdminUserStatus m_status;
        public AdminAccountUserObj m_adminUser;

        public AdminAccountUserResponse()
        {
        }

        public void Initialize(AdminUserStatus status, AdminAccountUserObj adminUserObj)
        {
            m_status = status;
            m_adminUser = adminUserObj;
        }
    }
}
