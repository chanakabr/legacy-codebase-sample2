using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class UserDynamicDataContainer
    {
        public UserDynamicDataContainer()
        {
            m_sDataType = "";
            m_sValue = "";
        }
        public void Initialize(string sDataType, string sValue)
        {
            m_sDataType = sDataType;
            m_sValue = sValue;
        }

        public string m_sDataType;
        public string m_sValue;
    }
}
