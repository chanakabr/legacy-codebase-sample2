using Core.Users;
using Newtonsoft.Json;
using System;

namespace TVPApiModule.Objects.CRM
{
    public class UserDynamicDataDTO
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UserDynamicDataContainerDTO[] m_sUserData;
        public int UserId;
        public int GroupId;
        public long Id;

        public static UserDynamicDataDTO ConvertToDTO(UserDynamicData m_oDynamicData)
        {
            if(m_oDynamicData == null)
            {
                return null;
            }
            UserDynamicDataDTO res = new UserDynamicDataDTO();
            res.Id = m_oDynamicData.Id;
            res.GroupId = m_oDynamicData.GroupId;
            res.UserId = m_oDynamicData.UserId;

            if (m_oDynamicData.m_sUserData != null && m_oDynamicData.m_sUserData.Length > 0)
            {
                res.m_sUserData = Array.ConvertAll(m_oDynamicData.m_sUserData, new Converter<UserDynamicDataContainer, UserDynamicDataContainerDTO>(UserDynamicDataContainerDTO.ConvertToDTO));
            }

            return res;
        }
    }
}