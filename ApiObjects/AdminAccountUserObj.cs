using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
     [Serializable]
    public class AdminAccountUserObj
    {
        public string m_accountUserName;

        public string m_accountEmail;

        public int m_accountUserID;

        public int m_groupID;

        public AdminAccountObj m_relatedAccount;

        public AdminAccountUserObj()
        {
        }

        public void Initialize(int accountUserID, string accountUserName, string accountUserEmail, int groupID, AdminAccountObj relatedAccount)
        {
            m_accountUserName = accountUserName;
            m_accountEmail = accountUserEmail;
            m_accountUserID = accountUserID;
            m_groupID = groupID;
            m_relatedAccount = relatedAccount;
        }
    }
}
