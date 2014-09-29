using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserCredentials
    {
        public string m_sUsername;
        public string m_sPassword;

        public UserCredentials()
        {
            this.m_sUsername = string.Empty;
            this.m_sPassword = string.Empty;
        }
    }
}
