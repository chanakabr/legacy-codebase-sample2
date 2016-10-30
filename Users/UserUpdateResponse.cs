using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class UserResponseObject
    {
        public UserResponseObject() { }
        public void Initialize(ResponseStatus theStatus, User u)
        {
            m_RespStatus = theStatus;
            m_user = u;           
        }

        public ResponseStatus m_RespStatus;
        public User m_user;
        public string m_userInstanceID;      
    }
}
