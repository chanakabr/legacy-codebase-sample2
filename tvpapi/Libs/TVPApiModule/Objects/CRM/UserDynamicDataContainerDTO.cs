using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Core.Users;

namespace TVPApiModule.Objects.CRM
{
    public class UserDynamicDataContainerDTO
    {
        public string m_sDataType;
        public string m_sValue;
   

        internal static UserDynamicDataContainerDTO ConvertToDTO(UserDynamicDataContainer userDynamicData)
        {
            if(userDynamicData == null)
            {
                return null;
            }
            var res = new UserDynamicDataContainerDTO
            {
                m_sDataType = userDynamicData.m_sDataType,
                m_sValue = userDynamicData.m_sValue
            };
            return res;

        }

    }
}