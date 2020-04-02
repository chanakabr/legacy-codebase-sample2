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
        public string m_sDataTypeField { get { return m_sDataType; } set { m_sDataType = value; } }
        public string m_sValue;
        public string m_sValueField { get { return m_sValue; } set { m_sValue = value; } }

        internal static UserDynamicDataContainerDTO ConvertToDTO(UserDynamicDataContainer userDynamicData)
        {
            if (userDynamicData == null)
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

        internal static UserDynamicDataContainer ConvertToCore(UserDynamicDataContainerDTO source)
        {
            if (source == null)
            {
                return null;
            }

            var res = new UserDynamicDataContainer
            {
                m_sDataType = source.m_sDataType,
                m_sValue = source.m_sValue
            };

            return res;
        }
    }
}