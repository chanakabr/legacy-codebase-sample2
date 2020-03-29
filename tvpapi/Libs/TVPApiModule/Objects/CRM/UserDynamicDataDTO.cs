using Core.Users;
using Newtonsoft.Json;
using System;

namespace TVPApiModule.Objects.CRM
{
    public class UserDynamicDataDTO
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UserDynamicDataContainerDTO[] m_sUserData;
        public UserDynamicDataContainerDTO[] m_sUserDataField { get { return m_sUserData; } set { m_sUserData = value; } }
        public int UserId;

        public static UserDynamicDataDTO ConvertToDTO(UserDynamicData m_oDynamicData)
        {
            if (m_oDynamicData == null)
            {
                return null;
            }
            UserDynamicDataDTO res = new UserDynamicDataDTO();

            if (m_oDynamicData.m_sUserData != null && m_oDynamicData.m_sUserData.Length > 0)
            {
                res.m_sUserData = Array.ConvertAll(m_oDynamicData.m_sUserData, new Converter<UserDynamicDataContainer, UserDynamicDataContainerDTO>(UserDynamicDataContainerDTO.ConvertToDTO));
            }

            return res;
        }

        public UserDynamicData ToCore()
        {
            UserDynamicData result = new UserDynamicData()
            {
                UserId = this.UserId
            };

            if (this.m_sUserData != null && this.m_sUserData.Length > 0)
            {
                result.m_sUserData = Array.ConvertAll(this.m_sUserData, new Converter<UserDynamicDataContainerDTO, UserDynamicDataContainer>(UserDynamicDataContainerDTO.ConvertToCore));
            }

            return result;
        }
    }
}