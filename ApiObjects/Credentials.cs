using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class Credentials
    {
        public string m_sUsername;
        public string m_sPassword;

        public Credentials()
        {
            this.m_sUsername = string.Empty;
            this.m_sPassword = string.Empty;
        }

        public Credentials(string user, string pass)
        {
            this.m_sUsername = user;
            this.m_sPassword = pass;
        }
    }
}
