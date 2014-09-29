using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserAndPass
    {
        private string m_sUsername;
        private string m_sPassword;

        public string Password
        {
            get { return this.m_sPassword; }
            set { this.m_sPassword = value; }
        }

        public string Username
        {
            get { return this.m_sUsername; }
            set { this.m_sUsername = value; }
        }
    }
}
